namespace Cnab.Tests;

using Cnab.Arquivo;
using Cnab.Layouts.Cnab240;
using Cnab.Layouts.Cnab240.Registros;
using Xunit;

/// <summary>
/// Verifica as posições dos layouts FEBRABAN V10.9 contra os valores
/// publicados no manual (docs/layouts.md) e propriedades de round-trip.
/// </summary>
public class Cnab240LayoutTests
{
    private static readonly DateOnly Data = new(2026, 11, 20);

    [Fact]
    public void HeaderArquivo_PosicoesConferemComManual()
    {
        var linha = RegistroCnab<HeaderArquivo240>.Escrever(new HeaderArquivo240
        {
            Banco = 237,
            Lote = 0,
            TipoRegistro = 0,
            TipoInscricaoEmpresa = 2,
            NumeroInscricaoEmpresa = "00000000000191",
            Convenio = "CONV".PadRight(20),
            Agencia = 1234,
            DvAgencia = "5",
            Conta = 98765432,
            DvConta = "6",
            DvAgenciaConta = "7",
            NomeEmpresa = "EMPRESA",
            NomeBanco = "BANCO",
            CodigoRemessaRetorno = 1,
            DataGeracao = Data,
            HoraGeracao = 101530,
            NumeroSequencialArquivo = 42,
            VersaoLayout = 103,
            Densidade = 1600,
        });

        Assert.Equal(240, linha.Length);
        Assert.Equal("237", linha.Substring(0, 3));
        Assert.Equal("0000", linha.Substring(3, 4));
        Assert.Equal("0", linha.Substring(7, 1));
        Assert.Equal(new string(' ', 9), linha.Substring(8, 9));
        Assert.Equal("2", linha.Substring(17, 1));
        Assert.Equal("00000000000191", linha.Substring(18, 14));
        Assert.Equal("CONV", linha.Substring(32, 4).TrimEnd());
        Assert.Equal("01234", linha.Substring(52, 5));
        Assert.Equal("5", linha.Substring(57, 1));
        Assert.Equal("000098765432", linha.Substring(58, 12));
        Assert.Equal("6", linha.Substring(70, 1));
        Assert.Equal("7", linha.Substring(71, 1));
        Assert.Equal("EMPRESA", linha.Substring(72, 7).TrimEnd());
        Assert.Equal("BANCO", linha.Substring(102, 5).TrimEnd());
        Assert.Equal("1", linha.Substring(142, 1));
        Assert.Equal("20112026", linha.Substring(143, 8));
        Assert.Equal("101530", linha.Substring(151, 6));
        Assert.Equal("000042", linha.Substring(157, 6));
        Assert.Equal("103", linha.Substring(163, 3));
        Assert.Equal("01600", linha.Substring(166, 5));
    }

    [Fact]
    public void HeaderLoteCobranca_PosicoesConferemComManual()
    {
        var linha = RegistroCnab<HeaderLoteCobranca240>.Escrever(new HeaderLoteCobranca240
        {
            Banco = 1,
            Lote = 1,
            TipoRegistro = 1,
            TipoOperacao = "R",
            TipoServico = 1,
            VersaoLayoutLote = 60,
            TipoInscricaoEmpresa = 2,
            NumeroInscricaoEmpresa = "000000000000191",
            Convenio = "1234".PadRight(20),
            Agencia = 1234,
            Conta = 133,
            NomeEmpresa = "EMPRESA",
            NumeroRemessaRetorno = 7,
            DataGravacao = Data,
        });

        Assert.Equal("R", linha.Substring(8, 1));
        Assert.Equal("01", linha.Substring(9, 2));
        Assert.Equal("060", linha.Substring(13, 3));
        Assert.Equal("000000000000191", linha.Substring(18, 15));
        Assert.Equal("1234", linha.Substring(33, 4).TrimEnd());
        Assert.Equal("01234", linha.Substring(53, 5));
        Assert.Equal("000000000133", linha.Substring(59, 12));
        Assert.Equal("EMPRESA", linha.Substring(73, 7).TrimEnd());
        Assert.Equal("00000007", linha.Substring(183, 8));
        Assert.Equal("20112026", linha.Substring(191, 8));
    }

