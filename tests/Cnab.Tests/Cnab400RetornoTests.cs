namespace Cnab.Tests;

using Cnab.Arquivo;
using Cnab.Layouts.Cnab240;
using Cnab.Layouts.Cnab400;
using Cnab.Modelo;
using Xunit;

/// <summary>Leitura de retorno CNAB 400, tabelas de ocorrências, valores e round-trip.</summary>
public class Cnab400RetornoTests
{
    [Fact]
    public void RetornoItau_ParseiaOcorrenciaTarifasValoresDataCreditoEMotivo378()
    {
        var retorno = LeitorRetorno400.Ler(EscritorDeArquivo.Montar(ItauRetorno()));
        Assert.Equal(Bancos.Itau, retorno.Banco);
        Assert.Equal(new DateOnly(2026, 11, 20), retorno.HeaderArquivo.DataGeracao);
        Assert.Equal(new DateOnly(2026, 11, 21), retorno.HeaderArquivo.DataCredito);
        var item = Assert.Single(retorno.Itens);
        Assert.Equal("98712345", item.NossoNumero);
        Assert.Equal("NF000001", item.SeuNumero);
        Assert.Equal("06", item.Ocorrencia!.Codigo);
        Assert.Equal(FamiliaOcorrencia.Liquidacao, item.Ocorrencia.Familia);
        Assert.Equal("03", Assert.Single(item.Motivos).Codigo);
        Assert.Equal(1500.50m, item.Valores!.ValorTitulo);
        Assert.Equal(1500.50m, item.Valores.ValorPago);
        Assert.Equal(2.50m, item.Valores.Tarifa);
        Assert.Equal(0.10m, item.Valores.Iof);
        Assert.Equal(0.75m, item.Valores.Desconto);
        Assert.Equal(1.25m, item.Valores.JurosMulta);
        Assert.Equal(new DateOnly(2026, 11, 20), item.Valores.DataOcorrencia);
        Assert.Equal(new DateOnly(2026, 11, 21), item.Valores.DataCredito);
        Assert.Equal(ItauRetorno(), LeitorRetorno400.EscreverLinhas(retorno));
    }

    [Fact]
    public void RetornoBradesco_ParseiaMotivos319EDataCredito296()
    {
        var retorno = LeitorRetorno400.Ler(EscritorDeArquivo.Montar(BradescoRetorno()), Bancos.Bradesco);
        Assert.Equal(Bancos.Bradesco, retorno.Banco);
        Assert.Equal(new DateOnly(2026, 11, 20), retorno.HeaderArquivo.DataGeracao);
        var item = Assert.Single(retorno.Itens);
        Assert.Equal("03", item.Ocorrencia!.Codigo);
        Assert.Equal(FamiliaOcorrencia.EntradaRejeitada, item.Ocorrencia.Familia);
        Assert.Equal(["08", "09"], item.Motivos.Select(m => m.Codigo));
        Assert.Equal(2.50m, item.Valores!.Tarifa);
        Assert.Equal(1500.50m, item.Valores.ValorPago);
        Assert.Equal(new DateOnly(2026, 11, 21), item.Valores.DataCredito);
        Assert.Equal(BradescoRetorno(), LeitorRetorno400.EscreverLinhas(retorno));
    }

    [Fact]
    public void RetornoSicredi_ParseiaLiquidacaoPixMotivoEDataAnsi()
    {
        var retorno = LeitorRetorno400.Ler(EscritorDeArquivo.Montar(SicrediRetorno()), Bancos.Sicredi);
        Assert.Equal(Bancos.Sicredi, retorno.Banco);
        Assert.Equal(new DateOnly(2026, 11, 20), retorno.HeaderArquivo.DataGeracao);
        var item = Assert.Single(retorno.Itens);
        Assert.Equal("06", item.Ocorrencia!.Codigo);
        Assert.Equal("P1", Assert.Single(item.Motivos).Codigo);
        Assert.Equal(1500.50m, item.Valores!.ValorPago);
        Assert.Equal(new DateOnly(2026, 11, 21), item.Valores.DataCredito);
        Assert.Equal(SicrediRetorno(), LeitorRetorno400.EscreverLinhas(retorno));
    }

    [Fact]
    public void RetornoInter_ParseiaOcorrencia90ValorPago160EDataCredito173()
    {
        var retorno = LeitorRetorno400.Ler(EscritorDeArquivo.Montar(InterRetorno()), Bancos.Inter);
        Assert.Equal(Bancos.Inter, retorno.Banco);
        var item = Assert.Single(retorno.Itens);
        Assert.Equal("06", item.Ocorrencia!.Codigo);
        Assert.Equal("Pago", item.Ocorrencia.Descricao);
        Assert.Equal(1500.50m, item.Valores!.ValorTitulo);
        Assert.Equal(1500.50m, item.Valores.ValorPago);
        Assert.Equal(new DateOnly(2026, 11, 20), item.Valores.DataOcorrencia);
        Assert.Equal(new DateOnly(2026, 11, 21), item.Valores.DataCredito);
        Assert.Equal(InterRetorno(), LeitorRetorno400.EscreverLinhas(retorno));
    }

