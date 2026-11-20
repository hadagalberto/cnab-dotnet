namespace Cnab.Tests;

using Cnab.Arquivo;
using Cnab.Boleto;
using Cnab.Layouts.Cnab240;
using Cnab.Layouts.Cnab240.Registros;
using Cnab.Layouts.Cnab400;
using Cnab.Layouts.Cnab400.Registros;
using Cnab.Modelo;
using Xunit;

/// <summary>
/// Regressões da re-verificação independente que seguiu o commit eebdc92.
/// Todo valor esperado é literal, derivado à mão do manual FEBRABAN de
/// arrecadação (v08, seções 05/07/09/10) ou de cálculo documentado em comentário.
/// Nenhum valor é pedido à própria biblioteca sob teste.
/// </summary>
public class FollowUpReviewFindingsTests
{
    // Montagem comum dos vetores (44 posições do código de barras):
    //   pos 1 = 8 (produto); pos 2 = 2 (saneamento); pos 3 = identificador;
    //   pos 4 = DV geral; pos 5-15 = valor (11); pos 16-19 = órgão 1042;
    //   pos 20-27 = 20261120 (AAAAMMDD); pos 28-44 = zeros.
    // Base do DV geral (43 posições) = pos 1-3 + pos 5-44.
    //
    // MÓDULO 10 (seção 07): pesos 2,1 da direita para a esquerda, soma dos
    //   algarismos dos produtos; DAC = (10 - soma%10)%10.
    // MÓDULO 11 (seção 09): pesos 2..9 reciclados, soma dos produtos;
    //   resto 0/1 -> 0, resto 10 -> 1, caso contrário 11 - resto.
    // Cada bloco de 11 posições da linha usa o MESMO algoritmo do DV geral
    // (seção 05: "módulo-10 ou módulo-11 de acordo com o código de moeda
    // escolhido, a cada grupo").
    //
    // 3º = 6, valor 00005000000 (R$ 50.000,00):
    //   geral: soma mod10 = 50, resto 0 -> 10-10 = 0
    //   blocos: 82600000500 soma 13 -> 7; 00001042202 soma 20 -> 0;
    //           61120000000 soma 8 -> 2; 00000000000 soma 0 -> 0.
    [InlineData(
        "82600000500000010422026112000000000000000000",
        "826000005007000010422020611200000002000000000000")]
    // 3º = 7, valor 00000000000 (referência):
    //   geral: soma mod10 = 47 -> 10-7 = 3
    //   blocos: 82730000000 soma 17 -> 3; demais como acima (0, 2, 0).
    [InlineData(
        "82730000000000010422026112000000000000000000",
        "827300000003000010422020611200000002000000000000")]
    // 3º = 8, valor 00005000000 (R$ 50.000,00):
    //   geral: soma mod11 = 188, 188%11=1 -> DV 0
    //   blocos: 82800000500 soma 74, resto 8 -> 3;
    //           00001042202 soma 54, resto 10 -> 1;
    //           61120000000 soma 47, resto 3 -> 8; zeros -> 0.
    [InlineData(
        "82800000500000010422026112000000000000000000",
        "828000005003000010422021611200000008000000000000")]
    // 3º = 9, valor 00000000000 (referência):
    //   geral: soma mod11 = 165, 165%11=0 -> 0
    //   blocos: 82900000000 soma 56, resto 1 -> 0;
    //           00001042202 soma 54, resto 10 -> 1;
    //           61120000000 soma 47, resto 3 -> 8; zeros -> 0.
    [InlineData(
        "82900000000000010422026112000000000000000000",
        "829000000000000010422021611200000008000000000000")]
    [Theory]
    public void Finding1_LinhaDeArrecadacao_UsaModulo10Ou11ConformeTerceiroDigito(
        string barrasEsperadas,
        string linhaEscritaAmao)
    {
        // A linha de 48 posições é montada à mão (quatro blocos de 11 + DV),
        // com o algoritmo escolhido pelo 3º dígito. O código de barras de 44
        // posições é o valor esperado, também escrito à mão.
        Assert.Equal(barrasEsperadas, Arrecadacao.Normalizar(linhaEscritaAmao));
    }

