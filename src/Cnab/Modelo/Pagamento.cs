namespace Cnab.Modelo;

/// <summary>
/// Chave Pix com tipo inferido: CPF/CNPJ, celular, e-mail ou chave aleatória
/// (evp). Regras de formato conforme o Pix (Banco Central do Brasil,
/// Manual Operacional do Pix — Endereçamento).
/// </summary>
public sealed record ChavePix
{
    /// <summary>Tipo da chave Pix.</summary>
    public enum TipoChave
    {
        /// <summary>CPF (11 dígitos) — exigido válido.</summary>
        Cpf,

        /// <summary>CNPJ (14 dígitos) — exigido válido.</summary>
        Cnpj,

        /// <summary>Celular no formato +55DDDNNNNNNNNN.</summary>
        Celular,

        /// <summary>E-mail.</summary>
        Email,

        /// <summary>Chave aleatória (evp) no formato UUID v4.</summary>
        Aleatoria,
    }

    private ChavePix(TipoChave tipo, string valor)
    {
        Tipo = tipo;
        Valor = valor;
    }

    /// <summary>Tipo da chave.</summary>
    public TipoChave Tipo { get; }

    /// <summary>Valor da chave já normalizado (e-mail em caixa baixa).</summary>
    public string Valor { get; }

    /// <summary>
    /// Cria uma chave Pix a partir do valor, inferindo o tipo e validando o
    /// formato (CPF/CNPJ com DV conferido; celular <c>+55</c> + DDD + 9
    /// dígitos; e-mail; evp UUID).
    /// </summary>
    /// <param name="valor">Valor bruto da chave.</param>
    /// <returns>Chave tipada e validada.</returns>
    /// <exception cref="ArgumentException">Valor não corresponde a nenhum formato de chave.</exception>
    public static ChavePix Criar(string valor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(valor);

        var digitos = new string([.. valor.Where(char.IsDigit)]);
        if (digitos.Length == 11 && valor.Length == 11 && DocumentoFiscal.EhValido(digitos))
        {
            return new ChavePix(TipoChave.Cpf, digitos);
        }

        if (digitos.Length == 14 && valor.Length == 14 && DocumentoFiscal.EhValido(digitos))
        {
            return new ChavePix(TipoChave.Cnpj, digitos);
        }

        var trim = valor.Trim();
        if (trim.StartsWith("+55", StringComparison.Ordinal) && digitos.Length == 13)
        {
            return new ChavePix(TipoChave.Celular, trim.ToLowerInvariant());
        }

        if (trim.Contains('@') && !trim.Contains(' ') && EmailValido(trim))
        {
            return new ChavePix(TipoChave.Email, trim.ToLowerInvariant());
        }

        if (EhUuid(trim))
        {
            return new ChavePix(TipoChave.Aleatoria, trim.ToLowerInvariant());
        }

        throw new ArgumentException(
            "Valor não corresponde a um formato válido de chave Pix (CPF, CNPJ, celular +55, e-mail ou chave aleatória).",
            nameof(valor));
    }

    private static bool EmailValido(string email)
    {
        var arrobas = 0;
        foreach (var c in email)
        {
            if (c == '@')
            {
                arrobas++;
            }
        }

        var posicao = email.IndexOf('@', StringComparison.Ordinal);
        return arrobas == 1 && posicao > 0 && posicao < email.Length - 1;
    }

    private static bool EhUuid(string valor)
    {
        if (valor.Length != 36 || valor[8] != '-' || valor[13] != '-' || valor[18] != '-' || valor[23] != '-')
        {
            return false;
        }

        return valor.Replace("-", string.Empty).Length == 32
            && valor.Replace("-", string.Empty).All(EhHexa);
    }

    private static bool EhHexa(char c) => c is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F';
}

/// <summary>Modalidade de pagamento a fornecedor (lote de pagamentos, CNAB 240 segmento A/J).</summary>
public enum TipoPagamento
{
    /// <summary>Crédito em conta no mesmo banco.</summary>
    CreditoEmConta,

