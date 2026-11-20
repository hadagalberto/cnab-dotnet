namespace Cnab.Layouts.Cnab240;

using System.Globalization;
using Cnab.Arquivo;
using Cnab.Boleto;
using Cnab.Layouts.Cnab240.Registros;
using Cnab.Modelo;

/// <summary>Classe das carteiras de cobrança usadas no trailer de lote FEBRABAN.</summary>
public enum CarteiraCobranca240
{
    /// <summary>Cobrança simples.</summary>
    Simples,

    /// <summary>Cobrança vinculada.</summary>
    Vinculada,

    /// <summary>Cobrança caucionada.</summary>
    Caucionada,

    /// <summary>Cobrança descontada.</summary>
    Descontada,
}

/// <summary>
/// Opções de mapeamento entre o domínio e os campos FEBRABAN. Os mapas podem
/// ser ajustados para cobrir tabelas específicas de cada banco; os padrões
/// seguem os domínios publicados nos manuais citados em <c>docs/layouts.md</c>.
/// </summary>
public sealed record OpcoesGeracao240
{
    /// <summary>Código de movimento remessa padrão (C004): "01" = entrada de títulos.</summary>
    public int CodigoMovimentoRemessa { get; init; } = 1; // C004 "01" — entrada de títulos

    /// <summary>Código da espécie documental (C007) por tipo do domínio.</summary>
    public IReadOnlyDictionary<EspecieDocumento, int> MapaEspecies { get; init; } = MapaEspeciesPadrao;

    /// <summary>Forma de lançamento (G029) por tipo de pagamento.</summary>
    public IReadOnlyDictionary<TipoPagamento, int> MapaFormaLancamento { get; init; } = MapaFormasPadrao;

    /// <summary>Câmara centralizadora (P001) por tipo de pagamento.</summary>
    public IReadOnlyDictionary<TipoPagamento, int> MapaCamara { get; init; } = MapaCamarasPadrao;

    /// <summary>Classifica a carteira de um título para o trailer de lote de cobrança.</summary>
    public Func<Titulo, CarteiraCobranca240> ClassificarCarteira { get; init; } = static _ => CarteiraCobranca240.Simples;

    private static IReadOnlyDictionary<EspecieDocumento, int> MapaEspeciesPadrao { get; } =
        new Dictionary<EspecieDocumento, int>
        {
            [EspecieDocumento.DuplicataMercantilIndicacao] = 1,
            [EspecieDocumento.DuplicataRural] = 2,
            [EspecieDocumento.NotaPromissoria] = 3,
            [EspecieDocumento.NotaPromissoriaRural] = 4,
            [EspecieDocumento.NotaSeguros] = 5,
            [EspecieDocumento.Recibo] = 6,
            [EspecieDocumento.LetraCambio] = 7,
            [EspecieDocumento.NotaDebito] = 8,
            [EspecieDocumento.DuplicataServicoIndicacao] = 9,
            [EspecieDocumento.Outros] = 13,
        };

    private static IReadOnlyDictionary<TipoPagamento, int> MapaFormasPadrao { get; } =
        new Dictionary<TipoPagamento, int>
        {
            [TipoPagamento.CreditoEmConta] = 1,
            [TipoPagamento.Doc] = 3,
            [TipoPagamento.Ted] = 41,
            [TipoPagamento.PixChave] = 45,
            [TipoPagamento.PixQrCode] = 47,
            [TipoPagamento.BoletoCodigoBarras] = 31,
            [TipoPagamento.TributoCodigoBarras] = 11,
        };

    private static IReadOnlyDictionary<TipoPagamento, int> MapaCamarasPadrao { get; } =
        new Dictionary<TipoPagamento, int>
        {
            [TipoPagamento.CreditoEmConta] = 0,
            [TipoPagamento.Doc] = 700,
            [TipoPagamento.Ted] = 18,
            [TipoPagamento.PixChave] = 9,
            [TipoPagamento.PixQrCode] = 9,
        };
}

/// <summary>
/// Arquivo de remessa CNAB 240: cabeçalho, lotes de cobrança e/ou pagamentos
/// e trailers com contadores e somatórias calculados automaticamente.
/// </summary>
public sealed record Remessa240
{
    /// <summary>Banco emissor do arquivo.</summary>
    public required Banco Banco { get; init; }

    /// <summary>Cedente/beneficiário (empresa).</summary>
    public required Cedente Cedente { get; init; }

    /// <summary>Número sequencial do arquivo (NSA), posições 158-163.</summary>
    public required int NumeroSequencialArquivo { get; init; }

    /// <summary>Data de geração do arquivo.</summary>
    public required DateOnly DataGeracao { get; init; }

    /// <summary>Hora de geração do arquivo.</summary>
    public required TimeOnly HoraGeracao { get; init; }

    /// <summary>Código do convênio no banco (quando aplicável ao banco).</summary>
    public string Convenio { get; init; } = string.Empty;

