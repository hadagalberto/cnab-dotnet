namespace Cnab.Tests;

using System.Globalization;
using Cnab.Layouts.Cnab240;
using Cnab.Layouts.Cnab400;
using Cnab.Modelo;

/// <summary>Objetos e utilitários compartilhados pelos testes do marco 3.</summary>
internal static class Milestone3Fixtures
{
    internal sealed class LinhaCnab
    {
        private readonly char[] _caracteres;

        internal LinhaCnab(int tamanho)
        {
            _caracteres = new char[tamanho];
            Array.Fill(_caracteres, ' ');
        }

        internal LinhaCnab Por(int posicao, string conteudo)
        {
            for (var i = 0; i < conteudo.Length; i++)
            {
                _caracteres[posicao - 1 + i] = conteudo[i];
            }

            return this;
        }

        internal LinhaCnab Zeros(int posicao, int tamanho) => Por(posicao, new string('0', tamanho));

        public override string ToString() => new(_caracteres);
    }

    internal static LinhaCnab NovaLinha240() => new(240);

    internal static LinhaCnab NovaLinha400() => new(400);

    internal static Cedente Cedente(Banco banco) => new()
    {
        Nome = "EMPRESA TESTE LTDA",
        Documento = new DocumentoFiscal(DocumentoFiscal.CnpjExemplo),
        Conta = new ContaBancaria
        {
            Banco = banco,
            Agencia = "0057",
            DvAgencia = "0",
            Conta = "12345",
            DvConta = "7",
            DvAgenciaConta = "0",
        },
    };

    internal static Titulo Titulo(string nossoNumero, string carteira) => new()
    {
        NossoNumero = nossoNumero,
        NumeroDocumento = "NF000001",
        Especie = EspecieDocumento.DuplicataMercantilIndicacao,
        DataEmissao = new DateOnly(2026, 11, 20),
        DataVencimento = new DateOnly(2026, 12, 20),
        Valor = 1500.50m,
        Carteira = carteira,
        Juros = new Juros
        {
            Tipo = TipoJuros.ValorDiario,
            Valor = 0.50m,
            DataInicio = new DateOnly(2026, 12, 21),
        },
        Multa = new Multa
        {
            Tipo = TipoMulta.Percentual,
            Valor = 2.00m,
            DataInicio = new DateOnly(2026, 12, 21),
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

    /// <summary>
    /// Nosso número no formato exigido pelo segmento P de cada banco. BB e
    /// Santander recebem a base sem DV (o gerador aplica o DV do manual);
    /// Sicredi/Sicoob/BTG usam vetores publicados ou 11 dígitos atribuídos.
    /// </summary>
    internal static string NossoNumero240(int codigoBanco, int numero)
    {
        var digitos = Math.Abs(numero).ToString("D8", CultureInfo.InvariantCulture);
        return codigoBanco switch
        {
            1 => digitos[..7],
            33 => digitos + digitos[..4],
            208 => digitos + digitos[..3],
            748 => "072000031", // vetor oficial Sicredi (8 + DV 1)
            756 => "00000218", // vetor Sicoob (7 + DV 8)
            _ => digitos,
        };
    }

    internal static Remessa240 Remessa240(Banco banco, string convenio = "1234") => new()
    {
        Banco = banco,
        Cedente = Cedente(banco),
        NumeroSequencialArquivo = 1,
        DataGeracao = new DateOnly(2026, 11, 20),
        HoraGeracao = new TimeOnly(10, 15, 30),
        Convenio = convenio,
        CodigoProduto = "009",
        LotesCobranca =
        [
            new LoteCobranca
            {
                Cedente = Cedente(banco),
                Numero = 1,
                Titulos = [Titulo(NossoNumero240(banco.Codigo, 2), banco.Codigo == 237 ? "19" : "109")],
            },
        ],
    };

    internal static Remessa400 Remessa400(Banco banco) => new()
    {
        Banco = banco,
        Cedente = Cedente(banco),
        NumeroSequencialArquivo = 1,
        DataGeracao = new DateOnly(2026, 11, 20),
        NumeroRemessa = 1,
        CodigoBeneficiario = "00623",
        CodigoProduto = "009",
        Titulos = [Titulo("98712345", banco.Codigo switch
        {
            341 => "109",
            237 => "19",
            748 => "1",
            77 => "112",
            _ => "1",
        })],
    };

    internal static string HojeddMMAA(DateOnly data) => data.ToString("ddMMyy", CultureInfo.InvariantCulture);
}