    [Fact]
    public void HeaderLotePagamento_PosicoesConferemComManual()
    {
        var linha = RegistroCnab<HeaderLotePagamento240>.Escrever(new HeaderLotePagamento240
        {
            Banco = 756,
            Lote = 1,
            TipoRegistro = 1,
            TipoOperacao = "C",
            TipoServico = 20,
            FormaLancamento = 45,
            VersaoLayoutLote = 46,
            TipoInscricaoEmpresa = 2,
            NumeroInscricaoEmpresa = "00000000000191",
            Convenio = "CONV".PadRight(20),
            Agencia = 1234,
            Conta = 133,
            NomeEmpresa = "EMPRESA",
            Logradouro = "RUA A",
            Cidade = "SAO PAULO",
            Cep = 12345,
            Estado = "SP",
        });

        Assert.Equal("C", linha.Substring(8, 1));
        Assert.Equal("20", linha.Substring(9, 2));
        Assert.Equal("45", linha.Substring(11, 2));
        Assert.Equal("046", linha.Substring(13, 3));
        Assert.Equal("00000000000191", linha.Substring(18, 14));
        Assert.Equal("CONV", linha.Substring(32, 4).TrimEnd());
        Assert.Equal("RUA A", linha.Substring(142, 5).TrimEnd());
        Assert.Equal("SAO PAULO", linha.Substring(192, 9).TrimEnd());
        Assert.Equal("12345", linha.Substring(212, 5));
        Assert.Equal("SP", linha.Substring(220, 2));
        Assert.Equal("          ", linha.Substring(224, 10));
    }

    [Fact]
    public void SegmentoP_PosicoesConferemComManual()
    {
        var linha = RegistroCnab<SegmentoP240>.Escrever(new SegmentoP240
        {
            Banco = 1,
            Lote = 1,
            TipoRegistro = 3,
            Sequencial = 1,
            CodigoMovimento = 1,
            Agencia = 1234,
            Conta = 133,
            NossoNumero = "123412345671",
            Carteira = 1,
            NumeroDocumento = "DOC1",
            DataVencimento = new DateOnly(2026, 12, 20),
            Valor = 1500.50m,
            DataEmissao = Data,
            CodigoMoeda = 9,
        });

        Assert.Equal("3", linha.Substring(7, 1));
        Assert.Equal("P", linha.Substring(13, 1));
        Assert.Equal("00001", linha.Substring(8, 5));
        Assert.Equal("01", linha.Substring(15, 2));
        Assert.Equal("123412345671", linha.Substring(37, 12));
        Assert.Equal("1", linha.Substring(57, 1));
        Assert.Equal("20122026", linha.Substring(77, 8));
        Assert.Equal("000000000150050", linha.Substring(85, 15));
        Assert.Equal("20112026", linha.Substring(109, 8));
        Assert.Equal("09", linha.Substring(227, 2));
        Assert.Equal(240, linha.Length);
    }

