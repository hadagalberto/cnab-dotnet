namespace Cnab.Bancos;

using Cnab.Digitos;

/// <summary>
/// Regras do Bradesco (237) para nosso número, DV e campo livre do código de
/// barras.
/// <para>Fonte oficial: Bradesco, <c>Layout da Cobrança – Versão Português
/// (4008-524-0121)</c>, seções "Identificações do Título no Banco (Nosso
/// Número)" (páginas 17-19) e "Código de Barras do Boleto para a Cobrança
/// Bradesco" (páginas 24-30).
/// https://assets.bradesco/content/dam/portal-bradesco/assets/pessoajuridica/pdf/4008-524-0121-layout-cobranca-versao-portugues.pdf</para>
/// <para>O DV de agência/conta ("DC") não tem algoritmo publicado no manual de
/// cobrança: o valor é fornecido pelo banco na contratação, e o campo livre do
/// código de barras usa agência e conta <b>sem</b> o DV.</para>
/// </summary>
public static class Bradesco
{
    /// <summary>
    /// Composição do nosso número: 11 algarismos + DV de autoconferência
    /// (posições 71-81 e 82 do registro de transação CNAB 400). O DV exige a
    /// carteira: "para o cálculo do dígito, será necessário acrescentar o
    /// número da carteira à esquerda antes do Nosso Número" (manual citado).
    /// </summary>
    /// <param name="carteira">Carteira com 2 dígitos (por exemplo 19).</param>
    /// <param name="sequencial">Sequencial de 11 dígitos, de 00000000001 em diante.</param>
    /// <returns>Nosso número com DV (12 caracteres).</returns>
    /// <exception cref="ArgumentException">Entradas fora do formato.</exception>
    public static string ComporNossoNumero(string carteira, string sequencial)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sequencial);
        if (sequencial.Length != 11 || !sequencial.All(char.IsDigit))
        {
            throw new ArgumentException("Nosso número Bradesco deve ter 11 dígitos.", nameof(sequencial));
        }

        return sequencial + CalcularDvNossoNumero(carteira, sequencial);
    }

    /// <summary>
    /// DV do nosso número: "acrescentar o número da carteira à esquerda antes
    /// do Nosso Número, e aplicar o módulo 11, com base 7" (manual citado).
    /// Resto 1 → dígito "P"; resto 0 → dígito 0; caso contrário 11 - resto.
    /// <para>Vetores do manual: carteira 19, NN 00000000002 → soma 69, resto 3,
    /// DV 8; carteira 19, NN 00000000001 → soma 67, resto 1, DV "P";
    /// carteira 19, NN 00000000006 → soma 77, resto 0, DV 0.</para>
    /// </summary>
    /// <param name="carteira">Carteira com 2 dígitos (por exemplo 19).</param>
    /// <param name="nossoNumero">Nosso número com 11 dígitos, sem DV.</param>
    /// <returns>DV: um dígito ou a letra "P".</returns>
    /// <exception cref="ArgumentException">Entradas fora do formato.</exception>
    public static string CalcularDvNossoNumero(string carteira, string nossoNumero)
    {
        if (carteira.Length != 2 || !carteira.All(char.IsDigit))
        {
            throw new ArgumentException("Carteira Bradesco deve ter 2 dígitos.", nameof(carteira));
        }

        if (nossoNumero.Length != 11 || !nossoNumero.All(char.IsDigit))
        {
            throw new ArgumentException("Nosso número Bradesco deve ter 11 dígitos.", nameof(nossoNumero));
        }

        var resto = Modulo.RestoModulo11([.. (carteira + nossoNumero)], 7);
        return resto switch
        {
            0 => "0",
            1 => "P",
            _ => (11 - resto).ToString(System.Globalization.CultureInfo.InvariantCulture),
        };
    }

    /// <summary>
    /// Campo livre do código de barras (posições 20-44), padrão Bradesco
    /// (manual citado, "Montagem dos Dados do Código de Barras"):
    /// agência beneficiária sem DV (4) + carteira (2) + nosso número sem DV
    /// (11) + conta do beneficiário sem DV (7) + zero fixo (1).
    /// <para>Vetor do manual: linha digitável
    /// "23790.03102 40031.772003 28009.527905 7 10010000000000" para agência
    /// 31, carteira 04, nosso número 00317720028, conta 95279, fator 1001 e
    /// valor 0,00.</para>
    /// </summary>
    /// <param name="agencia">Agência sem DV, até 4 dígitos.</param>
    /// <param name="carteira">Carteira, 2 dígitos.</param>
    /// <param name="nossoNumeroSemDv">Nosso número com 11 dígitos, sem DV.</param>
    /// <param name="conta">Conta sem DV, até 7 dígitos.</param>
    /// <returns>Campo livre com 25 posições.</returns>
    /// <exception cref="ArgumentException">Entradas fora do formato.</exception>
    public static string ComporCampoLivre(string agencia, string carteira, string nossoNumeroSemDv, string conta)
    {
        if (string.IsNullOrWhiteSpace(agencia) || !agencia.All(char.IsDigit) || agencia.Length > 4)
        {
            throw new ArgumentException("Agência Bradesco deve ter até 4 dígitos.", nameof(agencia));
        }

        if (carteira.Length != 2 || !carteira.All(char.IsDigit))
        {
            throw new ArgumentException("Carteira Bradesco deve ter 2 dígitos.", nameof(carteira));
        }

        if (nossoNumeroSemDv.Length != 11 || !nossoNumeroSemDv.All(char.IsDigit))
        {
            throw new ArgumentException("Nosso número Bradesco deve ter 11 dígitos.", nameof(nossoNumeroSemDv));
        }

        if (string.IsNullOrWhiteSpace(conta) || !conta.All(char.IsDigit) || conta.Length > 7)
        {
            throw new ArgumentException("Conta Bradesco deve ter até 7 dígitos.", nameof(conta));
        }

        return PadZero(agencia, 4) + carteira + nossoNumeroSemDv + PadZero(conta, 7) + "0";
    }

    internal static string PadZero(string valor, int tamanho) => valor.PadLeft(tamanho, '0');
}
