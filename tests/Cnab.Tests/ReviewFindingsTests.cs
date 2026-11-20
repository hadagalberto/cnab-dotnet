namespace Cnab.Tests;

using System.Globalization;
using Cnab.Arquivo;
using Cnab.Boleto;
using Cnab.Layouts.Cnab240;
using Cnab.Layouts.Cnab400;
using Cnab.Modelo;
using Xunit;

/// <summary>
/// Regressiones escritas a partir da revisão independente de 2026-11-20.
/// Todo valor esperado é derivado à mão de manual oficial / SPEC, nunca da
/// própria biblioteca sob teste.
/// </summary>
public class ReviewFindingsTests
{
    // FEBRABAN Layout Padrão de Arrecadação v07/v08, seções 04/05/10:
    // produto 8, segmento 2 (saneamento), identificador de valor 8 (efetivo,
    // módulo 11), valor 11 posições, campo livre com CNPJ/órgão + AAAAMMDD.
    // DV geral derivado à mão: base 43 posições "828"+"00001000000"+
    // "10422026112000000000000000000" → soma 168, resto 3, 11-3 = 8.
    private const string ArrecadacaoBarras =
        "82880000100000010422026112000000000000000000";

    [Fact]
    public void Finding1_TrailerLoteCobranca_QuantidadeDeRegistrosDoLote()
    {
        // Cabeçalho de lote + P + Q + R + trailer de lote = 5 registros.
        // G057: "Quantidade de registros do lote", posições 18-23, obrigatório numérico.
        var linhas = GeradorRemessa240.GerarLinhas(Milestone3Fixtures.Remessa240(Bancos.BancoDoBrasil));
        var trailerLote = linhas.Single(l => l.Substring(7, 1) == "5" && l.Substring(3, 4) != "9999");
        Assert.Equal("000005", trailerLote.Substring(17, 6));
    }

    [Fact]
    public void Finding2_ParaVencimento_Fator1667NoCicloRecente()
    {
        // Tabela oficial (Caixa Anexo II / FEBRABAN FB-009/2023): 21/12/2026 → 1667.
        // O fator 1667 também ocorre em 01/05/2002; a ambiguidade é resolvida
        // escolhendo o ciclo mais próximo da referência (hoje, 2026-11-20).
        Assert.Equal(new DateOnly(2026, 12, 21), FatorDeVencimento.ParaVencimento(1667));
    }

