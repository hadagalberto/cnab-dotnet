namespace Cnab.Texto;

using System.Globalization;
using System.Text;

/// <summary>
/// Normalização de textos para o conjunto de caracteres aceito em arquivos CNAB
/// (o "charset CNAB").
/// <para>Regras, conforme os manuais citados abaixo:</para>
/// <list type="bullet">
/// <item>Letras maiúsculas (os sistemas forçam caixa alta: Sicredi, doc
/// <c>Tipos de campos</c>, seção 6.4 do "CNAB 240 – Cobrança Sicredi").</item>
/// <item>Sem acentuação gráfica nem caracteres especiais (FEBRABAN,
/// <c>Layout Padrão CNAB 240 V10.9</c>, "Alinhamento de Campos" e observações
/// gerais; Banrisul, <c>Leiaute Padrão Febraban CNAB 240 V10.3</c>, observação 4).</item>
/// <item>O conjunto aceito pelo Sicredi (doc citado) é adotado como referência
/// por ser o mais restritivo encontrado em manual oficial: <c>A-Z</c>,
/// <c>0-9</c>, espaço e <c>! * - ( ) [ ] { } , . ; : / % + =</c>.</item>
/// </list>
/// <para>Caracteres fora do conjunto são substituídos por espaço; diacríticos
/// são removidos via decomposição Unicode (por exemplo <c>á</c> vira <c>A</c>);</para>
/// <para><c>ç</c>/<c>Ç</c> viram <c>C</c>; travessões (<c>–—</c>) viram
/// hífen. Aspas e apóstrofos (retos ou tipográficos) não pertencem ao
/// conjunto aceito e são substituídos por espaço.</para>
/// </summary>
public static class CharsetCnab
{
    private const string Permitidos = " !*-()[]{},.;:/%+=";

    /// <summary>
    /// Normaliza um texto para o charset CNAB: caixa alta, sem acentuação, com
    /// caracteres não suportados substituídos por espaço.
    /// </summary>
    /// <param name="texto">Texto original; <see langword="null"/> é tratado como vazio.</param>
    /// <returns>Texto normalizado; nunca <see langword="null"/>.</returns>
    public static string Normalizar(string? texto)
    {
        if (string.IsNullOrEmpty(texto))
        {
            return string.Empty;
        }

        var semAcento = new string([.. RemoverDiacriticos(texto)]);
        var resultado = new char[semAcento.Length];
        for (var i = 0; i < semAcento.Length; i++)
        {
            var c = semAcento[i];
            resultado[i] = char.IsAsciiLetterOrDigit(c) || Permitidos.Contains(c, StringComparison.Ordinal)
                ? c
                : ' ';
        }

        return new string(resultado);
    }

    /// <summary>
    /// Verifica se todos os caracteres de um texto já estão no charset CNAB.
    /// </summary>
    /// <param name="texto">Texto já normalizado a verificar.</param>
    /// <returns><see langword="true"/> quando o texto inteiro pertence ao charset.</returns>
    public static bool EstaNoCharset(string texto)
    {
        foreach (var c in texto)
        {
            if (!char.IsAsciiLetterOrDigit(c) && !Permitidos.Contains(c, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static IEnumerable<char> RemoverDiacriticos(string texto)
    {
        var decomposto = texto.Normalize(NormalizationForm.FormD);
        foreach (var c in decomposto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (c is 'ç' or 'Ç')
            {
                yield return 'C';
            }
            else if (c is '–' or '—' or '―')
            {
                // Travessões viram hífen, que pertence ao conjunto aceito.
                yield return '-';
            }
            else
            {
                yield return char.ToUpperInvariant(c);
            }
        }
    }
}
