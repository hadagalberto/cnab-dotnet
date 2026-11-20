namespace Cnab.Tests;

using Cnab.Modelo;
using FsCheck;
using FsCheck.Fluent;
using Xunit;

public class ModeloTests
{
    // --------------------------- documento fiscal (CPF/CNPJ)

    [Theory]
    [InlineData("11144477735", false)] // CPF de exemplo ( Receita Federal: módulo 11)
    [InlineData("00000000000191", true)] // CNPJ 00.000.000/0001-91 (Banco do Brasil)
    [InlineData("111.444.777-35", false)]
    [InlineData("00.000.000/0001-91", true)]
    public void DocumentoFiscal_AceitaCpfCnpjComOuSemPontuacao(string digitos, bool ehCnpj)
    {
        var documento = new DocumentoFiscal(digitos);
        Assert.Equal(ehCnpj, documento.EhCnpj);
        Assert.Equal(new string([.. digitos.Where(char.IsDigit)]), documento.Digitos);
    }

    [Theory]
    [InlineData("11144477734")] // DV final errado
    [InlineData("00000000000190")]
    [InlineData("11111111111")] // todos iguais
    [InlineData("12345678901")] // DV não confere
    [InlineData("1234")]
    [InlineData("")]
    public void DocumentoFiscal_RejeitaDocumentosInvalidos(string digitos)
    {
        Assert.False(DocumentoFiscal.EhValido(digitos));
        Assert.Throws<ArgumentException>(() => new DocumentoFiscal(digitos));
    }

    [Fact]
    public void DocumentoFiscal_TipoInscricaoSegueTabelaFebbraban()
    {
        Assert.Equal(1, new DocumentoFiscal("11144477735").TipoInscricao);
        Assert.Equal(2, new DocumentoFiscal("00000000000191").TipoInscricao);
    }

    [Fact]
    public void DocumentoFiscal_FormataComPontuacao()
    {
        Assert.Equal("111.444.777-35", new DocumentoFiscal("11144477735").ToString());
        Assert.Equal("00.000.000/0001-91", new DocumentoFiscal("00000000000191").ToString());
    }

    // --------------------------- chave Pix

    [Fact]
    public void ChavePix_CpfCnpjCelularEmailEvp()
    {
        var cpf = ChavePix.Criar("11144477735");
        Assert.Equal(ChavePix.TipoChave.Cpf, cpf.Tipo);

        var cnpj = ChavePix.Criar("00000000000191");
        Assert.Equal(ChavePix.TipoChave.Cnpj, cnpj.Tipo);

        var celular = ChavePix.Criar("+5511999999999");
        Assert.Equal(ChavePix.TipoChave.Celular, celular.Tipo);

        var email = ChavePix.Criar("Fulano@Exemplo.COM");
        Assert.Equal(ChavePix.TipoChave.Email, email.Tipo);
        Assert.Equal("fulano@exemplo.com", email.Valor);

        var evp = ChavePix.Criar("123e4567-e89b-12d3-a456-426614174000");
        Assert.Equal(ChavePix.TipoChave.Aleatoria, evp.Tipo);
    }

    [Theory]
    [InlineData("11144477734")] // CPF inválido
    [InlineData("+551199999999")] // celular curto
    [InlineData("abc")] // nenhum formato
    [InlineData("123e4567-e89b-12d3-a456-42661417400z")] // evp com letra inválida
    public void ChavePix_RejeitaFormatosInvalidos(string valor)
    {
        Assert.Throws<ArgumentException>(() => ChavePix.Criar(valor));
    }

    // --------------------------- endereço / título / lote

    [Fact]
    public void Endereco_RejeitaUfCepInvalidos()
    {
        var endereco = new Endereco
        {
            Logradouro = "Rua A",
            Bairro = "Centro",
            Cidade = "Cidade",
            Uf = "sp",
            Cep = "1234567",
        };
        Assert.Throws<InvalidOperationException>(() => endereco.Validar());

        var cepCerto = endereco with { Uf = "SP", Cep = "12345678" };
        cepCerto.Validar();
    }