    [Fact]
    public void Finding2_ArrecadacaoLerValor_ConverteCentavosEmReais()
    {
        // Vetor 3º=6 acima: posições 5-15 = "00005000000" = 5.000.000 centavos.
        // Manual FEBRABAN v08, seção 05: com identificador 6 ou 8 o campo é o
        // "valor a ser cobrado efetivamente em reais" com 11 posições, ou seja,
        // centavos implícitos (5.000.000 / 100 = R$ 50.000,00).
        const string barras = "82600000500000010422026112000000000000000000";
        Assert.Equal(50000.00m, Arrecadacao.LerValor(barras));
    }

    [Fact]
    public void Finding3_SegmentoO_RejeitaPagamentoComValorDiferenteDoCodigoDeBarras()
    {
        // 3º = 8 => valor efetivo. O campo valor do barras é "00005000000" =
        // 5.000.000 centavos = R$ 50.000,00 (vetor calculado acima). Pagar R$
        // 49.999,99 deve ser rejeitado — o banco liquidaria valor divergente.
        var remessa = RemessaComTributo(
            "82800000500000010422026112000000000000000000", 49999.99m);

        var ex = Assert.Throws<ArgumentException>(() => GeradorRemessa240.GerarLinhas(remessa));
        Assert.Contains("valor", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Finding3_SegmentoO_AceitaValorCorretoENaoValoraReferencia()
    {
        // Valor efetivo igual ao do código de barras: R$ 50.000,00.
        var correto = RemessaComTributo(
            "82800000500000010422026112000000000000000000", 50000.00m);
        var linhas = GeradorRemessa240.GerarLinhas(correto);
        Assert.Equal("O", linhas[2].Substring(13, 1));
        Assert.Equal("000000005000000", linhas[2].Substring(107, 15));

        // 3º = 9 => valor de referência (campo zerado): não há valor efetivo a
        // conferir; o pagamento pode ter valor próprio (R$ 250,75).
        var referencia = RemessaComTributo(
            "82900000000000010422026112000000000000000000", 250.75m);
        Assert.Equal("O", GeradorRemessa240.GerarLinhas(referencia)[2].Substring(13, 1));
    }

    [Fact]
    public void Finding4_Sicredi_SegmentoP_RejeitaDvIncorretoDoNossoNumero()
    {
        // Vetor oficial Sicredi (itens 4.5/10.4, também em DvBancosTests):
        // cooperativa 0165 + posto 02 + beneficiário 00623 + base "07200003"
        // formam "0165020062307200003": soma mod11 = 186, 186%11 = 10,
        // DV = 11-10 = 1. O nosso número correto é "072000031"; trocar só o
        // DV para 2 deve ser rejeitado pelo gerador.
        var remessa = RemessaCobrancaComCooperativa(Bancos.Sicredi, "072000032");
        var ex = Assert.Throws<ArgumentException>(() => GeradorRemessa240.GerarLinhas(remessa));
        Assert.Contains("DV", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Finding4_Sicoob_SegmentoP_RejeitaDvIncorretoDoNossoNumero()
    {
        // Vetor oficial Sicoob (item 3.13, também em DvBancosTests):
        // cooperativa 0001 + cliente 0000000019 + base "0000021" -> DV 8,
        // nosso número "00000218". DV 9 está errado.
        var remessa = RemessaCobrancaComCooperativa(Bancos.Sicoob, "00000219");
        var ex = Assert.Throws<ArgumentException>(() => GeradorRemessa240.GerarLinhas(remessa));
        Assert.Contains("DV", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Finding4_Cooperativas_ComputamDvQuandoNossoNumeroVemSemEle()
    {
        // Mesmos vetores, agora sem o DV: o gerador deve calculá-lo (como faz
        // para BB e Santander) e gravar o nosso número completo no segmento P.
        var sicredi = GeradorRemessa240.GerarLinhas(
            RemessaCobrancaComCooperativa(Bancos.Sicredi, "07200003"));
        Assert.Contains("072000031", sicredi[2].Substring(37, 20));

        var sicoob = GeradorRemessa240.GerarLinhas(
            RemessaCobrancaComCooperativa(Bancos.Sicoob, "0000021"));
        Assert.Contains("00000218", sicoob[2].Substring(37, 20));
    }

    [Fact]
    public void Finding6_GoldenRetorno240_AfirmaTodosOsCamposDeTEU()
    {
        // Golden escrito à mão, campo a campo, conforme FEBRABAN CNAB 240 V10.9
        // segmentos T (3.2.1) e U (3.2.2), códigos G001-G078. Cada valor
        // esperado abaixo é literal, lido das posições do manual — nenhum foi
        // obtido do escritor da biblioteca.
        var conteudo = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Golden", "001_cobranca_240.ret"));
        var retorno = LeitorRetorno240.Ler(conteudo, Bancos.BancoDoBrasil);
        var item = Assert.Single(Assert.Single(retorno.Lotes).Itens);
        var t = Assert.IsType<SegmentoT240>(item.Titulo);
        var u = Assert.IsType<SegmentoU240>(item.Valores);
        var linhas = conteudo.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);

        // --- Segmento T -------------------------------------------------
        Assert.Equal(1, t.Banco);                 // G001, 1-3
        Assert.Equal(1, t.Lote);                  // G002, 4-7
        Assert.Equal(3, t.TipoRegistro);          // G003, 8
        Assert.Equal(1, t.Sequencial);            // G038, 9-13
        Assert.Equal("T", t.Segmento);            // G039, 14
        Assert.Equal(" ", linhas[2].Substring(14, 1)); // G004, 15
        Assert.Equal(6, t.CodigoMovimento);       // C044, 16-17: liquidação
        Assert.Equal(57, t.Agencia);              // G008, 18-22
        Assert.Equal("0", t.DvAgencia);           // G009, 23
        Assert.Equal(12345, t.Conta);             // G010, 24-35
        Assert.Equal("7", t.DvConta);             // G011, 36
        Assert.Equal(string.Empty, t.DvAgenciaConta); // G012, 37 (branco)
        Assert.Equal("123412345671", t.NossoNumero); // G069, 38-57
        Assert.Equal(7, t.Carteira);              // C006, 58: carteira BB 17
        Assert.Equal("NF000001", t.NumeroDocumento); // C011, 59-73
        Assert.Equal(new DateOnly(2026, 12, 20), t.DataVencimento); // C012, 74-81
        Assert.Equal(1500.50m, t.Valor);          // G070, 82-96
        Assert.Equal(1, t.BancoCobrador);         // C045, 97-99
        Assert.Equal(57, t.AgenciaCobradora);     // G008, 100-104
        Assert.Equal("0", t.DvAgenciaCobradora);  // G009, 105
        Assert.Equal("NF000001", t.IdentificacaoTituloEmpresa); // G072, 106-130
        Assert.Equal(9, t.CodigoMoeda);           // G065, 131-132 (09 = Real)
        Assert.Equal(1, t.TipoInscricaoPagador);  // G005, 133 (1 = CPF)
        Assert.Equal("11144477735", t.NumeroInscricaoPagador); // G006, 134-148
        Assert.Equal("PAGADOR TESTE LTDA", t.NomePagador); // G013, 149-188
        Assert.Equal(string.Empty, t.NumeroContrato); // C030, 189-198 (sem contrato)
        Assert.Equal(2.50m, t.ValorTarifa);       // G076, 199-213
        Assert.Equal("61", t.MotivoOcorrencia);   // C047, 214-223
        Assert.Equal(new string(' ', 17), linhas[2].Substring(223, 17)); // G004, 224-240

        // --- Segmento U -------------------------------------------------
        Assert.Equal(1, u.Banco);
        Assert.Equal(1, u.Lote);
        Assert.Equal(3, u.TipoRegistro);
        Assert.Equal(2, u.Sequencial);
        Assert.Equal("U", u.Segmento);
        Assert.Equal(" ", linhas[3].Substring(14, 1));
        Assert.Equal(6, u.CodigoMovimento);       // C044, 16-17
        Assert.Equal(0m, u.JurosMultaEncargos);   // C048, 18-32
        Assert.Equal(0m, u.ValorDesconto);        // C049, 33-47
        Assert.Equal(0m, u.ValorAbatimento);      // C050, 48-62
        Assert.Equal(0m, u.ValorIof);             // G077, 63-77
        Assert.Equal(1500.50m, u.ValorPago);      // C052, 78-92
        Assert.Equal(1498.00m, u.ValorLiquido);   // G078, 93-107: 1500,50 - 2,50
        Assert.Equal(0m, u.OutrasDespesas);       // C054, 108-122
        Assert.Equal(0m, u.OutrosCreditos);       // C055, 123-137
        Assert.Equal(new DateOnly(2026, 11, 20), u.DataOcorrencia); // C056, 138-145
        Assert.Equal(new DateOnly(2026, 11, 20), u.DataCredito);    // C057, 146-153
        Assert.Equal("0101", u.CodigoOcorrenciaPagador); // A001, 154-157
        Assert.Equal(string.Empty, u.DataOcorrenciaComplementar); // A001, 158-165
        Assert.Equal(0m, u.ValorOcorrencia);      // A001, 166-180
        Assert.Equal(string.Empty, u.ComplementoOcorrencia); // A001, 181-210
        Assert.Equal(0, u.BancoCorrespondente);   // C031, 211-213
        Assert.Equal(string.Empty, u.NossoNumeroCorrespondente); // C032, 214-233
        Assert.Equal(new string(' ', 7), linhas[3].Substring(233, 7)); // G004, 234-240

        Assert.Equal(linhas, LeitorRetorno240.EscreverLinhas(retorno));
    }

    [Fact]
    public void Finding8_Bradesco400_CodigoEmpresaComLetras_Rejeitado()
    {
        // Código da Empresa Bradesco: 9(20). A remessa anterior removia
        // silenciosamente letras ("ABC12" -> "00000000000000000012"),
        // enviando um código diferente do informado pelo usuário. Deve falhar.
        var remessa = Milestone3Fixtures.Remessa400(Bancos.Bradesco) with
        {
            CodigoBeneficiario = "ABC12",
        };

        var ex = Assert.Throws<ArgumentException>(() => GeradorRemessa400.GerarLinhas(remessa));
        Assert.Contains("Código da Empresa", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Finding7_RetornoItau400_CodigoDeLiquidacaoAceitaLetras()
    {
        // Manual Itaú CNAB 400 (janeiro/2017), registro de transação de
        // retorno: código de liquidação, posições 393-394, é alfanumérico —
        // o banco envia códigos como "AA" e "BC", não apenas 2 dígitos.
        var detalhe = Milestone3Fixtures.NovaLinha400()
            .Por(1, "1").Por(109, "06").Por(111, "201126")
            .Por(153, "0000000150050").Por(254, "0000000150050")
            .Por(296, "211126").Por(393, "AA").ToString();
        var header = Milestone3Fixtures.NovaLinha400()
            .Por(1, "0").Por(2, "2").Por(3, "RETORNO").Por(77, "341")
            .Por(95, "201126").Por(114, "211126").ToString();
        var trailer = Milestone3Fixtures.NovaLinha400()
            .Por(1, "9").Por(2, "2").Por(3, "01").Por(5, "341").ToString();

        var tipo = RegistroCnab<DetalheRetornoItau400>.Ler(detalhe);
        Assert.Equal("AA", tipo.CodigoLiquidacao);

        var retorno = LeitorRetorno400.Ler(
            EscritorDeArquivo.Montar([header, detalhe, trailer]), Bancos.Itau);
        Assert.Equal("AA", Assert.Single(retorno.Itens).LinhaOriginal.Substring(392, 2));
    }

    [Fact]
    public void Finding5_Retorno240_EscreverLinhas_DevolveLinhaOriginalExatamenteComoLida()
    {
        // Linhas escritas à mão: nome em minúsculas e campos numéricos em
        // branco (saída real de banco, que não normaliza case nem preenche
        // campos não usados com zeros). O leitor normaliza os valores tipados,
        // mas a linha original precisa ser devolvida byte a byte.
        var header = Milestone3Fixtures.NovaLinha240()
            .Por(1, "001").Por(8, "0")
            .Por(18, "2").Por(19, "00000000000191")
            .Por(73, "empresa teste ltda")
            .Por(103, "BANCO DO BRASIL S.A.")
            .Por(143, "2").Por(144, "20112026") // 152-157 e 158-163 ficam em branco
            .Por(164, "083").Por(167, "01600")
            .ToString();
        var headerLote = Milestone3Fixtures.NovaLinha240()
            .Por(1, "001").Por(4, "0001").Por(8, "1")
            .Por(9, "T").Por(10, "01").Por(14, "042").Por(18, "2")
            .Por(19, "00000000000191").Por(74, "empresa teste ltda")
            .Por(192, "20112026") // 184-191 em branco
            .ToString();
        var trailerLote = Milestone3Fixtures.NovaLinha240()
            .Por(1, "001").Por(4, "0001").Por(8, "5").ToString();
        var trailerArquivo = Milestone3Fixtures.NovaLinha240()
            .Por(1, "001").Por(4, "9999").Por(8, "9").ToString(); // contadores em branco
        var linhas = new[] { header, headerLote, trailerLote, trailerArquivo };

        var retorno = LeitorRetorno240.Ler(EscritorDeArquivo.Montar(linhas), Bancos.BancoDoBrasil);

        Assert.Equal(linhas, LeitorRetorno240.EscreverLinhas(retorno));
        Assert.Equal(header, retorno.LinhaHeaderArquivo);
    }

    [Fact]
    public void Finding5_Retorno400_EscreverLinhas_DevolveLinhaOriginalExatamenteComoLida()
    {
        // Mesma premissa no CNAB 400: nomes em minúsculas e numéricos em branco.
        var header = Milestone3Fixtures.NovaLinha400()
            .Por(1, "0").Por(2, "2").Por(3, "RETORNO")
            .Por(10, "01").Por(12, "COBRANCA")
            .Por(47, "empresa teste ltda") // 27-38 em branco
            .Por(77, "341").Por(80, "BANCO ITAU SA")
            .Por(95, "201126").Por(114, "211126").ToString();
        var detalhe = Milestone3Fixtures.NovaLinha400()
            .Por(1, "1").Por(109, "06").Por(111, "201126")
            .Por(153, "0000000150050").Por(254, "0000000150050")
            .Por(296, "211126").ToString(); // 393-394 em branco
        var trailer = Milestone3Fixtures.NovaLinha400()
            .Por(1, "9").Por(2, "2").Por(3, "01").Por(5, "341").ToString();
        var linhas = new[] { header, detalhe, trailer };

        var retorno = LeitorRetorno400.Ler(EscritorDeArquivo.Montar(linhas), Bancos.Itau);

        Assert.Equal(linhas, LeitorRetorno400.EscreverLinhas(retorno));
        Assert.Equal(detalhe, Assert.Single(retorno.Itens).LinhaOriginal);
        Assert.Equal(1500.50m, Assert.Single(retorno.Itens).Valores!.ValorPago);
    }

    private static Remessa240 RemessaCobrancaComCooperativa(Banco banco, string nossoNumero)
    {
        var cedente = Milestone3Fixtures.Cedente(banco) with
        {
            DadosCooperativa = banco.Codigo switch
            {
                748 => new DadosCooperativa
                {
                    Cooperativa = "0165",
                    Posto = "02",
                    CodigoBeneficiario = "00623",
                },
                756 => new DadosCooperativa
                {
                    Cooperativa = "0001",
                    CodigoCliente = "0000000019",
                },
                _ => null,
            },
        };
        return new Remessa240
        {
            Banco = banco,
            Cedente = cedente,
            NumeroSequencialArquivo = 1,
            DataGeracao = new DateOnly(2026, 11, 20),
            HoraGeracao = new TimeOnly(10, 15, 30),
            LotesCobranca =
            [
                new LoteCobranca
                {
                    Cedente = cedente,
                    Numero = 1,
                    Titulos = [Milestone3Fixtures.Titulo(nossoNumero, "1")],
                },
            ],
        };
    }

    private static Remessa240 RemessaComTributo(string codigoDeBarras, decimal valor)
    {
        var cedente = Milestone3Fixtures.Cedente(Bancos.BancoDoBrasil);
        return new Remessa240
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
                            CodigoDeBarras = codigoDeBarras,
                            NomeFavorecido = "CONCESSIONARIA TESTE",
                            DocumentoFavorecido = new DocumentoFiscal(DocumentoFiscal.CnpjExemplo),
                            Valor = valor,
                            Data = new DateOnly(2026, 11, 25),
                            NumeroDocumento = "PAG002",
                        },
                    ],
                },
            ],
        };
    }
}
