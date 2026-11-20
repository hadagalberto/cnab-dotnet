namespace Cnab.Layouts.Cnab240;

/// <summary>
/// Códigos fixos e tabelas normativas do padrão FEBRABAN CNAB 240 posições
/// (versão 10.9, 14/10/2021). Nenhum valor aqui é inventado: as posições e os
/// domínios vêm do manual oficial citado em <c>docs/layouts.md</c>.
/// </summary>
public static class Codigos240
{
    /// <summary>Comprimento de todo registro CNAB 240 (FEBRABAN V10.9, "Tamanho do Registro").</summary>
    public const int TamanhoRegistro = 240;

    /// <summary>Tipo de registro "header de arquivo" (FEBRABAN G003).</summary>
    public const int HeaderArquivo = 0;

    /// <summary>Tipo de registro "header de lote" (FEBRABAN G003).</summary>
    public const int HeaderLote = 1;

    /// <summary>Tipo de registro "detalhe" (segmentos; FEBRABAN G003).</summary>
    public const int Detalhe = 3;

    /// <summary>Tipo de registro "trailer de lote" (FEBRABAN G003).</summary>
    public const int TrailerLote = 5;

    /// <summary>Tipo de registro "trailer de arquivo" (FEBRABAN G003).</summary>
    public const int TrailerArquivo = 9;

    /// <summary>Código "remessa" (empresa → banco), FEBRABAN G015.</summary>
    public const int Remessa = 1;

    /// <summary>Código "retorno" (banco → empresa), FEBRABAN G015.</summary>
    public const int Retorno = 2;

    /// <summary>Identificador do segmento P (cobrança, remessa; FEBRABAN G039).</summary>
    public const char SegmentoP = 'P';

    /// <summary>Identificador do segmento Q (cobrança, remessa; FEBRABAN G039).</summary>
    public const char SegmentoQ = 'Q';

    /// <summary>Identificador do segmento R (cobrança, remessa; FEBRABAN G039).</summary>
    public const char SegmentoR = 'R';

    /// <summary>Identificador do segmento T (cobrança, retorno; FEBRABAN G039).</summary>
    public const char SegmentoT = 'T';

    /// <summary>Identificador do segmento U (cobrança, retorno; FEBRABAN G039).</summary>
    public const char SegmentoU = 'U';

    /// <summary>Identificador do segmento A (pagamentos; FEBRABAN G039).</summary>
    public const char SegmentoA = 'A';

    /// <summary>Identificador do segmento B (pagamentos; FEBRABAN G039).</summary>
    public const char SegmentoB = 'B';

    /// <summary>Identificador do segmento J (pagamento de títulos; FEBRABAN G039).</summary>
    public const char SegmentoJ = 'J';

    /// <summary>Identificador do segmento O (pagamento de contas/tributos; FEBRABAN G039).</summary>
    public const char SegmentoO = 'O';

    /// <summary>Identificação fixa "52" do registro opcional J-52 (FEBRABAN G067).</summary>
    public const int IdentificadorJ52 = 52;

    /// <summary>Tipo de moeda "BRL" na forma alfanumérica (FEBRABAN G040).</summary>
    public const string MoedaRealAlfa = "BRL";

    /// <summary>Código FEBRABAN da moeda real (G065) usado em J/retorno: "09".</summary>
    public const int MoedaRealCodigo = 9;

    /// <summary>Operação FEBRABAN de crédito/pagamento (G028); no header de pagamentos é "C".</summary>
    public const char OperacaoPagamento = 'C';

    /// <summary>Operação FEBRABAN de cobrança (G028), conforme manual de cada banco ("R").</summary>
    public const char OperacaoCobranca = 'R';

    /// <summary>Operação FEBRABAN de retorno de cobrança (G028), conforme manual de cada banco ("T").</summary>
    public const char OperacaoRetornoCobranca = 'T';

    /// <summary>Tipo de serviço FEBRABAN "títulos em cobrança" (G025).</summary>
    public const string ServicoCobranca = "01";

    /// <summary>Tipo de serviço FEBRABAN "pagamento a fornecedores" (G025).</summary>
    public const string ServicoPagamentoFornecedores = "20";

    /// <summary>Tipo de serviço FEBRABAN "folha de pagamento" (G025).</summary>
    public const string ServicoFolhaPagamento = "30";

    /// <summary>Versão do layout de lote para cobrança no padrão FEBRABAN V10.9 (G030): "060".</summary>
    public const string VersaoLayoutLoteCobranca = "060";

    /// <summary>Versão do layout de lote para crédito em conta/TED/Pix (G030): "046".</summary>
    public const string VersaoLayoutLotePagamento = "046";