    /// <summary>
    /// Código do beneficiário da CAIXA (7 posições), usado no header de lote e
    /// no segmento P conforme o manual SIGCB 67.118 v031.
    /// </summary>
    public string CodigoBeneficiario { get; init; } = string.Empty;

    /// <summary>
    /// Código do produto do Bradesco (3 posições, identificação do título no
    /// banco no segmento P). Obrigatório para remessas de cobrança do Bradesco.
    /// </summary>
    public string CodigoProduto { get; init; } = string.Empty;

    /// <summary>Número remessa/retorno do header de lote de cobrança; usa NSA quando ausente.</summary>
    public long? NumeroRemessaRetorno { get; init; }

    /// <summary>Perfil com as peculiaridades do banco; deduzido de <see cref="Banco"/> quando ausente.</summary>
    public PerfilBanco240? Perfil { get; init; }

    /// <summary>Lotes de cobrança (segmentos P/Q/R).</summary>
    public IReadOnlyList<LoteCobranca> LotesCobranca { get; init; } = [];

    /// <summary>Lotes de pagamento (segmentos A/B, J/J-52 ou O).</summary>
    public IReadOnlyList<LotePagamento> LotesPagamento { get; init; } = [];

    /// <summary>Opções de mapeamento domínio → campos.</summary>
    public OpcoesGeracao240 Opcoes { get; init; } = new();

    /// <summary>Opções de montagem do arquivo (CRLF/Latin-1 por padrão).</summary>
    public OpcoesArquivo OpcoesArquivo { get; init; } = OpcoesArquivo.Padrao;
}

/// <summary>
/// Gera arquivos de remessa CNAB 240 a partir do domínio, calculando NSR,
/// contadores e somatórias dos trailers. Nenhum campo é deixado como stub:
/// todos os segmentos declarados são efetivamente preenchidos.
/// </summary>
public static class GeradorRemessa240
{
    /// <summary>Gera o conteúdo completo do arquivo (com delimitadores de linha).</summary>
    /// <param name="remessa">Dados da remessa.</param>
    /// <returns>Conteúdo do arquivo.</returns>
    public static string Gerar(Remessa240 remessa) =>
        EscritorDeArquivo.Montar(GerarLinhas(remessa), remessa.OpcoesArquivo);

    /// <summary>Gera as linhas de 240 Posições sem delimitadores.</summary>
    /// <param name="remessa">Dados da remessa.</param>
    /// <returns>Linhas na ordem header, lotes, trailer.</returns>
    public static IReadOnlyList<string> GerarLinhas(Remessa240 remessa)
    {
        ArgumentNullException.ThrowIfNull(remessa);
        Validar(remessa);

        var perfil = remessa.Perfil ?? PerfilBanco240.Obter(remessa.Banco);
        var headerArquivo = PerfilBanco240.Aplicar(MontarHeaderArquivo(remessa, perfil), perfil.AjustesHeaderArquivo);
        if (remessa.Banco.Codigo == 341)
        {
            headerArquivo = PerfilBanco240.Aplicar(headerArquivo, AjustesBanco240.HeaderArquivoItau(remessa));
        }

        var linhas = new List<string> { headerArquivo };

        var numeroLote = 0;
        foreach (var lote in remessa.LotesCobranca)
        {
            numeroLote++;
            linhas.AddRange(GerarLoteCobranca(remessa, perfil, lote, numeroLote));
        }

        foreach (var lote in remessa.LotesPagamento)
        {
            numeroLote++;
            linhas.AddRange(GerarLotePagamento(remessa, perfil, lote, numeroLote));
        }

        linhas.Add(RegistroCnab<TrailerArquivo240>.Escrever(new TrailerArquivo240
        {
            Banco = remessa.Banco.Codigo,
            Lote = 9999,
            TipoRegistro = Codigos240.TrailerArquivo,
            QuantidadeLotes = numeroLote,
            QuantidadeRegistros = linhas.Count + 1,
            QuantidadeContasConciliacao = 0,
        }));
        return linhas;
    }

