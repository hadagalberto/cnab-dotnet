namespace Cnab.Tests;

using Cnab.Arquivo;
using Cnab.Layouts.Cnab240;
using Cnab.Layouts.Cnab240.Registros;
using Cnab.Modelo;
using Xunit;

/// <summary>Leitura de retorno CNAB 240 e tabelas de ocorrências dos novos bancos.</summary>
public class Cnab240RetornoBancosAdicionaisTests
{
    private static IReadOnlyList<string> Retorno(int codigoBanco, int ocorrencia, string motivos, char operacao = 'T')
    {
        var header = Milestone3Fixtures.NovaLinha240()
            .Por(1, $"{codigoBanco:000}")
            .Por(4, "0000") // G002: header de arquivo usa lote 0000
            .Por(8, "0")
            .Por(18, "2")
            .Por(19, "00000000000191")
            .Por(53, "00000").Por(59, "000000000000")
            .Por(152, "000000") // HoraGeracao numérica (G017)
            .Por(158, "000000") // NSA (G018)
            .Por(73, "EMPRESA TESTE LTDA")
            .Por(103, "BANCO TESTE")
            .Por(143, "2")
            .Por(144, "20092026")
            .Por(164, "103")
            .Por(167, "01600")
            .ToString();
        var lote = Milestone3Fixtures.NovaLinha240()
            .Por(1, $"{codigoBanco:000}")
            .Por(4, "0001")
            .Por(8, "1")
            .Por(9, operacao.ToString())
            .Por(10, "01")
            .Por(14, "060")
            .Por(18, "2")
            .Por(19, "00000000000191")
            .Por(54, "00000").Por(60, "000000000000")
            .Por(74, "EMPRESA TESTE LTDA")
            .Por(184, "00000001")
            .Por(192, "20092026")
            .Por(200, "00000000") // DataCredito opcional zerada na remessa/retorno
            .ToString();
        var t = RegistroCnab<SegmentoT240>.Escrever(new SegmentoT240
        {
            Banco = codigoBanco,
            Lote = 1,
            TipoRegistro = 3,
            Sequencial = 1,
            CodigoMovimento = ocorrencia,
            NossoNumero = "00000000000000012345",
            NumeroDocumento = "NF000001",
            Valor = 1500.50m,
            ValorTarifa = 2.50m,
            MotivoOcorrencia = motivos,
        });
        var u = RegistroCnab<SegmentoU240>.Escrever(new SegmentoU240
        {
            Banco = codigoBanco,
            Lote = 1,
            TipoRegistro = 3,
            Sequencial = 2,
            CodigoMovimento = ocorrencia,
            JurosMultaEncargos = 1.25m,
            ValorDesconto = 0.75m,
            ValorAbatimento = 0m,
            ValorIof = 0.10m,
            ValorPago = 1500.50m,
            ValorLiquido = 1498.40m,
            DataOcorrencia = new DateOnly(2026, 11, 20),
            DataCredito = new DateOnly(2026, 11, 21),
            CodigoOcorrenciaPagador = "0101",
        });
        var trailerLote = RegistroCnab<TrailerLoteCobranca240>.Escrever(new TrailerLoteCobranca240
        {
            Banco = codigoBanco,
            Lote = 1,
            TipoRegistro = 5,
            QuantidadeSimples = 1,
            ValorSimples = 1500.50m,
        });
        var trailerArquivo = RegistroCnab<TrailerArquivo240>.Escrever(new TrailerArquivo240
        {
            Banco = codigoBanco,
            Lote = 9999,
            TipoRegistro = 9,
            QuantidadeLotes = 1,
            QuantidadeRegistros = 6,
        });
        return [header, lote, t, u, trailerLote, trailerArquivo];
    }

    [Theory]
    [InlineData(237, 6, "08")]
    [InlineData(341, 6, "03")]
    [InlineData(748, 6, "P1")]
    [InlineData(208, 85, "00")]
    public void Retorno_ParseiaOcorrenciasValoresEDataDeTodosOsBancos(int codigo, int ocorrencia, string motivos)
    {
        var linhas = Retorno(codigo, ocorrencia, motivos);
        var retorno = LeitorRetorno240.Ler(EscritorDeArquivo.Montar(linhas), Bancos.Obter(codigo));

        Assert.Equal(codigo, retorno.Banco.Codigo);
        var item = Assert.Single(Assert.Single(retorno.Lotes).Itens);
        Assert.NotNull(item.Titulo);
        Assert.NotNull(item.Valores);
        Assert.Equal($"{ocorrencia:00}", item.Ocorrencia!.Codigo);
        Assert.Equal(2.50m, item.Titulo.ValorTarifa);
        Assert.Equal(1500.50m, item.Valores.ValorPago);
        Assert.Equal(1498.40m, item.Valores.ValorLiquido);
        Assert.Equal(1.25m, item.Valores.JurosMultaEncargos);
        Assert.Equal(0.75m, item.Valores.ValorDesconto);
        Assert.Equal(0.10m, item.Valores.ValorIof);
        Assert.Equal(new DateOnly(2026, 11, 20), item.Valores.DataOcorrencia);
        Assert.Equal(new DateOnly(2026, 11, 21), item.Valores.DataCredito);
        Assert.Equal(["0101"], item.OcorrenciasPagador);
        Assert.Equal(linhas, LeitorRetorno240.EscreverLinhas(retorno));
    }

