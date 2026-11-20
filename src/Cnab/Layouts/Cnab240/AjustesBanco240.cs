namespace Cnab.Layouts.Cnab240;

using System.Globalization;
using Cnab.Bancos;
using Cnab.Modelo;

/// <summary>
/// Ajustes dinâmicos aplicados após a escrita dos registros base FEBRABAN.
/// Complementam os ajustes estáticos de <see cref="PerfilBanco240"/> com
/// valores que dependem do título (por exemplo o DAC do nosso número do Itaú
/// e a decomposição do nosso número do Bradesco). Todas as faixas e regras
/// vêm dos manuais citados em <c>docs/layouts.md</c>.
/// </summary>
public static class AjustesBanco240
{
    /// <summary>Ajustes do header de arquivo do Itaú (manual janeiro/2017).</summary>
    /// <param name="remessa">Remessa.</param>
    /// <returns>Ajustes de posições 33-72.</returns>
    public static IReadOnlyList<AjusteCampo240> HeaderArquivoItau(Remessa240 remessa)
    {
        var conta = remessa.Cedente.Conta;
        var agencia = Digitos(conta.Agencia, 4, "Agência");
        var numero = Digitos(conta.Conta, 5, "Conta");
        var dac = Itau.CalcularDacAgenciaConta(agencia, numero);
        return
        [
            new(33, 20, new string(' ', 20), "Itaú: convênio 33-52 em branco."),
            new(53, 1, "0", "Itaú: posição 53 fixa 0."),
            new(54, 4, agencia, "Itaú: agência em 54-57 (4 posições)."),
            new(58, 1, " ", "Itaú: posição 58 em branco."),
            new(59, 7, new string('0', 7), "Itaú: posições 59-65 fixas em zeros."),
            new(66, 5, numero, "Itaú: conta em 66-70 (5 posições)."),
            new(71, 1, " ", "Itaú: posição 71 em branco."),
            new(72, 1, dac.ToString(CultureInfo.InvariantCulture), "Itaú: DAC ag/conta na posição 72 (Anexo 4)."),
        ];
    }

    /// <summary>Ajustes do header de lote de cobrança do Itaú (manual janeiro/2017).</summary>
    /// <param name="remessa">Remessa.</param>
    /// <returns>Ajustes de posições 34-73.</returns>
    public static IReadOnlyList<AjusteCampo240> HeaderLoteItau(Remessa240 remessa)
    {
        var conta = remessa.Cedente.Conta;
        var agencia = Digitos(conta.Agencia, 4, "Agência");
        var numero = Digitos(conta.Conta, 5, "Conta");
        var dac = Itau.CalcularDacAgenciaConta(agencia, numero);
        return
        [
            new(34, 20, new string(' ', 20), "Itaú: convênio 34-53 em branco."),
            new(54, 1, "0", "Itaú: posição 54 fixa 0."),
            new(55, 4, agencia, "Itaú: agência em 55-58 (4 posições)."),
            new(59, 1, " ", "Itaú: posição 59 em branco."),
            new(60, 7, new string('0', 7), "Itaú: posições 60-66 fixas em zeros."),
            new(67, 5, numero, "Itaú: conta em 67-71 (5 posições)."),
            new(72, 1, " ", "Itaú: posição 72 em branco."),
            new(73, 1, dac.ToString(CultureInfo.InvariantCulture), "Itaú: DAC ag/conta na posição 73 (Anexo 4)."),
        ];
    }

    /// <summary>Ajustes do segmento P do Itaú (manual janeiro/2017).</summary>
    /// <param name="remessa">Remessa.</param>
    /// <param name="titulo">Título.</param>
    /// <returns>Ajustes de posições 18-77.</returns>
    public static IReadOnlyList<AjusteCampo240> SegmentoPItau(Remessa240 remessa, Titulo titulo)
    {
        var conta = remessa.Cedente.Conta;
        var agencia = Digitos(conta.Agencia, 4, "Agência");
        var numero = Digitos(conta.Conta, 5, "Conta");
        var carteira = Digitos(titulo.Carteira, 3, "Carteira");
        var nossoNumero = Digitos(titulo.NossoNumero, 8, "Nosso Número");
        var dac = Itau.CalcularDacNossoNumero(agencia, numero, carteira, nossoNumero);
        return
        [
            new(18, 1, "0", "Itaú: posição 18 fixa 0."),
            new(19, 4, agencia, "Itaú: agência em 19-22."),
            new(23, 1, " ", "Itaú: posição 23 em branco."),
            new(24, 7, new string('0', 7), "Itaú: posições 24-30 fixas em zeros."),
            new(31, 5, numero, "Itaú: conta em 31-35."),
            new(36, 1, " ", "Itaú: posição 36 em branco."),
            new(37, 1, Itau.CalcularDacAgenciaConta(agencia, numero).ToString(CultureInfo.InvariantCulture), "Itaú: DAC ag/conta na posição 37."),
            new(38, 3, carteira, "Itaú: carteira em 38-40."),
            new(41, 8, nossoNumero, "Itaú: nosso número em 41-48."),
            new(49, 1, dac.ToString(CultureInfo.InvariantCulture), "Itaú: DAC do nosso número na posição 49."),
            new(50, 8, new string(' ', 8), "Itaú: posições 50-57 em branco."),
            new(58, 5, new string('0', 5), "Itaú: posições 58-62 fixas em zeros."),
            new(63, 10, titulo.NumeroDocumento.PadRight(10)[..10], "Itaú: seu número em 63-72."),
            new(73, 5, new string(' ', 5), "Itaú: posições 73-77 em branco."),
        ];
    }

    /// <summary>Ajustes do segmento Q do Itaú (manual janeiro/2017).</summary>
    /// <returns>Ajuste das posições 64-73 (brancos após o nome do pagador).</returns>
    public static IReadOnlyList<AjusteCampo240> SegmentoQItau() =>
    [
        new(64, 10, new string(' ', 10), "Itaú: posições 64-73 em branco (NOTA 20)."),
        new(210, 3, new string('0', 3), "Itaú: posições 210-212 fixas em zeros."),
        new(213, 28, new string(' ', 28), "Itaú: posições 213-240 em branco."),
    ];

    /// <summary>Decompõe o nosso número do Bradesco em produto+zeros+NN+DV (manual 4008.524.0121).</summary>
    /// <param name="produto">Código do produto (3 posições).</param>
    /// <param name="carteira">Carteira (2 dígitos).</param>
    /// <param name="titulo">Título.</param>
    /// <returns>Ajuste das posições 38-57 do segmento P.</returns>
    public static IReadOnlyList<AjusteCampo240> SegmentoPBradesco(string produto, string carteira, Titulo titulo)
    {
        var seq = Digitos(titulo.NossoNumero, 11, "Nosso Número");
        var nn = Bradesco.ComporNossoNumero(carteira, seq);
        var conteudo = Digitos(produto, 3, "Produto") + new string('0', 5) + nn;
        return [new(38, 20, conteudo, "Bradesco: produto(3)+zeros(5)+nosso número(11)+DV(1) nas posições 38-57.")];
    }

    private static string Digitos(string valor, int tamanho, string nome)
    {
        var digitos = new string([.. valor.Where(char.IsAsciiDigit)]);
        if (digitos.Length > tamanho)
        {
            throw new ArgumentException($"{nome} excede {tamanho} dígitos.", nome);
        }

        return digitos.PadLeft(tamanho, '0');
    }
}
