namespace Cnab.Bancos;

using System.Globalization;
using Cnab.Digitos;

/// <summary>
/// Regras do Banco do Brasil (001) para nosso número, DV, campo livre do
/// código de barras e DV do prefixo da agência.
/// <para>Fontes oficiais:</para>
/// <list type="bullet">
/// <item>BB, <c>Especificações Técnicas para Confecção de Boleto de Pagamento
/// do Banco do Brasil</c> (Doc5175Bloqueto.pdf, maio/2019), Anexos VII a XI:
/// composição do nosso número por tamanho de convênio, campo livre do código
/// de barras e <b>Anexo XI</b> (módulo 11 com pesos 9 a 2 da direita para a
/// esquerda; resto 1-9 → DV = resto; resto 10 → "X"; resto 0 → 0).
/// https://www.bb.com.br/docs/pub/emp/empl/dwn/Doc5175Bloqueto.pdf
/// (espelho consultado:
/// https://boletobancario-codigodebarras.com/docs/Doc5175Bloqueto.pdf).</item>
/// <item>BB, <c>Particularidades BB – Leiaute CNAB 240</c> (junho/2019), campo
/// 13.3P: convênio de 4 → 11 + DV; de 6 → 11 + DV; de 7 → 17 sem DV.
/// https://vrsystem.info/assets/storage/os/2021/05/CNAB240BB_d2f7007d-5f2e-49a4-966b-4a347147ee8b.pdf</item>
/// </list>
/// <para>Vetor resolvido do Anexo XI: nosso número <c>05009401448</c> → soma
/// 221, resto 1, DV 1 (<c>05009401448-1</c>).</para>
/// </summary>
public static class BancoDoBrasil
{
    /// <summary>Tamanho do nosso número para convênios de 4 ou 6 posições (11 + 1 DV).</summary>
    public const int TamanhoNossoNumeroComDv = 12;

    /// <summary>Tamanho do nosso número para convênios de 7 posições (17, sem DV).</summary>
    public const int TamanhoNossoNumeroSemDv = 17;

    /// <summary>
    /// DV do nosso número, do prefixo da agência e do código do beneficiário
    /// segundo o Anexo XI do manual citado: módulo 11 com multiplicadores de
    /// 9 a 2 da direita para a esquerda (reciclando em 9 após o 2), soma dos
    /// produtos dividida por 11. O DV é o próprio resto quando este está entre
    /// 1 e 9; resto 10 produce "X" e resto 0 produce "0" — regra diferente da
    /// maioria dos bancos, que usam 11 - resto.
    /// </summary>
    /// <param name="digitos">Base de cálculo (apenas dígitos, 1 a 17 posições).</param>
    /// <returns>"0" a "9" ou "X".</returns>
    /// <exception cref="ArgumentException">Entrada fora do formato.</exception>
    public static string CalcularDvModulo11(ReadOnlySpan<char> digitos)
    {
        if (digitos.Length is < 1 or > 17 || !SaoDigitos(digitos))
        {
            throw new ArgumentException(
                "A base de cálculo do DV BB deve ter de 1 a 17 dígitos.", nameof(digitos));
        }

        var soma = 0;
        var peso = 9;
        for (var i = digitos.Length - 1; i >= 0; i--)
        {
            soma += (digitos[i] - '0') * peso;
            peso = peso == 2 ? 9 : peso - 1;
        }

        var resto = soma % 11;
        return resto switch
        {
            0 => "0",
            10 => "X",
            _ => resto.ToString(CultureInfo.InvariantCulture),
        };
    }

    /// <summary>
    /// DV do nosso número de 11 posições (convênios de 4 e 6 dígitos),
    /// calculado sobre convênio + sequencial pelo Anexo XI.
    /// <para>Vetor do manual: <c>05009401448</c> → 1.</para>
    /// </summary>
    /// <param name="nossoNumeroSemDv">Convênio + sequencial, 11 dígitos.</param>
    /// <returns>DV ("0" a "9" ou "X").</returns>
    /// <exception cref="ArgumentException">Entrada fora do formato.</exception>
    public static string CalcularDvNossoNumero(string nossoNumeroSemDv)
    {
        Exigir(nossoNumeroSemDv, 11, nameof(nossoNumeroSemDv));
        return CalcularDvModulo11(nossoNumeroSemDv);
    }

