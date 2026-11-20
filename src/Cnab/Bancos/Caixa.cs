namespace Cnab.Bancos;

using Cnab.Digitos;

/// <summary>
/// Regras da Caixa (104) para cobrança SIGCB: DV do nosso número, DV do
/// código do beneficiário, DV do campo livre, DV geral e DVs de conta.
/// <para>Fonte oficial: Caixa, <c>Especificações técnicas para emissão de
/// boletos de cobrança bancária – CAIXA (67.119 v014, SIGCB)</c>:
/// https://caixa.gov.br/Downloads/cobranca-caixa/ESP_COD_BARRAS_SIGCB_COBRANCA_CAIXA.pdf</para>
/// <para>Para os DVs usados no CNAB 400 (DV da conta e DV agência/conta):
/// Caixa, <c>Manual de Leiaute de Arquivo Eletrônico CNAB 400 (67.126
/// v029)</c>, notas NE051 e NE052:
/// https://www.caixa.gov.br/Downloads/cobranca-caixa/Manual_de_Leiaute_de_Arquivo_Eletronico_CNAB_400.pdf</para>
/// </summary>
public static class Caixa
{
    /// <summary>Constante 1 do campo livre: 1ª posição do nosso número (tipo de cobrança: 1 = registrada).</summary>
    public const string TipoCobrancaRegistrada = "1";

    /// <summary>Constante 2 do campo livre: 2ª posição do nosso número (identificador de emissão: 4 = beneficiário).</summary>
    public const string IdentificadorEmissaoBeneficiario = "4";

