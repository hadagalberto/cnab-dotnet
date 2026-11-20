namespace Cnab.Modelo;

/// <summary>
/// Lote de cobrança: um cedente e seus títulos para a mesma carteira/banco.
/// O trailer do lote (quantidade de registros e soma dos valores) é derivado
/// automaticamente — requisito do SPEC ("trailers with automatic counts and
/// sums").
/// </summary>
public sealed record LoteCobranca
{
    /// <summary>Cedente/beneficiário dono do lote.</summary>
    public required Cedente Cedente { get; init; }

    /// <summary>Número do lote (sequencial no arquivo; o header do arquivo usa 0000).</summary>
    public required int Numero { get; init; }

    /// <summary>Títulos do lote.</summary>
    public required IReadOnlyList<Titulo> Titulos { get; init; }

    /// <summary>
    /// Calcula os totais do lote: quantidade de títulos e soma exata dos
    /// valores (decimal, sem arredondamento).
    /// </summary>
    /// <returns>Totais calculados.</returns>
    /// <exception cref="InvalidOperationException">Lote vazio.</exception>
    public TotaisLote CalcularTotais()
    {
        if (Titulos.Count == 0)
        {
            throw new InvalidOperationException("Lote de cobrança sem títulos.");
        }

        decimal soma = 0;
        foreach (var titulo in Titulos)
        {
            soma += titulo.Valor;
        }

        return new TotaisLote(Titulos.Count, soma);
    }
}

/// <summary>
/// Lote de pagamento a fornecedores (TED/DOC/Pix/boleto/tributo).
/// </summary>
public sealed record LotePagamento
{
    /// <summary>Empresa pagadora (cédente no pagamento).</summary>
    public required Cedente Pagadora { get; init; }

    /// <summary>Número do lote (sequencial no arquivo).</summary>
    public required int Numero { get; init; }

    /// <summary>Pagamentos do lote.</summary>
    public required IReadOnlyList<Pagamento> Pagamentos { get; init; }

    /// <summary>
    /// Calcula os totais do lote de pagamento: quantidade e soma exata dos valores.
    /// </summary>
    /// <returns>Totais calculados.</returns>
    /// <exception cref="InvalidOperationException">Lote vazio.</exception>
    public TotaisLote CalcularTotais()
    {
        if (Pagamentos.Count == 0)
        {
            throw new InvalidOperationException("Lote de pagamento sem lançamentos.");
        }

        decimal soma = 0;
        foreach (var pagamento in Pagamentos)
        {
            soma += pagamento.Valor;
        }

        return new TotaisLote(Pagamentos.Count, soma);
    }
}

/// <summary>
/// Totais do trailer de lote: quantidade de registros de detalhe e soma dos
/// valores (FEBRABAN V10.9, registro trailer de lote: "Quantidade de Registros
/// do Lote" e "Somatória dos Valores").
/// </summary>
public sealed record TotaisLote
{
    /// <summary>Inicializa os totais.</summary>
    /// <param name="quantidadeRegistros">Quantidade de registros de detalhe no lote.</param>
    /// <param name="somaValores">Soma exata dos valores dos lançamentos.</param>
    public TotaisLote(int quantidadeRegistros, decimal somaValores)
    {
        QuantidadeRegistros = quantidadeRegistros;
        SomaValores = somaValores;
    }

    /// <summary>Quantidade de registros de detalhe no lote.</summary>
    public int QuantidadeRegistros { get; }

    /// <summary>Soma exata dos valores (2 decimais, aritmética decimal).</summary>
    public decimal SomaValores { get; }
}

/// <summary>
/// Totais do trailer de arquivo: quantidade de lotes e quantidade total de
/// registros (FEBRABAN V10.9, trailer de arquivo).
/// </summary>
public sealed record TotaisArquivo
{
    /// <summary>Inicializa os totais.</summary>
    /// <param name="quantidadeLotes">Quantidade de lotes do arquivo.</param>
    /// <param name="quantidadeRegistros">Quantidade total de registros do arquivo (incluindo headers e trailers).</param>
    public TotaisArquivo(int quantidadeLotes, int quantidadeRegistros)
    {
        QuantidadeLotes = quantidadeLotes;
        QuantidadeRegistros = quantidadeRegistros;
    }

    /// <summary>Quantidade de lotes do arquivo.</summary>
    public int QuantidadeLotes { get; }

    /// <summary>Quantidade total de registros do arquivo.</summary>
    public int QuantidadeRegistros { get; }
}

/// <summary>
/// Utilitário para calcular os totais de trailer a partir dos lotes: o
/// arquivo tem header + lotes + trailer, cada lote tem header + detalhes +
/// trailer. Os contadores seguem o formato FEBRABAN V10.9 (contagem de
/// registros do lote inclui header e trailer do próprio lote).
/// </summary>
public static class CalculadoraDeTotais
{
    /// <summary>
    /// Calcula os totais de arquivo a partir das quantidades de registros de
    /// cada lote (detalhe + header + trailer de lote por lote) mais o header e
    /// o trailer de arquivo.
    /// </summary>
    /// <param name="registrosPorLote">Quantidade de registros de cada lote, incluindo header e trailer do lote.</param>
    /// <returns>Totais do arquivo.</returns>
    /// <exception cref="ArgumentException">Nenhum lote informado.</exception>
    public static TotaisArquivo CalcularTotaisArquivo(IReadOnlyList<int> registrosPorLote)
    {
        ArgumentNullException.ThrowIfNull(registrosPorLote);
        if (registrosPorLote.Count == 0)
        {
            throw new ArgumentException("O arquivo precisa de pelo menos um lote.", nameof(registrosPorLote));
        }

        var total = 2 + registrosPorLote.Sum();
        return new TotaisArquivo(registrosPorLote.Count, total);
    }

    /// <summary>
    /// Quantidade de registros de um lote de cobrança: header de lote + 1
    /// registro por título (+1 por segmento adicional em layouts que usem
    /// segmentos Q/R) + trailer de lote. No nível do domínio, cada título
    /// contribui com um registro de detalhe.
    /// </summary>
    /// <param name="lote">Lote de cobrança.</param>
    /// <returns>Quantidade de registros do lote.</returns>
    public static int QuantidadeRegistros(LoteCobranca lote) => 2 + lote.Titulos.Count;

    /// <summary>Quantidade de registros de um lote de pagamento (header + detalhes + trailer).</summary>
    /// <param name="lote">Lote de pagamento.</param>
    /// <returns>Quantidade de registros do lote.</returns>
    public static int QuantidadeRegistros(LotePagamento lote) => 2 + lote.Pagamentos.Count;
}