    [Fact]
    public void SegmentosQRTU_PosicoesConferemComManual()
    {
        var q = RegistroCnab<SegmentoQ240>.Escrever(new SegmentoQ240
        {
            TipoInscricaoPagador = 1,
            NumeroInscricaoPagador = "000011144477735",
            NomePagador = "PAGADOR",
            CepPagador = 70000,
            SufixoCep = 0,
            UfPagador = "DF",
        });
        Assert.Equal("Q", q.Substring(13, 1));
        Assert.Equal("000011144477735", q.Substring(18, 15));
        Assert.Equal("PAGADOR", q.Substring(33, 7).TrimEnd());
        Assert.Equal("70000", q.Substring(128, 5));
        Assert.Equal("DF", q.Substring(151, 2));

        var r = RegistroCnab<SegmentoR240>.Escrever(new SegmentoR240
        {
            CodigoMulta = "2",
            DataMulta = Data,
            ValorMulta = 10.25m,
            Mensagem3 = "M3",
        });
        Assert.Equal("R", r.Substring(13, 1));
        Assert.Equal("2", r.Substring(65, 1));
        Assert.Equal("20112026", r.Substring(66, 8));
        Assert.Equal("000000000001025", r.Substring(74, 15));
        Assert.Equal("M3", r.Substring(99, 2).TrimEnd());

        var t = RegistroCnab<SegmentoT240>.Escrever(new SegmentoT240
        {
            CodigoMovimento = 6,
            NumeroDocumento = "DOC1",
            Valor = 100m,
            ValorTarifa = 2.5m,
            MotivoOcorrencia = "0402",
        });
        Assert.Equal("T", t.Substring(13, 1));
        Assert.Equal("06", t.Substring(15, 2));
        Assert.Equal("000000000010000", t.Substring(81, 15));
        Assert.Equal("000000000000250", t.Substring(198, 15));
        Assert.Equal("0402", t.Substring(213, 4));

        var u = RegistroCnab<SegmentoU240>.Escrever(new SegmentoU240
        {
            CodigoMovimento = 6,
            ValorPago = 98m,
            DataOcorrencia = Data,
            CodigoOcorrenciaPagador = "0101",
        });
        Assert.Equal("U", u.Substring(13, 1));
        Assert.Equal("000000000009800", u.Substring(77, 15));
        Assert.Equal("20112026", u.Substring(137, 8));
        Assert.Equal("0101", u.Substring(153, 4));
    }

    [Fact]
    public void SegmentosA_B_J_J52_O_PosicoesConferemComManual()
    {
        var a = RegistroCnab<SegmentoA240>.Escrever(new SegmentoA240
        {
            CamaraCentralizadora = 18,
            BancoFavorecido = 237,
            AgenciaFavorecido = 1234,
            ContaFavorecido = 1234567,
            NomeFavorecido = "FORNECEDOR",
            DataPagamento = Data,
            ValorPagamento = 250.75m,
        });
        Assert.Equal("A", a.Substring(13, 1));
        Assert.Equal("018", a.Substring(17, 3));
        Assert.Equal("237", a.Substring(20, 3));
        Assert.Equal("BRL", a.Substring(101, 3));
        Assert.Equal("000000000025075", a.Substring(119, 15));

        var b = RegistroCnab<SegmentoB240>.Escrever(new SegmentoB240
        {
            TipoInscricaoFavorecido = 2,
            NumeroInscricaoFavorecido = "00000000000191",
        });
        Assert.Equal("B", b.Substring(13, 1));
        Assert.Equal("00000000000191", b.Substring(18, 14));

        var bEnd = RegistroCnab<SegmentoBEndereco240>.Escrever(new SegmentoBEndereco240
        {
            Logradouro = "RUA A",
            Cep = 70000,
            Uf = "DF",
        });
        Assert.Equal("RUA A", bEnd.Substring(32, 5).TrimEnd());
        Assert.Equal("70000", bEnd.Substring(117, 5));
        Assert.Equal("DF", bEnd.Substring(125, 2));

        var j = RegistroCnab<SegmentoJ240>.Escrever(new SegmentoJ240
        {
            CodigoBarras = new string('1', 44),
            DataPagamento = Data,
            ValorPagamento = 10m,
            CodigoMoeda = 9,
        });
        Assert.Equal("J", j.Substring(13, 1));
        Assert.Equal(new string('1', 44), j.Substring(17, 44));
        Assert.Equal("09", j.Substring(222, 2));

        var j52 = RegistroCnab<SegmentoJ52_240>.Escrever(new SegmentoJ52_240());
        Assert.Equal("J", j52.Substring(13, 1));
        Assert.Equal("52", j52.Substring(17, 2));

        var o = RegistroCnab<SegmentoO240>.Escrever(new SegmentoO240
        {
            CodigoBarras = new string('2', 44),
            DataPagamento = Data,
            ValorPagamento = 33.33m,
        });
        Assert.Equal("O", o.Substring(13, 1));
        Assert.Equal(new string('2', 44), o.Substring(17, 44));
        Assert.Equal("000000000003333", o.Substring(107, 15));
    }

