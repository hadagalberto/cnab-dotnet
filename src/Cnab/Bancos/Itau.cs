namespace Cnab.Bancos;

using System.Globalization;
using Cnab.Digitos;

/// <summary>
/// Regras do Itaú (341) para DAC do nosso número, DAC agência/conta e campo
/// livre do código de barras.
/// <para>Fonte oficial: Itaú, <c>COBRANÇA CNAB 400 BYTES – Janeiro 2017</c>:
/// Anexo 2 (cálculo do DAC do código de barras), Anexo 3 (DAC da
/// representação numérica), Anexo 4 (DAC do campo Nosso Número em boletos
/// emitidos pelo próprio cliente) e seção do código de barras.
/// https://download.itau.com.br/bankline/layout_cobranca_400bytes_cnab_itau.pdf</para>
/// </summary>
public static class Itau
{
    /// <summary>
    /// Carteiras cujo DAC do nosso número usa apenas carteira + nosso número
    /// (manual citado, Anexo 4): 126, 131, 146, 150 e 168. As demais usam
    /// agência + conta + carteira + nosso número.
    /// </summary>
    public static readonly IReadOnlySet<string> CarteirasDacSoNossoNumero =
        new HashSet<string>(["126", "131", "146", "150", "168"], StringComparer.Ordinal);

    /// <summary>
    /// DAC do nosso número, módulo 10: "Para a grande maioria das carteiras,
    /// são considerados os dados AGÊNCIA / CONTA (sem DAC) / CARTEIRA / NOSSO
    /// NÚMERO" (manual citado, Anexo 4); para as carteiras 126/131/146/150/168
    /// usa apenas CARTEIRA / NOSSO NÚMERO.
    /// <para>Vetor do manual: agência 0057, conta 12345, carteira 110, nosso
    /// número 12345678 → soma 72, resto 2, DAC 8 ("110/12345678-8").</para>
    /// </summary>
    /// <param name="agencia">Agência com 4 dígitos.</param>
    /// <param name="conta">Conta com 5 dígitos, sem DAC.</param>
    /// <param name="carteira">Carteira com 3 dígitos.</param>
    /// <param name="nossoNumero">Nosso número com 8 dígitos.</param>
    /// <returns>DAC (um dígito).</returns>
    /// <exception cref="ArgumentException">Entradas fora do formato.</exception>
    public static int CalcularDacNossoNumero(string agencia, string conta, string carteira, string nossoNumero)
    {
        ExigirDigitos(agencia, 4, nameof(agencia));
        ExigirDigitos(conta, 5, nameof(conta));
        ExigirDigitos(carteira, 3, nameof(carteira));
        ExigirDigitos(nossoNumero, 8, nameof(nossoNumero));

        var baseCalculo = CarteirasDacSoNossoNumero.Contains(carteira)
            ? carteira + nossoNumero
            : agencia + conta + carteira + nossoNumero;
        return Modulo.Modulo10([.. baseCalculo]);
    }

    /// <summary>
    /// DAC da agência/conta do beneficiário: módulo 10 sobre agência (4) +
    /// conta (5) (manual citado: "DAC dígito de auto-conferência ag/conta
    /// empresa").
    /// <para>Vetor do manual: agência 0057 + conta 12345 → DAC 7
    /// ("0057/12345-7" no exemplo do Anexo 4).</para>
    /// </summary>
    /// <param name="agencia">Agência com 4 dígitos.</param>
    /// <param name="conta">Conta com 5 dígitos, sem DAC.</param>
    /// <returns>DAC (um dígito).</returns>
    /// <exception cref="ArgumentException">Entradas fora do formato.</exception>
    public static int CalcularDacAgenciaConta(string agencia, string conta)
    {
        ExigirDigitos(agencia, 4, nameof(agencia));
        ExigirDigitos(conta, 5, nameof(conta));
        return Modulo.Modulo10([.. (agencia + conta)]);
    }

    /// <summary>
    /// Campo livre do código de barras (posições 20-44), padrão Itaú (manual
    /// citado, seção do código de barras): carteira (3) + nosso número (8) +
    /// DAC do nosso número (1) + agência (4) + conta (5) + DAC ag/conta (1) +
    /// "000".
    /// <para>Vetor do manual: carteira 110, NN 12345678-8, agência 0057, conta
    /// 12345-7 → campo livre "110123456780057123457000"; código de barras
    /// "34196166700000123451101234567880057123457000".</para>
    /// </summary>
    /// <param name="carteira">Carteira com 3 dígitos.</param>
    /// <param name="nossoNumero">Nosso número com 8 dígitos, sem DAC.</param>
    /// <param name="agencia">Agência com 4 dígitos.</param>
    /// <param name="conta">Conta com 5 dígitos, sem DAC.</param>
    /// <returns>Campo livre com 25 posições.</returns>
    /// <exception cref="ArgumentException">Entradas fora do formato.</exception>
    public static string ComporCampoLivre(string carteira, string nossoNumero, string agencia, string conta)
    {
        ExigirDigitos(carteira, 3, nameof(carteira));
        ExigirDigitos(nossoNumero, 8, nameof(nossoNumero));
        ExigirDigitos(agencia, 4, nameof(agencia));
        ExigirDigitos(conta, 5, nameof(conta));

        var dacNossoNumero = CalcularDacNossoNumero(agencia, conta, carteira, nossoNumero);
        var dacAgenciaConta = CalcularDacAgenciaConta(agencia, conta);
        return carteira
            + nossoNumero
            + dacNossoNumero.ToString(CultureInfo.InvariantCulture)
            + agencia
            + conta
            + dacAgenciaConta.ToString(CultureInfo.InvariantCulture)
            + "000";
    }

    private static void ExigirDigitos(string valor, int tamanho, string nome)
    {
        if (valor.Length != tamanho || !valor.All(char.IsDigit))
        {
            throw new ArgumentException($"'{nome}' deve ter exatamente {tamanho} dígito(s).", nome);
        }
    }
}