    private static Titulo CriarTituloValido() => new()
    {
        NossoNumero = "000000000028",
        NumeroDocumento = "123",
        Especie = EspecieDocumento.DuplicataMercantilIndicacao,
        Aceite = false,
        DataEmissao = new DateOnly(2026, 11, 20),
        DataVencimento = new DateOnly(2026, 12, 20),
        Valor = 1500.50m,
        Pagador = new Pagador
        {
            Nome = "PAGADOR MODELO",
            Documento = new DocumentoFiscal("11144477735"),
        },
    };

    [Fact]
    public void Titulo_ValidoPassa()
    {
        CriarTituloValido().Validar();
    }

    [Fact]
    public void Titulo_RejeitaRegrasVioladas()
    {
        Assert.Throws<InvalidOperationException>(() => (CriarTituloValido() with { Valor = 0 }).Validar());
        Assert.Throws<InvalidOperationException>(() => (CriarTituloValido() with { Valor = -1 }).Validar());

        var vencidoAntes = CriarTituloValido() with
        {
            DataEmissao = new DateOnly(2026, 12, 20),
            DataVencimento = new DateOnly(2026, 11, 20),
        };
        Assert.Throws<InvalidOperationException>(() => vencidoAntes.Validar());

        Assert.Throws<InvalidOperationException>(() => (CriarTituloValido() with { Abatimento = 2000m }).Validar());

        var protestoCurto = CriarTituloValido() with
        {
            Protesto = new Protesto { Tipo = TipoProtesto.ProtestarDiasCorridos, Dias = 1 },
        };
        Assert.Throws<InvalidOperationException>(() => protestoCurto.Validar());

        var protestoValido = CriarTituloValido() with
        {
            Protesto = new Protesto { Tipo = TipoProtesto.ProtestarDiasCorridos, Dias = 5 },
        };
        protestoValido.Validar();
    }

    [Fact]
    public void Titulo_SemVencimentoEAceito()
    {
        var titulo = CriarTituloValido() with { DataVencimento = null };
        titulo.Validar();
    }

    [Fact]
    public void ContaBancaria_RejeitaConteudoNaoNumerico()
    {
        var conta = new ContaBancaria
        {
            Banco = Bancos.Bradesco,
            Agencia = "12A4",
            Conta = "12345",
        };
        Assert.Throws<InvalidOperationException>(() => conta.Validar());

        (conta with { Agencia = "1234" }).Validar();
    }

    [Fact]
    public void LoteCobranca_TotaisSomamTitulosExatamente()
    {
        var lote = new LoteCobranca
        {
            Cedente = new Cedente
            {
                Nome = "EMPRESA MODELO LTDA",
                Documento = new DocumentoFiscal("00000000000191"),
                Conta = new ContaBancaria { Banco = Bancos.Bradesco, Agencia = "1234", Conta = "12345" },
            },
            Numero = 1,
            Titulos =
            [
                CriarTituloValido() with { Valor = 100.10m },
                CriarTituloValido() with { Valor = 200.20m },
                CriarTituloValido() with { Valor = 300.30m },
            ],
        };
        var totais = lote.CalcularTotais();
        Assert.Equal(3, totais.QuantidadeRegistros);
        Assert.Equal(600.60m, totais.SomaValores);
        Assert.Equal(5, CalculadoraDeTotais.QuantidadeRegistros(lote));
    }

    [Fact]
    public void LoteVazioERejeitado()
    {
        var lote = new LoteCobranca
        {
            Cedente = new Cedente
            {
                Nome = "EMPRESA",
                Documento = new DocumentoFiscal("00000000000191"),
                Conta = new ContaBancaria { Banco = Bancos.Bradesco, Agencia = "1", Conta = "1" },
            },
            Numero = 1,
            Titulos = [],
        };
        Assert.Throws<InvalidOperationException>(() => lote.CalcularTotais());
    }

    [Fact]
    public void CalculadoraDeTotais_ArquivoContabilizaHeaderETrailer()
    {
        var totais = CalculadoraDeTotais.CalcularTotaisArquivo([2 + 3, 2 + 2]);
        Assert.Equal(2, totais.QuantidadeLotes);
        Assert.Equal(2 + 5 + 4, totais.QuantidadeRegistros);
    }

    [Fact]
    public void CalculadoraDeTotais_SemLotesERejeitada()
    {
        Assert.Throws<ArgumentException>(() => CalculadoraDeTotais.CalcularTotaisArquivo([]));
    }