    /// <summary>
    /// DV do prefixo da agência (Anexo XI: o mesmo módulo 11 é usado para o
    /// prefixo da agência), calculado sobre os 4 dígitos da agência sem DV.
    /// </summary>
    /// <param name="agencia">Agência com 4 dígitos, sem DV.</param>
    /// <returns>DV ("0" a "9" ou "X").</returns>
    /// <exception cref="ArgumentException">Entrada fora do formato.</exception>
    public static string CalcularDvAgencia(string agencia)
    {
        Exigir(agencia, 4, nameof(agencia));
        return CalcularDvModulo11(agencia);
    }

    /// <summary>
    /// DV do código do beneficiário (Anexo XI), calculado pelo mesmo módulo 11.
    /// </summary>
    /// <param name="codigoBeneficiario">Código do beneficiário (dígitos).</param>
    /// <returns>DV ("0" a "9" ou "X").</returns>
    /// <exception cref="ArgumentException">Entrada fora do formato.</exception>
    public static string CalcularDvCodigoBeneficiario(string codigoBeneficiario)
    {
        if (string.IsNullOrWhiteSpace(codigoBeneficiario)
            || !codigoBeneficiario.All(static c => c is >= '0' and <= '9'))
        {
            throw new ArgumentException(
                "Código do beneficiário BB deve conter apenas dígitos.", nameof(codigoBeneficiario));
        }

        return CalcularDvModulo11(codigoBeneficiario);
    }

    /// <summary>
    /// Composição do nosso número para convênios de 4 posições (carteira 17,
    /// convênios de 0001 a 9.999): convênio (4) + sequencial (7) + DV calculado.
    /// </summary>
    /// <param name="convenio">Convênio com 4 dígitos.</param>
    /// <param name="sequencial">Sequencial com 7 dígitos.</param>
    /// <returns>Nosso número com 12 posições.</returns>
    /// <exception cref="ArgumentException">Entradas fora do formato.</exception>
    public static string ComporNossoNumeroConvenio4(string convenio, string sequencial)
    {
        Exigir(convenio, 4, nameof(convenio));
        Exigir(sequencial, 7, nameof(sequencial));
        return convenio + sequencial + CalcularDvNossoNumero(convenio + sequencial);
    }

    /// <summary>
    /// Composição do nosso número para convênios de 6 posições (10.000 a
    /// 999.999): convênio (6) + sequencial (5) + DV calculado.
    /// </summary>
    /// <param name="convenio">Convênio com 6 dígitos.</param>
    /// <param name="sequencial">Sequencial com 5 dígitos.</param>
    /// <returns>Nosso número com 12 posições.</returns>
    /// <exception cref="ArgumentException">Entradas fora do formato.</exception>
    public static string ComporNossoNumeroConvenio6(string convenio, string sequencial)
    {
        Exigir(convenio, 6, nameof(convenio));
        Exigir(sequencial, 5, nameof(sequencial));
        return convenio + sequencial + CalcularDvNossoNumero(convenio + sequencial);
    }

    /// <summary>
    /// Composição do nosso número para convênios de 4 posições com DV informado
    /// pelo banco ("X" é aceito quando o cálculo resulta em 10).
    /// </summary>
    /// <param name="convenio">Convênio com 4 dígitos.</param>
    /// <param name="sequencial">Sequencial com 7 dígitos.</param>
    /// <param name="dv">DV informado pelo banco; um dígito ou "X".</param>
    /// <returns>Nosso número com 12 posições.</returns>
    /// <exception cref="ArgumentException">Entradas fora do formato.</exception>
    public static string ComporNossoNumeroConvenio4(string convenio, string sequencial, string dv)
    {
        Exigir(convenio, 4, nameof(convenio));
        Exigir(sequencial, 7, nameof(sequencial));
        ExigirDv(dv);
        return convenio + sequencial + dv;
    }

    /// <summary>
    /// Composição do nosso número para convênios de 6 posições com DV informado
    /// pelo banco ("X" é aceito quando o cálculo resulta em 10).
    /// </summary>
    /// <param name="convenio">Convênio com 6 dígitos.</param>
    /// <param name="sequencial">Sequencial com 5 dígitos.</param>
    /// <param name="dv">DV informado pelo banco; um dígito ou "X".</param>
    /// <returns>Nosso número com 12 posições.</returns>
    /// <exception cref="ArgumentException">Entradas fora do formato.</exception>
    public static string ComporNossoNumeroConvenio6(string convenio, string sequencial, string dv)
    {
        Exigir(convenio, 6, nameof(convenio));
        Exigir(sequencial, 5, nameof(sequencial));
        ExigirDv(dv);
        return convenio + sequencial + dv;
    }

