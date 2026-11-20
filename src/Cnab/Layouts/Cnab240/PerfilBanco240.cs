namespace Cnab.Layouts.Cnab240;

using Cnab.Modelo;

/// <summary>
/// Ajuste de uma faixa de posições aplicado após a escrita do registro base
/// FEBRABAN. Usado para codificar explicitamente as peculiaridades publicadas
/// por cada banco (por exemplo o "código do beneficiário" da CAIXA nas
/// posições 34-40 do header de lote). Posições 1-based.
/// </summary>
public sealed record AjusteCampo240
{
    /// <summary>Inicializa um ajuste.</summary>
    /// <param name="posicao">Posição inicial (1-based).</param>
    /// <param name="conteudo">Conteúdo exato a gravar; truncado/preenchido com espaços até <paramref name="tamanho"/>.</param>
    /// <param name="descricao">Descrição da regra, citando o manual.</param>
    /// <param name="tamanho">Quantidade de posições sobrescritas.</param>
    public AjusteCampo240(int posicao, int tamanho, string conteudo, string descricao)
    {
        if (posicao < 1 || tamanho < 1 || posicao + tamanho - 1 > Codigos240.TamanhoRegistro)
        {
            throw new ArgumentOutOfRangeException(nameof(posicao), "Faixa de ajuste fora do registro de 240 posições.");
        }

        Posicao = posicao;
        Tamanho = tamanho;
        Conteudo = conteudo;
        Descricao = descricao;
    }

    /// <summary>Posição inicial (1-based).</summary>
    public int Posicao { get; }

    /// <summary>Quantidade de posições.</summary>
    public int Tamanho { get; }

    /// <summary>Conteúdo exato.</summary>
    public string Conteudo { get; }

    /// <summary>Descrição da regra e sua fonte.</summary>
    public string Descricao { get; }
}

/// <summary>
/// Perfil de um banco no CNAB 240: constantes de layout e ajustes de posições
/// que implementam as peculiaridades documentadas no manual do banco. Toda
/// alteração em relação ao padrão FEBRABAN é explícita e citada em
/// <c>docs/layouts.md</c>.
/// </summary>
public sealed record PerfilBanco240
{
    /// <summary>
    /// Inicializa o perfil.
    /// </summary>
    /// <param name="banco">Banco.</param>
    /// <param name="nomeBanco">Nome do banco escrito no header de arquivo.</param>
    public PerfilBanco240(Banco banco, string nomeBanco)
    {
        ArgumentNullException.ThrowIfNull(banco);
        Banco = banco;
        NomeBanco = nomeBanco;
    }

    /// <summary>Banco do perfil.</summary>
    public Banco Banco { get; }

    /// <summary>Nome do banco gravado nas posições 103-132 do header de arquivo.</summary>
    public string NomeBanco { get; init; }

    /// <summary>Versão do layout de arquivo (posições 164-166 do header).</summary>
    public int VersaoLayoutArquivo { get; init; } = Codigos240.VersaoLayoutArquivoPadrao;

    /// <summary>Versão do layout de lote de cobrança (posições 14-16 do header de lote).</summary>
    public int VersaoLayoutLoteCobranca { get; init; } = 60;

    /// <summary>Versão do layout de lote de pagamentos.</summary>
    public int VersaoLayoutLotePagamento { get; init; } = 46;

    /// <summary>Operação usada no header de lote de cobrança remessa (G028).</summary>
    public char OperacaoCobrancaRemessa { get; init; } = Codigos240.OperacaoCobranca;

    /// <summary>Operação usada no header de lote de cobrança retorno (G028).</summary>
    public char OperacaoCobrancaRetorno { get; init; } = Codigos240.OperacaoRetornoCobranca;

    /// <summary>Indica que o segmento B usa o layout de endereço (comum em BB/CAIXA/Sicoob).</summary>
    public bool SegmentoBComEndereco { get; init; }

    /// <summary>Ajustes aplicados ao header de arquivo.</summary>
    public IReadOnlyList<AjusteCampo240> AjustesHeaderArquivo { get; init; } = [];

    /// <summary>Ajustes aplicados ao header de lote de cobrança.</summary>
    public IReadOnlyList<AjusteCampo240> AjustesHeaderLoteCobranca { get; init; } = [];

    /// <summary>Ajustes aplicados ao header de lote de pagamentos.</summary>
    public IReadOnlyList<AjusteCampo240> AjustesHeaderLotePagamento { get; init; } = [];

    /// <summary>Ajustes aplicados ao segmento P.</summary>
    public IReadOnlyList<AjusteCampo240> AjustesSegmentoP { get; init; } = [];

    /// <summary>Ajustes aplicados ao segmento Q.</summary>
    public IReadOnlyList<AjusteCampo240> AjustesSegmentoQ { get; init; } = [];

