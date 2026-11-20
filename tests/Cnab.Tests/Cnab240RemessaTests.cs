namespace Cnab.Tests;

using System.Globalization;
using Cnab.Layouts.Cnab240;
using Cnab.Modelo;
using Xunit;

/// <summary>Testes de geração de remessa CNAB 240, incluindo golden byte-exato.</summary>
public class Cnab240RemessaTests
{
    [Fact]
    public void Golden_BancoDoBrasil_Cobranca_ByteExato()
    {
        var esperado = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Golden", "bb_cobranca.rem"));
        var atual = GeradorRemessa240.Gerar(Cnab240Fixtures.RemessaCobranca(Bancos.BancoDoBrasil));
        Assert.Equal(esperado, atual);
    }

    [Fact]
    public void Golden_BancoDoBrasil_Cobranca_TemSeisRegistrosDe240()
    {
        var linhas = GeradorRemessa240.GerarLinhas(Cnab240Fixtures.RemessaCobranca(Bancos.BancoDoBrasil));
        Assert.Equal(6, linhas.Count);
        Assert.All(linhas, linha => Assert.Equal(240, linha.Length));
    }

    [Fact]
    public void BancoDoBrasil_UsaVersoesDeLayoutDoManual()
    {
        var linhas = GeradorRemessa240.GerarLinhas(Cnab240Fixtures.RemessaCobranca(Bancos.BancoDoBrasil));
        Assert.Equal("083", linhas[0].Substring(163, 3));
        Assert.Equal("042", linhas[1].Substring(13, 3));
        Assert.Equal("R", linhas[1].Substring(8, 1));
        Assert.Equal("BANCO DO BRASIL S.A.", linhas[0].Substring(102, 20).TrimEnd());
    }

    [Fact]
    public void Caixa_CodigoBeneficiario_OcupaPosicoesDoManual()
    {
        var remessa = Cnab240Fixtures.RemessaCobranca(Bancos.Caixa);
        var titulo = remessa.LotesCobranca[0].Titulos[0] with { NossoNumero = "123412345671" };
        remessa = remessa with
        {
            CodigoBeneficiario = "339578",
            LotesCobranca = [remessa.LotesCobranca[0] with { Titulos = [titulo] }],
        };
        var linhas = GeradorRemessa240.GerarLinhas(remessa);

        // Header de arquivo: 33-52 uso exclusivo CAIXA (zeros), 59-65 código
        // do beneficiário com 7 posições (G007 do manual SIGCB 67.118 v031).
        Assert.Equal(new string('0', 20), linhas[0].Substring(32, 20));
        Assert.Equal("0339578", linhas[0].Substring(58, 7));
        // Header de lote: 34-40 beneficiário, 41-53 zeros.
        Assert.Equal("0339578", linhas[1].Substring(33, 7));
        Assert.Equal(new string('0', 13), linhas[1].Substring(40, 13));
        // Segmento P: 24-30 beneficiário, 31-37 zeros, 43-57 nosso número.
        Assert.Equal("0339578", linhas[2].Substring(23, 7));
        Assert.Equal(new string('0', 7), linhas[2].Substring(30, 7));
        Assert.Equal("000123412345671", linhas[2].Substring(42, 15));
        Assert.Equal(new string('0', 10), linhas[2].Substring(229, 10));
    }

    [Fact]
    public void Santander_UsaLayoutProprioNosHeadersEPeriodoP()
    {
        var remessa = Cnab240Fixtures.RemessaCobranca(Bancos.Santander);
        var titulo = remessa.LotesCobranca[0].Titulos[0] with { NossoNumero = "123412345671" };
        remessa = remessa with
        {
            Convenio = "000000000000001",
            LotesCobranca = [remessa.LotesCobranca[0] with { Titulos = [titulo] }],
        };
        var linhas = GeradorRemessa240.GerarLinhas(remessa);

        Assert.Equal("033", linhas[0].Substring(0, 3));
        Assert.Equal(new string(' ', 8), linhas[0].Substring(8, 8));
        Assert.Equal("000000000000191", linhas[0].Substring(17, 15));
        Assert.Equal("000000000000001", linhas[0].Substring(32, 15));
        Assert.Equal("040", linhas[0].Substring(163, 3));
        Assert.Equal("030", linhas[1].Substring(13, 3));
        Assert.Equal("000000000000191", linhas[1].Substring(18, 15));
        Assert.Equal("000000000000001", linhas[1].Substring(53, 15));
        // Revisão 2026-11-20 (item 7): o nosso número do Santander agora entra
        // com o DV do manual v34 (12 dígitos + DV = 123412345671-4), não cru.
        Assert.Equal("1234123456714", linhas[2].Substring(44, 13));
        Assert.Equal("0", linhas[2].Substring(224, 1));
    }

