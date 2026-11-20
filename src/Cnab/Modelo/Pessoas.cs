namespace Cnab.Modelo;

/// <summary>
/// Endereço usado para pagadores, beneficiários e sacadores avalistas nos
/// arquivos de cobrança.
/// </summary>
public sealed record Endereco
{
    /// <summary>Logradouro (sem número).</summary>
    public required string Logradouro { get; init; }

    /// <summary>Número do imóvel.</summary>
    public string? Numero { get; init; }

    /// <summary>Complemento.</summary>
    public string? Complemento { get; init; }

    /// <summary>Bairro/distrito.</summary>
    public required string Bairro { get; init; }

    /// <summary>Cidade.</summary>
    public required string Cidade { get; init; }

    /// <summary>Unidade federativa (sigla de 2 letras).</summary>
    public required string Uf { get; init; }

    /// <summary>CEP com 8 dígitos (sem hífen).</summary>
    public required string Cep { get; init; }

    /// <summary>Valida o endereço, exigindo UF em caixa alta e CEP com 8 dígitos.</summary>
    /// <exception cref="InvalidOperationException">UF ou CEP inválidos.</exception>
    public void Validar()
    {
        if (Uf.Length != 2 || !Uf.All(char.IsAsciiLetterUpper))
        {
            throw new InvalidOperationException($"UF '{Uf}' inválida: use a sigla com 2 letras maiúsculas.");
        }

        if (Cep.Length != 8 || !Cep.All(char.IsDigit))
        {
            throw new InvalidOperationException($"CEP '{Cep}' inválido: use 8 dígitos, sem hífen.");
        }
    }
}

/// <summary>
/// Dados do cedente junto a uma cooperativa (Sicredi 748 / Sicoob 756),
/// necessários para calcular o DV do nosso número. O DV desses bancos usa
/// cooperativa, posto e código do beneficiário (Sicredi) ou cooperativa e
/// código do cliente (Sicoob) — não apenas os dígitos do nosso número.
/// Fontes: Sicredi "CNAB 240 – Cobrança" itens 4.4/4.5; Sicoob
/// "Layouts para troca de informações — Cobrança" item 3.13.
/// </summary>
public sealed record DadosCooperativa
{
    /// <summary>Cooperativa/agência, 4 dígitos (Sicredi e Sicoob).</summary>
    public string Cooperativa { get; init; } = string.Empty;

    /// <summary>Posto, 2 dígitos — "00" quando o posto for alfanumérico (Sicredi).</summary>
    public string Posto { get; init; } = string.Empty;

    /// <summary>Código do beneficiário, 5 dígitos (Sicredi).</summary>
    public string CodigoBeneficiario { get; init; } = string.Empty;

    /// <summary>Código do cliente/associado, 10 dígitos (Sicoob).</summary>
    public string CodigoCliente { get; init; } = string.Empty;
}

/// <summary>
/// Beneficiário/cedente da cobrança: a empresa que emite os títulos. FEBRABAN
/// usa "Beneficiário" (cobrança) e "Empresa Cliente" (pagamentos); o SPEC usa
/// "Cedente/Beneficiário".
/// </summary>
public sealed record Cedente
{
    /// <summary>Nome/razão social.</summary>
    public required string Nome { get; init; }

    /// <summary>CPF/CNPJ do cedente.</summary>
    public required DocumentoFiscal Documento { get; init; }

    /// <summary>Endereço do cedente.</summary>
    public Endereco? Endereco { get; init; }

    /// <summary>Conta bancária do cedente (agência, conta, DVs).</summary>
    public required ContaBancaria Conta { get; init; }

    /// <summary>
    /// Dados da cooperativa (Sicredi/Sicoob). Obrigatórios para esses bancos:
    /// sem eles o DV do nosso número não pode ser conferido.
    /// </summary>
    public DadosCooperativa? DadosCooperativa { get; init; }
}

/// <summary>
/// Pagador/sacado: de quem se cobra o título.
/// </summary>
public sealed record Pagador
{
    /// <summary>Nome/razão social.</summary>
    public required string Nome { get; init; }

    /// <summary>CPF/CNPJ do pagador.</summary>
    public required DocumentoFiscal Documento { get; init; }

    /// <summary>Endereço do pagador.</summary>
    public Endereco? Endereco { get; init; }

    /// <summary>E-mail para aviso de boleto, quando o banco suportar.</summary>
    public string? Email { get; init; }
}

/// <summary>
/// Sacador avalista / beneficiário final: quem originalmente emitiu o
/// documento cobrado, quando a cobrança é feita por terceiro (FEBRABAN
/// renomeou "Sacador Avalista" para "Beneficiário Final"; Santander v32).
/// </summary>
public sealed record SacadorAvalista
{
    /// <summary>Nome/razão social.</summary>
    public required string Nome { get; init; }

    /// <summary>CPF/CNPJ.</summary>
    public required DocumentoFiscal Documento { get; init; }

    /// <summary>Endereço, quando exigido pelo layout.</summary>
    public Endereco? Endereco { get; init; }
}