    /// <summary>Ajustes aplicados ao segmento A.</summary>
    public IReadOnlyList<AjusteCampo240> AjustesSegmentoA { get; init; } = [];

    /// <summary>Ajustes aplicados ao segmento B.</summary>
    public IReadOnlyList<AjusteCampo240> AjustesSegmentoB { get; init; } = [];

    /// <summary>Ajustes aplicados ao segmento R.</summary>
    public IReadOnlyList<AjusteCampo240> AjustesSegmentoR { get; init; } = [];

    /// <summary>Ajustes aplicados ao trailer de lote de pagamentos.</summary>
    public IReadOnlyList<AjusteCampo240> AjustesTrailerLotePagamento { get; init; } = [];

    /// <summary>
    /// Obtém o perfil de um banco suportado (001, 033, 104, 756, 237, 341, 748, 077, 208).
    /// </summary>
    /// <param name="banco">Banco.</param>
    /// <returns>Perfil correspondente.</returns>
    /// <exception cref="ArgumentException">Banco sem perfil de CNAB 240 neste marco.</exception>
    public static PerfilBanco240 Obter(Banco banco)
    {
        ArgumentNullException.ThrowIfNull(banco);
        return banco.Codigo switch
        {
            1 => BancoDoBrasil,
            33 => Santander,
            77 => Inter,
            104 => Caixa,
            208 => Btg,
            237 => Bradesco,
            341 => Itau,
            748 => Sicredi,
            756 => Sicoob,
            _ => throw new ArgumentException(
                $"Banco {banco} não tem perfil CNAB 240 implementado neste marco.", nameof(banco)),
        };
    }

    /// <summary>Perfil do Banco do Brasil (001).</summary>
    public static PerfilBanco240 BancoDoBrasil { get; } = new(Bancos.BancoDoBrasil, "BANCO DO BRASIL S.A.")
    {
        // "Particularidades BB — Leiaute CNAB 240" (junho/2026): versão padrão
        // do arquivo 083 (aceita 084) e do lote 042; o BB não trata DV ag/conta.
        VersaoLayoutArquivo = 83,
        VersaoLayoutLoteCobranca = 42,
        SegmentoBComEndereco = true,
        AjustesSegmentoP =
        [
            new(37, 1, " ", "BB: campo 12.3P (DV ag/conta) não tratado — branco ou zero (Particularidades BB)."),
        ],
        AjustesSegmentoQ =
        [
            new(210, 3, new string('0', 3), "BB: 20.3Q (banco correspondente) = zeros."),
            new(213, 20, new string(' ', 20), "BB: 21.3Q (nosso número correspondente) = brancos."),
        ],
        AjustesSegmentoR =
        [
            new(200, 8, new string('0', 8), "BB: 21.3R (código ocorrência pagador) = zeros."),
            new(208, 3, new string('0', 3), "BB: 22.3R (banco débito) = zeros."),
            new(211, 5, new string('0', 5), "BB: 23.3R (agência débito) = zeros."),
            new(217, 12, new string('0', 12), "BB: 25.3R (conta débito) = zeros."),
            new(231, 1, "0", "BB: 28.3R (aviso débito automático) = zero."),
        ],
        AjustesTrailerLotePagamento =
        [
            new(231, 10, new string('0', 10), "BB: ocorrências na remessa de pagamentos = zeros (Particularidades BB PAG)."),
        ],
    };

    /// <summary>Perfil do Santander (033).</summary>
    public static PerfilBanco240 Santander { get; } = new(Bancos.Santander, "Banco Santander")
    {
        // Manual "Cobrança — Layout CNAB 240" H7815 v6 (fevereiro/2023):
        // header de arquivo com 8 posições reservadas (9-16), inscrição em
        // 18-32 (15) e "código de transmissão" em 33-47 (15); versão do
        // arquivo 040 e do lote de cobrança 030. Registros dedicados são
        // usados pelo gerador porque as posições diferem do padrão FEBRABAN.
        VersaoLayoutArquivo = 40,
        VersaoLayoutLoteCobranca = 30,
        SegmentoBComEndereco = false,
    };