    [Fact]
    public void Retorno_RejeitaLinhaComTamanhoIncorreto()
    {
        var linhas = ItauRetorno().ToList();
        linhas[1] = linhas[1][..^1];
        var ex = Assert.Throws<TamanhoRegistroException>(
            () => LeitorRetorno400.Ler(EscritorDeArquivo.Montar(linhas), Bancos.Itau));
        Assert.Equal(400, ex.Esperado);
        Assert.Equal(399, ex.Encontrado);
        Assert.Equal(2, ex.Linha);
    }

    [Fact]
    public void Tabelas400_ContemOcorrenciasEMotivosPublicados()
    {
        Assert.Equal("Liquidação normal", TabelasOcorrencias400.Obter(Bancos.Bradesco, "06").Descricao);
        Assert.Equal("Entrada rejeitada", TabelasOcorrencias400.Obter(Bancos.Itau, "03").Descricao);
        Assert.Equal("Liquidação normal", TabelasOcorrencias400.Obter(Bancos.Sicredi, "06").Descricao);
        Assert.Equal("Custas de protesto", TabelasOcorrencias400.ObterMotivo(Bancos.Bradesco, "08"));
        Assert.Equal("Sem fundos — 2ª apresentação", TabelasOcorrencias400.ObterMotivo(Bancos.Itau, "12"));
        Assert.Equal("Confirmado com QrCode", TabelasOcorrencias400.ObterMotivo(Bancos.Sicredi, "P1"));
        Assert.Throws<ArgumentException>(() => TabelasOcorrencias400.Obter(Bancos.Itau, "99"));
        Assert.Throws<ArgumentException>(() => TabelasOcorrencias400.ObterMotivo(Bancos.Itau, "ZZ"));
    }

    private static IReadOnlyList<string> ItauRetorno() =>
    [
        Milestone3Fixtures.NovaLinha400()
            .Por(1, "0").Por(2, "2").Por(3, "RETORNO")
            .Por(10, "01").Por(12, "COBRANCA")
            // 27-38 numérico: agência 4 + zeros fixos 31-32 + conta 5 + DAC 1.
            .Por(27, "000000000000")
            .Por(47, "EMPRESA TESTE LTDA")
            .Por(77, "341").Por(80, "BANCO ITAU SA")
            .Por(95, "201126").Por(101, "01600").Por(106, "BPI")
            .Por(109, "00001").Por(114, "211126").Por(395, "000001").ToString(),
        Milestone3Fixtures.NovaLinha400()
            .Por(1, "1").Por(2, "02").Por(4, "00000000000191")
            .Por(18, "0057").Por(22, "00").Por(24, "12345").Por(29, "7")
            .Por(38, "NF000001")
            .Por(63, "98712345").Por(83, "109").Por(86, "98712345").Por(94, "1")
            .Por(108, "I").Por(109, "06").Por(111, "201126")
            .Por(117, "NF000001").Por(127, "98712345")
            .Por(147, "201226").Por(153, "0000000150050")
            .Por(166, "341").Por(169, "0000000").Por(174, "01")
            .Por(176, "0000000000250").Por(215, "0000000000010")
            .Por(228, "0000000000000").Por(241, "0000000000075")
            .Por(254, "0000000150050").Por(267, "0000000000125")
            .Por(280, new string('0', 13)).Por(302, "0000")
            .Por(306, new string('0', 8)).Por(312, new string('0', 13))
            .Por(296, "211126").Por(378, "03").Por(393, "00")
            .Por(395, "000002").ToString(),
        Milestone3Fixtures.NovaLinha400()
            .Por(1, "9").Por(2, "2").Por(3, "01").Por(5, "341")
            .Por(18, "00000001").Por(26, "00000000150050")
            .Por(58, new string('0', 22)) // 58-65 + 66-79 numéricos
            .Por(178, new string('0', 22)) // direta/escritural numérica
            .Por(208, new string('0', 27)) // controle/qtde/valor numéricos
            .Por(395, "000003").ToString(),
    ];

