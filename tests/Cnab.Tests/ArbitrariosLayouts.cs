namespace Cnab.Tests;

using Cnab.Layouts.Cnab240;
using Cnab.Layouts.Cnab400;
using Cnab.Modelo;
using FsCheck;
using FsCheck.Fluent;

/// <summary>Geradores FsCheck de modelos válidos para todos os bancos/layouts.</summary>
public static class ArbitrariosLayouts
{
    /// <summary>Bancos com CNAB 240 implementado (SPEC + PerfilBanco240).</summary>
    public static readonly int[] Bancos240 = [1, 33, 77, 104, 208, 237, 341, 748, 756];

    /// <summary>Bancos com CNAB 400 de cobrança implementado.</summary>
    public static readonly int[] Bancos400 = [77, 237, 341, 748];

    /// <summary>Dados de um retorno 240 sintético.</summary>
    /// <param name="CodigoBanco">Código do banco.</param>
    /// <param name="QuantidadeItens">Quantidade de pares T/U.</param>
    /// <param name="Valor">Valor de cada título.</param>
    public sealed record DadosRetorno240(int CodigoBanco, int QuantidadeItens, decimal Valor);

    /// <summary>Dados de um retorno 400 sintético.</summary>
    /// <param name="CodigoBanco">Código do banco.</param>
    /// <param name="QuantidadeItens">Quantidade de detalhes.</param>
    /// <param name="Centavos">Valor pago em centavos.</param>
    public sealed record DadosRetorno400(int CodigoBanco, int QuantidadeItens, int Centavos);

    /// <summary>Gera remessas 240 de cobrança válidas para qualquer banco suportado.</summary>
    /// <returns>Arbitrário de <see cref="Remessa240"/>.</returns>
    public static Arbitrary<Remessa240> Remessas240() =>
        (from codigo in Gen.Elements(Bancos240)
         from quantidadeLotes in Gen.Choose(1, 2)
         from quantidadeTitulos in Gen.Choose(1, 3)
         from centavos in Gen.Choose(1, 9_999_999)
         from dia in Gen.Choose(1, 27)
         from mes in Gen.Choose(1, 12)
         from numero in Gen.Choose(0, 99_999_999)
         select CriarRemessa240(codigo, quantidadeLotes, quantidadeTitulos, centavos, dia, mes, numero))
        .ToArbitrary();

    /// <summary>Gera remessas 240 de pagamentos válidas para qualquer banco suportado.</summary>
    /// <returns>Arbitrário de <see cref="Remessa240"/>.</returns>
    public static Arbitrary<Remessa240> Remessas240Pagamento() =>
        (from codigo in Gen.Elements(Bancos240)
         from centavos in Gen.Choose(1, 9_999_999)
         from dia in Gen.Choose(1, 27)
         from mes in Gen.Choose(1, 12)
         select CriarRemessaPagamento(codigo, centavos, new DateOnly(2026, mes, dia)))
        .ToArbitrary();

    /// <summary>Gera remessas 400 de cobrança válidas para os quatro bancos suportados.</summary>
    /// <returns>Arbitrário de <see cref="Remessa400"/>.</returns>
    public static Arbitrary<Remessa400> Remessas400() =>
        (from codigo in Gen.Elements(Bancos400)
         from quantidade in Gen.Choose(1, 3)
         from centavos in Gen.Choose(1, 9_999_999)
         from dia in Gen.Choose(1, 27)
         from mes in Gen.Choose(1, 12)
         from numero in Gen.Choose(0, 99_999_999)
         select CriarRemessa400(codigo, quantidade, centavos, dia, mes, numero))
        .ToArbitrary();

    /// <summary>Gera retornos 240 sintéticos válidos.</summary>
    /// <returns>Arbitrário de dados de retorno.</returns>
    public static Arbitrary<DadosRetorno240> Retornos240() =>
        (from codigo in Gen.Elements(Bancos240)
         from quantidade in Gen.Choose(1, 3)
         from centavos in Gen.Choose(1, 9_999_999)
         select new DadosRetorno240(codigo, quantidade, centavos / 100m))
        .ToArbitrary();

    /// <summary>Gera retornos 400 sintéticos válidos.</summary>
    /// <returns>Arbitrário de dados de retorno.</returns>
    public static Arbitrary<DadosRetorno400> Retornos400() =>
        (from codigo in Gen.Elements(Bancos400)
         from quantidade in Gen.Choose(1, 3)
         from centavos in Gen.Choose(1, 9_999_999)
         select new DadosRetorno400(codigo, quantidade, centavos))
        .ToArbitrary();