    /// <summary>
    /// Composição do nosso número para convênios de 7 posições (a partir de
    /// 1.000.000): convênio (7) + sequencial (10), sem DV.
    /// </summary>
    /// <param name="convenio">Convênio com 7 dígitos.</param>
    /// <param name="sequencial">Sequencial com 10 dígitos.</param>
    /// <returns>Nosso número com 17 posições, sem DV.</returns>
    /// <exception cref="ArgumentException">Entradas fora do formato.</exception>
    public static string ComporNossoNumeroConvenio7(string convenio, string sequencial)
    {
        Exigir(convenio, 7, nameof(convenio));
        Exigir(sequencial, 10, nameof(sequencial));
        return convenio + sequencial;
    }

    /// <summary>
    /// Campo livre (posições 20-44) para convênio de 4 posições (Anexo VII):
    /// convênio (4) + sequencial (7, sem DV) + agência (4, sem DV) + conta (8,
    /// sem DV) + carteira/modalidade (2).
    /// </summary>
    /// <param name="convenio">Convênio com 4 dígitos.</param>
    /// <param name="sequencial">Sequencial com 7 dígitos.</param>
    /// <param name="agencia">Agência de relacionamento, até 4 dígitos.</param>
    /// <param name="conta">Conta de relacionamento, até 8 dígitos.</param>
    /// <param name="carteira">Carteira/modalidade com 2 dígitos.</param>
    /// <returns>Campo livre com 25 posições.</returns>
    /// <exception cref="ArgumentException">Entradas fora do formato.</exception>
    public static string ComporCampoLivreConvenio4(
        string convenio,
        string sequencial,
        string agencia,
        string conta,
        string carteira)
    {
        Exigir(convenio, 4, nameof(convenio));
        Exigir(sequencial, 7, nameof(sequencial));
        ExigirCarteira(carteira);
        return convenio + sequencial
            + ExigirAte(agencia, 4, nameof(agencia))
            + ExigirAte(conta, 8, nameof(conta))
            + carteira;
    }

    /// <summary>
    /// Campo livre (posições 20-44) para convênio de 6 posições (Anexo VIII):
    /// convênio (6) + sequencial (5, sem DV) + agência (4, sem DV) + conta (8,
    /// sem DV) + carteira/modalidade (2).
    /// </summary>
    /// <param name="convenio">Convênio com 6 dígitos.</param>
    /// <param name="sequencial">Sequencial com 5 dígitos.</param>
    /// <param name="agencia">Agência de relacionamento, até 4 dígitos.</param>
    /// <param name="conta">Conta de relacionamento, até 8 dígitos.</param>
    /// <param name="carteira">Carteira/modalidade com 2 dígitos.</param>
    /// <returns>Campo livre com 25 posições.</returns>
    /// <exception cref="ArgumentException">Entradas fora do formato.</exception>
    public static string ComporCampoLivreConvenio6(
        string convenio,
        string sequencial,
        string agencia,
        string conta,
        string carteira)
    {
        Exigir(convenio, 6, nameof(convenio));
        Exigir(sequencial, 5, nameof(sequencial));
        ExigirCarteira(carteira);
        return convenio + sequencial
            + ExigirAte(agencia, 4, nameof(agencia))
            + ExigirAte(conta, 8, nameof(conta))
            + carteira;
    }

    /// <summary>
    /// Campo livre (posições 20-44) para convênio de 7 posições (Anexo IX):
    /// zeros (6) + convênio (7) + sequencial (10) + carteira/modalidade (2).
    /// </summary>
    /// <param name="convenio">Convênio com 7 dígitos.</param>
    /// <param name="sequencial">Sequencial com 10 dígitos.</param>
    /// <param name="carteira">Carteira/modalidade com 2 dígitos.</param>
    /// <returns>Campo livre com 25 posições.</returns>
    /// <exception cref="ArgumentException">Entradas fora do formato.</exception>
    public static string ComporCampoLivreConvenio7(string convenio, string sequencial, string carteira)
    {
        Exigir(convenio, 7, nameof(convenio));
        Exigir(sequencial, 10, nameof(sequencial));
        ExigirCarteira(carteira);
        return "000000" + convenio + sequencial + carteira;
    }

    private static bool SaoDigitos(ReadOnlySpan<char> digitos)
    {
        foreach (var c in digitos)
        {
            if (c is < '0' or > '9')
            {
                return false;
            }
        }

        return true;
    }