    [Fact]
    public void Sicoob_UsaVersoesDoGuiaDeImportacao()
    {
        var linhas = GeradorRemessa240.GerarLinhas(Cnab240Fixtures.RemessaCobranca(Bancos.Sicoob));
        Assert.Equal("087", linhas[0].Substring(163, 3));
        Assert.Equal("0", linhas[0].Substring(71, 1));
        Assert.Equal("00000000", linhas[1].Substring(199, 8));
        Assert.Equal("09", linhas[2].Substring(227, 2));
    }

    [Fact]
    public void TrailerDeArquivo_ContaLotesERegistros()
    {
        var remessa = Cnab240Fixtures.RemessaCobranca(Bancos.BancoDoBrasil);
        remessa = remessa with
        {
            LotesPagamento = [Cnab240Fixtures.LoteTed(Cnab240Fixtures.Cedente(Bancos.BancoDoBrasil))],
        };
        var linhas = GeradorRemessa240.GerarLinhas(remessa);
        var trailer = linhas[^1];
        Assert.Equal("000002", trailer.Substring(17, 6));
        Assert.Equal(linhas.Count.ToString("D6", CultureInfo.InvariantCulture), trailer.Substring(23, 6));
        Assert.Equal("9999", trailer.Substring(3, 4));
    }

    [Fact]
    public void TrailerDePagamento_SomaValoresEQuantidadeDeRegistros()
    {
        var cedente = Cnab240Fixtures.Cedente(Bancos.BancoDoBrasil);
        var remessa = new Remessa240
        {
            Banco = Bancos.BancoDoBrasil,
            Cedente = cedente,
            NumeroSequencialArquivo = 1,
            DataGeracao = new DateOnly(2026, 11, 20),
            HoraGeracao = new TimeOnly(10, 15, 30),
            LotesPagamento = [Cnab240Fixtures.LoteTed(cedente)],
        };
        var linhas = GeradorRemessa240.GerarLinhas(remessa);
        var trailer = linhas[^2];
        Assert.Equal("5", trailer.Substring(7, 1));
        Assert.Equal("000000000000025075", trailer.Substring(23, 18));
        // header de lote + A + B + trailer = 4 registros
        Assert.Equal("000004", trailer.Substring(17, 6));
    }

    [Fact]
    public void PagamentoTed_GeraSegmentosAEBComNSRSequencial()
    {
        var cedente = Cnab240Fixtures.Cedente(Bancos.BancoDoBrasil);
        var remessa = new Remessa240
        {
            Banco = Bancos.BancoDoBrasil,
            Cedente = cedente,
            NumeroSequencialArquivo = 1,
            DataGeracao = new DateOnly(2026, 11, 20),
            HoraGeracao = new TimeOnly(10, 15, 30),
            LotesPagamento = [Cnab240Fixtures.LoteTed(cedente)],
        };
        var linhas = GeradorRemessa240.GerarLinhas(remessa);
        Assert.Equal("A", linhas[2].Substring(13, 1));
        Assert.Equal("B", linhas[3].Substring(13, 1));
        Assert.Equal("00001", linhas[2].Substring(8, 5));
        Assert.Equal("00002", linhas[3].Substring(8, 5));
        Assert.Equal("018", linhas[2].Substring(17, 3));
        Assert.Equal("237", linhas[2].Substring(20, 3));
        Assert.Equal("000000000025075", linhas[2].Substring(119, 15));
        Assert.Equal("00000000000191", linhas[3].Substring(18, 14));
    }

    [Fact]
    public void Arquivo_UsaCrLfPorPadraoELatin1()
    {
        var conteudo = GeradorRemessa240.Gerar(Cnab240Fixtures.RemessaCobranca(Bancos.BancoDoBrasil));
        Assert.Contains("\r\n", conteudo, StringComparison.Ordinal);
        Assert.DoesNotContain("\n\n", conteudo, StringComparison.Ordinal);
    }
}