    private static Remessa240 CriarRemessa240(
        int codigo,
        int quantidadeLotes,
        int quantidadeTitulos,
        int centavos,
        int dia,
        int mes,
        int numero)
    {
        var banco = Bancos.Obter(codigo);
        var cedente = Milestone3Fixtures.Cedente(banco);
        var emissao = new DateOnly(2026, mes, dia);
        var lotes = new List<LoteCobranca>();
        for (var lote = 0; lote < quantidadeLotes; lote++)
        {
            var titulos = new List<Titulo>(quantidadeTitulos);
            for (var titulo = 0; titulo < quantidadeTitulos; titulo++)
            {
                titulos.Add(CriarTitulo(codigo, numero + titulo, centavos, emissao));
            }

            lotes.Add(new LoteCobranca
            {
                Cedente = cedente,
                Numero = lote + 1,
                Titulos = titulos,
            });
        }

        return new Remessa240
        {
            Banco = banco,
            Cedente = cedente,
            NumeroSequencialArquivo = 1,
            DataGeracao = emissao,
            HoraGeracao = new TimeOnly(10, 15, 30),
            Convenio = "1234",
            CodigoProduto = "009",
            CodigoBeneficiario = "00623",
            LotesCobranca = lotes,
        };
    }

    private static Remessa240 CriarRemessaPagamento(int codigo, int centavos, DateOnly data)
    {
        var banco = Bancos.Obter(codigo);
        var cedente = Milestone3Fixtures.Cedente(banco);
        return new Remessa240
        {
            Banco = banco,
            Cedente = cedente,
            NumeroSequencialArquivo = 1,
            DataGeracao = data,
            HoraGeracao = new TimeOnly(10, 15, 30),
            Convenio = "1234",
            LotesPagamento =
            [
                new LotePagamento
                {
                    Pagadora = cedente,
                    Numero = 1,
                    Pagamentos =
                    [
                        new Pagamento
                        {
                            Tipo = TipoPagamento.Ted,
                            BancoDestino = Bancos.Bradesco,
                            AgenciaDestino = "1234",
                            ContaDestino = "1234567",
                            NomeFavorecido = "FORNECEDOR TESTE LTDA",
                            DocumentoFavorecido = new DocumentoFiscal(DocumentoFiscal.CnpjExemplo),
                            Valor = centavos / 100m,
                            Data = data,
                            NumeroDocumento = "PAG001",
                        },
                    ],
                },
            ],
        };
    }

    private static Remessa400 CriarRemessa400(
        int codigo,
        int quantidade,
        int centavos,
        int dia,
        int mes,
        int numero)
    {
        var banco = Bancos.Obter(codigo);
        var emissao = new DateOnly(2026, mes, dia);
        var titulos = new List<Titulo>(quantidade);
        for (var i = 0; i < quantidade; i++)
        {
            titulos.Add(CriarTitulo(codigo, numero + i, centavos, emissao));
        }

        return Milestone3Fixtures.Remessa400(banco) with
        {
            DataGeracao = emissao,
            Titulos = titulos,
        };
    }

    private static Titulo CriarTitulo(int codigo, int numero, int centavos, DateOnly emissao) => new()
    {
        NossoNumero = numero.ToString("D8", System.Globalization.CultureInfo.InvariantCulture),
        NumeroDocumento = "NF" + numero.ToString("D6", System.Globalization.CultureInfo.InvariantCulture),
        Especie = EspecieDocumento.DuplicataMercantilIndicacao,
        DataEmissao = emissao,
        DataVencimento = emissao.AddDays(10),
        Valor = centavos / 100m,
        Carteira = codigo switch
        {
            341 => "109",
            237 => "19",
            748 => "1",
            77 => "112",
            _ => "1",
        },
        Pagador = new Pagador
        {
            Nome = "PAGADOR TESTE LTDA",
            Documento = new DocumentoFiscal(DocumentoFiscal.CpfExemplo),
            Endereco = new Endereco
            {
                Logradouro = "RUA DAS FLORES",
                Bairro = "CENTRO",
                Cidade = "BRASILIA",
                Uf = "DF",
                Cep = "70000000",
            },
        },
    };
}