    private static string MontarHeaderArquivo(Remessa240 remessa, PerfilBanco240 perfil)
    {
        if (remessa.Banco.Codigo == 33)
        {
            return MontarHeaderArquivoSantander(remessa, perfil);
        }

        var conta = remessa.Cedente.Conta;
        return RegistroCnab<HeaderArquivo240>.Escrever(new HeaderArquivo240
        {
            Banco = remessa.Banco.Codigo,
            Lote = 0,
            TipoRegistro = Codigos240.HeaderArquivo,
            TipoInscricaoEmpresa = remessa.Cedente.Documento.TipoInscricao,
            NumeroInscricaoEmpresa = Insercao(remessa.Cedente.Documento, 14),
            Convenio = remessa.Convenio.PadRight(20),
            Agencia = inteiro(conta.Agencia, 5, "Agência"),
            DvAgencia = conta.DvAgencia ?? string.Empty,
            Conta = ContaNoHeaderArquivo(remessa, perfil, conta),
            DvConta = conta.DvConta ?? string.Empty,
            DvAgenciaConta = conta.DvAgenciaConta ?? string.Empty,
            NomeEmpresa = remessa.Cedente.Nome,
            NomeBanco = perfil.NomeBanco,
            CodigoRemessaRetorno = Codigos240.Remessa,
            DataGeracao = remessa.DataGeracao,
            HoraGeracao = int.Parse(remessa.HoraGeracao.ToString("HHmmss", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture),
            NumeroSequencialArquivo = remessa.NumeroSequencialArquivo,
            VersaoLayout = perfil.VersaoLayoutArquivo,
            Densidade = Codigos240.DensidadePadrao,
        });
    }

    private static IEnumerable<string> GerarLoteCobranca(
        Remessa240 remessa,
        PerfilBanco240 perfil,
        LoteCobranca lote,
        int numeroLote)
    {
        var conta = remessa.Cedente.Conta;
        var convenio = ConvenioCobranca(remessa, perfil);
        if (remessa.Banco.Codigo == 33)
        {
            yield return MontarHeaderLoteSantander(remessa, perfil, numeroLote);
        }
        else
        {
            var header = RegistroCnab<HeaderLoteCobranca240>.Escrever(new HeaderLoteCobranca240
            {
                Banco = remessa.Banco.Codigo,
                Lote = numeroLote,
                TipoRegistro = Codigos240.HeaderLote,
                TipoOperacao = perfil.OperacaoCobrancaRemessa.ToString(),
                TipoServico = int.Parse(Codigos240.ServicoCobranca, CultureInfo.InvariantCulture),
                VersaoLayoutLote = perfil.VersaoLayoutLoteCobranca,
                TipoInscricaoEmpresa = remessa.Cedente.Documento.TipoInscricao,
                NumeroInscricaoEmpresa = Insercao(remessa.Cedente.Documento, 15),
                Convenio = convenio,
                Agencia = inteiro(conta.Agencia, 5, "Agência"),
                DvAgencia = conta.DvAgencia ?? string.Empty,
                Conta = longo(conta.Conta, 12, "Conta"),
                DvConta = conta.DvConta ?? string.Empty,
                DvAgenciaConta = conta.DvAgenciaConta ?? string.Empty,
                NomeEmpresa = remessa.Cedente.Nome,
                NumeroRemessaRetorno = remessa.NumeroRemessaRetorno ?? remessa.NumeroSequencialArquivo,
                DataGravacao = remessa.DataGeracao,
            });
            header = PerfilBanco240.Aplicar(header, perfil.AjustesHeaderLoteCobranca);
            if (remessa.Banco.Codigo == 341)
            {
                header = PerfilBanco240.Aplicar(header, AjustesBanco240.HeaderLoteItau(remessa));
            }

            yield return header;
        }

        var sequencial = 0;
        var contadores = new Dictionary<CarteiraCobranca240, (int Quantidade, decimal Valor)>();
        foreach (var titulo in lote.Titulos)
        {
            titulo.Validar();
            sequencial++;
            var movimento = remessa.Opcoes.CodigoMovimentoRemessa;

            var p = new SegmentoP240
            {
                Banco = remessa.Banco.Codigo,
                Lote = numeroLote,
                TipoRegistro = Codigos240.Detalhe,
                Sequencial = sequencial,
                CodigoMovimento = movimento,
                Agencia = inteiro(conta.Agencia, 5, "Agência"),
                DvAgencia = conta.DvAgencia ?? string.Empty,
                Conta = ContaNoSegmentoP(remessa, perfil, conta),
                DvConta = conta.DvConta ?? string.Empty,
                DvAgenciaConta = conta.DvAgenciaConta ?? string.Empty,
                NossoNumero = NossoNumeroNoSegmentoP(remessa, titulo),
                Carteira = Carteira(titulo),
                FormaCadastramento = 1,
                TipoDocumento = "1",
                IdentificacaoEmissao = 2,
                IdentificacaoDistribuicao = "2",
                NumeroDocumento = titulo.NumeroDocumento,
                DataVencimento = titulo.DataVencimento,
                Valor = titulo.Valor,
                Especie = remessa.Opcoes.MapaEspecies[titulo.Especie],
                Aceite = titulo.Aceite ? "S" : "N",
                DataEmissao = titulo.DataEmissao,
                CodigoJurosMora = titulo.Juros is null ? 0 : titulo.Juros.Tipo == TipoJuros.ValorDiario ? 1 : 2,
                DataJurosMora = titulo.Juros?.DataInicio,
                ValorJurosMora = titulo.Juros?.Valor,
                CodigoDesconto = titulo.Desconto is null ? 0 : 1,
                DataDesconto = titulo.Desconto?.DataLimite,
                ValorDesconto = titulo.Desconto?.Valor,
                ValorAbatimento = titulo.Abatimento,
                IdentificacaoTituloEmpresa = titulo.NumeroDocumento,
                CodigoProtesto = CodigoProtesto(titulo.Protesto),
                DiasProtesto = titulo.Protesto?.Dias ?? 0,
                CodigoMoeda = Codigos240.MoedaRealCodigo,
            };
            string linhaP;
            if (remessa.Banco.Codigo == 33)
            {
                linhaP = RegistroCnab<SegmentoPSantander240>.Escrever(
                    MontarSegmentoPSantander(remessa, titulo, numeroLote, sequencial, movimento));
            }
            else
            {
                linhaP = PerfilBanco240.Aplicar(RegistroCnab<SegmentoP240>.Escrever(p), perfil.AjustesSegmentoP);
                if (remessa.Banco.Codigo == 341)
                {
                    linhaP = PerfilBanco240.Aplicar(linhaP, AjustesBanco240.SegmentoPItau(remessa, titulo));
                }
                else if (remessa.Banco.Codigo == 237)
                {
                    var carteiraBradesco = DigitosSomente(titulo.Carteira, 2, "Carteira");
                    linhaP = PerfilBanco240.Aplicar(
                        linhaP,
                        AjustesBanco240.SegmentoPBradesco(remessa.CodigoProduto, carteiraBradesco, titulo));
                }
            }

            yield return linhaP;

            sequencial++;
            var q = MontarSegmentoQ(remessa, perfil, titulo, numeroLote, sequencial, movimento);
            var linhaQ = PerfilBanco240.Aplicar(RegistroCnab<SegmentoQ240>.Escrever(q), perfil.AjustesSegmentoQ);
            if (remessa.Banco.Codigo == 341)
            {
                linhaQ = PerfilBanco240.Aplicar(linhaQ, AjustesBanco240.SegmentoQItau());
            }

            yield return linhaQ;

            if (titulo.Multa is not null || titulo.Instrucoes.Count > 0)
            {
                sequencial++;
                yield return PerfilBanco240.Aplicar(
                    RegistroCnab<SegmentoR240>.Escrever(MontarSegmentoR(
                        remessa, titulo, numeroLote, sequencial, movimento)), perfil.AjustesSegmentoR);
            }

            var carteira = remessa.Opcoes.ClassificarCarteira(titulo);
            var atual = contadores.TryGetValue(carteira, out var valor) ? valor : (Quantidade: 0, Valor: 0m);
            contadores[carteira] = (atual.Quantidade + 1, atual.Valor + titulo.Valor);
        }

        yield return RegistroCnab<TrailerLoteCobranca240>.Escrever(new TrailerLoteCobranca240
        {
            Banco = remessa.Banco.Codigo,
            Lote = numeroLote,
            TipoRegistro = Codigos240.TrailerLote,
            QuantidadeSimples = Quantidade(contadores, CarteiraCobranca240.Simples),
            ValorSimples = Valor(contadores, CarteiraCobranca240.Simples),
            QuantidadeVinculada = Quantidade(contadores, CarteiraCobranca240.Vinculada),
            ValorVinculada = Valor(contadores, CarteiraCobranca240.Vinculada),
            QuantidadeCaucionada = Quantidade(contadores, CarteiraCobranca240.Caucionada),
            ValorCaucionada = Valor(contadores, CarteiraCobranca240.Caucionada),
            QuantidadeDescontada = Quantidade(contadores, CarteiraCobranca240.Descontada),
            ValorDescontada = Valor(contadores, CarteiraCobranca240.Descontada),
            FillerInicial = string.Empty,
            // O trailer de cobrança FEBRABAN não tem campo de quantidade total
            // de registros: ele discrimina quantidades/valores por carteira.
            NumeroAvisoLancamento = string.Empty,
        });
    }

    private static IEnumerable<string> GerarLotePagamento(
        Remessa240 remessa,
        PerfilBanco240 perfil,
        LotePagamento lote,
        int numeroLote)
    {
        var conta = remessa.Cedente.Conta;
        var primeiroTipo = lote.Pagamentos.Count > 0 ? lote.Pagamentos[0].Tipo : TipoPagamento.CreditoEmConta;
        var header = RegistroCnab<HeaderLotePagamento240>.Escrever(new HeaderLotePagamento240
        {
            Banco = remessa.Banco.Codigo,
            Lote = numeroLote,
            TipoRegistro = Codigos240.HeaderLote,
            TipoOperacao = Codigos240.OperacaoPagamento.ToString(),
            TipoServico = int.Parse(Servico(primeiroTipo), CultureInfo.InvariantCulture),
            FormaLancamento = remessa.Opcoes.MapaFormaLancamento[primeiroTipo],
            VersaoLayoutLote = perfil.VersaoLayoutLotePagamento,
            TipoInscricaoEmpresa = remessa.Cedente.Documento.TipoInscricao,
            NumeroInscricaoEmpresa = Insercao(remessa.Cedente.Documento, 14),
            Convenio = remessa.Convenio.PadRight(20),
            Agencia = inteiro(conta.Agencia, 5, "Agência"),
            DvAgencia = conta.DvAgencia ?? string.Empty,
            Conta = longo(conta.Conta, 12, "Conta"),
            DvConta = conta.DvConta ?? string.Empty,
            DvAgenciaConta = conta.DvAgenciaConta ?? string.Empty,
            NomeEmpresa = remessa.Cedente.Nome,
            IndicativoFormaPagamento = 0,
        });
        yield return PerfilBanco240.Aplicar(header, perfil.AjustesHeaderLotePagamento);

        var sequencial = 0;
        decimal soma = 0;
        foreach (var pagamento in lote.Pagamentos)
        {
            pagamento.Validar();
            soma += pagamento.Valor;
            sequencial++;
            switch (pagamento.Tipo)
            {
                case TipoPagamento.BoletoCodigoBarras:
                    foreach (var linha in GerarBoleto(remessa, pagamento, numeroLote, sequencial))
                    {
                        yield return linha;
                    }

                    sequencial++; // J-52 consome o próximo NSR
                    break;
                case TipoPagamento.TributoCodigoBarras:
                    yield return RegistroCnab<SegmentoO240>.Escrever(MontarSegmentoO(
                        remessa, pagamento, numeroLote, sequencial));
                    break;
                default:
                    yield return PerfilBanco240.Aplicar(
                        RegistroCnab<SegmentoA240>.Escrever(MontarSegmentoA(
                            remessa, pagamento, numeroLote, sequencial)), perfil.AjustesSegmentoA);
                    sequencial++;
                    yield return PerfilBanco240.Aplicar(MontarSegmentoB(
                        remessa, perfil, pagamento, numeroLote, sequencial), perfil.AjustesSegmentoB);
                    break;
            }
        }

        // O NSR final é sequencial + 1 quando o último lançamento usou A+B
        // (o incremento extra de B); ajustamos como o maior NSR emitido.
        var quantidadeRegistros = sequencial + 2;
        yield return PerfilBanco240.Aplicar(
            RegistroCnab<TrailerLotePagamento240>.Escrever(new TrailerLotePagamento240
            {
                Banco = remessa.Banco.Codigo,
                Lote = numeroLote,
                TipoRegistro = Codigos240.TrailerLote,
                QuantidadeRegistros = quantidadeRegistros,
                SomatoriaValores = decimal.Round(soma, 2, MidpointRounding.ToEven),
            }), perfil.AjustesTrailerLotePagamento);
    }

    private static IEnumerable<string> GerarBoleto(
        Remessa240 remessa, Pagamento pagamento, int lote, int sequencial)
    {
        var codigo = pagamento.CodigoDeBarras!.Length == LinhaDigitavel.TamanhoDigitos
            ? LinhaDigitavel.ParaCodigoDeBarras(pagamento.CodigoDeBarras)
            : pagamento.CodigoDeBarras;
        var j = RegistroCnab<SegmentoJ240>.Escrever(new SegmentoJ240
        {
            Banco = remessa.Banco.Codigo,
            Lote = lote,
            TipoRegistro = Codigos240.Detalhe,
            Sequencial = sequencial,
            CodigoBarras = codigo,
            NomeBeneficiario = pagamento.NomeFavorecido,
            DataVencimento = CodigoDeBarras.LerVencimento(codigo),
            ValorTitulo = CodigoDeBarras.LerValor(codigo),
            DataPagamento = pagamento.Data,
            ValorPagamento = pagamento.Valor,
            CodigoMoeda = Codigos240.MoedaRealCodigo,
            NumeroDocumentoEmpresa = pagamento.NumeroDocumento,
        });
        yield return j;

        var j52 = RegistroCnab<SegmentoJ52_240>.Escrever(new SegmentoJ52_240
        {
            Banco = remessa.Banco.Codigo,
            Lote = lote,
            TipoRegistro = Codigos240.Detalhe,
            Sequencial = sequencial + 1,
            TipoInscricaoPagador = remessa.Cedente.Documento.TipoInscricao,
            NumeroInscricaoPagador = Insercao(remessa.Cedente.Documento, 15),
            NomePagador = remessa.Cedente.Nome,
            TipoInscricaoBeneficiario = pagamento.DocumentoFavorecido.TipoInscricao,
            NumeroInscricaoBeneficiario = Insercao(pagamento.DocumentoFavorecido, 15),
            NomeBeneficiario = pagamento.NomeFavorecido,
        });
        yield return j52;
    }

    private static SegmentoO240 MontarSegmentoO(
        Remessa240 remessa, Pagamento pagamento, int lote, int sequencial)
    {
        var codigo = pagamento.CodigoDeBarras!.Length == LinhaDigitavel.TamanhoDigitos
            ? LinhaDigitavel.ParaCodigoDeBarras(pagamento.CodigoDeBarras)
            : pagamento.CodigoDeBarras;
        return new SegmentoO240
        {
            Banco = remessa.Banco.Codigo,
            Lote = lote,
            TipoRegistro = Codigos240.Detalhe,
            Sequencial = sequencial,
            CodigoBarras = codigo,
            NomeConcessionaria = pagamento.NomeFavorecido,
            DataVencimento = CodigoDeBarras.LerVencimento(codigo),
            DataPagamento = pagamento.Data,
            ValorPagamento = pagamento.Valor,
            NumeroDocumentoEmpresa = pagamento.NumeroDocumento,
        };
    }

    private static SegmentoA240 MontarSegmentoA(
        Remessa240 remessa, Pagamento pagamento, int lote, int sequencial)
    {
        var tipo = pagamento.Tipo;
        return new SegmentoA240
        {
            Banco = remessa.Banco.Codigo,
            Lote = lote,
            TipoRegistro = Codigos240.Detalhe,
            Sequencial = sequencial,
            CamaraCentralizadora = remessa.Opcoes.MapaCamara.TryGetValue(tipo, out var camara) ? camara : 0,
            BancoFavorecido = pagamento.BancoDestino?.Codigo ?? remessa.Banco.Codigo,
            AgenciaFavorecido = inteiro(pagamento.AgenciaDestino ?? "0", 5, "Agência de destino"),
            ContaFavorecido = longo(pagamento.ContaDestino ?? "0", 12, "Conta de destino"),
            DvContaFavorecido = pagamento.DvContaDestino ?? string.Empty,
            NomeFavorecido = pagamento.NomeFavorecido,
            NumeroDocumentoEmpresa = pagamento.NumeroDocumento,
            DataPagamento = pagamento.Data,
            ValorPagamento = pagamento.Valor,
        };
    }

    private static string MontarSegmentoB(
        Remessa240 remessa, PerfilBanco240 perfil, Pagamento pagamento, int lote, int sequencial)
    {
        var documento = pagamento.DocumentoFavorecido;
        if (perfil.SegmentoBComEndereco)
        {
            return RegistroCnab<SegmentoBEndereco240>.Escrever(new SegmentoBEndereco240
            {
                Banco = remessa.Banco.Codigo,
                Lote = lote,
                TipoRegistro = Codigos240.Detalhe,
                Sequencial = sequencial,
                TipoInscricaoFavorecido = documento.TipoInscricao,
                NumeroInscricaoFavorecido = Insercao(documento, 14),
                CodigoDocumentoFavorecido = documento.Digitos,
            });
        }

        return RegistroCnab<SegmentoB240>.Escrever(new SegmentoB240
        {
            Banco = remessa.Banco.Codigo,
            Lote = lote,
            TipoRegistro = Codigos240.Detalhe,
            Sequencial = sequencial,
            TipoInscricaoFavorecido = documento.TipoInscricao,
            NumeroInscricaoFavorecido = Insercao(documento, 14),
        });
    }

    private static string MontarHeaderArquivoSantander(Remessa240 remessa, PerfilBanco240 perfil) =>
        RegistroCnab<HeaderArquivoSantander240>.Escrever(new HeaderArquivoSantander240
        {
            Banco = remessa.Banco.Codigo,
            Lote = 0,
            TipoRegistro = Codigos240.HeaderArquivo,
            TipoInscricaoEmpresa = remessa.Cedente.Documento.TipoInscricao,
            NumeroInscricaoEmpresa = Insercao(remessa.Cedente.Documento, 15),
            CodigoTransmissao = remessa.Convenio.PadRight(15),
            NomeEmpresa = remessa.Cedente.Nome,
            NomeBanco = perfil.NomeBanco,
            CodigoRemessaRetorno = Codigos240.Remessa,
            DataGeracao = remessa.DataGeracao,
            HoraGeracao = int.Parse(remessa.HoraGeracao.ToString("HHmmss", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture),
            NumeroSequencialArquivo = remessa.NumeroSequencialArquivo,
            VersaoLayout = perfil.VersaoLayoutArquivo,
        });

    private static string MontarHeaderLoteSantander(Remessa240 remessa, PerfilBanco240 perfil, int numeroLote) =>
        RegistroCnab<HeaderLoteCobrancaSantander240>.Escrever(new HeaderLoteCobrancaSantander240
        {
            Banco = remessa.Banco.Codigo,
            Lote = numeroLote,
            TipoRegistro = Codigos240.HeaderLote,
            TipoOperacao = perfil.OperacaoCobrancaRemessa.ToString(),
            TipoServico = 1,
            VersaoLayoutLote = perfil.VersaoLayoutLoteCobranca,
            TipoInscricaoEmpresa = remessa.Cedente.Documento.TipoInscricao,
            NumeroInscricaoEmpresa = Insercao(remessa.Cedente.Documento, 15),
            CodigoTransmissao = remessa.Convenio.PadRight(15),
            NomeBeneficiario = remessa.Cedente.Nome,
            NumeroRemessaRetorno = remessa.NumeroRemessaRetorno ?? remessa.NumeroSequencialArquivo,
            DataGravacao = remessa.DataGeracao,
        });

    private static SegmentoPSantander240 MontarSegmentoPSantander(
        Remessa240 remessa, Titulo titulo, int numeroLote, int sequencial, int movimento)
    {
        var conta = remessa.Cedente.Conta;
        var nosso = new string([.. titulo.NossoNumero.Where(char.IsAsciiDigit)]);
        return new SegmentoPSantander240
        {
            Banco = remessa.Banco.Codigo,
            Lote = numeroLote,
            TipoRegistro = Codigos240.Detalhe,
            Sequencial = sequencial,
            CodigoMovimento = movimento,
            Agencia = inteiro(conta.Agencia, 4, "Agência"),
            DvAgencia = conta.DvAgencia ?? string.Empty,
            Conta = longo(conta.Conta, 9, "Conta"),
            DvConta = conta.DvConta ?? string.Empty,
            NossoNumero = nosso.PadLeft(13, '0'),
            Carteira = Carteira(titulo),
            FormaCadastramento = 1,
            TipoDocumento = "1",
            NumeroDocumento = titulo.NumeroDocumento,
            DataVencimento = titulo.DataVencimento,
            Valor = titulo.Valor,
            Especie = remessa.Opcoes.MapaEspecies[titulo.Especie],
            Aceite = "N",
            DataEmissao = titulo.DataEmissao,
            CodigoJurosMora = titulo.Juros is null ? 0 : titulo.Juros.Tipo == TipoJuros.ValorDiario ? 1 : 2,
            DataJurosMora = titulo.Juros?.DataInicio,
            ValorJurosMora = titulo.Juros?.Valor,
            CodigoDesconto = titulo.Desconto is null ? 0 : 1,
            DataDesconto = titulo.Desconto?.DataLimite,
            ValorDesconto = titulo.Desconto?.Valor,
            ValorAbatimento = titulo.Abatimento,
            IdentificacaoTituloEmpresa = titulo.NumeroDocumento,
            CodigoProtesto = CodigoProtesto(titulo.Protesto),
            DiasProtesto = titulo.Protesto?.Dias ?? 0,
        };
    }

    private static SegmentoQ240 MontarSegmentoQ(
        Remessa240 remessa,
        PerfilBanco240 perfil,
        Titulo titulo,
        int numeroLote,
        int sequencial,
        int movimento)
    {
        var pagador = titulo.Pagador;
        var endereco = pagador.Endereco;
        var cep = endereco is null ? "00000000" : endereco.Cep;
        return new SegmentoQ240
        {
            Banco = remessa.Banco.Codigo,
            Lote = numeroLote,
            TipoRegistro = Codigos240.Detalhe,
            Sequencial = sequencial,
            CodigoMovimento = movimento,
            TipoInscricaoPagador = pagador.Documento.TipoInscricao,
            NumeroInscricaoPagador = Insercao(pagador.Documento, 15),
            NomePagador = pagador.Nome,
            EnderecoPagador = endereco is null ? string.Empty : endereco.Logradouro,
            BairroPagador = endereco?.Bairro ?? string.Empty,
            CepPagador = inteiro(cep[..5], 5, "CEP"),
            SufixoCep = inteiro(cep[5..], 3, "Sufixo do CEP"),
            CidadePagador = endereco?.Cidade ?? string.Empty,
            UfPagador = endereco?.Uf ?? string.Empty,
            TipoInscricaoSacador = titulo.SacadorAvalista is null ? 0 : titulo.SacadorAvalista.Documento.TipoInscricao,
            NumeroInscricaoSacador = titulo.SacadorAvalista is null ? string.Empty : Insercao(titulo.SacadorAvalista.Documento, 15),
            NomeSacador = titulo.SacadorAvalista?.Nome ?? string.Empty,
        };
    }

    private static SegmentoR240 MontarSegmentoR(
        Remessa240 remessa, Titulo titulo, int numeroLote, int sequencial, int movimento) => new()
        {
            Banco = remessa.Banco.Codigo,
            Lote = numeroLote,
            TipoRegistro = Codigos240.Detalhe,
            Sequencial = sequencial,
            CodigoMovimento = movimento,
            CodigoMulta = titulo.Multa is null ? string.Empty : titulo.Multa.Tipo == TipoMulta.Valor ? "1" : "2",
            DataMulta = titulo.Multa?.DataInicio,
            ValorMulta = titulo.Multa?.Valor,
            Mensagem3 = titulo.Instrucoes.Count > 0 ? titulo.Instrucoes[0] : string.Empty,
            Mensagem4 = titulo.Instrucoes.Count > 1 ? titulo.Instrucoes[1] : string.Empty,
        };

    private static int CodigoProtesto(Protesto? protesto) => protesto?.Tipo switch
    {
        null => 0,
        TipoProtesto.Nenhum => 0,
        TipoProtesto.ProtestarDiasCorridos => 1,
        TipoProtesto.CancelarInstrucao => 2,
        TipoProtesto.NegativarDiasCorridos => 9,
        _ => 0,
    };

    private static int Carteira(Titulo titulo)
    {
        var digitos = new string([.. titulo.Carteira.Where(char.IsAsciiDigit)]);
        return digitos.Length == 0 ? 1 : inteiro(digitos[^1..], 1, "Carteira");
    }

    private static string Servico(TipoPagamento tipo) => tipo switch
    {
        TipoPagamento.PixChave or TipoPagamento.PixQrCode or TipoPagamento.Ted or TipoPagamento.Doc
            or TipoPagamento.CreditoEmConta => Codigos240.ServicoPagamentoFornecedores,
        _ => Codigos240.ServicoPagamentoFornecedores,
    };

    private static string ConvenioCobranca(Remessa240 remessa, PerfilBanco240 perfil)
    {
        if (remessa.Banco.Codigo == 104 && !string.IsNullOrWhiteSpace(remessa.CodigoBeneficiario))
        {
            return remessa.CodigoBeneficiario.PadLeft(7, '0') + new string('0', 13);
        }

        return remessa.Convenio.PadRight(20);
    }

    private static long ContaNoSegmentoP(Remessa240 remessa, PerfilBanco240 perfil, ContaBancaria conta)
    {
        if (remessa.Banco.Codigo == 104 && !string.IsNullOrWhiteSpace(remessa.CodigoBeneficiario))
        {
            return long.Parse(
                remessa.CodigoBeneficiario.PadLeft(7, '0') + new string('0', 5),
                CultureInfo.InvariantCulture);
        }

        return longo(conta.Conta, 12, "Conta");
    }

    private static long ContaNoHeaderArquivo(Remessa240 remessa, PerfilBanco240 perfil, ContaBancaria conta)
    {
        if (remessa.Banco.Codigo == 104 && !string.IsNullOrWhiteSpace(remessa.CodigoBeneficiario))
        {
            // SIGCB 67.118 v031: 59-65 = código do beneficiário (7), 66-71 = zeros.
            return long.Parse(
                remessa.CodigoBeneficiario.PadLeft(7, '0') + new string('0', 5),
                CultureInfo.InvariantCulture);
        }

        return longo(conta.Conta, 12, "Conta");
    }

    private static string NossoNumeroNoSegmentoP(Remessa240 remessa, Titulo titulo)
    {
        if (remessa.Banco.Codigo == 104)
        {
            // SIGCB: 38-39 filler, 40 modalidade SINCO, 41-42 modalidade
            // SIGCB, 43-57 identificação do título (15 posições).
            return "  " + "0" + "00" + titulo.NossoNumero.PadLeft(15, '0');
        }

        return titulo.NossoNumero;
    }

    private static string Insercao(DocumentoFiscal documento, int tamanho) =>
        documento.Digitos.PadLeft(tamanho, '0');

    private static string DigitosSomente(string valor, int tamanho, string nome)
    {
        var digitos = new string([.. valor.Where(char.IsAsciiDigit)]);
        if (digitos.Length > tamanho)
        {
            throw new ArgumentException($"{nome} excede {tamanho} dígitos.", nome);
        }

        return digitos.PadLeft(tamanho, '0');
    }

    private static int inteiro(string valor, int tamanho, string nome)
    {
        var digitos = new string([.. valor.Where(char.IsAsciiDigit)]);
        if (digitos.Length == 0)
        {
            return 0;
        }

        if (digitos.Length > tamanho || !long.TryParse(digitos, NumberStyles.None, CultureInfo.InvariantCulture, out var numero)
            || numero > int.MaxValue)
        {
            throw new ArgumentException($"{nome} não cabe em {tamanho} dígitos.", nome);
        }

        return (int)numero;
    }

    private static long longo(string valor, int tamanho, string nome)
    {
        var digitos = new string([.. valor.Where(char.IsAsciiDigit)]);
        if (digitos.Length == 0)
        {
            return 0;
        }

        if (digitos.Length > tamanho || !long.TryParse(digitos, NumberStyles.None, CultureInfo.InvariantCulture, out var numero))
        {
            throw new ArgumentException($"{nome} não cabe em {tamanho} dígitos.", nome);
        }

        return numero;
    }

    private static int Quantidade(Dictionary<CarteiraCobranca240, (int Quantidade, decimal Valor)> map, CarteiraCobranca240 carteira) =>
        map.TryGetValue(carteira, out var valor) ? valor.Quantidade : 0;

    private static decimal Valor(Dictionary<CarteiraCobranca240, (int Quantidade, decimal Valor)> map, CarteiraCobranca240 carteira) =>
        map.TryGetValue(carteira, out var valor) ? valor.Valor : 0m;

    private static void Validar(Remessa240 remessa)
    {
        if (remessa.NumeroSequencialArquivo is < 0 or > 999999)
        {
            throw new ArgumentOutOfRangeException(nameof(remessa), "NSA deve ter até 6 dígitos.");
        }

        remessa.Cedente.Conta.Validar();
        if (remessa.Banco.Codigo == 237 && remessa.LotesCobranca.Count > 0
            && remessa.CodigoProduto.Length == 0)
        {
            throw new ArgumentException("Bradesco exige o código do produto (3 posições) para cobrança.", nameof(remessa));
        }

        foreach (var lote in remessa.LotesCobranca)
        {
            if (lote.Titulos.Count == 0)
            {
                throw new InvalidOperationException("Lote de cobrança sem títulos.");
            }
        }

        foreach (var lote in remessa.LotesPagamento)
        {
            if (lote.Pagamentos.Count == 0)
            {
                throw new InvalidOperationException("Lote de pagamento sem lançamentos.");
            }
        }
    }
}
