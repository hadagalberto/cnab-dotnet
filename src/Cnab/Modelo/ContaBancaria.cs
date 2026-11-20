namespace Cnab.Modelo;

/// <summary>
/// Conta bancária do cedente na ótica CNAB: agência, conta e dígitos
/// verificadores. Os DVs de agência/conta têm algoritmos por banco; quando o
/// banco não publica o algoritmo (por exemplo Bradesco, que fornece o
/// "DC" na contratação, ou Santander, que usa o "código do beneficiário
/// padrão"/PSK), os valores são fornecidos pelo banco e não calculados aqui —
/// cada classe de banco em <c>Cnab.Bancos</c> documenta o que é calculável.
/// <para>FEBRABAN G009/G010/G011/G012 (Layout Padrão CNAB 240 V10.9, campos
/// genéricos): DV da agência, número da conta, DV da conta e DV ag/conta.</para>
/// </summary>
public sealed record ContaBancaria
{
    /// <summary>Banco da conta.</summary>
    public required Banco Banco { get; init; }

    /// <summary>Agência (prefixo), sem DV, apenas dígitos.</summary>
    public required string Agencia { get; init; }

    /// <summary>DV da agência, quando o banco usar; fornecido pelo banco quando não calculável.</summary>
    public string? DvAgencia { get; init; }

    /// <summary>Conta corrente, sem DV, apenas dígitos.</summary>
    public required string Conta { get; init; }

    /// <summary>DV da conta, quando o banco usar.</summary>
    public string? DvConta { get; init; }

    /// <summary>DV agência/conta, quando o banco usar campo único (por exemplo Itaú).</summary>
    public string? DvAgenciaConta { get; init; }

    /// <summary>
    /// Código de operação/portabilidade da conta (por exemplo operação
    /// Caixa "003").
    /// </summary>
    public string? Operacao { get; init; }

    /// <summary>Valida formatos: agência e conta apenas dígitos.</summary>
    /// <exception cref="InvalidOperationException">Campos com conteúdo inválido.</exception>
    public void Validar()
    {
        if (string.IsNullOrWhiteSpace(Agencia) || !Agencia.All(char.IsDigit))
        {
            throw new InvalidOperationException($"Agência '{Agencia}' inválida: use apenas dígitos, sem DV.");
        }

        if (string.IsNullOrWhiteSpace(Conta) || !Conta.All(char.IsDigit))
        {
            throw new InvalidOperationException($"Conta '{Conta}' inválida: use apenas dígitos, sem DV.");
        }
    }
}
