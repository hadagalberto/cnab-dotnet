namespace Cnab.Boleto;

using System.Globalization;
using Cnab.Digitos;

/// <summary>
/// Código de barras/linha digitável de arrecadação (FEBRABAN, <c>Layout
/// Padrão de Arrecadação/Recebimento com Utilização do Código de Barras</c>,
/// v07/v08 — https://cmsarquivos.febraban.org.br ). Diferente da cobrança,
/// não há banco emissor nem fator de vencimento: a 3ª posição indica se os
/// DVs usam módulo 10 (6/7) ou módulo 11 (8/9), a 4ª é o DV geral e as
/// posições 5-15 trazem o valor (11 dígitos, sem decimais implícitos).
/// <para>A representação numérica tem 48 dígitos: quatro blocos de 11
/// posições do código de barras, cada um seguido de um DV calculado pelo mesmo
/// algoritmo do DV geral (módulo 10 para 6/7, módulo 11 para 8/9 — FEBRABAN
/// v08, seção 05).</para>
/// </summary>
public static class Arrecadacao
{
    /// <summary>Quantidade de posições do código de barras.</summary>
    public const int TamanhoBarras = 44;

    /// <summary>Quantidade de dígitos da linha digitável.</summary>
    public const int TamanhoLinha = 48;

    private const string Identificador = "8";

    /// <summary>
    /// Verifica se a sequência de dígitos é um código de barras/linha de
    /// arrecadação (prefixo 8 do produto, com 44 ou 48 dígitos).
    /// </summary>
    /// <param name="digitos">Apenas dígitos (sem pontuação).</param>
    /// <returns><see langword="true"/> quando é arrecadação.</returns>
    public static bool EhArrecadacao(string digitos) =>
        digitos.Length is TamanhoBarras or TamanhoLinha
        && digitos.StartsWith(Identificador, StringComparison.Ordinal);

    /// <summary>
    /// Converte a linha digitável de 48 posições no código de barras de 44 e
    /// confere todos os DVs (geral e dos quatro blocos). Um código de barras
    /// de 44 posições é devolvido após conferência do DV geral.
    /// </summary>
    /// <param name="codigo">Código de barras (44) ou linha digitável (48), com ou sem pontuação.</param>
    /// <returns>Código de barras de 44 dígitos validado.</returns>
    /// <exception cref="ArgumentException">Formato ou DV inválido.</exception>
    public static string Normalizar(string codigo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        var digitos = new string([.. codigo.Where(char.IsAsciiDigit)]);
        var barras = digitos.Length switch
        {
            TamanhoBarras => digitos,
            TamanhoLinha => DeLinhaParaBarras(digitos),
            _ => throw new ArgumentException(
                $"Arrecadação exige código de barras de {TamanhoBarras} ou linha digitável de {TamanhoLinha} dígitos; "
                    + $"recebidos {digitos.Length}.", nameof(codigo)),
        };

        if (!EhArrecadacao(barras))
        {
            throw new ArgumentException("Código de arrecadação deve começar com o produto '8'.", nameof(codigo));
        }

        if (barras[2] is not ('6' or '7' or '8' or '9'))
        {
            throw new ArgumentException(
                "3ª posição da arrecadação deve ser 6/7 (módulo 10) ou 8/9 (módulo 11).", nameof(codigo));
        }

        if (!ConferirDvGeral(barras))
        {
            throw new ArgumentException("DV geral da arrecadação não confere.", nameof(codigo));
        }

        return barras;
    }

