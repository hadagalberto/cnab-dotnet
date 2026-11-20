namespace Cnab.Bancos;

using Cnab.Digitos;

/// <summary>
/// Regras do Santander (033) para DV do nosso número e campo livre do código
/// de barras.
/// <para>Fonte oficial: Santander, <c>Código de Barras – Cobrança v34
/// (Setembro 2021)</c>:
/// https://cms.santander.com.br/sites/WPS/documentos/arq-cobranca-barras-port-jan22/22-01-13_181337_cobrança+-+código+de+barras+português.pdf</para>
/// <para>O Santander não publica DV de agência/conta para cobrança: usa o
/// "código do beneficiário padrão Santander" (PSK), fornecido pelo banco.</para>
/// </summary>
public static class Santander
{
    /// <summary>Modalidade de carteira do Santander no código de barras (posições 42-44).</summary>
    public enum ModalidadeCarteira
    {
        /// <summary>101 - Cobrança Rápida COM Registro.</summary>
        CobrancaRapidaComRegistro = 101,

        /// <summary>104 - Cobrança Eletrônica COM Registro.</summary>
        CobrancaEletronicaComRegistro = 104,
    }

    /// <summary>
    /// Composição do nosso número: faixa sequencial de 12 dígitos + DV
    /// (000000000001 a 999999999999).
    /// </summary>
    /// <param name="sequencial">Sequencial de 12 dígitos, sem DV.</param>
    /// <returns>Nosso número com DV (13 dígitos).</returns>
    /// <exception cref="ArgumentException">Sequencial fora do formato.</exception>
    public static string ComporNossoNumero(string sequencial)
    {
        if (sequencial.Length != 12 || !sequencial.All(char.IsDigit))
        {
            throw new ArgumentException("Nosso número Santander deve ter 12 dígitos (sem DV).", nameof(sequencial));
        }

        return sequencial + CalcularDvNossoNumero(sequencial).ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// DV do nosso número: módulo 11 com pesos 2 a 9 da direita para a
    /// esquerda. "Se o resto desta divisão for igual a 10 o dígito será 1; se
    /// for igual a 0 ou 1 o dígito será 0. Qualquer resto diferente de 0, 1 ou
    /// 10, subtrair o resto de 11" (manual citado, seção "Nosso Número").
    /// <para>Vetor do manual: nosso número 566612457800 → soma 229, resto 9,
    /// DV 2 ("566612457800-2", usado no exemplo do código de barras).</para>
    /// </summary>
    /// <param name="nossoNumeroSemDv">Nosso número com 12 dígitos.</param>
    /// <returns>DV (um dígito).</returns>
    /// <exception cref="ArgumentException">Entrada fora do formato.</exception>
    public static int CalcularDvNossoNumero(string nossoNumeroSemDv)
    {
        if (nossoNumeroSemDv.Length != 12 || !nossoNumeroSemDv.All(char.IsDigit))
        {
            throw new ArgumentException("Nosso número Santander deve ter 12 dígitos.", nameof(nossoNumeroSemDv));
        }

        var resto = Modulo.RestoModulo11([.. nossoNumeroSemDv], 9);
        return resto switch
        {
            0 or 1 => 0,
            10 => 1,
            _ => 11 - resto,
        };
    }

    /// <summary>
    /// Campo livre do código de barras (posições 20-44) para cobrança
    /// registrada com nosso número de 13 posições (manual citado, "Layout do
    /// código de barras"): fixo "9" + código do beneficiário padrão Santander
    /// (PSK, 7) + nosso número com DV (13) + fixo "0" + tipo de modalidade da
    /// carteira (3).
    /// <para>Vetor do manual: PSK 0282033, NN 566612457800-2, carteira 101,
    /// fator 2046, valor 273,71 → DV geral 8 e IPTE "033990282.7 0335666124.3
    /// 5780020101.4 8 20460000027371".</para>
    /// </summary>
    /// <param name="psk">Código do beneficiário padrão Santander, 7 dígitos.</param>
    /// <param name="nossoNumeroComDv">Nosso número com 13 dígitos, incluindo o DV.</param>
    /// <param name="modalidade">Tipo de modalidade da carteira (101 ou 104).</param>
    /// <returns>Campo livre com 25 posições.</returns>
    /// <exception cref="ArgumentException">Entradas fora do formato.</exception>
    public static string ComporCampoLivre(string psk, string nossoNumeroComDv, ModalidadeCarteira modalidade)
    {
        if (psk.Length != 7 || !psk.All(char.IsDigit))
        {
            throw new ArgumentException("Código do beneficiário Santander (PSK) deve ter 7 dígitos.", nameof(psk));
        }

        if (nossoNumeroComDv.Length != 13 || !nossoNumeroComDv.All(char.IsDigit))
        {
            throw new ArgumentException("Nosso número Santander com DV deve ter 13 dígitos.", nameof(nossoNumeroComDv));
        }

        var carteira = ((int)modalidade).ToString(System.Globalization.CultureInfo.InvariantCulture);
        return "9" + psk + nossoNumeroComDv + "0" + carteira;
    }

    /// <summary>
    /// Campo livre para boletos com numeração de 8 posições usada em
    /// convênios legados do CNAB 400 (manual citado: "Banco 353 - Utilizar
    /// somente 08 posições do Nosso Numero (07 posições + DV), zerando os 05
    /// primeiros dígitos"): fixo "9" + PSK (7) + "00000" + NN 8 posições com
    /// DV + fixo "0" + modalidade (3).
    /// </summary>
    /// <param name="psk">Código do beneficiário padrão Santander, 7 dígitos.</param>
    /// <param name="nossoNumeroComDv">Nosso número com 8 dígitos (7 + DV).</param>
    /// <param name="modalidade">Tipo de modalidade da carteira (101 ou 104).</param>
    /// <returns>Campo livre com 25 posições.</returns>
    /// <exception cref="ArgumentException">Entradas fora do formato.</exception>
    public static string ComporCampoLivreNossoNumeroCurto(
        string psk,
        string nossoNumeroComDv,
        ModalidadeCarteira modalidade)
    {
        if (psk.Length != 7 || !psk.All(char.IsDigit))
        {
            throw new ArgumentException("Código do beneficiário Santander (PSK) deve ter 7 dígitos.", nameof(psk));
        }

        if (nossoNumeroComDv.Length != 8 || !nossoNumeroComDv.All(char.IsDigit))
        {
            throw new ArgumentException("Nosso número curto Santander deve ter 8 dígitos (7 + DV).", nameof(nossoNumeroComDv));
        }

        var carteira = ((int)modalidade).ToString(System.Globalization.CultureInfo.InvariantCulture);
        return "9" + psk + "00000" + nossoNumeroComDv + "0" + carteira;
    }
}
