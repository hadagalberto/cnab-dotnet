namespace Cnab.Arquivo;

/// <summary>
/// Erro de leitura ou escrita de um campo de um registro CNAB, com posição
/// (linha do arquivo, coluna do registro e nome do campo) para diagnóstico
/// preciso — requisito do SPEC ("precise errors with line/column/field name").
/// </summary>
public sealed class CampoCnabException : Exception
{
    /// <summary>Inicializa a exceção com contexto de localização.</summary>
    /// <param name="mensagem">Descrição do problema.</param>
    /// <param name="campo">Nome do campo, quando conhecido.</param>
    /// <param name="linha">Número da linha no arquivo (1-based); <see langword="null"/> fora de arquivo.</param>
    /// <param name="coluna">Coluna inicial do campo (1-based).</param>
    /// <param name="tamanho">Tamanho do campo em posições.</param>
    public CampoCnabException(
        string mensagem,
        string? campo = null,
        int? linha = null,
        int? coluna = null,
        int? tamanho = null)
        : base(ComporMensagem(mensagem, campo, linha, coluna, tamanho))
    {
        Campo = campo;
        Linha = linha;
        Coluna = coluna;
        Tamanho = tamanho;
    }

    /// <summary>Nome do campo envolvido, quando conhecido.</summary>
    public string? Campo { get; }

    /// <summary>Número da linha no arquivo (1-based); <see langword="null"/> fora de arquivo.</summary>
    public int? Linha { get; }

    /// <summary>Coluna inicial (1-based) do campo no registro.</summary>
    public int? Coluna { get; }

    /// <summary>Tamanho do campo em posições.</summary>
    public int? Tamanho { get; }

    private static string ComporMensagem(
        string mensagem,
        string? campo,
        int? linha,
        int? coluna,
        int? tamanho)
    {
        var localizacao = new List<string>(4);
        if (linha is not null)
        {
            localizacao.Add($"linha {linha}");
        }

        if (coluna is not null)
        {
            localizacao.Add(tamanho is null
                ? $"coluna {coluna}"
                : $"colunas {coluna}-{coluna + tamanho.Value - 1}");
        }

        if (campo is not null)
        {
            localizacao.Add($"campo '{campo}'");
        }

        return localizacao.Count == 0
            ? mensagem
            : $"{mensagem} ({string.Join(", ", localizacao)})";
    }
}

/// <summary>
/// Erro de tamanho de registro: a linha não tem o comprimento exigido pelo
/// layout (240/400 posições).
/// </summary>
public sealed class TamanhoRegistroException : Exception
{
    /// <summary>Inicializa a exceção com tamanhos esperado e recebido.</summary>
    /// <param name="mensagem">Descrição do problema.</param>
    /// <param name="esperado">Tamanho exigido pelo layout.</param>
    /// <param name="encontrado">Tamanho da linha recebida.</param>
    /// <param name="linha">Número da linha no arquivo (1-based); <see langword="null"/> fora de arquivo.</param>
    public TamanhoRegistroException(string mensagem, int esperado, int encontrado, int? linha = null)
        : base(linha is null
            ? $"{mensagem} (esperado {esperado}, encontrado {encontrado})"
            : $"{mensagem} (esperado {esperado}, encontrado {encontrado}, linha {linha})")
    {
        Esperado = esperado;
        Encontrado = encontrado;
        Linha = linha;
    }

    /// <summary>Tamanho exigido pelo layout.</summary>
    public int Esperado { get; }

    /// <summary>Tamanho da linha recebida.</summary>
    public int Encontrado { get; }

    /// <summary>Número da linha no arquivo (1-based); <see langword="null"/> fora de arquivo.</summary>
    public int? Linha { get; }
}
