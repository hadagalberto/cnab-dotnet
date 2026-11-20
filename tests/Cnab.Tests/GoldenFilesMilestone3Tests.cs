namespace Cnab.Tests;

using Cnab.Arquivo;
using Cnab.Layouts.Cnab240;
using Cnab.Layouts.Cnab400;
using Cnab.Modelo;
using Xunit;

/// <summary>
/// Arquivos golden byte-exatos por banco e layout. Cada arquivo foi gerado a
/// partir das posições publicadas nos manuais oficiais citados em
/// docs/layouts.md e é comparado integralmente com a saída do gerador/leitor.
/// </summary>
public class GoldenFilesMilestone3Tests
{
    private static string Golden(string nome) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Golden", nome));

    [Theory]
    [InlineData(1, "001_cobranca_240.rem")]
    [InlineData(33, "033_cobranca_240.rem")]
    [InlineData(104, "104_cobranca_240.rem")]
    [InlineData(237, "237_cobranca_240.rem")]
    [InlineData(341, "341_cobranca_240.rem")]
    [InlineData(748, "748_cobranca_240.rem")]
    [InlineData(756, "756_cobranca_240.rem")]
    [InlineData(208, "208_cobranca_240.rem")]
    public void Remessa240_ByteExata(int codigo, string arquivo)
    {
        var esperado = Golden(arquivo);
        var atual = GeradorRemessa240.Gerar(Milestone3Fixtures.Remessa240(Bancos.Obter(codigo)));
        Assert.Equal(esperado, atual);
        Assert.Contains("\r\n", atual);
    }

    [Fact]
    public void Remessa240InterPagamento_ByteExata()
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
        Assert.Equal(Golden("077_pagamento_240.rem"), GeradorRemessa240.Gerar(remessa));
    }

    [Theory]
    [InlineData(341, "341_cobranca_400.rem")]
    [InlineData(237, "237_cobranca_400.rem")]
    [InlineData(748, "748_cobranca_400.rem")]
    [InlineData(77, "077_cobranca_400.rem")]
    public void Remessa400_ByteExata(int codigo, string arquivo)
    {
        var esperado = Golden(arquivo);
        var atual = GeradorRemessa400.Gerar(Milestone3Fixtures.Remessa400(Bancos.Obter(codigo)));
        Assert.Equal(esperado, atual);
        Assert.Contains("\r\n", atual);
    }

    [Theory]
    [InlineData(341, "341_cobranca_400.ret", "06", "03")]
    [InlineData(237, "237_cobranca_400.ret", "03", "08")]
    [InlineData(748, "748_cobranca_400.ret", "06", "P1")]
    [InlineData(77, "077_cobranca_400.ret", "06", null)]
    public void Retorno400_ParseiaERoundTripByteExato(int codigo, string arquivo, string ocorrencia, string? motivo)
    {
        var conteudo = Golden(arquivo);
        var retorno = LeitorRetorno400.Ler(conteudo);
        Assert.Equal(codigo, retorno.Banco.Codigo);
        var item = Assert.Single(retorno.Itens);
        Assert.Equal(ocorrencia, item.Ocorrencia!.Codigo);
        if (motivo is not null)
        {
            Assert.Contains(motivo, item.Motivos.Select(m => m.Codigo));
        }

        Assert.Equal(1500.50m, item.Valores!.ValorPago);
        Assert.Equal(
            conteudo,
            EscritorDeArquivo.Montar(LeitorRetorno400.EscreverLinhas(retorno)));
    }
}