    /// <summary>
    /// DV geral da arrecadação, calculado sobre 43 posições: 1-3 + 5-44.
    /// Módulo 10 para identificação de valor 6/7; módulo 11 (pesos 2-9 da
    /// direita para a esquerda) para 8/9, com resto 0/1 → 0 e resto 10 → 1
    /// (manual FEBRABAN v07/v08, seções 07 e 09/10).
    /// </summary>
    /// <param name="barrasSemDv">43 dígitos (posições 1-3 e 5-44).</param>
    /// <param name="identificadorValor">Dígito da 3ª posição do código de barras.</param>
    /// <returns>DV geral (0 a 9).</returns>
    /// <exception cref="ArgumentException">Entrada fora do formato.</exception>
    public static int CalcularDvGeral(string barrasSemDv, char identificadorValor)
    {
        if (barrasSemDv.Length != TamanhoBarras - 1 || !barrasSemDv.All(char.IsAsciiDigit))
        {
            throw new ArgumentException("A base do DV da arrecadação deve ter 43 dígitos.", nameof(barrasSemDv));
        }

        return identificadorValor switch
        {
            '6' or '7' => Modulo.Modulo10(barrasSemDv),
            '8' or '9' => CalcularDvModulo11(barrasSemDv),
            _ => throw new ArgumentException(
                "Identificador de valor deve ser 6, 7, 8 ou 9.", nameof(identificadorValor)),
        };
    }

    /// <summary>Confere o DV geral de um código de barras de arrecadação.</summary>
    /// <param name="barras">Código com 44 dígitos.</param>
    /// <returns><see langword="true"/> quando o DV confere.</returns>
    /// <exception cref="ArgumentException">Formato inválido.</exception>
    public static bool ConferirDvGeral(string barras)
    {
        if (barras.Length != TamanhoBarras || !barras.All(char.IsAsciiDigit))
        {
            throw new ArgumentException("Código de barras deve ter 44 dígitos.", nameof(barras));
        }

        var esperado = CalcularDvGeral(barras[..3] + barras[4..], barras[2]);
        return barras[3] - '0' == esperado;
    }

    /// <summary>
    /// Valor da arrecadação (posições 5-15 do código de barras, 11 dígitos em
    /// centavos). Manual FEBRABAN v08, seção 05: com identificador 6 ou 8 o
    /// campo traz o "valor a ser cobrado efetivamente em reais"; as 11 posições
    /// são centavos implícitos, como no campo valor da cobrança. Com 7 ou 9 o
    /// campo é uma quantidade/valor de referência e não deve ser cobrado.
    /// </summary>
    /// <param name="barras">Código de barras validado com 44 dígitos.</param>
    /// <returns>Valor monetário em reais (centavos / 100).</returns>
    /// <exception cref="ArgumentException">Formato inválido.</exception>
    public static decimal LerValor(string barras)
    {
        if (barras.Length != TamanhoBarras || !barras.All(char.IsAsciiDigit))
        {
            throw new ArgumentException("Código de barras deve ter 44 dígitos.", nameof(barras));
        }

        return long.Parse(barras.Substring(4, 11), CultureInfo.InvariantCulture) / 100m;
    }

    private static string DeLinhaParaBarras(string digitos)
    {
        var barras = digitos[..11] + digitos.Substring(12, 11)
            + digitos.Substring(24, 11) + digitos.Substring(36, 11);
        var identificador = digitos[2];
        for (var bloco = 0; bloco < 4; bloco++)
        {
            var inicio = bloco * 12;
            var baseBloco = digitos.Substring(inicio, 11);
            var esperado = identificador switch
            {
                '6' or '7' => Modulo.Modulo10([.. baseBloco]),
                '8' or '9' => CalcularDvModulo11(baseBloco),
                _ => throw new ArgumentException(
                    "3ª posição da arrecadação deve ser 6, 7, 8 ou 9.", nameof(digitos)),
            };
            if (digitos[inicio + 11] - '0' != esperado)
            {
                throw new ArgumentException(
                    $"DV do bloco {bloco + 1} da linha digitável de arrecadação não confere.", nameof(digitos));
            }
        }

        return barras;
    }

    private static int CalcularDvModulo11(string barrasSemDv)
    {
        var resto = Modulo.RestoModulo11(barrasSemDv, 9);
        return resto switch
        {
            0 or 1 => 0,
            10 => 1,
            _ => 11 - resto,
        };
    }
}
