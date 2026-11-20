namespace Cnab.Tests;

using Cnab.Layouts.Cnab240;
using Cnab.Modelo;

/// <summary>Objetos de domínio reutilizados nos testes de CNAB 240.</summary>
internal static class Cnab240Fixtures
{
    internal static Cedente Cedente(Banco banco) => new()
    {
        Nome = "EMPRESA TESTE LTDA",
        Documento = new DocumentoFiscal(DocumentoFiscal.CnpjExemplo),
        Conta = new ContaBancaria
        {
            Banco = banco,
            Agencia = "0001",
            DvAgencia = "0",
            Conta = "00000133",
            DvConta = "6",
            DvAgenciaConta = " ",
        },
    };

    internal static Titulo Titulo(int codigoBanco = 1) => new()
    {
        NossoNumero = Milestone3Fixtures.NossoNumero240(codigoBanco, 2),
        NumeroDocumento = "NF000001",
        Especie = EspecieDocumento.DuplicataMercantilIndicacao,
        DataEmissao = new DateOnly(2026, 11, 20),
        DataVencimento = new DateOnly(2026, 12, 20),
        Valor = 1500.50m,
        Carteira = "1",
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

    internal static LoteCobranca LoteCobranca(Cedente cedente, int codigoBanco = 1) => new()
    {
        Cedente = cedente,
        Numero = 1,
        Titulos = [Titulo(codigoBanco)],
    };

    internal static Remessa240 RemessaCobranca(Banco banco, string convenio = "1234") => new()
    {
        Banco = banco,
        Cedente = Cedente(banco),
        NumeroSequencialArquivo = 1,
        DataGeracao = new DateOnly(2026, 11, 20),
        HoraGeracao = new TimeOnly(10, 15, 30),
        Convenio = convenio,
        LotesCobranca = [LoteCobranca(Cedente(banco), banco.Codigo)],
    };

    internal static LotePagamento LoteTed(Cedente pagadora) => new()
    {
        Pagadora = pagadora,
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
                Valor = 250.75m,
                Data = new DateOnly(2026, 11, 25),
                NumeroDocumento = "PAG001",
            },
        ],
    };
}