    /// <summary>Perfil da Caixa Econômica Federal (104).</summary>
    public static PerfilBanco240 Caixa { get; } = new(Bancos.Caixa, "CAIXA ECONOMICA FEDERAL")
    {
        // "Manual de Leiaute de Arquivo Eletrônico CNAB 240 — SIGCB" (67.118
        // v031): o convênio do header de lote é o código do beneficiário com
        // 7 posições (34-40) seguido de 13 zeros (41-53); no segmento P o
        // código do beneficiário ocupa 24-30.
        AjustesHeaderArquivo =
        [
            new(33, 20, new string('0', 20), "CAIXA: campo 07.0 é uso exclusivo CAIXA — zeros (manual SIGCB)."),
            new(66, 6, new string('0', 6), "CAIXA: 11.0 (66-71) = zeros após o código do beneficiário em 59-65."),
        ],
        // O código do beneficiário (7 posições) é gravado pelo gerador em
        // 34-40 do header de lote e em 24-30 do segmento P; os 5 dígitos
        // restantes da conta no P e os 13 do convênio no header são zeros.
        AjustesHeaderLoteCobranca =
        [
            new(41, 13, new string('0', 13), "CAIXA: uso exclusivo 41-53 — zeros (manual SIGCB)."),
        ],
        AjustesSegmentoP =
        [
            new(31, 7, new string('0', 7), "CAIXA: 11.3P (31-37) = zeros (manual SIGCB 67.118 v031)."),
            new(230, 10, new string('0', 10), "CAIXA: 41.3P (230-239) = zeros (manual SIGCB)."),
        ],
        SegmentoBComEndereco = true,
    };

    /// <summary>Perfil do Sicoob (756).</summary>
    public static PerfilBanco240 Sicoob { get; } = new(Bancos.Sicoob, "SICOOB")
    {
        // "Guia de importação de arquivos CNAB 240" (Sicoob, versão 2.0):
        // versão do layout de arquivo "087"; o segmento B de pagamentos usa
        // o layout de endereço (logradouro 33-62, CEP 118-122, UF 126-127).
        // Guia de Importação de Arquivos CNAB 240 v3.2 (pagamentos): versão
        // do arquivo 087 e do lote 045. Para cobrança, o espelho não oficial
        // do XLS de 03/09/2018 indica 081 e DV ag/conta em branco; a fonte
        // oficial do XLS é protegida por senha — ver docs/layouts.md.
        VersaoLayoutArquivo = 87,
        VersaoLayoutLotePagamento = 45,
        SegmentoBComEndereco = true,
        AjustesHeaderArquivo =
        [
            new(72, 1, "0", "Sicoob: 12.0 (DV ag/conta) = 0 no layout de cobrança (espelho do XLS)."),
        ],
        AjustesHeaderLoteCobranca =
        [
            new(200, 8, new string('0', 8), "Sicoob: 22.1 (200-207) = zeros no layout de cobrança."),
        ],
        AjustesSegmentoP =
        [
            new(59, 1, "0", "Sicoob: 15.3P = 0."),
            new(60, 1, " ", "Sicoob: 16.3P = branco."),
            new(224, 1, "0", "Sicoob: 38.3P (baixa/devolução) = 0."),
            new(228, 2, "09", "Sicoob: 40.3P (moeda) = 09."),
            new(230, 10, new string('0', 10), "Sicoob: 41.3P = zeros para simples com registro."),
        ],
    };

    /// <summary>Perfil do Bradesco (237) — manual 4008.524.0339 v09 / MPO 240 v04.</summary>
    public static PerfilBanco240 Bradesco { get; } = new(Bancos.Bradesco, "BRADESCO")
    {
        // "MPO — Troca de Arquivos 240 Posições" v04 (dezembro/2024) e
        // 4008.524.0339 v09: versão do arquivo 084 e do lote de cobrança 042.
        VersaoLayoutArquivo = 84,
        VersaoLayoutLoteCobranca = 42,
        AjustesTrailerLotePagamento =
        [
            new(231, 10, new string('0', 10), "Bradesco: ocorrências 231-240 = zeros na remessa de pagamentos."),
        ],
    };

    /// <summary>Perfil do Itaú (341) — manual de cobrança CNAB 240, janeiro/2017.</summary>
    public static PerfilBanco240 Itau { get; } = new(Bancos.Itau, "BANCO ITAU SA")
    {
        // "Cobrança Bancária — Layout de Arquivos FEBRABAN 240", janeiro/2017:
        // versão do arquivo 040 (164-166) e do lote 030 (014-016). Agência de
        // 4 posições e conta de 5 posições — ver registros dedicados no gerador.
        VersaoLayoutArquivo = 40,
        VersaoLayoutLoteCobranca = 30,
    };