    /// <summary>
    /// Composição do nosso número SIGCB: 17 posições, sendo as duas iniciais
    /// para carteira e emissão e as 15 restantes de livre utilização do
    /// beneficiário (manual citado, Nota 1), com DV na 17ª posição.
    /// <para>Vetor do manual (Anexo IV): nosso número "14/000000000000019" →
    /// soma 59, resto 4, DV 7.</para>
    /// </summary>
    /// <param name="tipoCobranca">1ª posição: 1 = cobrança registrada, 2 = sem registro.</param>
    /// <param name="identificadorEmissao">2ª posição: 4 = emissão pelo beneficiário, 1 = pelo banco.</param>
    /// <param name="sequencial">15 posições livres do beneficiário.</param>
    /// <returns>Nosso número com 17 posições, com DV.</returns>
    /// <exception cref="ArgumentException">Entradas fora do formato.</exception>
    public static string ComporNossoNumero(string tipoCobranca, string identificadorEmissao, string sequencial)
    {
        if (tipoCobranca.Length != 1 || !tipoCobranca.All(char.IsDigit))
        {
            throw new ArgumentException("Tipo de cobrança SIGCB deve ter 1 dígito.", nameof(tipoCobranca));
        }

        if (identificadorEmissao.Length != 1 || !identificadorEmissao.All(char.IsDigit))
        {
            throw new ArgumentException("Identificador de emissão SIGCB deve ter 1 dígito.", nameof(identificadorEmissao));
        }

        if (sequencial.Length != 15 || !sequencial.All(char.IsDigit))
        {
            throw new ArgumentException("Sequencial SIGCB deve ter 15 dígitos.", nameof(sequencial));
        }

        var nossoNumero = tipoCobranca + identificadorEmissao + sequencial;
        return nossoNumero + CalcularDvNossoNumero(nossoNumero).ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// DV do nosso número SIGCB: módulo 11 com pesos 2 a 9 sobre as 17
    /// posições; "se o RESULTADO for maior que 9 o DV será 0" (manual citado,
    /// Anexo IV).
    /// <para>Vetor do manual: "1400000000000019" → DV 7.</para>
    /// </summary>
    /// <param name="nossoNumeroSemDv">Nosso número com 17 dígitos.</param>
    /// <returns>DV (um dígito).</returns>
    /// <exception cref="ArgumentException">Entrada fora do formato.</exception>
    public static int CalcularDvNossoNumero(string nossoNumeroSemDv)
    {
        if (nossoNumeroSemDv.Length != 17 || !nossoNumeroSemDv.All(char.IsDigit))
        {
            throw new ArgumentException("Nosso número SIGCB deve ter 17 dígitos.", nameof(nossoNumeroSemDv));
        }

        var resto = Modulo.RestoModulo11([.. nossoNumeroSemDv], 9);
        var dv = 11 - resto;
        return dv > 9 ? 0 : dv;
    }

    /// <summary>
    /// DV do código do beneficiário: módulo 11 com pesos 2 a 9 sobre as 6
    /// posições do código; "se o RESULTADO for maior que 9 o DV será 0" (manual
    /// citado, Anexo VI). Aplica-se apenas a códigos entre 000001 e 999999;
    /// códigos a partir de 1100000 usam 7 posições sem DV (Nota 3).
    /// <para>Vetor do manual (Anexo VI): código do beneficiário "005507" →
    /// soma 59, resto 4, DV 7.</para>
    /// </summary>
    /// <param name="codigoBeneficiario">Código com 6 dígitos.</param>
    /// <returns>DV (um dígito).</returns>
    /// <exception cref="ArgumentException">Entrada fora do formato.</exception>
    public static int CalcularDvCodigoBeneficiario(string codigoBeneficiario)
    {
        if (codigoBeneficiario.Length != 6 || !codigoBeneficiario.All(char.IsDigit))
        {
            throw new ArgumentException("Código do beneficiário deve ter 6 dígitos.", nameof(codigoBeneficiario));
        }

        var resto = Modulo.RestoModulo11([.. codigoBeneficiario], 9);
        var dv = 11 - resto;
        return dv > 9 ? 0 : dv;
    }

    /// <summary>
    /// Campo livre do código de barras (posições 20-44), SIGCB (manual citado,
    /// Anexo III): código do beneficiário + DV (7) + NN seq 1 (3) + constante 1
    /// (NN posição 1) + NN seq 2 (3) + constante 2 (NN posição 2) + NN seq 3
    /// (9) + DV do campo livre (1).
    /// <para>Vetor do manual: beneficiário "005507" DV 7, NN
    /// "14/222333777777777", DV do campo livre 1 → campo livre
    /// "0055077" + "222" + "1" + "333" + "4" + "777777777" + "1".</para>
    /// </summary>
    /// <param name="codigoBeneficiario">Código com 6 dígitos (entre 000001 e 999999).</param>
    /// <param name="nossoNumeroSemDv">Nosso número com 17 dígitos, sem DV.</param>
    /// <returns>Campo livre com 25 posições, com DV.</returns>
    /// <exception cref="ArgumentException">Entradas fora do formato.</exception>
    public static string ComporCampoLivre(string codigoBeneficiario, string nossoNumeroSemDv)
    {
        if (codigoBeneficiario.Length != 6 || !codigoBeneficiario.All(char.IsDigit))
        {
            throw new ArgumentException(
                "Use a variante de campo livre do SIGCB apenas com código do beneficiário de 6 dígitos (até 999999).",
                nameof(codigoBeneficiario));
        }

        if (nossoNumeroSemDv.Length != 17 || !nossoNumeroSemDv.All(char.IsDigit))
        {
            throw new ArgumentException("Nosso número SIGCB deve ter 17 dígitos.", nameof(nossoNumeroSemDv));
        }

        var dvCodigo = CalcularDvCodigoBeneficiario(codigoBeneficiario);
        // Ordem do campo livre (manual citado, Anexo III): código+DV (7),
        // NN posições 3-5 (seq 1), NN posição 1 (constante 1), NN posições 6-8
        // (seq 2), NN posição 2 (constante 2), NN posições 9-17 (seq 3).
        var semDv = codigoBeneficiario
            + dvCodigo.ToString(System.Globalization.CultureInfo.InvariantCulture)
            + nossoNumeroSemDv.Substring(2, 3)
            + nossoNumeroSemDv[0]
            + nossoNumeroSemDv.Substring(5, 3)
            + nossoNumeroSemDv[1]
            + nossoNumeroSemDv.Substring(8, 9);
        var resto = Modulo.RestoModulo11([.. semDv], 9);
        var dvCampoLivre = 11 - resto;
        return semDv + (dvCampoLivre > 9 ? 0 : dvCampoLivre).ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// DV da conta corrente (CNAB 400, nota NE051 do manual citado): módulo 11
    /// com pesos 2 a 9 sobre a conta com 12 posições, sem operação e sem DV;
    /// "se o resultado da subtração for maior que 9 o dígito será 0".
    /// <para>Vetor do manual (NE051): conta "000000109990" → soma 115, resto 5,
    /// DV 6.</para>
    /// </summary>
    /// <param name="conta">Conta com 12 dígitos, sem operação e sem DV.</param>
    /// <returns>DV da conta.</returns>
    /// <exception cref="ArgumentException">Entrada fora do formato.</exception>
    public static int CalcularDvConta(string conta)
    {
        if (conta.Length != 12 || !conta.All(char.IsDigit))
        {
            throw new ArgumentException("Conta Caixa deve ter 12 dígitos (sem operação e sem DV).", nameof(conta));
        }

        var resto = Modulo.RestoModulo11([.. conta], 9);
        var dv = 11 - resto;
        return dv > 9 ? 0 : dv;
    }

    /// <summary>
    /// DV agência/conta (CNAB 400, nota NE052 do manual citado): módulo 11 com
    /// pesos 2 a 9 sobre agência (4) + conta (12); resultado maior que 9 → 0.
    /// <para>Vetor do manual (NE052): agência 0161 + conta 000000109990 →
    /// soma 171, resto 6, DV 5.</para>
    /// </summary>
    /// <param name="agencia">Agência com 4 dígitos.</param>
    /// <param name="conta">Conta com 12 dígitos, sem operação e sem DV.</param>
    /// <returns>DV agência/conta.</returns>
    /// <exception cref="ArgumentException">Entradas fora do formato.</exception>
    public static int CalcularDvAgenciaConta(string agencia, string conta)
    {
        if (agencia.Length != 4 || !agencia.All(char.IsDigit))
        {
            throw new ArgumentException("Agência Caixa deve ter 4 dígitos.", nameof(agencia));
        }

        if (conta.Length != 12 || !conta.All(char.IsDigit))
        {
            throw new ArgumentException("Conta Caixa deve ter 12 dígitos.", nameof(conta));
        }

        var resto = Modulo.RestoModulo11([.. (agencia + conta)], 9);
        var dv = 11 - resto;
        return dv > 9 ? 0 : dv;
    }
}
