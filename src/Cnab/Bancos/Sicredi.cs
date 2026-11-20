namespace Cnab.Bancos;

using Cnab.Digitos;

/// <summary>
/// Regras do Sicredi (748) para nosso número, DV do campo livre e campo livre
/// do código de barras.
/// <para>Fontes oficiais: Sicredi, portal do desenvolvedor — "CNAB 240 -
/// Cobrança Sicredi" (itens 4.4/4.5 "Formato do Nosso Número" e "Cálculo para
/// encontrar o DV do Nosso Número", e itens 10.2/10.3/10.4/10.5/10.7/10.9
/// "Código de Barras"):
/// https://developers.sicredi.com.br/public/docs/getting-started-cnabs-240-billing.md
/// e "Geração do Nosso Número":
/// https://developers.sicredi.com.br/public/docs/geração-do-nosso-número.md</para>
/// <para>DV de agência/conta: o header do CNAB 240 do Sicredi traz "Conta DV"
/// como dado do banco; não há algoritmo publicado para o DV da conta na
/// documentação pública.</para>
/// </summary>
public static class Sicredi
{
    /// <summary>
    /// Composição do nosso número no formato AA/BNNNNN-D: ano (2) + byte de
    /// geração (0 a 9; o byte 1 só pode ser informado pela cooperativa) +
    /// sequencial livre de 00000 a 99999 (5) + DV.
    /// </summary>
    /// <param name="cooperativa">Cooperativa/agência com 4 dígitos.</param>
    /// <param name="posto">Posto com 2 dígitos (use "00" quando alfanumérico).</param>
    /// <param name="codigoBeneficiario">Código do beneficiário com 5 dígitos.</param>
    /// <param name="ano">Ano com 2 dígitos.</param>
    /// <param name="byteGeracao">Byte de geração, 0 a 9.</param>
    /// <param name="sequencial">Sequencial livre com 5 dígitos.</param>
    /// <returns>Nosso número com 9 posições, com DV.</returns>
    /// <exception cref="ArgumentException">Entradas fora do formato.</exception>
    public static string ComporNossoNumero(
        string cooperativa,
        string posto,
        string codigoBeneficiario,
        string ano,
        int byteGeracao,
        string sequencial)
    {
        Exigir(cooperativa, 4, nameof(cooperativa));
        Exigir(posto, 2, nameof(posto));
        Exigir(codigoBeneficiario, 5, nameof(codigoBeneficiario));
        Exigir(ano, 2, nameof(ano));
        Exigir(sequencial, 5, nameof(sequencial));
        if (byteGeracao is < 0 or > 9)
        {
            throw new ArgumentOutOfRangeException(nameof(byteGeracao), "Byte de geração deve estar entre 0 e 9.");
        }

        var semDv = ano + byteGeracao.ToString(System.Globalization.CultureInfo.InvariantCulture) + sequencial;
        var dv = CalcularDvNossoNumero(cooperativa, posto, codigoBeneficiario, semDv);
        return semDv + dv.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// DV do nosso número, módulo 11 com pesos 2 a 9: base = cooperativa (4) +
    /// posto (2) + beneficiário (5) + nosso número sem DV (8: ano + byte +
    /// sequencial). "O dígito verificador será 11 - resto; se o resultado da
    /// subtração for 10 ou 11, o dígito verificador será 0" (documentação
    /// citada, item 4.5). Posto alfanumérico entra como "00".
    /// <para>Vetor da documentação citada: cooperativa 0165, posto 02,
    /// beneficiário 00623, ano 07, byte 2, sequencial 00003 → DV 1 (nosso
    /// número "072000031", usado no exemplo do campo livre).</para>
    /// </summary>
    /// <param name="cooperativa">Cooperativa com 4 dígitos.</param>
    /// <param name="posto">Posto com 2 dígitos.</param>
    /// <param name="codigoBeneficiario">Código do beneficiário com 5 dígitos.</param>
    /// <param name="nossoNumeroSemDv">Nosso número com 8 dígitos (ano + byte + sequencial).</param>
    /// <returns>DV (um dígito).</returns>
    /// <exception cref="ArgumentException">Entradas fora do formato.</exception>
    public static int CalcularDvNossoNumero(
        string cooperativa,
        string posto,
        string codigoBeneficiario,
        string nossoNumeroSemDv)
    {
        Exigir(cooperativa, 4, nameof(cooperativa));
        Exigir(posto, 2, nameof(posto));
        Exigir(codigoBeneficiario, 5, nameof(codigoBeneficiario));
        Exigir(nossoNumeroSemDv, 8, nameof(nossoNumeroSemDv));

        var baseCalculo = cooperativa + posto + codigoBeneficiario + nossoNumeroSemDv;
        var resto = Modulo.RestoModulo11([.. baseCalculo], 9);
        var dv = 11 - resto;
        return dv is 10 or 11 ? 0 : dv;
    }

    /// <summary>
    /// Campo livre do código de barras (posições 20-44), padrão Sicredi
    /// (documentação citada, item 10.3): tipo de cobrança "1" (com registro) +
    /// tipo de carteira "1" (simples) + nosso número com DV (9) +
    /// cooperativa/agência (4) + posto (2) + código do beneficiário (5) +
    /// "1" quando há valor no título ("0" quando o valor for zerado) + filler
    /// "0" + DV do campo livre (1).
    /// <para>Vetor da documentação citada (item 10.4): campo livre
    /// "1" + "1" + "072000031" + "0165" + "02" + "00623" + "1" + "0" →
    /// somatório 223, resto 3, DV 8.</para>
    /// <para>Vetor real da API de Cobrança Sicredi (v3.9.1, resposta de
    /// exemplo): campo livre "112510061420512031533510" + DV 3 no código de
    /// barras "74891886400000099901125100614205120315335103".</para>
    /// </summary>
    /// <param name="nossoNumeroComDv">Nosso número com 9 posições, com DV.</param>
    /// <param name="cooperativa">Cooperativa/agência com 4 dígitos.</param>
    /// <param name="posto">Posto com 2 dígitos.</param>
    /// <param name="codigoBeneficiario">Código do beneficiário com 5 dígitos.</param>
    /// <param name="temValor"><see langword="true"/> quando o título tem valor nominal maior que zero.</param>
    /// <returns>Campo livre com 25 posições, com DV.</returns>
    /// <exception cref="ArgumentException">Entradas fora do formato.</exception>
    public static string ComporCampoLivre(
        string nossoNumeroComDv,
        string cooperativa,
        string posto,
        string codigoBeneficiario,
        bool temValor)
    {
        Exigir(nossoNumeroComDv, 9, nameof(nossoNumeroComDv));
        Exigir(cooperativa, 4, nameof(cooperativa));
        Exigir(posto, 2, nameof(posto));
        Exigir(codigoBeneficiario, 5, nameof(codigoBeneficiario));

        var semDv = "1" + "1" + nossoNumeroComDv + cooperativa + posto + codigoBeneficiario
            + (temValor ? "1" : "0") + "0";
        var resto = Modulo.RestoModulo11([.. semDv], 9);
        var dv = 11 - resto;
        return semDv + (dv > 9 ? 1 : dv).ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    private static void Exigir(string valor, int tamanho, string nome)
    {
        if (valor.Length != tamanho || !valor.All(char.IsDigit))
        {
            throw new ArgumentException($"'{nome}' deve ter exatamente {tamanho} dígito(s).", nome);
        }
    }
}