    [Fact]
    public void Finding2_SegmentJ_VencimentoDoFator1667NoCicloRecente()
    {
        // Código de barras oficial Itaú Anexo 2: fator 1667, valor 123,45.
        // Segmento J posições 92-99 devem conter 21122026 e não 01052002.
        const string codigo = "34196166700000123451101234567880057123457000";
        var cedente = Milestone3Fixtures.Cedente(Bancos.BancoDoBrasil);
        var remessa = new Remessa240
        {
            Banco = Bancos.BancoDoBrasil,
            Cedente = cedente,
            NumeroSequencialArquivo = 1,
            DataGeracao = new DateOnly(2026, 11, 20),
            HoraGeracao = new TimeOnly(10, 15, 30),
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
                            Tipo = TipoPagamento.BoletoCodigoBarras,
                            CodigoDeBarras = codigo,
                            NomeFavorecido = "FORNECEDOR TESTE LTDA",
                            DocumentoFavorecido = new DocumentoFiscal(DocumentoFiscal.CnpjExemplo),
                            Valor = 123.45m,
                            Data = new DateOnly(2026, 11, 25),
                            NumeroDocumento = "PAG001",
                        },
                    ],
                },
            ],
        };

        var linhas = GeradorRemessa240.GerarLinhas(remessa);
        Assert.Equal("J", linhas[2].Substring(13, 1));
        Assert.Equal("21122026", linhas[2].Substring(91, 8));
    }

    [Fact]
    public void Finding3_LinhaDeArrecadacao48Digitos_GeraSegmentoO()
    {
        // Montagem independente da linha de 48 dígitos: quatro blocos de 11
        // posições do código de barras oficial, cada um com DV módulo 10
        // calculado pela regra da seção 07 do manual FEBRABAN de arrecadação.
        var linha =
            BlocoComDv(ArrecadacaoBarras[..11])
            + BlocoComDv(ArrecadacaoBarras.Substring(11, 11))
            + BlocoComDv(ArrecadacaoBarras.Substring(22, 11))
            + BlocoComDv(ArrecadacaoBarras.Substring(33, 11));
        Assert.Equal(48, linha.Length);

        var cedente = Milestone3Fixtures.Cedente(Bancos.BancoDoBrasil);
        var remessa = new Remessa240
        {
            Banco = Bancos.BancoDoBrasil,
            Cedente = cedente,
            NumeroSequencialArquivo = 1,
            DataGeracao = new DateOnly(2026, 11, 20),
            HoraGeracao = new TimeOnly(10, 15, 30),
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
                            Tipo = TipoPagamento.TributoCodigoBarras,
                            CodigoDeBarras = linha,
                            NomeFavorecido = "CONCESSIONARIA TESTE",
                            DocumentoFavorecido = new DocumentoFiscal(DocumentoFiscal.CnpjExemplo),
                            Valor = 10000.00m,
                            Data = new DateOnly(2026, 11, 25),
                            NumeroDocumento = "PAG002",
                        },
                    ],
                },
            ],
        };

        var linhas = GeradorRemessa240.GerarLinhas(remessa);
        Assert.Equal("O", linhas[2].Substring(13, 1));
        Assert.Equal(ArrecadacaoBarras, linhas[2].Substring(17, 44));
        Assert.Equal("000000001000000", linhas[2].Substring(107, 15));
    }

    [Fact]
    public void Finding4_LoteComFormasDeLancamentoDiferentes_Rejeitado()
    {
        var cedente = Milestone3Fixtures.Cedente(Bancos.BancoDoBrasil);
        var remessa = new Remessa240
        {
            Banco = Bancos.BancoDoBrasil,
            Cedente = cedente,
            NumeroSequencialArquivo = 1,
            DataGeracao = new DateOnly(2026, 11, 20),
            HoraGeracao = new TimeOnly(10, 15, 30),
            LotesPagamento =
            [
                new LotePagamento
                {
                    Pagadora = cedente,
                    Numero = 1,
                    Pagamentos =
                    [
                        Cnab240Fixtures.LoteTed(cedente).Pagamentos[0],
                        new Pagamento
                        {
                            Tipo = TipoPagamento.TributoCodigoBarras,
                            CodigoDeBarras = ArrecadacaoBarras,
                            NomeFavorecido = "CONCESSIONARIA TESTE",
                            DocumentoFavorecido = new DocumentoFiscal(DocumentoFiscal.CnpjExemplo),
                            Valor = 100m,
                            Data = new DateOnly(2026, 11, 25),
                            NumeroDocumento = "PAG002",
                        },
                    ],
                },
            ],
        };

        Assert.Throws<ArgumentException>(() => GeradorRemessa240.GerarLinhas(remessa));
    }

    [Fact]
    public void Finding5_CamposDocumentadosComoZeros_SaoEscritosComZeros()
    {
        var itau = GeradorRemessa400.GerarLinhas(Milestone3Fixtures.Remessa400(Bancos.Itau));
        Assert.Equal("00", itau[0].Substring(30, 2)); // header 31-32
        Assert.Equal("00", itau[1].Substring(21, 2)); // detalhe 22-23

        var bradesco = GeradorRemessa400.GerarLinhas(Milestone3Fixtures.Remessa400(Bancos.Bradesco));
        Assert.Equal("0", bradesco[1].Substring(20, 1)); // detalhe 21
    }

    [Fact]
    public void Finding6_Bradesco400_CodigoEmpresaNumericoZeroPadded()
    {
        var remessa = Milestone3Fixtures.Remessa400(Bancos.Bradesco) with
        {
            CodigoBeneficiario = "623",
        };
        var linhas = GeradorRemessa400.GerarLinhas(remessa);
        Assert.Equal("00000000000000000623", linhas[0].Substring(26, 20));
    }

    [Fact]
    public void Finding7_BB_SegmentoP_UsaConvênioEFromDoNossoNumero()
    {
        // Vetor do Anexo XI do manual BB: convênio 0500 + sequencial 9401448
        // → NN 05009401448 com DV 1 (soma 221, resto 1).
        var remessa = Milestone3Fixtures.Remessa240(Bancos.BancoDoBrasil, convenio: "0500") with
        {
            LotesCobranca =
            [
                new LoteCobranca
                {
                    Cedente = Milestone3Fixtures.Cedente(Bancos.BancoDoBrasil),
                    Numero = 1,
                    Titulos = [Milestone3Fixtures.Titulo("9401448", "17")],
                },
            ],
        };
        var linhas = GeradorRemessa240.GerarLinhas(remessa);
        Assert.Contains("050094014481", linhas[2].Substring(37, 20));
    }

    [Fact]
    public void Finding7_Santander_SegmentoP_UsaDvDoNossoNumero()
    {
        // Vetor do manual Santander v34: sequencial 566612457800 → DV 2.
        var remessa = Milestone3Fixtures.Remessa240(Bancos.Santander) with
        {
            LotesCobranca =
            [
                new LoteCobranca
                {
                    Cedente = Milestone3Fixtures.Cedente(Bancos.Santander),
                    Numero = 1,
                    Titulos = [Milestone3Fixtures.Titulo("566612457800", "101")],
                },
            ],
        };
        var linhas = GeradorRemessa240.GerarLinhas(remessa);
        Assert.Equal("5666124578002", linhas[2].Substring(44, 13));
    }

    [Fact]
    public void Finding8_CalcularFatorAlemUltimoDiaDoCiclo_Rejeita()
    {
        // 13/10/2049 → 9999; 14/10/2049 exigiria 10000 (5 dígitos) e deve falhar.
        Assert.Equal(9999, FatorDeVencimento.Calcular(new DateOnly(2049, 10, 13)));
        Assert.Throws<ArgumentOutOfRangeException>(() => FatorDeVencimento.Calcular(new DateOnly(2049, 10, 14)));
    }

    [Fact]
    public void Finding9_Retorno400_EscreverLinhas_ReserializaRegistrosTipados()
    {
        // Linha escrita à mão (não pelo gerador da biblioteca): nome da empresa
        // em minúsculas. O leitor normaliza para maiúsculas; a reescrita deve
        // usar o registro tipado — não devolver a LinhaOriginal.
        var linha =
            Milestone3Fixtures.NovaLinha400()
                .Por(1, "0").Por(2, "2").Por(3, "RETORNO")
                .Por(10, "01").Por(12, "COBRANCA")
                .Por(47, "empresa teste ltda")
                .Por(77, "341").Por(80, "BANCO ITAU SA")
                .Por(95, "201126").Por(101, "01600").Por(106, "BPI")
                .Por(109, "00001").Por(114, "211126").Por(395, "000001").ToString();
        var detalhe =
            Milestone3Fixtures.NovaLinha400()
                .Por(1, "1").Por(2, "06").Por(4, "00000000000191")
                .Por(63, "98712345").Por(109, "06").Por(111, "201126")
                .Por(153, "0000000150050").Por(254, "0000000150050")
                .Por(395, "000002").ToString();
        var trailer =
            Milestone3Fixtures.NovaLinha400()
                .Por(1, "9").Por(2, "2").Por(3, "01").Por(5, "341")
                .Por(395, "000003").ToString();
        var conteudo = EscritorDeArquivo.Montar([linha, detalhe, trailer]);

        var retorno = LeitorRetorno400.Ler(conteudo, Bancos.Itau);
        var reescrito = LeitorRetorno400.EscreverLinhas(retorno);
        Assert.Equal("EMPRESA TESTE LTDA", reescrito[0].Substring(46, 30).TrimEnd());
    }

    [Fact]
    public void Finding9_Retorno240_GoldenIndependenteExisteEParseia()
    {
        // SPEC §Testes: golden de retorno 240 construído à mão, sem uso do
        // escritor da biblioteca, com round-trip byte-exato.
        var conteudo = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Golden", "001_cobranca_240.ret"));
        var retorno = LeitorRetorno240.Ler(conteudo, Bancos.BancoDoBrasil);
        var item = Assert.Single(retorno.Lotes[0].Itens);
        Assert.Equal("123412345671", item.Titulo!.NossoNumero.TrimEnd());
        Assert.Equal(1500.50m, item.Valores!.ValorPago);
        Assert.Equal("06", item.Ocorrencia!.Codigo);
        Assert.Equal(
            conteudo.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries),
            LeitorRetorno240.EscreverLinhas(retorno));
    }

    [Fact]
    public void Finding9_Retorno240_EscreverLinhasReserializaRegistrosTipados()
    {
        var header =
            Milestone3Fixtures.NovaLinha240()
                .Por(1, "001").Por(8, "0")
                .Por(18, "2").Por(19, "00000000000191")
                .Por(73, "empresa teste ltda")
                .Por(103, "BANCO DO BRASIL S.A.")
                .Por(143, "2").Por(144, "20112026").Por(152, "101530")
                .Por(158, "000001").Por(164, "083").Por(167, "01600")
                .ToString();
        var headerLote =
            Milestone3Fixtures.NovaLinha240()
                .Por(1, "001").Por(4, "0001").Por(8, "1")
                .Por(9, "T").Por(10, "01").Por(14, "042").Por(18, "2")
                .Por(19, "00000000000191").Por(74, "EMPRESA TESTE LTDA")
                .Por(192, "20112026").ToString();
        var trailerLote =
            Milestone3Fixtures.NovaLinha240()
                .Por(1, "001").Por(4, "0001").Por(8, "5").ToString();
        var trailerArquivo =
            Milestone3Fixtures.NovaLinha240()
                .Por(1, "001").Por(4, "9999").Por(8, "9")
                .Por(18, "000001").Por(24, "000004").ToString();
        var retorno = LeitorRetorno240.Ler(
            EscritorDeArquivo.Montar([header, headerLote, trailerLote, trailerArquivo]), Bancos.BancoDoBrasil);
        var reescrito = LeitorRetorno240.EscreverLinhas(retorno);
        Assert.Equal("EMPRESA TESTE LTDA", reescrito[0].Substring(72, 30).TrimEnd());
    }

    [Fact]
    public void VerificacaoAdicional_CaixaSegmentoPModalidadeSIGCB()
    {
        // Manual SIGCB 67.118 v031, notas G069/G069A: posição 40 fixa "0"
        // (SINCO) e 41-42 = "14" (registrado, emissão pelo beneficiário).
        var linhas = GeradorRemessa240.GerarLinhas(Milestone3Fixtures.Remessa240(Bancos.Caixa));
        Assert.Equal("0", linhas[2].Substring(39, 1));
        Assert.Equal("14", linhas[2].Substring(40, 2));
    }

    /// <summary>Módulo 10 independente, seção 07 do manual FEBRABAN de arrecadação.</summary>
    private static string BlocoComDv(string bloco)
    {
        var soma = 0;
        var peso = 2;
        for (var i = bloco.Length - 1; i >= 0; i--)
        {
            var produto = (bloco[i] - '0') * peso;
            soma += produto < 10 ? produto : produto - 9;
            peso = peso == 2 ? 1 : 2;
        }

        var dv = (10 - (soma % 10)) % 10;
        return bloco + dv.ToString(CultureInfo.InvariantCulture);
    }
}
