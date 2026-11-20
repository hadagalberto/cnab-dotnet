namespace Cnab.Tests;

using Cnab.Bancos;
using Cnab.Layouts.Cnab240;
using Cnab.Modelo;
using Xunit;

/// <summary>
/// Layouts CNAB 240 e desvios publicados dos bancos adicionados no marco 3:
/// Bradesco, Itaú, Sicredi, Inter e BTG. Todas as posições abaixo vêm dos
/// manuais registrados em docs/layouts.md.
/// </summary>
public class Cnab240BancosAdicionaisTests
{
    [Theory]
    [InlineData(237, "BRADESCO", 84, 42)]
    [InlineData(341, "BANCO ITAU SA", 40, 30)]
    [InlineData(748, "SICREDI", 81, 40)]
    [InlineData(208, "BTG PACTUAL", 103, 60)]
    public void PerfilBanco_ContemVersoesPublicadas(int codigo, string nome, int versaoArquivo, int versaoLote)
    {
        var perfil = PerfilBanco240.Obter(Bancos.Obter(codigo));
        Assert.Equal(nome, perfil.NomeBanco);
        Assert.Equal(versaoArquivo, perfil.VersaoLayoutArquivo);
        Assert.Equal(versaoLote, perfil.VersaoLayoutLoteCobranca);
    }

    [Fact]
    public void PerfilInter_PagamentosUsaVersao107E46()
    {
        var perfil = PerfilBanco240.Obter(Bancos.Inter);
        Assert.Equal(107, perfil.VersaoLayoutArquivo);
        Assert.Equal(46, perfil.VersaoLayoutLotePagamento);
        Assert.Equal("BANCO INTER", perfil.NomeBanco);
    }

    [Theory]
    [InlineData(237)]
    [InlineData(341)]
    [InlineData(748)]
    [InlineData(208)]
    public void RemessaCobranca_TemSeteLinhasDe240Posicoes(int codigo)
    {
        var linhas = GeradorRemessa240.GerarLinhas(Milestone3Fixtures.Remessa240(Bancos.Obter(codigo)));
        Assert.Equal(7, linhas.Count);
        Assert.All(linhas, linha => Assert.Equal(240, linha.Length));
        Assert.Equal($"{codigo:000}", linhas[0][..3]);
        Assert.All(linhas, linha => Assert.Equal($"{codigo:000}", linha[..3]));
    }

    [Fact]
    public void RemessaBradesco_CompõeNossoNumeroConformeManual()
    {
        var linhas = GeradorRemessa240.GerarLinhas(Milestone3Fixtures.Remessa240(Bancos.Bradesco));
        var p = linhas[2];
        Assert.Equal("P", p.Substring(13, 1));
        Assert.Equal("009", p.Substring(37, 3));
        Assert.Equal("00000", p.Substring(40, 5));
        var esperado = Bradesco.ComporNossoNumero("19", "00000000002");
        Assert.Equal(esperado, p.Substring(45, 12));
        Assert.Equal(esperado[^1..], p.Substring(56, 1));
    }

    [Fact]
    public void RemessaItau_UsaAgencia4Conta5EExigeDac()
    {
        var linhas = GeradorRemessa240.GerarLinhas(Milestone3Fixtures.Remessa240(Bancos.Itau));
        var header = linhas[0];
        Assert.Equal("040", header.Substring(163, 3));
        Assert.Equal("0", header.Substring(52, 1));
        Assert.Equal("0057", header.Substring(53, 4));
        Assert.Equal("0000000", header.Substring(58, 7));
        Assert.Equal("12345", header.Substring(65, 5));
        Assert.Equal("7", header.Substring(71, 1));

        var headerLote = linhas[1];
        Assert.Equal("030", headerLote.Substring(13, 3));
        Assert.Equal("0057", headerLote.Substring(54, 4));

        var p = linhas[2];
        Assert.Equal("00000002", p.Substring(40, 8));
        Assert.Equal(
            Itau.CalcularDacNossoNumero("0057", "12345", "109", "00000002").ToString(System.Globalization.CultureInfo.InvariantCulture),
            p.Substring(48, 1));
        Assert.Equal("          ", linhas[3].Substring(63, 10));

        // O DAC da agência/conta do manual (vetor 0057/12345 → 7).
        Assert.Equal(7, Itau.CalcularDacAgenciaConta("0057", "12345"));
    }

    [Fact]
    public void RemessaSicredi_DeixaConvênioEDVsDaAgenciaEmBranco()
    {
        var linhas = GeradorRemessa240.GerarLinhas(Milestone3Fixtures.Remessa240(Bancos.Sicredi));
        Assert.Equal(new string(' ', 20), linhas[0].Substring(32, 20));
        Assert.Equal(" ", linhas[0].Substring(57, 1));
        Assert.Equal(" ", linhas[0].Substring(71, 1));
        Assert.Equal("081", linhas[0].Substring(163, 3));
        Assert.Equal(new string(' ', 20), linhas[1].Substring(33, 20));
        Assert.Equal("040", linhas[1].Substring(13, 3));
        Assert.Equal("1", linhas[2].Substring(223, 1));
        Assert.Equal("000", linhas[2].Substring(224, 3));
    }

    [Fact]
    public void RemessaBtg_UsaCarteiraCadastramentoDocumentoEmissaoDistribuicaoFixos()
    {
        var linhas = GeradorRemessa240.GerarLinhas(Milestone3Fixtures.Remessa240(Bancos.Btg));
        Assert.Equal("103", linhas[0].Substring(163, 3));
        Assert.Equal("060", linhas[1].Substring(13, 3));
        var p = linhas[2];
        Assert.Equal("11211", p.Substring(57, 5));
    }

    [Fact]
    public void RemessaInterPagamento_UsaVersao107AgenciaUnicaEOperacaoC()
    {
        var cedente = Milestone3Fixtures.Cedente(Bancos.Inter);
        var remessa = new Remessa240
        {
            Banco = Bancos.Inter,
            Cedente = cedente,
            NumeroSequencialArquivo = 1,
            DataGeracao = new DateOnly(2026, 11, 20),
            HoraGeracao = new TimeOnly(10, 15, 30),
            LotesPagamento = [Cnab240Fixtures.LoteTed(cedente)],
        };
        var linhas = GeradorRemessa240.GerarLinhas(remessa);
        Assert.Equal(6, linhas.Count);
        Assert.Equal("107", linhas[0].Substring(163, 3));
        Assert.Equal("00001", linhas[0].Substring(52, 5));
        Assert.Equal("9", linhas[0].Substring(57, 1));
        Assert.Equal("046", linhas[1].Substring(13, 3));
        Assert.Equal("C", linhas[1].Substring(8, 1));
        Assert.Equal("00001", linhas[1].Substring(52, 5));
    }

    [Fact]
    public void Trailers_CalculamQuantidadeERegistrosAutomaticamente()
    {
        var linhas = GeradorRemessa240.GerarLinhas(Milestone3Fixtures.Remessa240(Bancos.Bradesco));
        Assert.Equal("000001", linhas[5].Substring(23, 6));
        Assert.Equal("00000000000150050", linhas[5].Substring(29, 17));
        Assert.Equal("000001", linhas[6].Substring(17, 6));
        Assert.Equal("000007", linhas[6].Substring(23, 6));
    }
}