    private static IReadOnlyList<string> BradescoRetorno() =>
    [
        Milestone3Fixtures.NovaLinha400()
            .Por(1, "0").Por(2, "2").Por(3, "RETORNO")
            .Por(10, "01").Por(12, "COBRANCA")
            // Código da empresa é numérico 9(20), alinhado à direita com zeros.
            .Por(27, "00000000000000000623").Por(47, "EMPRESA TESTE LTDA")
            .Por(77, "237").Por(80, "BRADESCO")
            .Por(95, "201126").Por(101, "01600000").Por(109, "00001")
            .Por(380, "211126").Por(395, "000001").ToString(),
        Milestone3Fixtures.NovaLinha400()
            .Por(1, "1").Por(2, "02").Por(4, "00000000000191")
            .Por(21, "00000000000623").Por(63, "00000000")
            .Por(166, "00000000") // banco/agência cobradores numéricos
            .Por(38, "NF000001").Por(71, "19" + "0000098712")
            .Por(106, "00").Por(108, "1").Por(109, "03").Por(111, "201126")
            .Por(117, "NF000001").Por(127, "00000000000000987123")
            .Por(147, "201226").Por(153, "0000000150050")
            .Por(174, "01").Por(176, "0000000000250")
            .Por(189, new string('0', 13)) // custas de protesto numéricas
            .Por(202, new string('0', 52)) // juros/IOF/abatimento/desconto numéricos
            .Por(254, "0000000150050").Por(267, new string('0', 26))
            .Por(296, "211126").Por(302, "000")
            .Por(319, "0809").Por(369, "00").Por(395, "000002").ToString(),
        Milestone3Fixtures.NovaLinha400()
            .Por(1, "9").Por(2, "2").Por(3, "01").Por(5, "237")
            .Por(18, "00000001").Por(26, "00000000150050")
            .Por(58, new string('0', 22)) // 58-65 + 66-79 numéricos
            .Por(75, new string('0', 29)) // liquidações e baixas numéricas
            .Por(363, new string('0', 23)) // rateios numéricos
            .Por(395, "000003").ToString(),
    ];

    private static IReadOnlyList<string> SicrediRetorno() =>
    [
        Milestone3Fixtures.NovaLinha400()
            .Por(1, "0").Por(2, "2").Por(3, "RETORNO")
            .Por(10, "01").Por(12, "COBRANCA")
            .Por(27, "00623")
            .Por(77, "748").Por(80, "BANSICREDI")
            .Por(95, "20261120").Por(111, "0000001")
            .Por(390, "99.99").Por(395, "000001").ToString(),
        Milestone3Fixtures.NovaLinha400()
            .Por(1, "1").Por(2, "A").Por(14, "A")
            .Por(48, "98712345").Por(109, "06").Por(111, "201126")
            .Por(189, new string('0', 13)) // custas de protesto numéricas
            .Por(202, new string('0', 26)) // zeros fixos 202-227 no detalhe de retorno
            .Por(228, new string('0', 13)) // abatimento numérico
            .Por(241, new string('0', 13)) // desconto numérico
            .Por(117, "NF000001").Por(147, "201226").Por(153, "0000000150050")
            .Por(175, "A").Por(176, "0000000000250")
            .Por(254, "0000000150050").Por(267, "0000000000125")
            .Por(280, new string('0', 13)) // juros de mora numéricos
            .Por(319, "P1").Por(329, "20261121").Por(395, "000002").ToString(),
        Milestone3Fixtures.NovaLinha400()
            .Por(1, "9").Por(2, "2").Por(3, "748").Por(6, "00623")
            .Por(395, "000003").ToString(),
    ];

    private static IReadOnlyList<string> InterRetorno() =>
    [
        Milestone3Fixtures.NovaLinha400()
            .Por(1, "0").Por(2, "2").Por(3, "RETORNO")
            .Por(10, "01").Por(12, "COBRANCA")
            .Por(47, "EMPRESA TESTE LTDA")
            .Por(77, "077").Por(80, "INTER").Por(95, "201126")
            .Por(395, "000001").ToString(),
        Milestone3Fixtures.NovaLinha400()
            .Por(1, "1").Por(2, "02").Por(4, "00000000000191")
            .Por(18, "000") // zeros fixos 18-20
            .Por(21, "112").Por(24, "0001").Por(28, "0000012345")
            .Por(63, "00000000") // zeros fixos 63-70
            .Por(38, "NF000001")
            .Por(71, "00098712345").Por(87, "112")
            .Por(90, "06").Por(92, "201126").Por(98, "NF000001")
            .Por(108, "00098712345").Por(119, "201226").Por(125, "0000000150050")
            .Por(138, "077").Por(141, "0001").Por(145, "01")
            .Por(160, "0000000150050").Por(173, "211126")
            .Por(182, "PAGADOR TESTE LTDA")
            .Por(227, "11144477735").Por(381, "0000001")
            .Por(395, "000002").ToString(),
        Milestone3Fixtures.NovaLinha400()
            .Por(1, "9").Por(2, "2").Por(3, "01").Por(5, "077")
            .Por(18, "00000001").Por(116, "00001").Por(121, "000000150050")
            .Por(58, new string('0', 17)) // ocorrência 02: 58-62 + 63-74
            .Por(87, "00000") // ocorrência 03 numérica
            .Por(395, "000003").ToString(),
    ];
}
