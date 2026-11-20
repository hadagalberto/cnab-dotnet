namespace Cnab.Modelo;

/// <summary>
/// Banco participante da compensação (código de 3 dígitos na câmara). Os
/// bancos suportados pelo Cnab.NET são os listados no SPEC (001, 033, 104,
/// 237, 341, 748, 756, 077, 208).
/// </summary>
public sealed record Banco
{
    /// <summary>Inicializa um banco com código e nome.</summary>
    /// <param name="codigo">Código de 3 dígitos na câmara de compensação (por exemplo 237).</param>
    /// <param name="nome">Nome curto do banco (por exemplo "Bradesco").</param>
    public Banco(int codigo, string nome)
    {
        if (codigo is < 1 or > 999)
        {
            throw new ArgumentOutOfRangeException(nameof(codigo), "Código do banco deve ter de 1 a 3 dígitos.");
        }

        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException("Nome do banco é obrigatório.", nameof(nome));
        }

        Codigo = codigo;
        Nome = nome;
    }

    /// <summary>Código de 3 dígitos na câmara de compensação.</summary>
    public int Codigo { get; }

    /// <summary>Nome curto do banco.</summary>
    public string Nome { get; }

    /// <summary>Representação usual "código - nome".</summary>
    /// <returns>Texto formatado.</returns>
    public override string ToString() => $"{Codigo:000} - {Nome}";
}

/// <summary>
/// Catálogo dos bancos com suporte no SPEC, com o código usado nos arquivos
/// CNAB e no código de barras.
/// </summary>
public static class Bancos
{
    /// <summary>Banco do Brasil (001).</summary>
    public static Banco BancoDoBrasil { get; } = new(1, "Banco do Brasil");

    /// <summary>Santander (033).</summary>
    public static Banco Santander { get; } = new(33, "Santander");

    /// <summary>Caixa Econômica Federal (104).</summary>
    public static Banco Caixa { get; } = new(104, "Caixa Econômica Federal");

    /// <summary>Bradesco (237).</summary>
    public static Banco Bradesco { get; } = new(237, "Bradesco");

    /// <summary>Itaú (341).</summary>
    public static Banco Itau { get; } = new(341, "Itaú");

    /// <summary>Sicredi (748).</summary>
    public static Banco Sicredi { get; } = new(748, "Sicredi");

    /// <summary>Sicoob (756).</summary>
    public static Banco Sicoob { get; } = new(756, "Sicoob");

    /// <summary>Banco Inter (077).</summary>
    public static Banco Inter { get; } = new(77, "Inter");

    /// <summary>BTG Pactual (208).</summary>
    public static Banco Btg { get; } = new(208, "BTG Pactual");

    private static readonly IReadOnlyDictionary<int, Banco> Todos = new Dictionary<int, Banco>
    {
        [BancoDoBrasil.Codigo] = BancoDoBrasil,
        [Santander.Codigo] = Santander,
        [Caixa.Codigo] = Caixa,
        [Bradesco.Codigo] = Bradesco,
        [Itau.Codigo] = Itau,
        [Sicredi.Codigo] = Sicredi,
        [Sicoob.Codigo] = Sicoob,
        [Inter.Codigo] = Inter,
        [Btg.Codigo] = Btg,
    };

    /// <summary>Busca um banco suportado pelo código de compensação.</summary>
    /// <param name="codigo">Código de 3 dígitos (1 a 999).</param>
    /// <returns>Banco correspondente.</returns>
    /// <exception cref="ArgumentException">Código sem banco suportado.</exception>
    public static Banco Obter(int codigo) =>
        Todos.TryGetValue(codigo, out var banco)
            ? banco
            : throw new ArgumentException($"Banco {codigo:000} não é suportado pelo Cnab.NET.", nameof(codigo));

    /// <summary>Tenta buscar um banco suportado pelo código de compensação.</summary>
    /// <param name="codigo">Código de 3 dígitos (1 a 999).</param>
    /// <param name="banco">Banco encontrado, quando existir.</param>
    /// <returns><see langword="true"/> quando o código tem banco suportado.</returns>
    public static bool TentarObter(int codigo, out Banco? banco) => Todos.TryGetValue(codigo, out banco);
}
