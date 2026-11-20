namespace Cnab.Tests;

using Cnab.Bancos;
using Cnab.Layouts.Cnab400;
using Cnab.Modelo;
using Xunit;

/// <summary>
/// Posições e valores fixos da remessa CNAB 400, extraídos dos manuais
/// oficiais citados em docs/layouts.md (Itaú, Bradesco, Sicredi e Inter).
/// </summary>
public class Cnab400RemessaTests
{
    [Theory]
    [InlineData(341, 4)]
    [InlineData(237, 3)]
    [InlineData(748, 3)]
    [InlineData(77, 3)]
    public void Remessa_TemLinhasDe400PosicoesENumeroSequencial(int codigo, int quantidadeLinhas)
    {
        var linhas = GeradorRemessa400.GerarLinhas(Milestone3Fixtures.Remessa400(Bancos.Obter(codigo)));
        Assert.Equal(quantidadeLinhas, linhas.Count);
        Assert.All(linhas, linha => Assert.Equal(400, linha.Length));
        Assert.Equal("9", linhas[^1][..1]);
    }

    [Fact]
    public void RemessaItau_UsaAgencia4Conta5Carteira3ENossoNumero8()
    {
        var l = GeradorRemessa400.GerarLinhas(Milestone3Fixtures.Remessa400(Bancos.Itau));
        Assert.Equal("0", l[0][..1]);
        Assert.Equal("1", l[0].Substring(1, 1));
        Assert.Equal("REMESSA", l[0].Substring(2, 7));
        Assert.Equal("COBRANCA", l[0].Substring(11, 15).TrimEnd());
        Assert.Equal("341", l[0].Substring(76, 3));
        Assert.Equal("BANCO ITAU SA", l[0].Substring(79, 15).TrimEnd());
        Assert.Equal("201126", l[0].Substring(94, 6));
        Assert.Equal("000001", l[0].Substring(394, 6));

        Assert.Equal("1", l[1][..1]);
        Assert.Equal("98712345", l[1].Substring(62, 8));
        Assert.Equal("109", l[1].Substring(83, 3));
        Assert.Equal("I", l[1].Substring(107, 1));
        Assert.Equal("01", l[1].Substring(108, 2));
        Assert.Equal("201226", l[1].Substring(120, 6));
        Assert.Equal("0000000150050", l[1].Substring(126, 13));

        Assert.Equal("2", l[2][..1]);
        Assert.Equal("2", l[2].Substring(1, 1));
        Assert.Equal("000004", l[3].Substring(394, 6));
    }

    [Fact]
    public void RemessaBradesco_UsaCarteira3Agencia5Conta7ENossoNumeroComDV()
    {
        var l = GeradorRemessa400.GerarLinhas(Milestone3Fixtures.Remessa400(Bancos.Bradesco));
        Assert.Equal("REMESSA", l[0].Substring(2, 7));
        Assert.Equal("237", l[0].Substring(76, 3));
        Assert.Equal("BRADESCO", l[0].Substring(79, 15).TrimEnd());
        Assert.Equal("MX", l[0].Substring(108, 2));
        Assert.Equal("0000001", l[0].Substring(110, 7));

        Assert.Equal("019", l[1].Substring(21, 3));
        Assert.Equal("00057", l[1].Substring(24, 5));
        Assert.Equal("0012345", l[1].Substring(29, 7));
        var nn = Bradesco.ComporNossoNumero("19", "00098712345");
        Assert.Equal(nn, l[1].Substring(70, 12));
        Assert.Equal("01", l[1].Substring(108, 2));
        Assert.Equal("0000000150050", l[1].Substring(126, 13));
    }

    [Fact]
    public void RemessaSicredi_UsaLiteralSicrediDataAnsiENossoNumero9()
    {
        var l = GeradorRemessa400.GerarLinhas(Milestone3Fixtures.Remessa400(Bancos.Sicredi));
        Assert.Equal("REMESSA", l[0].Substring(2, 7));
        Assert.Equal("COBRANCA", l[0].Substring(11, 8));
        Assert.Equal("00623", l[0].Substring(26, 5));
        Assert.Equal("748", l[0].Substring(76, 3));
        Assert.Equal("SICREDI", l[0].Substring(79, 15).TrimEnd());
        Assert.Equal("20261120", l[0].Substring(94, 8));
        Assert.Equal("0000001", l[0].Substring(110, 7));
        Assert.Equal("2.00", l[0].Substring(390, 4));

        Assert.Equal("A", l[1].Substring(1, 1));
        Assert.Equal("98712345", l[1].Substring(47, 9).TrimStart('0'));
        Assert.Equal("01", l[1].Substring(108, 2));
        Assert.Equal("NF000001", l[1].Substring(110, 10).TrimEnd());
        Assert.Equal("0000000150050", l[1].Substring(126, 13));
        Assert.Equal("9", l[2][..1]);
        Assert.Equal("1", l[2].Substring(1, 1));
        Assert.Equal("748", l[2].Substring(2, 3));
    }

    [Fact]
    public void RemessaInter_UsaCarteira112Agencia00001ELiteralINTER()
    {
        var l = GeradorRemessa400.GerarLinhas(Milestone3Fixtures.Remessa400(Bancos.Inter));
        Assert.Equal("077", l[0].Substring(76, 3));
        Assert.Equal("INTER", l[0].Substring(79, 15).TrimEnd());
        Assert.Equal("0000001", l[0].Substring(110, 7));

        Assert.Equal("112", l[1].Substring(20, 3));
        Assert.Equal("0001", l[1].Substring(23, 4));
        Assert.Equal("00098712345", l[1].Substring(89, 11));
        Assert.Equal("01", l[1].Substring(108, 2));
        Assert.Equal("201226", l[1].Substring(120, 6));
        Assert.Equal("0000000150050", l[1].Substring(126, 13));
        Assert.Equal("01", l[1].Substring(220, 2));
        Assert.Equal("000001", l[2].Substring(1, 6));
    }

    [Fact]
    public void Remessa_RejeitaBancoSemLayout400()
    {
        Assert.Throws<ArgumentException>(() =>
            GeradorRemessa400.GerarLinhas(Milestone3Fixtures.Remessa400(Bancos.BancoDoBrasil)));
    }
}