    [Fact]
    public void Trailers_PosicoesConferemComManual()
    {
        var cobranca = RegistroCnab<TrailerLoteCobranca240>.Escrever(new TrailerLoteCobranca240
        {
            Banco = 1,
            Lote = 1,
            TipoRegistro = 5,
            QuantidadeSimples = 2,
            ValorSimples = 1500.50m,
        });
        Assert.Equal("5", cobranca.Substring(7, 1));
        Assert.Equal("000002", cobranca.Substring(23, 6));
        Assert.Equal("00000000000150050", cobranca.Substring(29, 17));

        var pagamento = RegistroCnab<TrailerLotePagamento240>.Escrever(new TrailerLotePagamento240
        {
            Banco = 756,
            Lote = 1,
            TipoRegistro = 5,
            QuantidadeRegistros = 5,
            SomatoriaValores = 250.75m,
        });
        Assert.Equal("000005", pagamento.Substring(17, 6));
        Assert.Equal("000000000000025075", pagamento.Substring(23, 18));

        var arquivo = RegistroCnab<TrailerArquivo240>.Escrever(new TrailerArquivo240
        {
            Banco = 1,
            Lote = 9999,
            TipoRegistro = 9,
            QuantidadeLotes = 3,
            QuantidadeRegistros = 15,
        });
        Assert.Equal("9999", arquivo.Substring(3, 4));
        Assert.Equal("9", arquivo.Substring(7, 1));
        Assert.Equal("000003", arquivo.Substring(17, 6));
        Assert.Equal("000015", arquivo.Substring(23, 6));
    }

    [Fact]
    public void Santander_HeaderArquivoUsaLayoutProprio()
    {
        var linha = RegistroCnab<HeaderArquivoSantander240>.Escrever(new HeaderArquivoSantander240
        {
            Banco = 33,
            TipoInscricaoEmpresa = 2,
            NumeroInscricaoEmpresa = "000000000000191",
            CodigoTransmissao = "000000000000001",
            NomeEmpresa = "EMPRESA",
            NomeBanco = "Banco Santander",
            CodigoRemessaRetorno = 1,
            DataGeracao = Data,
        });

        Assert.Equal("033", linha.Substring(0, 3));
        Assert.Equal(new string(' ', 8), linha.Substring(8, 8));
        Assert.Equal("2", linha.Substring(16, 1));
        Assert.Equal("000000000000191", linha.Substring(17, 15));
        Assert.Equal("000000000000001", linha.Substring(32, 15));
        Assert.Equal("BANCO SANTANDER", linha.Substring(102, 15).TrimEnd());
        Assert.Equal("040", linha.Substring(163, 3));
    }

    [Fact]
    public void TodasAsLinhas_Tem240PosicoesEArquivoCrLf()
    {
        var remessa = Cnab240Fixtures.RemessaCobranca(Cnab.Modelo.Bancos.BancoDoBrasil);
        var arquivo = GeradorRemessa240.Gerar(remessa);
        var linhas = arquivo.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        Assert.All(linhas, linha => Assert.Equal(240, linha.Length));
        Assert.EndsWith("\r\n", arquivo, StringComparison.Ordinal);
    }

    [Fact]
    public void EscreverELer_RegistrosSaoIdenticos()
    {
        var p = RegistroCnab<SegmentoP240>.Escrever(new SegmentoP240
        {
            Banco = 104,
            Lote = 1,
            TipoRegistro = 3,
            Sequencial = 1,
            CodigoMovimento = 1,
            Agencia = 123,
            Conta = 456,
            NossoNumero = "1400000000000197",
            Carteira = 1,
            NumeroDocumento = "DOC",
            Valor = 530.44m,
            DataEmissao = Data,
        });
        Assert.Equal(p, RegistroCnab<SegmentoP240>.Escrever(RegistroCnab<SegmentoP240>.Ler(p, 3)));
    }
}