    [Fact]
    public void RetornoItau_MapeiaMotivoDaPosicao214EClassificaLiquidacao()
    {
        var retorno = LeitorRetorno240.Ler(
            EscritorDeArquivo.Montar(Retorno(341, 6, "03")), Bancos.Itau);
        var item = Assert.Single(Assert.Single(retorno.Lotes).Itens);
        Assert.Equal(FamiliaOcorrencia.Liquidacao, item.Ocorrencia!.Familia);
        Assert.Equal("03", Assert.Single(item.Motivos).Codigo);
    }

    [Fact]
    public void RetornoSicredi_ReconheceLiquidacaoPixENegativacao()
    {
        var retorno = LeitorRetorno240.Ler(
            EscritorDeArquivo.Montar(Retorno(748, 61, "P1")), Bancos.Sicredi);
        var item = Assert.Single(Assert.Single(retorno.Lotes).Itens);
        Assert.Equal(FamiliaOcorrencia.Liquidacao, item.Ocorrencia!.Familia);
        Assert.Equal("Liquidação PIX", item.Ocorrencia.Descricao);
        Assert.Equal("Confirmado com QrCode", Assert.Single(item.Motivos).Descricao);
    }

    [Fact]
    public void RetornoBtg_ReconheceOcorrenciasProprietarias()
    {
        Assert.Equal("Recompra com adiantamento", TabelasOcorrencias.Obter(Bancos.Btg, "85").Descricao);
        Assert.Equal(FamiliaOcorrencia.Rejeicao, TabelasOcorrencias.Obter(Bancos.Btg, "93").Familia);
        Assert.Equal("Recompra com substituição", TabelasOcorrencias.Obter(Bancos.Btg, "74").Descricao);
    }

    [Fact]
    public void TabelaInterPagamentos_ContemCodigosPublicados()
    {
        Assert.Equal("Pix não efetivado", TabelasOcorrencias.Obter(Bancos.Inter, "PA").Descricao);
        Assert.Equal(FamiliaOcorrencia.Rejeicao, TabelasOcorrencias.Obter(Bancos.Inter, "AG").Familia);
        Assert.Equal("Crédito ou débito efetivado", TabelasOcorrencias.ObterPagamento("00").Descricao);
        Assert.Throws<ArgumentException>(() => TabelasOcorrencias.Obter(Bancos.Inter, "99"));
    }

    [Fact]
    public void RetornoInterPagamento_ParseiaOcorrenciasNaFaixa231A240()
    {
        var linhas = new[]
        {
            Milestone3Fixtures.NovaLinha240()
                .Por(1, "077").Por(4, "0000").Por(8, "0").Por(18, "2").Por(19, "00000000000191")
                .Por(53, "00000").Por(59, "000000000000")
                .Por(152, "000000").Por(158, "000000")
                .Por(73, "EMPRESA TESTE LTDA").Por(103, "BANCO INTER")
                .Por(143, "2").Por(144, "20092026").Por(164, "107").Por(167, "01600").ToString(),
            Milestone3Fixtures.NovaLinha240()
                .Por(1, "077").Por(4, "0001").Por(8, "1").Por(9, "C").Por(10, "20")
                .Por(12, "41").Por(14, "046").Por(18, "2").Por(19, "00000000000191")
                .Por(53, "00000").Por(59, "000000000000")
                .Por(74, "EMPRESA TESTE LTDA").Por(173, "00000").Por(192, "20092026")
                .Por(213, "00000").Por(223, "00").ToString(),
            Milestone3Fixtures.NovaLinha240()
                .Por(1, "077").Por(4, "0001").Por(8, "3").Por(9, "00001")
                .Por(14, "A").Por(231, "PA").ToString(),
            RegistroCnab<TrailerLotePagamento240>.Escrever(new TrailerLotePagamento240
            {
                Banco = 77, Lote = 1, TipoRegistro = 5, QuantidadeRegistros = 3, SomatoriaValores = 250.75m,
            }),
            RegistroCnab<TrailerArquivo240>.Escrever(new TrailerArquivo240
            {
                Banco = 77, Lote = 9999, TipoRegistro = 9, QuantidadeLotes = 1, QuantidadeRegistros = 5,
            }),
        };
        var retorno = LeitorRetorno240.Ler(EscritorDeArquivo.Montar(linhas), Bancos.Inter);

        var lote = Assert.Single(retorno.Lotes);
        Assert.NotNull(lote.HeaderPagamento);
        var item = Assert.Single(lote.Itens);
        Assert.Equal("PA", Assert.Single(item.OcorrenciasPagamento).Codigo);
        Assert.Equal("Pix não efetivado", item.OcorrenciasPagamento[0].Descricao);
        Assert.Equal(linhas, LeitorRetorno240.EscreverLinhas(retorno));
    }

    [Fact]
    public void Retorno_RejeitaLinhaComTamanhoIncorreto()
    {
        var linhas = Retorno(237, 6, "08").ToList();
        linhas[2] = linhas[2][..^1];
        var ex = Assert.Throws<TamanhoRegistroException>(
            () => LeitorRetorno240.Ler(EscritorDeArquivo.Montar(linhas), Bancos.Bradesco));
        Assert.Equal(240, ex.Esperado);
        Assert.Equal(239, ex.Encontrado);
        Assert.Equal(3, ex.Linha);
    }
}
