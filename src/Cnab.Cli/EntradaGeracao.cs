namespace Cnab.Cli;

using Cnab.Modelo;

/// <summary>JSON de entrada aceito por <c>cnab generate</c>.</summary>
public sealed class EntradaGeracao
{
    /// <summary>Data de geração do arquivo (AAAA-MM-DD). Obrigatória.</summary>
    public DateOnly? DataGeracao { get; set; }

    /// <summary>Hora de geração do arquivo (HH:mm:ss ou HHmmss). Padrão: 00:00:00.</summary>
    public string? HoraGeracao { get; set; }

    /// <summary>Número sequencial do arquivo (NSA). Padrão: 1.</summary>
    public int? NumeroSequencialArquivo { get; set; }

    /// <summary>Número da remessa. Padrão: 1.</summary>
    public int NumeroRemessa { get; set; } = 1;

    /// <summary>Convênio junto ao banco (quando aplicável).</summary>
    public string Convenio { get; set; } = string.Empty;

    /// <summary>Código do beneficiário (CAIXA, Bradesco, Sicredi).</summary>
    public string CodigoBeneficiario { get; set; } = string.Empty;

    /// <summary>Código do produto (Bradesco).</summary>
    public string CodigoProduto { get; set; } = string.Empty;

    /// <summary>Cedente/beneficiário e sua conta.</summary>
    public EntradaCedente? Cedente { get; set; }

    /// <summary>Títulos da remessa (um lote de cobrança no CNAB 240).</summary>
    public IReadOnlyList<EntradaTitulo>? Titulos { get; set; }
}

/// <summary>Cedente e conta no JSON de entrada.</summary>
public sealed class EntradaCedente
{
    /// <summary>Nome/razão social.</summary>
    public string? Nome { get; set; }

    /// <summary>CPF ou CNPJ (com ou sem pontuação).</summary>
    public string? Documento { get; set; }

    /// <summary>Agência, apenas dígitos e sem DV.</summary>
    public string? Agencia { get; set; }

    /// <summary>DV da agência, quando o banco usar.</summary>
    public string? DvAgencia { get; set; }

    /// <summary>Conta corrente, apenas dígitos e sem DV.</summary>
    public string? Conta { get; set; }

    /// <summary>DV da conta, quando o banco usar.</summary>
    public string? DvConta { get; set; }

    /// <summary>DV agência/conta, quando o banco usar campo único.</summary>
    public string? DvAgenciaConta { get; set; }
}

/// <summary>Título no JSON de entrada.</summary>
public sealed class EntradaTitulo
{
    /// <summary>Identificação do título no banco.</summary>
    public string? NossoNumero { get; set; }

    /// <summary>Número do documento ("seu número").</summary>
    public string? NumeroDocumento { get; set; }

    /// <summary>Espécie do documento (nome do enum ou valor numérico).</summary>
    public EspecieDocumento Especie { get; set; } = EspecieDocumento.DuplicataMercantilIndicacao;

    /// <summary>Data de emissão.</summary>
    public DateOnly? DataEmissao { get; set; }

    /// <summary>Data de vencimento.</summary>
    public DateOnly? DataVencimento { get; set; }

    /// <summary>Valor nominal.</summary>
    public decimal Valor { get; set; }

    /// <summary>Carteira de cobrança.</summary>
    public string Carteira { get; set; } = string.Empty;

    /// <summary>Pagador/sacado.</summary>
    public EntradaPagador? Pagador { get; set; }
}

/// <summary>Pagador no JSON de entrada.</summary>
public sealed class EntradaPagador
{
    /// <summary>Nome/razão social.</summary>
    public string? Nome { get; set; }

    /// <summary>CPF ou CNPJ.</summary>
    public string? Documento { get; set; }

    /// <summary>Endereço (opcional, mas necessário para os segmentos Q/CNAB 240).</summary>
    public EntradaEndereco? Endereco { get; set; }
}

/// <summary>Endereço no JSON de entrada.</summary>
public sealed class EntradaEndereco
{
    /// <summary>Logradouro.</summary>
    public string Logradouro { get; set; } = string.Empty;

    /// <summary>Bairro.</summary>
    public string Bairro { get; set; } = string.Empty;

    /// <summary>Cidade.</summary>
    public string Cidade { get; set; } = string.Empty;

    /// <summary>UF (duas letras maiúsculas).</summary>
    public string Uf { get; set; } = string.Empty;

    /// <summary>CEP com 8 dígitos.</summary>
    public string Cep { get; set; } = string.Empty;
}
