namespace Cnab.Bancos;

/// <summary>
/// Regras do Banco do Brasil (001) para composição do nosso número.
/// <para>Fonte oficial: BB, <c>Particularidades BB – Leiaute CNAB 240
/// (Junho/2019)</c>, campo 13.3P "Identificação do Título no Banco":
/// convênios de 4 posições (carteira 17 até 9.999) usam nosso número com 11
/// posições + DV (convênio 4 + sequencial 7 + DV); convênios de 6 posições
/// (10.000 a 999.999) usam 11 posições + DV (convênio 6 + sequencial 5 +
/// DV); convênios de 7 posições (a partir de 1.000.000) usam 17 posições SEM
/// DV (convênio 7 + sequencial 10). "Não há DV para o Nosso-Número quando o
/// número de convênio de cobrança for acima de 1.000.000" (CNAB 400,
/// nota 06).
/// https://www.bb.com.br (manual espelhado em docs/layouts.md).</para>
/// <para>
/// O BB não publica em seus manuais o algoritmo do DV do nosso número nem o
/// layout do campo livre do código de barras em canal publicamente acessível
/// (o manual "Utilização do Código de Barras" é distribuído na contratação);
/// por isso este tipo expõe apenas a composição publicada e valida o DV
/// informado (o manual registra que o dígito pode ser "X", informado em
/// maiúsculo — nota do campo DV da agência/conta do mesmo leiaute).
/// </para>
/// </summary>
public static class BancoDoBrasil
{
    /// <summary>Tamanho do nosso número para convênios de 4 ou 6 posições (11 + 1 DV).</summary>
    public const int TamanhoNossoNumeroComDv = 12;

    /// <summary>Tamanho do nosso número para convênios de 7 posições (17, sem DV).</summary>
    public const int TamanhoNossoNumeroSemDv = 17;

    /// <summary>
    /// Composição do nosso número para convênios de 4 posições (carteira 17,
    /// convênios de 0001 a 9.999): convênio (4) + sequencial (7) + DV.
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
    /// Composição do nosso número para convênios de 6 posições (10.000 a
    /// 999.999): convênio (6) + sequencial (5) + DV.
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
    /// 1.000.000): convênio (7) + sequencial (10), sem DV — "padrão mais
    /// utilizado atualmente" (manual citado).
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

    private static void Exigir(string valor, int tamanho, string nome)
    {
        if (valor.Length != tamanho || !valor.All(char.IsDigit))
        {
            throw new ArgumentException($"'{nome}' deve ter exatamente {tamanho} dígito(s).", nome);
        }
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
/// <para>
/// O Inter atribui o nosso número: a remessa CNAB 400 registra o título e o
/// "Nosso Número" é preenchido pelo próprio Inter no retorno — "Identificação
/// do título no Inter ('Nosso número'), será preenchido pelo Inter" (Inter,
/// <c>Manual de Emissão CNAB400 – Conta Digital PJ</c>, registro de
/// transação, posições 380-390; portal developers.inter.co/docs/cnab/manuais
/// — manual distribuído no portal do desenvolvedor, espelhado em
/// docs/layouts.md). O DV de agência/conta não tem algoritmo publicado na
/// documentação pública.
/// </para>
/// <para>
/// Este tipo expõe o formato aceito na entrada do CNAB 400: 11 dígitos
/// numéricos, atribuídos pelo banco.
/// </para>
/// </summary>
public static class Inter
{
    /// <summary>Tamanho do nosso número no CNAB 400 do Inter (11 posições, atribuído pelo banco).</summary>
    public const int TamanhoNossoNumero = 11;

    /// <summary>
    /// Valida o formato do nosso número atribuído pelo Inter: 11 dígitos.
    /// </summary>
    /// <param name="nossoNumero">Nosso número retornado pelo banco.</param>
    /// <exception cref="ArgumentException">Formato inválido.</exception>
    public static void ValidarNossoNumero(string nossoNumero)
    {
        if (nossoNumero.Length != TamanhoNossoNumero || !nossoNumero.All(char.IsDigit))
        {
            throw new ArgumentException(
                $"Nosso número Inter deve ter {TamanhoNossoNumero} dígitos atribuídos pelo banco.",
                nameof(nossoNumero));
        }
    }
}

/// <summary>
/// Regras do BTG Pactual (208).
/// <para>
/// A documentação pública do BTG (developers.btgpactual.com) distribui os
/// manuais de CNAB no onboarding da cobrança; não há algoritmo de DV de
/// agência/conta ou de nosso número publicado em canal publicamente
/// acessível no momento deste marco. A API de cobrança do BTG retorna
/// nosso número, linha digitável e código de barras gerados pelo banco
/// (API BTG Cobrança — create_boleto retorna nosso_numero, linha_digitável e
/// barcode).
/// </para>
/// <para>
/// Este tipo expõe o formato aceito na entrada: 11 dígitos numéricos
/// (também usado no espelho do manual CNAB 400 do BTG), atribuídos pelo banco.
/// </para>
/// </summary>
public static class Btg
{
    /// <summary>Tamanho do nosso número aceito no fluxo de cobrança do BTG (11 posições).</summary>
    public const int TamanhoNossoNumero = 11;

    /// <summary>
    /// Valida o formato do nosso número atribuído pelo BTG: 11 dígitos.
    /// </summary>
    /// <param name="nossoNumero">Nosso número retornado pelo banco.</param>
    /// <exception cref="ArgumentException">Formato inválido.</exception>
    public static void ValidarNossoNumero(string nossoNumero)
    {
        if (nossoNumero.Length != TamanhoNossoNumero || !nossoNumero.All(char.IsDigit))
        {
            throw new ArgumentException(
                $"Nosso número BTG deve ter {TamanhoNossoNumero} dígitos atribuídos pelo banco.",
                nameof(nossoNumero));
        }
    }
}
