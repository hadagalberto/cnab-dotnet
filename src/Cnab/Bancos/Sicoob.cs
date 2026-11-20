namespace Cnab.Bancos;

using System.Globalization;

/// <summary>
/// Regras do Sicoob (756) para DV do nosso número e campo livre do código de
/// barras.
/// <para>Fonte oficial: Sicoob, <c>Layouts para troca de informações –
/// Cobrança</c> (planilha "02.Especificações do Boleto", itens 3.13 e 3.29):
/// https://www.sicoob.com.br (o mesmo documento é distribuído pelo banco em
/// formato .xlsx; cópia de trabalho arquivada em docs/layouts.md).</para>
/// <para>Vetores do documento citado: cooperativa 0001 + código do cliente
/// 0000000019 + nosso número 0000021 → somatório 36, resto 3, DV 8
/// ("21-8"); fórmula do campo livre: carteira (1) + agência/cooperativa (4) +
/// modalidade (2) + código do cliente (7) + nosso número com DV (8) + parcela
/// (3, "001" se única).</para>
/// </summary>
public static class Sicoob
{
    /// <summary>
    /// Composição do nosso número: 7 dígitos de sequencial (iniciados em 1 na
    /// numeração — o valor, não o preenchimento com zeros à esquerda) + DV
    /// calculado sobre cooperativa + código do cliente + nosso número.
    /// </summary>
    /// <param name="cooperativa">Número da cooperativa com 4 dígitos.</param>
    /// <param name="codigoCliente">Código do cliente com 10 dígitos.</param>
    /// <param name="sequencial">Sequencial com 7 dígitos, valor iniciado em 1 (zeros à esquerda permitidos).</param>
    /// <returns>Nosso número com 8 posições (7 + DV).</returns>
    /// <exception cref="ArgumentException">Entradas fora do formato ou sequencial nulo/zero.</exception>
    public static string ComporNossoNumero(string cooperativa, string codigoCliente, string sequencial)
    {
        Exigir(cooperativa, 4, nameof(cooperativa));
        Exigir(codigoCliente, 10, nameof(codigoCliente));
        Exigir(sequencial, 7, nameof(sequencial));
        if (long.Parse(sequencial, System.Globalization.CultureInfo.InvariantCulture) < 1)
        {
            throw new ArgumentException("Nosso número Sicoob deve ser iniciado em 1 (valor numérico).", nameof(sequencial));
        }

        var dv = CalcularDvNossoNumero(cooperativa, codigoCliente, sequencial);
        return sequencial + dv.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Valida o formato do nosso número gravado no segmento P: 7 dígitos de
    /// sequencial + 1 DV (8 posições). Quando cooperativa e código do cliente
    /// são informados, o DV é conferido com <see cref="CalcularDvNossoNumero"/>.
    /// </summary>
    /// <param name="nossoNumero">Nosso número com DV (8 dígitos).</param>
    /// <param name="cooperativa">Cooperativa com 4 dígitos, quando disponível.</param>
    /// <param name="codigoCliente">Código do cliente com 10 dígitos, quando disponível.</param>
    /// <exception cref="ArgumentException">Formato ou DV inválidos.</exception>
    public static void ValidarNossoNumero(
        string nossoNumero,
        string? cooperativa = null,
        string? codigoCliente = null)
    {
        Exigir(nossoNumero, 8, nameof(nossoNumero));
        if (cooperativa is not null && codigoCliente is not null)
        {
            var esperado = CalcularDvNossoNumero(cooperativa, codigoCliente, nossoNumero[..7]);
            if (nossoNumero[7] - '0' != esperado)
            {
                throw new ArgumentException(
                    $"DV do nosso número Sicoob inválido: esperado {esperado}.", nameof(nossoNumero));
            }
        }
    }

    /// <summary>
    /// DV do nosso número, módulo 11 com constante 3197: concatenar cooperativa
    /// (4) + código do cliente (10) + nosso número (7) completando com zeros à
    /// esquerda, alinhar a constante "3197" repetida da direita para a
    /// esquerda, multiplicar cada componente pela constante e somar; DV =
    /// 11 - resto, com resto 0 ou 1 → DV 0 (documento citado, item 3.13).
    /// <para>Vetor do documento citado: cooperativa "0001", cliente
    /// "0000000019", nosso número "0000021" → DV 8 ("21-8").</para>
    /// </summary>
    /// <param name="cooperativa">Número da cooperativa com 4 dígitos.</param>
    /// <param name="codigoCliente">Código do cliente com 10 dígitos.</param>
    /// <param name="nossoNumeroSemDv">Nosso número com 7 dígitos.</param>
    /// <returns>DV (um dígito).</returns>
    /// <exception cref="ArgumentException">Entradas fora do formato.</exception>
    public static int CalcularDvNossoNumero(string cooperativa, string codigoCliente, string nossoNumeroSemDv)
    {
        Exigir(cooperativa, 4, nameof(cooperativa));
        Exigir(codigoCliente, 10, nameof(codigoCliente));
        Exigir(nossoNumeroSemDv, 7, nameof(nossoNumeroSemDv));

        var baseCalculo = cooperativa + codigoCliente + nossoNumeroSemDv;
        var soma = 0;
        // Constante "3197" repetida da ESQUERDA para a direita, alinhada ao
        // número (documento citado: "Alinhar a constante com a seqüência
        // repetindo de trás para frente", exemplo
        // "000100000000190000021" / "319731973197319731973").
        int[] constante = [3, 1, 9, 7];
        for (var i = 0; i < baseCalculo.Length; i++)
        {
            soma += (baseCalculo[i] - '0') * constante[i % constante.Length];
        }

        var resto = soma % 11;
        return resto is 0 or 1 ? 0 : 11 - resto;
    }

    /// <summary>
    /// Campo livre do código de barras (posições 20-44), padrão Sicoob
    /// (documento citado, item 3.29): código da carteira (1) +
    /// agência/cooperativa (4) + código da modalidade (2) + código do
    /// associado/cliente (7) + nosso número com DV (8) + número da parcela
    /// (3, "001" se parcela única).
    /// <para>Na linha digitável do Sicoob o campo 1 segue o formato
    /// "AAABC.DDDDE" (banco + moeda + carteira + agência + DV) e o campo 3
    /// carrega o nosso número com DV + parcela (documento citado, tabela de
    /// composição da linha digitável).</para>
    /// </summary>
    /// <param name="carteira">Código da carteira, 1 dígito (1 ou 3).</param>
    /// <param name="agencia">Agência/cooperativa com 4 dígitos.</param>
    /// <param name="modalidade">Código da modalidade com 2 dígitos.</param>
    /// <param name="codigoCliente">Código do cliente com 7 dígitos.</param>
    /// <param name="nossoNumeroComDv">Nosso número com 8 posições, com DV.</param>
    /// <param name="parcela">Parcela com 3 dígitos ("001" para parcela única).</param>
    /// <returns>Campo livre com 25 posições.</returns>
    /// <exception cref="ArgumentException">Entradas fora do formato.</exception>
    public static string ComporCampoLivre(
        string carteira,
        string agencia,
        string modalidade,
        string codigoCliente,
        string nossoNumeroComDv,
        string parcela)
    {
        Exigir(carteira, 1, nameof(carteira));
        Exigir(agencia, 4, nameof(agencia));
        Exigir(modalidade, 2, nameof(modalidade));
        Exigir(codigoCliente, 7, nameof(codigoCliente));
        Exigir(nossoNumeroComDv, 8, nameof(nossoNumeroComDv));
        Exigir(parcela, 3, nameof(parcela));
        return carteira + agencia + modalidade + codigoCliente + nossoNumeroComDv + parcela;
    }

    private static void Exigir(string valor, int tamanho, string nome)
    {
        if (valor.Length != tamanho || !valor.All(char.IsDigit))
        {
            throw new ArgumentException($"'{nome}' deve ter exatamente {tamanho} dígito(s).", nome);
        }
    }
}