    private static void Exigir(string valor, int tamanho, string nome)
    {
        if (valor.Length != tamanho || !valor.All(static c => c is >= '0' and <= '9'))
        {
            throw new ArgumentException($"'{nome}' deve ter exatamente {tamanho} dígito(s).", nome);
        }
    }

    private static void ExigirCarteira(string carteira)
    {
        if (carteira.Length != 2 || !carteira.All(static c => c is >= '0' and <= '9'))
        {
            throw new ArgumentException("Carteira/modalidade BB deve ter 2 dígitos.", nameof(carteira));
        }
    }

    private static string ExigirAte(string valor, int tamanho, string nome)
    {
        if (valor.Length is < 1 || valor.Length > tamanho || !valor.All(static c => c is >= '0' and <= '9'))
        {
            throw new ArgumentException($"'{nome}' deve ter de 1 a {tamanho} dígito(s).", nome);
        }

        return valor.PadLeft(tamanho, '0');
    }

    private static void ExigirDv(string dv)
    {
        if (dv.Length != 1 || (!char.IsAsciiDigit(dv[0]) && dv != "X"))
        {
            throw new ArgumentException("DV do BB deve ser um dígito ou a letra 'X' (maiúscula).", nameof(dv));
        }
    }
}

/// <summary>
/// Regras do Banco Inter (077).
/// <para>Fonte oficial: Inter, <c>Manual CNAB400 – Emissão de boletos de
/// cobrança</c> (V9, 06/07/2026), seções 7.1.3 (campo livre), 7.3 (DV do
/// nosso número) e 13 (carteiras 110, 112 e 121):
/// https://s3.sa-east-1.amazonaws.com/static.bancointer.com.br/openbanking/cnab/manual/manual.pdf
/// </para>
/// <para>O DV do nosso número é módulo 10 sobre agência (4) + carteira (3) +
/// nosso número sem DV (10). Vetor resolvido da seção 7.3: agência 0001,
/// carteira 110, NN 0004309540 → base "00011100004309540" → DV 1.</para>
/// </summary>
public static class Inter
{
    /// <summary>Tamanho do nosso número no CNAB 400 do Inter (10 + DV = 11 posições).</summary>
    public const int TamanhoNossoNumero = 11;

    /// <summary>
    /// DV do nosso número Inter: módulo 10 sobre agência (4) + carteira (3) +
    /// nosso número sem DV (10), pesos 2 e 1 da direita para a esquerda.
    /// </summary>
    /// <param name="agencia">Agência com 4 dígitos (sem DV).</param>
    /// <param name="carteira">Carteira com 3 dígitos (por exemplo "110").</param>
    /// <param name="nossoNumeroSemDv">Nosso número com 10 dígitos, sem DV.</param>
    /// <returns>DV (0 a 9).</returns>
    /// <exception cref="ArgumentException">Entradas fora do formato.</exception>
    public static int CalcularDvNossoNumero(string agencia, string carteira, string nossoNumeroSemDv)
    {
        Exigir(agencia, 4, nameof(agencia));
        Exigir(carteira, 3, nameof(carteira));
        Exigir(nossoNumeroSemDv, 10, nameof(nossoNumeroSemDv));
        return Modulo.Modulo10([.. (agencia + carteira + nossoNumeroSemDv)]);
    }

