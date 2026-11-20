namespace Cnab.Boleto;

using System.Globalization;
using Cnab.Digitos;

/// <summary>
/// Linha digitável (representação numérica, 47 dígitos) do código de barras.
/// <para>
/// Composição FEBRABAN (Bradesco 4008-524-0121 "Montagem dos Dados para
/// Composição da Linha Digitável"; Itaú CNAB 400 Anexo 3; Caixa SIGCB Anexo V;
/// Sicredi item 10.9):
/// 1º campo = banco (3) + moeda (1) + 5 primeiras posições do campo livre +
/// DV módulo 10; 2º campo = posições 6-15 do campo livre + DV; 3º campo =
/// posições 16-25 do campo livre + DV; 4º campo = DV geral do código de
/// barras; 5º campo = fator de vencimento (4) + valor (10).
/// Edição: ponto após as 5 primeiras posições de cada um dos três primeiros
/// campos; espaço entre os campos — 47 dígitos no total.
/// </para>
/// </summary>
public static class LinhaDigitavel
{
    /// <summary>Quantidade de dígitos da linha digitável.</summary>
    public const int TamanhoDigitos = 47;

    /// <summary>
    /// Monta a linha digitável formatada a partir do código de barras.
    /// </summary>
    /// <param name="codigoDeBarras">Código de barras com 44 dígitos, com DV válido.</param>
    /// <returns>Linha digitável no formato "#####.##### #####.###### #####.###### # ##############".</returns>
    /// <exception cref="ArgumentException">Código de barras inválido.</exception>
    /// <exception cref="InvalidOperationException">DV geral do código de barras não confere.</exception>
    public static string Compor(string codigoDeBarras)
    {
        var digitos = ComporDigitos(codigoDeBarras);
        return Formatar(digitos);
    }

    /// <summary>
    /// Monta a linha digitável apenas com os 47 dígitos (sem pontuação).
    /// </summary>
    /// <param name="codigoDeBarras">Código de barras com 44 dígitos, com DV válido.</param>
    /// <returns>47 dígitos: campos 1-3 com DVs, DV geral, fator + valor.</returns>
    /// <exception cref="ArgumentException">Código de barras inválido.</exception>
    /// <exception cref="InvalidOperationException">DV geral do código de barras não confere.</exception>
    public static string ComporDigitos(string codigoDeBarras)
    {
        CodigoDeBarras.Validar(codigoDeBarras);
        if (!CodigoDeBarras.ConferirDvGeral(codigoDeBarras))
        {
            throw new InvalidOperationException("DV geral do código de barras não confere.");
        }

        var bancoMoeda = codigoDeBarras[..4];
        var campoLivre = codigoDeBarras[19..];
        var campo1 = bancoMoeda + campoLivre[..5];
        var campo2 = campoLivre.Substring(5, 10);
        var campo3 = campoLivre.Substring(15, 10);
        var dvGeral = codigoDeBarras[4];
        var fatorValor = codigoDeBarras.Substring(5, 14);

        var dv1 = Modulo.DvCampoLinhaDigitavel([.. campo1]);
        var dv2 = Modulo.DvCampoLinhaDigitavel([.. campo2]);
        var dv3 = Modulo.DvCampoLinhaDigitavel([.. campo3]);

        return campo1 + dv1.ToString(CultureInfo.InvariantCulture)
            + campo2 + dv2.ToString(CultureInfo.InvariantCulture)
            + campo3 + dv3.ToString(CultureInfo.InvariantCulture)
            + dvGeral
            + fatorValor;
    }

    /// <summary>
    /// Formata os 47 dígitos com a pontuação padrão: ponto após as 5
    /// primeiras posições de cada um dos três primeiros campos e espaço entre
    /// os campos.
    /// </summary>
    /// <param name="digitos">47 dígitos da linha digitável.</param>
    /// <returns>Linha formatada "#####.##### #####.###### #####.###### # ##############".</returns>
    /// <exception cref="ArgumentException">Quantidade de dígitos incorreta.</exception>
    public static string Formatar(string digitos)
    {
        ExigirDigitos(digitos);
        return string.Create(
            CultureInfo.InvariantCulture,
            $"{digitos[..5]}.{digitos.Substring(5, 5)} "
            + $"{digitos.Substring(10, 5)}.{digitos.Substring(15, 6)} "
            + $"{digitos.Substring(21, 5)}.{digitos.Substring(26, 6)} "
            + $"{digitos[32]} "
            + $"{digitos[33..]}");
    }

    /// <summary>
    /// Remove pontuação de uma linha digitável e devolve os 47 dígitos.
    /// </summary>
    /// <param name="linha">Linha digitável com ou sem pontos e espaços.</param>
    /// <returns>47 dígitos.</returns>
    /// <exception cref="ArgumentException">Formato inválido.</exception>
    public static string SomenteDigitos(string linha)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(linha);
        var digitos = new string([.. linha.Where(char.IsAsciiDigit)]);
        return ExigirDigitos(digitos);
    }

    /// <summary>
    /// Converte a linha digitável (formatada ou só dígitos) de volta para o
    /// código de barras de 44 posições. Os DVs dos três primeiros campos não
    /// fazem parte do código de barras (Bradesco, Nota 3: "os dígitos
    /// verificadores referentes aos campos 1, 2, 3, não são representados no
    /// código de barras").
    /// </summary>
    /// <param name="linha">Linha digitável com ou sem pontuação.</param>
    /// <returns>Código de barras com 44 dígitos.</returns>
    /// <exception cref="ArgumentException">Formato inválido.</exception>
    public static string ParaCodigoDeBarras(string linha)
    {
        var digitos = SomenteDigitos(linha);
        // [0..8] campo 1 (9 dígitos), [9] DV1, [10..19] campo 2 (10),
        // [20] DV2, [21..30] campo 3 (10), [31] DV3, [32] DV geral,
        // [33..46] fator + valor.
        var campoLivre = digitos.Substring(4, 5) + digitos.Substring(10, 10) + digitos.Substring(21, 10);
        var dvGeral = digitos[32];
        var fatorValor = digitos[33..];
        return digitos[..4] + dvGeral + fatorValor + campoLivre;
    }

    /// <summary>
    /// Confere os DVs de bloco (campos 1-3, módulo 10) e o DV geral de uma
    /// linha digitável.
    /// </summary>
    /// <param name="linha">Linha digitável com ou sem pontuação.</param>
    /// <returns><see langword="true"/> quando todos os DVs conferem.</returns>
    /// <exception cref="ArgumentException">Formato inválido.</exception>
    public static bool ConferirDvs(string linha)
    {
        var digitos = SomenteDigitos(linha);
        var dv1 = Modulo.DvCampoLinhaDigitavel([.. digitos[..9]]);
        var dv2 = Modulo.DvCampoLinhaDigitavel([.. digitos.Substring(10, 10)]);
        var dv3 = Modulo.DvCampoLinhaDigitavel([.. digitos.Substring(21, 10)]);
        var codigo = ParaCodigoDeBarras(digitos);
        return dv1 == digitos[9] - '0'
            && dv2 == digitos[20] - '0'
            && dv3 == digitos[31] - '0'
            && CodigoDeBarras.ConferirDvGeral(codigo);
    }

    private static string ExigirDigitos(string digitos)
    {
        if (digitos.Length != TamanhoDigitos)
        {
            throw new ArgumentException(
                $"Linha digitável deve ter {TamanhoDigitos} dígitos; recebidos {digitos.Length}.",
                nameof(digitos));
        }

        return digitos;
    }
}
