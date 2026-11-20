namespace Cnab.Boleto;

/// <summary>
/// Fator de vencimento do código de barras (posições 6 a 9): número de 4
/// dígitos que representa a quantidade de dias entre a data-base e o
/// vencimento do título.
/// <para>
/// Ciclo original: data-base <c>07/10/1997</c> — o fator 1000 corresponde ao
/// vencimento 03/07/2000 (Bradesco 4008-524-0121, seção "Fator de
/// Vencimento"; Santander <c>Código de Barras v34</c>: "quantidade de dias
/// entre 07/10/1997 até a data de vencimento"; Sicoob: "fator de vencimento =
/// (data de vencimento) - (03/07/2000) + 1000"; Sicredi: "data base fixada em
/// 07.10.1997").
/// </para>
/// <para>
/// Reinício de fevereiro de 2025 (Comunicado FEBRABAN FB-009/2023; reproduzido
/// em manuais oficiais de banco, por exemplo Banpara "Manual de Procedimento
/// de Impressão de Boletos por Terceiros – Cobrança Registrada v1.2", Anexos
/// 02 e 03: https://www.banpara.b.br/PortalImagens/2tdgh54u/manual-de-procedimento-de-impressao-de-boletos-por-terceiros-cobranca-banpara-v1-2.pdf ):
/// o fator atinge 9999 em 21/02/2025 e,
/// a partir de <c>22/02/2025</c>, retorna a 1000, somando 1 a cada dia — a
/// data de 22/02/2025 passa a ser a nova data-base do ciclo.
/// Tabelas publicadas (Bradesco, Itaú Anexo 6, Caixa Anexo II, Sicredi 10.7):
/// 03/07/2000 → 1000, 05/07/2000 → 1002, 01/05/2002 → 1667, 17/11/2010 → 4789,
/// 21/02/2025 → 9999, 22/02/2025 → 1000, 23/02/2025 → 1001, 24/02/2025 → 1002;
/// ciclo novo (Caixa): 21/12/2026 → 1667, 09/07/2035 → 4789, 13/10/2049 → 9999.
/// </para>
/// <para>
/// Títulos sem vencimento (contra apresentação) usam o fator 0000 (Bradesco,
/// "Sem Fator de Vencimento"; Caixa SIGCB Anexo II: "quando a primeira posição
/// do campo valor for zero, não consta o fator de vencimento").
/// </para>
/// </summary>
public static class FatorDeVencimento
{
    /// <summary>Data-base do ciclo original: 07/10/1997 (fator 1000 em 03/07/2000).</summary>
    public static readonly DateOnly DataBaseOriginal = new(1997, 10, 7);

    /// <summary>
    /// Data-base do novo ciclo a partir de 22/02/2025 (fator 1000). O manual
    /// Banpara citado acima expressa a mesma regra com data-base 29/05/2022,
    /// pois 29/05/2022 + 1000 dias = 22/02/2025.
    /// </summary>
    public static readonly DateOnly NovaDataBase = new(2025, 2, 22);

    /// <summary>Último vencimento do ciclo original (fator 9999).</summary>
    public static readonly DateOnly UltimoVencimentoCicloOriginal = new(2025, 2, 21);

    /// <summary>
    /// Calcula o fator de vencimento de 4 dígitos.
    /// </summary>
    /// <param name="vencimento">Data de vencimento; <see langword="null"/> para títulos sem vencimento (fator 0000).</param>
    /// <returns>Fator entre 1000 e 9999, ou 0 quando não há vencimento.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Vencimento anterior à data-base original (07/10/1997).</exception>
    public static int Calcular(DateOnly? vencimento)
    {
        if (vencimento is null)
        {
            return 0;
        }

        var data = vencimento.Value;
        if (data < DataBaseOriginal)
        {
            throw new ArgumentOutOfRangeException(
                nameof(vencimento),
                "Vencimento anterior à data-base original 07/10/1997 não possui fator de vencimento.");
        }

        return data >= NovaDataBase
            ? 1000 + (data.DayNumber - NovaDataBase.DayNumber)
            : data.DayNumber - DataBaseOriginal.DayNumber;
    }

    /// <summary>
    /// Converte um fator de vencimento de volta para a data de vencimento
    /// correspondente. Fator 0 (título sem vencimento) devolve
    /// <see langword="null"/>.
    /// </summary>
    /// <param name="fator">Fator de vencimento (0 ou 1000 a 9999).</param>
    /// <returns>Data de vencimento do fator, no ciclo em que ele cai.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Fator fora do domínio 0/1000-9999.</exception>
    public static DateOnly? ParaVencimento(int fator)
    {
        if (fator == 0)
        {
            return null;
        }

        if (fator is < 1000 or > 9999)
        {
            throw new ArgumentOutOfRangeException(
                nameof(fator),
                "Fator de vencimento deve ser 0 (sem vencimento) ou estar entre 1000 e 9999.");
        }

        return fator <= UltimoVencimentoCicloOriginal.DayNumber - DataBaseOriginal.DayNumber
            ? DateOnly.FromDayNumber(DataBaseOriginal.DayNumber + fator)
            : DateOnly.FromDayNumber(NovaDataBase.DayNumber + (fator - 1000));
    }
}
