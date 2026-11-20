namespace Cnab.Modelo;

/// <summary>
/// CPF ou CNPJ validado: dígitos verificadores conferidos com o algoritmo
/// oficial de módulo 11 usado pela Receita Federal.
/// </summary>
public sealed record DocumentoFiscal
{
    /// <summary>CPF válido de exemplo para testes: 111.444.777-35.</summary>
    public const string CpfExemplo = "11144477735";

    /// <summary>CNPJ válido de exemplo para testes: 00.000.000/0001-91 (Banco do Brasil).</summary>
    public const string CnpjExemplo = "00000000000191";

    /// <summary>Inicializa a partir da sequência de dígitos, validando o DV.</summary>
    /// <param name="digitos">CPF (11 dígitos) ou CNPJ (14 dígitos), com ou sem pontuação.</param>
    /// <exception cref="ArgumentException">Quantidade de dígitos incorreta ou DV inválido.</exception>
    public DocumentoFiscal(string digitos)
    {
        if (string.IsNullOrWhiteSpace(digitos))
        {
            throw new ArgumentException("Documento fiscal não pode ser vazio.", nameof(digitos));
        }

        var apenasDigitos = new string([.. digitos.Where(char.IsDigit)]);
        EhCnpj = apenasDigitos.Length == 14;
        if (apenasDigitos.Length is not (11 or 14))
        {
            throw new ArgumentException(
                $"Documento fiscal deve ter 11 dígitos (CPF) ou 14 (CNPJ); recebidos {apenasDigitos.Length}.",
                nameof(digitos));
        }

        if (!ValidarDigitosVerificadores(apenasDigitos))
        {
            throw new ArgumentException("Dígitos verificadores do documento fiscal não conferem.", nameof(digitos));
        }

        Digitos = apenasDigitos;
    }

    /// <summary>Indica se o documento é CNPJ (e não CPF).</summary>
    public bool EhCnpj { get; }

    /// <summary>Dígitos sem pontuação.</summary>
    public string Digitos { get; }

    /// <summary>Tipo de inscrição usado nos arquivos CNAB: 1 = CPF, 2 = CNPJ (FEBRABAN G005).</summary>
    public int TipoInscricao => EhCnpj ? 2 : 1;

    /// <summary>Verifica se um documento é válido sem lançar exceção.</summary>
    /// <param name="digitos">CPF/CNPJ com ou sem pontuação.</param>
    /// <returns><see langword="true"/> quando o documento é válido.</returns>
    public static bool EhValido(string digitos)
    {
        try
        {
            _ = new DocumentoFiscal(digitos);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    /// <summary>Representação com pontuação (000.000.000-00 ou 00.000.000/0000-00).</summary>
    /// <returns>Documento formatado.</returns>
    public override string ToString() => EhCnpj
        ? $"{Digitos[..2]}.{Digitos.Substring(2, 3)}.{Digitos.Substring(5, 3)}/{Digitos.Substring(8, 4)}-{Digitos[^2..]}"
        : $"{Digitos[..3]}.{Digitos.Substring(3, 3)}.{Digitos.Substring(6, 3)}-{Digitos[^2..]}";

    private static bool ValidarDigitosVerificadores(string digitos)
    {
        if (TodosIguais(digitos))
        {
            return false;
        }

        return digitos.Length == 11
            ? ValidarCpf(digitos)
            : ValidarCnpj(digitos);
    }

    private static bool TodosIguais(string digitos)
    {
        foreach (var c in digitos)
        {
            if (c != digitos[0])
            {
                return false;
            }
        }

        return true;
    }

    private static bool ValidarCpf(string digitos)
    {
        var soma = 0;
        for (var i = 0; i < 9; i++)
        {
            soma += (digitos[i] - '0') * (10 - i);
        }

        var resto = (soma * 10) % 11;
        var dv1 = resto == 10 ? 0 : resto;
        if (dv1 != digitos[9] - '0')
        {
            return false;
        }

        soma = 0;
        for (var i = 0; i < 10; i++)
        {
            soma += (digitos[i] - '0') * (11 - i);
        }

        resto = (soma * 10) % 11;
        var dv2 = resto == 10 ? 0 : resto;
        return dv2 == digitos[10] - '0';
    }

    private static bool ValidarCnpj(string digitos)
    {
        var pesosPrimeiro = new[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        var soma = 0;
        for (var i = 0; i < 12; i++)
        {
            soma += (digitos[i] - '0') * pesosPrimeiro[i];
        }

        var resto = soma % 11;
        var dv1 = resto < 2 ? 0 : 11 - resto;
        if (dv1 != digitos[12] - '0')
        {
            return false;
        }

        var pesosSegundo = new[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        soma = 0;
        for (var i = 0; i < 13; i++)
        {
            soma += (digitos[i] - '0') * pesosSegundo[i];
        }

        resto = soma % 11;
        var dv2 = resto < 2 ? 0 : 11 - resto;
        return dv2 == digitos[13] - '0';
    }
}
