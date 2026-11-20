namespace Cnab.Tests;

using System.Text;
using Cnab.Cli;
using Cnab.Layouts.Cnab240;
using Cnab.Layouts.Cnab400;
using Cnab.Modelo;
using Xunit;

/// <summary>
/// Executa literalmente os exemplos públicos dos READMEs (pt-BR e en) para
/// garantir que compilam e produzem a saída documentada. Evita documentação
/// "runnable" apenas na aparência.
/// </summary>
public sealed class ReadmeExamplesTests
{
    [Fact]
    public void ExemploRemessa400_DoReadme_ProduzArquivoComTresRegistrosDe400Posicoes()
    {
        var banco = Bancos.Bradesco;
        var conta = new ContaBancaria
        {
            Banco = banco,
            Agencia = "0057",
            Conta = "12345",
            DvConta = "7",
        };
        var cedente = new Cedente
        {
            Nome = "EMPRESA TESTE LTDA",
            Documento = new DocumentoFiscal(DocumentoFiscal.CnpjExemplo),
            Conta = conta,
        };
        var titulo = new Titulo
        {
            NossoNumero = "98712345",
            NumeroDocumento = "NF000001",
            Especie = EspecieDocumento.DuplicataMercantilIndicacao,
            DataEmissao = new DateOnly(2026, 11, 20),
            DataVencimento = new DateOnly(2026, 12, 20),
            Valor = 1500.50m,
            Carteira = "19",
            Pagador = new Pagador
            {
                Nome = "PAGADOR TESTE LTDA",
                Documento = new DocumentoFiscal(DocumentoFiscal.CpfExemplo),
            },
        };
        var remessa = new Remessa400
        {
            Banco = banco,
            Cedente = cedente,
            NumeroSequencialArquivo = 1,
            DataGeracao = new DateOnly(2026, 11, 20),
            CodigoBeneficiario = "00623",
            Titulos = [titulo],
        };

        var conteudo = GeradorRemessa400.Gerar(remessa);
        var bytes = Encoding.Latin1.GetBytes(conteudo);

        Assert.Equal(3 * (400 + 2), bytes.Length); // 3 registros de 400 + CRLF após cada um
        var linhasSeparadas = ArquivoCnab.SepararLinhas(conteudo);
        Assert.Equal(3, linhasSeparadas.Count);
        Assert.All(linhasSeparadas, linha => Assert.Equal(400, linha.Length));

        Assert.Contains("BRADESCO", conteudo, StringComparison.Ordinal);
        Assert.Equal(0x0D, bytes[400]);
        Assert.Equal(0x0A, bytes[401]);
    }

    [Fact]
    public void ExemploLeituraRetorno_DoReadme_LeOsDoisLayouts()
    {
        var retorno400 = LeitorRetorno400.Ler(ArquivoTexto.Ler(Golden("341_cobranca_400.ret")));
        var item400 = Assert.Single(retorno400.Itens);
        Assert.Equal("06", item400.Ocorrencia!.Codigo);
        Assert.Equal(1500.50m, item400.Valores!.ValorPago);

        var retorno240 = LeitorRetorno240.Ler(RetornoFixtures.Cnab240(208, 1, 999.99m));
        var lote = Assert.Single(retorno240.Lotes);
        var item240 = Assert.Single(lote.Itens);
        Assert.Equal("06", item240.Ocorrencia!.Codigo);
        Assert.Equal(999.99m, item240.Valores!.ValorPago);
    }

    private static string Golden(string nome) =>
        Path.Combine(AppContext.BaseDirectory, "Golden", nome);
}