    /// <summary>
    /// Compõe o nosso número de 11 posições (10 + DV) para a carteira 110, na
    /// qual o cliente numera os títulos a partir de faixa reservada pelo Inter.
    /// Nas carteiras 112/121 o banco atribui o número, disponível no retorno.
    /// </summary>
    /// <param name="agencia">Agência com 4 dígitos.</param>
    /// <param name="carteira">Carteira com 3 dígitos.</param>
    /// <param name="sequencial">Sequencial com 10 dígitos, sem DV.</param>
    /// <returns>Nosso número com 11 posições, incluindo o DV.</returns>
    /// <exception cref="ArgumentException">Entradas fora do formato.</exception>
    public static string ComporNossoNumero(string agencia, string carteira, string sequencial)
    {
        Exigir(sequencial, 10, nameof(sequencial));
        return sequencial
            + CalcularDvNossoNumero(agencia, carteira, sequencial).ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Campo livre do código de barras (posições 20-44), seção 7.1.3:
    /// agência (4) + carteira (3) + número da operação (7) + nosso número com
    /// DV (11).
    /// </summary>
    /// <param name="agencia">Agência com 4 dígitos.</param>
    /// <param name="carteira">Carteira com 3 dígitos.</param>
    /// <param name="numeroOperacao">Número da operação com 7 dígitos.</param>
    /// <param name="nossoNumeroComDv">Nosso número com 11 posições, com DV.</param>
    /// <returns>Campo livre com 25 posições.</returns>
    /// <exception cref="ArgumentException">Entradas fora do formato.</exception>
    public static string ComporCampoLivre(
        string agencia,
        string carteira,
        string numeroOperacao,
        string nossoNumeroComDv)
    {
        Exigir(agencia, 4, nameof(agencia));
        Exigir(carteira, 3, nameof(carteira));
        Exigir(numeroOperacao, 7, nameof(numeroOperacao));
        Exigir(nossoNumeroComDv, 11, nameof(nossoNumeroComDv));
        return agencia + carteira + numeroOperacao + nossoNumeroComDv;
    }

    /// <summary>
    /// Valida o formato do nosso número atribuído pelo Inter: 11 dígitos
    /// (10 + DV), conforme o item 13 do registro de transação CNAB 400.
    /// </summary>
    /// <param name="nossoNumero">Nosso número retornado pelo banco.</param>
    /// <exception cref="ArgumentException">Formato inválido.</exception>
    public static void ValidarNossoNumero(string nossoNumero) =>
        Exigir(nossoNumero, TamanhoNossoNumero, nameof(nossoNumero));

    private static void Exigir(string valor, int tamanho, string nome)
    {
        if (valor.Length != tamanho || !valor.All(static c => c is >= '0' and <= '9'))
        {
            throw new ArgumentException($"'{nome}' deve ter exatamente {tamanho} dígito(s).", nome);
        }
    }
}

/// <summary>
/// Regras do BTG Pactual (208).
/// <para>Fonte oficial: BTG Pactual, portal do desenvolvedor — "Cobrança",
/// seção "Código de Barras" (campo livre: agência 4 + carteira 2 + nosso
/// número 11 sem DV + conta 7 + zero fixo; fator com reinício em 22/02/2025):
/// https://developers.empresas.btgpactual.com/docs/cobran%C3%A7a
/// </para>
/// <para>O BTG não publica o algoritmo do DV do nosso número: o número e o DV
/// são atribuídos pelo banco e devolvidos no retorno/API. A biblioteca codifica
/// o formato aceito e o campo livre publicado; nenhum DV é inventado.</para>
/// </summary>
public static class Btg
{
    /// <summary>Tamanho do nosso número aceito no fluxo de cobrança do BTG (11 posições).</summary>
    public const int TamanhoNossoNumero = 11;

    /// <summary>
    /// Campo livre do código de barras (posições 20-44), conforme o portal do
    /// desenvolvedor citado: agência (4) + carteira (2) + nosso número (11,
    /// sem DV) + conta do beneficiário (7) + zero fixo (1).
    /// </summary>
    /// <param name="agencia">Agência com 4 dígitos.</param>
    /// <param name="carteira">Carteira com 2 dígitos.</param>
    /// <param name="nossoNumeroSemDv">Nosso número com 11 dígitos, sem DV.</param>
    /// <param name="conta">Conta do beneficiário com 7 dígitos, sem DV.</param>
    /// <returns>Campo livre com 25 posições.</returns>
    /// <exception cref="ArgumentException">Entradas fora do formato.</exception>
    public static string ComporCampoLivre(string agencia, string carteira, string nossoNumeroSemDv, string conta)
    {
        Exigir(agencia, 4, nameof(agencia));
        Exigir(carteira, 2, nameof(carteira));
        Exigir(nossoNumeroSemDv, 11, nameof(nossoNumeroSemDv));
        Exigir(conta, 7, nameof(conta));
        return agencia + carteira + nossoNumeroSemDv + conta + "0";
    }

    /// <summary>
    /// Valida o formato do nosso número atribuído pelo BTG: 11 dígitos.
    /// </summary>
    /// <param name="nossoNumero">Nosso número retornado pelo banco.</param>
    /// <exception cref="ArgumentException">Formato inválido.</exception>
    public static void ValidarNossoNumero(string nossoNumero) =>
        Exigir(nossoNumero, TamanhoNossoNumero, nameof(nossoNumero));

    private static void Exigir(string valor, int tamanho, string nome)
    {
        if (valor.Length != tamanho || !valor.All(static c => c is >= '0' and <= '9'))
        {
            throw new ArgumentException($"'{nome}' deve ter exatamente {tamanho} dígito(s).", nome);
        }
    }
}