    /// <summary>DOC — transferência entre bancos por baixo valor (hoje em extinção).</summary>
    Doc,

    /// <summary>TED — transferência eletrônica disponível.</summary>
    Ted,

    /// <summary>Pagamento Pix por chave.</summary>
    PixChave,

    /// <summary>Pagamento Pix com QR Code (dinâmico ou estático).</summary>
    PixQrCode,

    /// <summary>Pagamento de boleto de cobrança pelo código de barras/linha digitável.</summary>
    BoletoCodigoBarras,

    /// <summary>Pagamento de tributo/concessionária pelo código de barras.</summary>
    TributoCodigoBarras,
}

/// <summary>
/// Pagamento a fornecedor (TED/DOC/Pix/boleto/tributo) na ótica do domínio
/// CNAB 240 (segmentos A/B/J/O).
/// </summary>
public sealed record Pagamento
{
    /// <summary>Modalidade do pagamento.</summary>
    public required TipoPagamento Tipo { get; init; }

    /// <summary>Banco de destino (favorecido); obrigatório em TED/DOC/conta.</summary>
    public Banco? BancoDestino { get; init; }

    /// <summary>Agência de destino, sem DV.</summary>
    public string? AgenciaDestino { get; init; }

    /// <summary>Conta de destino, sem DV.</summary>
    public string? ContaDestino { get; init; }

    /// <summary>DV da conta de destino, quando exigido.</summary>
    public string? DvContaDestino { get; init; }

    /// <summary>Chave Pix, quando <see cref="Tipo"/> for <see cref="TipoPagamento.PixChave"/>.</summary>
    public ChavePix? Chave { get; init; }

    /// <summary>Código de barras ou linha digitável (47 posições), para boleto/tributo.</summary>
    public string? CodigoDeBarras { get; init; }

    /// <summary>Favorecido (nome e documento).</summary>
    public required string NomeFavorecido { get; init; }

    /// <summary>CPF/CNPJ do favorecido.</summary>
    public required DocumentoFiscal DocumentoFavorecido { get; init; }

    /// <summary>Valor do pagamento.</summary>
    public required decimal Valor { get; init; }

    /// <summary>Data do pagamento/agendamento.</summary>
    public required DateOnly Data { get; init; }

    /// <summary>Número do documento do pagamento ("seu número").</summary>
    public required string NumeroDocumento { get; init; }

    /// <summary>Valida coerência entre tipo e campos preenchidos.</summary>
    /// <exception cref="InvalidOperationException">Campos obrigatórios ausentes ou conflitantes.</exception>
    public void Validar()
    {
        if (Valor <= 0)
        {
            throw new InvalidOperationException("Valor do pagamento deve ser maior que zero.");
        }

        switch (Tipo)
        {
            case TipoPagamento.PixChave:
                if (Chave is null)
                {
                    throw new InvalidOperationException("Pagamento Pix por chave exige a chave Pix.");
                }

                break;
            case TipoPagamento.BoletoCodigoBarras or TipoPagamento.TributoCodigoBarras:
                if (string.IsNullOrWhiteSpace(CodigoDeBarras))
                {
                    throw new InvalidOperationException("Pagamento de boleto/tributo exige o código de barras.");
                }

                break;
            case TipoPagamento.Ted or TipoPagamento.Doc or TipoPagamento.CreditoEmConta:
                if (BancoDestino is null || string.IsNullOrWhiteSpace(AgenciaDestino)
                    || string.IsNullOrWhiteSpace(ContaDestino))
                {
                    throw new InvalidOperationException("Transferência exige banco, agência e conta de destino.");
                }

                break;
            case TipoPagamento.PixQrCode:
                if (string.IsNullOrWhiteSpace(CodigoDeBarras))
                {
                    throw new InvalidOperationException("Pagamento Pix com QR Code exige o BR Code (payload).");
                }

                break;
            default:
                throw new InvalidOperationException($"Tipo de pagamento {Tipo} não tratado na validação.");
        }
    }
}
