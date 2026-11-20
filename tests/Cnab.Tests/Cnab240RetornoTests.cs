namespace Cnab.Tests;

using Cnab.Arquivo;
using Cnab.Layouts.Cnab240;
using Cnab.Layouts.Cnab240.Registros;
using Xunit;

/// <summary>Testes de leitura de retorno, tabelas de ocorrência e round-trip.</summary>
public class Cnab240RetornoTests
{
    private static IReadOnlyList<string> RetornoBancoDoBrasil()
    {
        var data = new DateOnly(2026, 11, 20);
        var headerArquivo = RegistroCnab<HeaderArquivo240>.Escrever(new HeaderArquivo240
        {
            Banco = 1,
            TipoRegistro = 0,
            TipoInscricaoEmpresa = 2,
            NumeroInscricaoEmpresa = "00000000000191",
            NomeEmpresa = "EMPRESA TESTE LTDA",
            NomeBanco = "BANCO DO BRASIL S.A.",
            CodigoRemessaRetorno = 2,
            DataGeracao = data,
            HoraGeracao = 101530,
            NumeroSequencialArquivo = 3,
            VersaoLayout = 83,
            Densidade = 1600,
        });
        var headerLote = RegistroCnab<HeaderLoteCobranca240>.Escrever(new HeaderLoteCobranca240
        {
            Banco = 1,
            Lote = 1,
            TipoRegistro = 1,
            TipoOperacao = "T",
            TipoServico = 1,
            VersaoLayoutLote = 42,
            TipoInscricaoEmpresa = 2,
            NumeroInscricaoEmpresa = "00000000000191",
            NomeEmpresa = "EMPRESA TESTE LTDA",
            NumeroRemessaRetorno = 1,
            DataGravacao = data,
        });
        var t = RegistroCnab<SegmentoT240>.Escrever(new SegmentoT240
        {
            Banco = 1,
            Lote = 1,
            TipoRegistro = 3,
            Sequencial = 1,
            CodigoMovimento = 6,
            NossoNumero = "123412345671",
            NumeroDocumento = "NF000001",
            Valor = 1500.50m,
            ValorTarifa = 2.50m,
            MotivoOcorrencia = "61",
        });
        var u = RegistroCnab<SegmentoU240>.Escrever(new SegmentoU240
        {
            Banco = 1,
            Lote = 1,
            TipoRegistro = 3,
            Sequencial = 2,
            CodigoMovimento = 6,
            ValorPago = 1500.50m,
            ValorLiquido = 1498.00m,
            DataOcorrencia = data,
            DataCredito = data,
            CodigoOcorrenciaPagador = "0101",
        });
        var trailerLote = RegistroCnab<TrailerLoteCobranca240>.Escrever(new TrailerLoteCobranca240
        {
            Banco = 1,
            Lote = 1,
            TipoRegistro = 5,
            QuantidadeSimples = 1,
            ValorSimples = 1500.50m,
        });
        var trailerArquivo = RegistroCnab<TrailerArquivo240>.Escrever(new TrailerArquivo240
        {
            Banco = 1,
            Lote = 9999,
            TipoRegistro = 9,
            QuantidadeLotes = 1,
            QuantidadeRegistros = 7,
        });
        return [headerArquivo, headerLote, t, u, trailerLote, trailerArquivo];
    }

    [Fact]
    public void Retorno_ParseiaOcorrenciaLiquidacaoEValores()
    {
        var conteudo = EscritorDeArquivo.Montar(RetornoBancoDoBrasil());
        var retorno = LeitorRetorno240.Ler(conteudo, Cnab.Modelo.Bancos.BancoDoBrasil);

        Assert.Equal(Cnab.Modelo.Bancos.BancoDoBrasil.Codigo, retorno.Banco.Codigo);
        Assert.Single(retorno.Lotes);
        var lote = retorno.Lotes[0];
        Assert.NotNull(lote.HeaderCobranca);
        Assert.Equal("T", lote.HeaderCobranca.TipoOperacao);
        Assert.Single(lote.Itens);

        var item = lote.Itens[0];
        Assert.NotNull(item.Titulo);
        Assert.NotNull(item.Valores);
        Assert.Equal("123412345671", item.Titulo.NossoNumero.TrimEnd());
        Assert.Equal(1500.50m, item.Titulo.Valor);
        Assert.Equal(2.50m, item.Titulo.ValorTarifa);
        Assert.Equal(FamiliaOcorrencia.Liquidacao, item.Ocorrencia!.Familia);
        Assert.Equal("06", item.Ocorrencia.Codigo);
        Assert.Single(item.Motivos);
        Assert.Equal("Alteração da agência cobradora/DV inválida", item.Motivos[0].Descricao);
        Assert.Equal(1500.50m, item.Valores.ValorPago);
        Assert.Equal(1498.00m, item.Valores.ValorLiquido);
        Assert.Equal(["0101"], item.OcorrenciasPagador);
    }

