namespace Cnab.Boleto;

using System.Globalization;
using Cnab.Digitos;
using Cnab.Modelo;

/// <summary>
/// Código de barras da cobrança (44 posições) e sua montagem/desmontagem.
/// <para>Estrutura (FEBRABAN, conforme manuais citados por banco):
/// 01-03 código do banco, 04 código da moeda (9 = Real), 05 DV geral
/// (módulo 11, pesos 2 a 9, sobre as posições 1-4 e 6-44), 06-09 fator de
/// vencimento, 10-19 valor nominal (9(8)V9(2)), 20-44 campo livre (25
/// posições, definido por cada banco).</para>
/// </summary>
public static class CodigoDeBarras
{
    /// <summary>Tamanho do código de barras.</summary>
    public const int Tamanho = 44;

    /// <summary>Código da moeda Real.</summary>
    public const int MoedaReal = 9;

    /// <summary>
    /// Monta o código de barras de 44 posições com o DV geral calculado.
    /// </summary>
    /// <param name="banco">Banco emissor (código de 3 dígitos na câmara).</param>
    /// <param name="vencimento">Vencimento do título; <see langword="null"/> usa fator 0000 (contra apresentação).</param>
    /// <param name="valor">Valor nominal com até 2 decimais (máximo 99.999.999,99 — Caixa SIGCB Anexo II: "o valor nominal não poderá exceder R$ 9.999.999,99").</param>
    /// <param name="campoLivre">Campo livre do banco com exatamente 25 dígitos.</param>
    /// <returns>Código de barras com 44 dígitos, com DV geral na posição 5.</returns>
    /// <exception cref="ArgumentException">Entradas fora do formato.</exception>
    public static string Compor(Banco banco, DateOnly? vencimento, decimal valor, string campoLivre)
    {
        ArgumentNullException.ThrowIfNull(banco);
        if (campoLivre.Length != 25 || !campoLivre.All(char.IsAsciiDigit))
        {
            throw new ArgumentException("Campo livre deve ter exatamente 25 dígitos.", nameof(campoLivre));
        }

        if (valor < 0)
        {
            throw new ArgumentException("Valor nominal não pode ser negativo.", nameof(valor));
        }

        if (decimal.Round(valor, 2, MidpointRounding.ToEven) != valor)
        {
            throw new ArgumentException("Valor nominal deve ter no máximo 2 decimais.", nameof(valor));
        }

        if (valor > 99_999_999.99m)
        {
            throw new ArgumentException("Valor nominal não pode exceder R$ 9.999.999,99.", nameof(valor));
        }

        var semDv = banco.Codigo.ToString("000", CultureInfo.InvariantCulture)
            + MoedaReal.ToString(CultureInfo.InvariantCulture)
            + FatorDeVencimento.Calcular(vencimento).ToString("0000", CultureInfo.InvariantCulture)
            + decimal.Truncate(valor * 100m).ToString("0000000000", CultureInfo.InvariantCulture)
            + campoLivre;
        var dv = Modulo.DvCodigoDeBarras([.. semDv]);
        return semDv[..4] + dv.ToString(CultureInfo.InvariantCulture) + semDv[4..];
    }

    /// <summary>
    /// Calcula o DV geral de um código de barras sem DV (43 dígitos).
    /// Regra: módulo 11 com pesos 2 a 9 da direita para a esquerda;
    /// resultado 0, 1 ou maior que 9 → DV 1 (Bradesco 4008-524-0121; Caixa
    /// SIGCB Anexo I: "não admite 0").
    /// </summary>
    /// <param name="semDv">Código de barras sem o DV (posições 1-4 e 6-44 na ordem).</param>
    /// <returns>DV geral.</returns>
    /// <exception cref="ArgumentException">Entrada fora do formato.</exception>
    public static int CalcularDvGeral(string semDv)
    {
        if (semDv.Length != Tamanho - 1 || !semDv.All(char.IsAsciiDigit))
        {
            throw new ArgumentException("Código de barras sem DV deve ter 43 dígitos.", nameof(semDv));
        }

        return Modulo.DvCodigoDeBarras([.. semDv]);
    }

    /// <summary>
    /// Confere o DV geral de um código de barras completo.
    /// </summary>
    /// <param name="codigoDeBarras">Código com 44 dígitos.</param>
    /// <returns><see langword="true"/> quando o DV geral confere.</returns>
    /// <exception cref="ArgumentException">Entrada fora do formato.</exception>
    public static bool ConferirDvGeral(string codigoDeBarras)
    {
        Validar(codigoDeBarras);
        var calculado = Modulo.DvCodigoDeBarras([.. (codigoDeBarras[..4] + codigoDeBarras[5..])]);
        return codigoDeBarras[4] == calculado.ToString(CultureInfo.InvariantCulture)[0];
    }

    /// <summary>Extrai o banco (posições 1-3).</summary>
    /// <param name="codigoDeBarras">Código com 44 dígitos.</param>
    /// <returns>Banco suportado correspondente.</returns>
    /// <exception cref="ArgumentException">Código inválido ou banco não suportado.</exception>
    public static Banco LerBanco(string codigoDeBarras)
    {
        Validar(codigoDeBarras);
        return Bancos.Obter(int.Parse(codigoDeBarras[..3], CultureInfo.InvariantCulture));
    }

    /// <summary>Extrai o fator de vencimento (posições 6-9).</summary>
    /// <param name="codigoDeBarras">Código com 44 dígitos.</param>
    /// <param name="referencia">Referência para desempatar os dois ciclos do fator (omitido = hoje).</param>
    /// <returns>Data de vencimento; <see langword="null"/> para fator 0000.</returns>
    /// <exception cref="ArgumentException">Código inválido.</exception>
    public static DateOnly? LerVencimento(string codigoDeBarras, DateOnly? referencia = null)
    {
        Validar(codigoDeBarras);
        return FatorDeVencimento.ParaVencimento(
            int.Parse(codigoDeBarras.Substring(5, 4), CultureInfo.InvariantCulture),
            referencia);
    }

    /// <summary>Extrai o valor nominal (posições 10-19, 2 decimais).</summary>
    /// <param name="codigoDeBarras">Código com 44 dígitos.</param>
    /// <returns>Valor nominal.</returns>
    /// <exception cref="ArgumentException">Código inválido.</exception>
    public static decimal LerValor(string codigoDeBarras)
    {
        Validar(codigoDeBarras);
        var centavos = long.Parse(codigoDeBarras.Substring(9, 10), CultureInfo.InvariantCulture);
        return centavos / 100m;
    }

    /// <summary>Extrai o campo livre (posições 20-44).</summary>
    /// <param name="codigoDeBarras">Código com 44 dígitos.</param>
    /// <returns>Campo livre com 25 dígitos.</returns>
    /// <exception cref="ArgumentException">Código inválido.</exception>
    public static string LerCampoLivre(string codigoDeBarras)
    {
        Validar(codigoDeBarras);
        return codigoDeBarras[19..];
    }

    /// <summary>Valida o formato do código de barras (44 dígitos).</summary>
    /// <param name="codigoDeBarras">Código a validar.</param>
    /// <exception cref="ArgumentException">Formato inválido.</exception>
    public static void Validar(string codigoDeBarras)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigoDeBarras);
        if (codigoDeBarras.Length != Tamanho || !codigoDeBarras.All(char.IsAsciiDigit))
        {
            throw new ArgumentException(
                $"Código de barras deve ter {Tamanho} dígitos; recebidos {codigoDeBarras.Length}.",
                nameof(codigoDeBarras));
        }
    }
}