    /// <summary>
    /// Densidade de gravação (G020): domínio FEBRABAN "1600 BPI" ou "6250 BPI".
    /// Os manuais escrevem 01600 (1.600), não 16.000 — ver docs/layouts.md.
    /// </summary>
    public const int DensidadePadrao = 1600;

    /// <summary>Versão do layout de arquivo (G019) no padrão FEBRABAN V10.9: "103".</summary>
    public const int VersaoLayoutArquivoPadrao = 103;
}

/// <summary>
/// Códigos de movimento de remessa (FEBRABAN C004), usados nos segmentos P/Q/R.
/// </summary>
public enum CodigoMovimentoRemessa
{
    /// <summary>01 — Entrada de títulos.</summary>
    EntradaTitulos = 1,

    /// <summary>02 — Pedido de baixa.</summary>
    PedidoBaixa = 2,

    /// <summary>03 — Protesto.</summary>
    Protesto = 3,

    /// <summary>04 — Concessão de abatimento.</summary>
    ConcessaoAbatimento = 4,

    /// <summary>05 — Cancelamento de abatimento.</summary>
    CancelamentoAbatimento = 5,

    /// <summary>06 — Alteração de vencimento.</summary>
    AlteracaoVencimento = 6,

    /// <summary>07 — Concessão de desconto.</summary>
    ConcessaoDesconto = 7,

    /// <summary>08 — Cancelamento de desconto.</summary>
    CancelamentoDesconto = 8,

    /// <summary>09 — Protestar (agrupamento em cartório).</summary>
    Protestar = 9,

    /// <summary>10 — Sustar protesto e baixar título.</summary>
    SustarProtestoBaixar = 10,

    /// <summary>11 — Sustar protesto e manter em carteira.</summary>
    SustarProtestoManter = 11,

    /// <summary>12 — Alteração de juros de mora.</summary>
    AlteracaoJurosMora = 12,

    /// <summary>13 — Dispensar cobrança de juros de mora.</summary>
    DispensarJurosMora = 13,

    /// <summary>14 — Alteração do valor de desconto.</summary>
    AlteracaoValorDesconto = 14,

    /// <summary>15 — Não conceder desconto.</summary>
    NaoConcederDesconto = 15,

    /// <summary>16 — Alteração do prazo de desconto.</summary>
    AlteracaoPrazoDesconto = 16,

    /// <summary>17 — Não cobrar juros de mora.</summary>
    NaoCobrarJurosMora = 17,

    /// <summary>18 — Alteração da espécie.</summary>
    AlteracaoEspecie = 18,

    /// <summary>30 — Alteração de outros dados.</summary>
    AlteracaoOutrosDados = 30,

    /// <summary>31 — Alteração de dados do pagador.</summary>
    AlteracaoDadosPagador = 31,

    /// <summary>45 — Negativação sem protesto.</summary>
    NegativacaoSemProtesto = 45,

    /// <summary>75 — Excluir negativação e manter em carteira.</summary>
    ExcluirNegativacaoManter = 75,

    /// <summary>76 — Excluir negativação e baixar.</summary>
    ExcluirNegativacaoBaixar = 76,
}

/// <summary>
/// Família de uma ocorrência de retorno, usada para agrupar os códigos em
/// categorias de negócio (liquidação, entrada confirmada, rejeição etc.).
/// </summary>
public enum FamiliaOcorrencia
{
    /// <summary>Confirmação de entrada de título.</summary>
    EntradaConfirmada,

    /// <summary>Entrada rejeitada pelo banco.</summary>
    EntradaRejeitada,

    /// <summary>Liquidação do título.</summary>
    Liquidacao,

    /// <summary>Baixa do título.</summary>
    Baixa,

    /// <summary>Débito de tarifa ou custas.</summary>
    Tarifa,

    /// <summary>Rejeição de instrução ou alteração.</summary>
    Rejeicao,

    /// <summary>Cancelamento/sustação de instrução.</summary>
    Cancelamento,

    /// <summary>Ocorrência informativa sem outra classificação.</summary>
    Informativo,
}

/// <summary>Uma ocorrência de retorno: código, descrição e família.</summary>
public sealed record OcorrenciaRetorno
{
    /// <summary>Inicializa uma ocorrência.</summary>
    /// <param name="codigo">Código (dois dígitos) publicado no manual.</param>
    /// <param name="descricao">Descrição literal do manual.</param>
    /// <param name="familia">Familia de negócio da ocorrência.</param>
    public OcorrenciaRetorno(string codigo, string descricao, FamiliaOcorrencia familia)
    {
        Codigo = codigo;
        Descricao = descricao;
        Familia = familia;
    }

    /// <summary>Código da ocorrência.</summary>
    public string Codigo { get; }

    /// <summary>Descrição publicada no manual.</summary>
    public string Descricao { get; }

    /// <summary>Família de negócio.</summary>
    public FamiliaOcorrencia Familia { get; }
}