    /// <summary>Perfil do Sicredi (748) — manual CNAB 240 cobrança (portal do desenvolvedor).</summary>
    public static PerfilBanco240 Sicredi { get; } = new(Bancos.Sicredi, "SICREDI")
    {
        // "CNAB 240 — Cobrança Sicredi": versão do arquivo 081 e do lote 040;
        // convênio em branco (não usado) e agência sem DV.
        VersaoLayoutArquivo = 81,
        VersaoLayoutLoteCobranca = 40,
        AjustesHeaderArquivo =
        [
            new(33, 20, new string(' ', 20), "Sicredi: convênio 33-52 em branco (o banco não usa convênio)."),
            new(58, 1, " ", "Sicredi: DV da agência (58) em branco."),
            new(72, 1, " ", "Sicredi: DV ag/conta (72) em branco."),
        ],
        AjustesHeaderLoteCobranca =
        [
            new(34, 20, new string(' ', 20), "Sicredi: convênio 34-53 em branco."),
            new(59, 1, " ", "Sicredi: DV da agência (59) em branco."),
            new(73, 1, " ", "Sicredi: DV ag/conta (73) em branco."),
        ],
        AjustesSegmentoP =
        [
            new(23, 1, " ", "Sicredi: DV da agência (23) em branco."),
            new(37, 1, " ", "Sicredi: DV ag/coop (37) em branco."),
            new(224, 1, "1", "Sicredi: código de baixa/devolução = 1 (usar sempre)."),
            new(225, 3, "000", "Sicredi: prazo de baixa/devolução = 000 (não usado)."),
        ],
    };

    /// <summary>Perfil do Banco Inter (077) — manual CNAB 240 pagamentos V1.21.</summary>
    public static PerfilBanco240 Inter { get; } = new(Bancos.Inter, "BANCO INTER")
    {
        // "Manual CNAB240 Pagamentos" V1.21 (10/02/2026): versão do arquivo
        // 107 e do lote 046; agência única 00001 com DV 9. O Inter não tem
        // layout CNAB 240 de cobrança — a cobrança é CNAB 400 (vide Cnab400).
        VersaoLayoutArquivo = 107,
        VersaoLayoutLotePagamento = 46,
        AjustesHeaderArquivo =
        [
            new(33, 20, new string(' ', 20), "Inter: posições 33-52 em branco (manual V1.21)."),
            new(53, 5, "00001", "Inter: agência única 00001 (manual V1.21)."),
            new(58, 1, "9", "Inter: DV da agência 9 (manual V1.21)."),
            new(72, 1, " ", "Inter: posição 72 em branco (manual V1.21)."),
        ],
        AjustesHeaderLotePagamento =
        [
            new(33, 20, new string(' ', 20), "Inter: posições 33-52 em branco (manual V1.21)."),
            new(53, 5, "00001", "Inter: agência única 00001."),
            new(58, 1, "9", "Inter: DV da agência 9."),
            new(72, 1, " ", "Inter: posição 72 em branco."),
        ],
        AjustesTrailerLotePagamento =
        [
            new(231, 10, new string(' ', 10), "Inter: ocorrências 231-240 preenchidas pelo banco no retorno."),
        ],
    };

    /// <summary>Perfil do BTG Pactual (208) — manual CNAB 240 FEBRABAN 10.9.</summary>
    public static PerfilBanco240 Btg { get; } = new(Bancos.Btg, "BTG PACTUAL")
    {
        // Portal do desenvolvedor BTG: FEBRABAN 10.9 suportada; versão do
        // arquivo 103 e do lote 060 com carteira/cadastramento fixos em 1,
        // tipo de documento 2, emissão e distribuição 1 (Origem/ERP, 11/11/2026).
        VersaoLayoutArquivo = 103,
        VersaoLayoutLoteCobranca = 60,
        AjustesSegmentoP =
        [
            new(58, 1, "1", "BTG: carteira fixa 1."),
            new(59, 1, "1", "BTG: forma de cadastramento 1."),
            new(60, 1, "2", "BTG: tipo de documento 2."),
            new(61, 1, "1", "BTG: emissão do boleto 1."),
            new(62, 1, "1", "BTG: distribuição do boleto 1."),
        ],
    };

    /// <summary>
    /// Aplica os ajustes deste perfil a uma linha já escrita, substituindo
    /// exatamente as faixas declaradas.
    /// </summary>
    /// <param name="linha">Linha de 240 posições.</param>
    /// <param name="ajustes">Ajustes a aplicar.</param>
    /// <returns>Nova linha com os ajustes aplicados.</returns>
    public static string Aplicar(string linha, IReadOnlyList<AjusteCampo240> ajustes)
    {
        ArgumentNullException.ThrowIfNull(linha);
        if (linha.Length != Codigos240.TamanhoRegistro)
        {
            throw new ArgumentException("A linha deve ter exatamente 240 posições.", nameof(linha));
        }

        if (ajustes.Count == 0)
        {
            return linha;
        }

        var caracteres = linha.ToCharArray();
        foreach (var ajuste in ajustes)
        {
            var conteudo = ajuste.Conteudo.Length >= ajuste.Tamanho
                ? ajuste.Conteudo[..ajuste.Tamanho]
                : ajuste.Conteudo.PadRight(ajuste.Tamanho, ' ');
            conteudo.CopyTo(0, caracteres, ajuste.Posicao - 1, ajuste.Tamanho);
        }

        return new string(caracteres);
    }
}
