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
    /// Obtém o perfil de um banco suportado neste marco (001, 033, 104, 756).
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
            104 => Caixa,
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