    [Fact]
    public void Retorno_RoundTripByteExato()
    {
        var linhas = RetornoBancoDoBrasil();
        var conteudo = EscritorDeArquivo.Montar(linhas);
        var retorno = LeitorRetorno240.Ler(conteudo, Cnab.Modelo.Bancos.BancoDoBrasil);
        Assert.Equal(linhas, LeitorRetorno240.EscreverLinhas(retorno));
    }

    [Fact]
    public void Retorno_RejeitaLinhaComTamanhoIncorreto()
    {
        var linhas = RetornoBancoDoBrasil().ToList();
        linhas[2] = linhas[2][..^1];
        var ex = Assert.Throws<TamanhoRegistroException>(
            () => LeitorRetorno240.Ler(EscritorDeArquivo.Montar(linhas), Cnab.Modelo.Bancos.BancoDoBrasil));
        Assert.Equal(240, ex.Esperado);
        Assert.Equal(239, ex.Encontrado);
        Assert.Equal(3, ex.Linha);
    }

    [Fact]
    public void TabelaCobranca_ClassificaFamilias()
    {
        Assert.Equal(FamiliaOcorrencia.EntradaConfirmada, TabelasOcorrencias.ObterCobranca("02").Familia);
        Assert.Equal(FamiliaOcorrencia.EntradaRejeitada, TabelasOcorrencias.ObterCobranca(3).Familia);
        Assert.Equal(FamiliaOcorrencia.Liquidacao, TabelasOcorrencias.ObterCobranca("06").Familia);
        Assert.Equal(FamiliaOcorrencia.Baixa, TabelasOcorrencias.ObterCobranca("09").Familia);
        Assert.Equal(FamiliaOcorrencia.Tarifa, TabelasOcorrencias.ObterCobranca("28").Familia);
        Assert.Equal(FamiliaOcorrencia.Rejeicao, TabelasOcorrencias.ObterCobranca("26").Familia);
        Assert.Equal(FamiliaOcorrencia.Cancelamento, TabelasOcorrencias.ObterCobranca("20").Familia);
        Assert.Contains(TabelasOcorrencias.TodasCobranca(), o => o.Codigo == "06");
    }

    [Fact]
    public void TabelaCobranca_NaoInventaCodigo10()
    {
        Assert.Throws<ArgumentException>(() => TabelasOcorrencias.ObterCobranca("10"));
        Assert.Throws<ArgumentException>(() => TabelasOcorrencias.ObterCobranca("ZZ"));
    }

    [Fact]
    public void TabelaBanco_SobrepoeCodigo52DoBancoDoBrasil()
    {
        var bb = TabelasOcorrencias.Obter(Cnab.Modelo.Bancos.BancoDoBrasil, "52");
        Assert.Equal("Registro de título já liquidado — carteira 17", bb.Descricao);
        Assert.Equal(FamiliaOcorrencia.Rejeicao, bb.Familia);
        var dda = TabelasOcorrencias.Obter(Cnab.Modelo.Bancos.BancoDoBrasil, "A4");
        Assert.Equal("Sacado DDA", dda.Descricao);
    }

    [Fact]
    public void TabelaPagamento_EObterMotivoRejeicao()
    {
        var ok = TabelasOcorrencias.ObterPagamento("00");
        Assert.Equal("Crédito ou débito efetivado", ok.Descricao);
        Assert.Equal(FamiliaOcorrencia.Liquidacao, ok.Familia);
        Assert.Equal("CEP inválido", TabelasOcorrencias.ObterMotivoRejeicao("48"));
        Assert.Equal("Prazo para protesto inválido", TabelasOcorrencias.ObterMotivoRejeicao("38"));
        Assert.Throws<ArgumentException>(() => TabelasOcorrencias.ObterMotivoRejeicao("YY"));
    }
}