    [Fact]
    public void SomatorioDoLoteSempreConferem_Propriedade()
    {
        Prop.ForAll(ArbitrariosDeModelo.ValoresETamanhos(), (valoresQuantidade) =>
        {
            var (valores, quantidade) = valoresQuantidade;
            var titulos = valores.Take(quantidade)
                .Select(v => CriarTituloValido() with { Valor = v / 100m })
                .ToArray();
            var lote = new LoteCobranca
            {
                Cedente = new Cedente
                {
                    Nome = "EMPRESA",
                    Documento = new DocumentoFiscal("00000000000191"),
                    Conta = new ContaBancaria { Banco = Bancos.Bradesco, Agencia = "1", Conta = "1" },
                },
                Numero = 1,
                Titulos = titulos,
            };
            var totais = lote.CalcularTotais();
            return totais.QuantidadeRegistros == quantidade
                && totais.SomaValores == titulos.Sum(t => t.Valor);
        }).QuickCheckThrowOnFailure();
    }

    // --------------------------- pagamento

    [Fact]
    public void PagamentoPixPorChave_ExigeChave()
    {
        var pagamento = new Pagamento
        {
            Tipo = TipoPagamento.PixChave,
            NomeFavorecido = "FAVORECIDO",
            DocumentoFavorecido = new DocumentoFiscal("11144477735"),
            Valor = 10m,
            Data = new DateOnly(2026, 11, 20),
            NumeroDocumento = "1",
            Chave = ChavePix.Criar("11144477735"),
        };
        pagamento.Validar();

        Assert.Throws<InvalidOperationException>(
            () => (pagamento with { Chave = null }).Validar());
    }

    [Fact]
    public void PagamentoTed_ExigeBancoAgenciaConta()
    {
        var pagamento = new Pagamento
        {
            Tipo = TipoPagamento.Ted,
            NomeFavorecido = "FAVORECIDO",
            DocumentoFavorecido = new DocumentoFiscal("11144477735"),
            Valor = 10m,
            Data = new DateOnly(2026, 11, 20),
            NumeroDocumento = "1",
            BancoDestino = Bancos.Itau,
            AgenciaDestino = "1234",
            ContaDestino = "56789",
        };
        pagamento.Validar();

        Assert.Throws<InvalidOperationException>(
            () => (pagamento with { ContaDestino = null }).Validar());
    }

    [Fact]
    public void PagamentoBoleto_ExigeCodigoDeBarras()
    {
        var pagamento = new Pagamento
        {
            Tipo = TipoPagamento.BoletoCodigoBarras,
            NomeFavorecido = "FAVORECIDO",
            DocumentoFavorecido = new DocumentoFiscal("11144477735"),
            Valor = 10m,
            Data = new DateOnly(2026, 11, 20),
            NumeroDocumento = "1",
            CodigoDeBarras = "34196166700000123451101234567880057123457000",
        };
        pagamento.Validar();

        Assert.Throws<InvalidOperationException>(
            () => (pagamento with { CodigoDeBarras = null }).Validar());
    }

    [Fact]
    public void Pagamento_SemValorERejeitado()
    {
        Assert.Throws<InvalidOperationException>(
            () => new Pagamento
            {
                Tipo = TipoPagamento.Ted,
                NomeFavorecido = "F",
                DocumentoFavorecido = new DocumentoFiscal("11144477735"),
                Valor = 0m,
                Data = new DateOnly(2026, 11, 20),
                NumeroDocumento = "1",
                BancoDestino = Bancos.Itau,
                AgenciaDestino = "1",
                ContaDestino = "1",
            }.Validar());
    }

    private static class ArbitrariosDeModelo
    {
        public static Arbitrary<(decimal[], int)> ValoresETamanhos()
        {
            // Lista sempre com pelo menos um elemento (lote vazio é inválido).
            var gerador = Gen.ListOf(Gen.Choose(1, int.MaxValue))
                .Select(centavos => centavos.Prepend(1).ToList())
                .Select(centavos =>
                {
                    var valores = centavos.Select(c => c / 100m).ToArray();
                    return (valores, valores.Length);
                });
            return gerador.ToArbitrary();
        }
    }
}
