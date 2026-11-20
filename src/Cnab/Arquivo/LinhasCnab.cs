namespace Cnab.Arquivo;

/// <summary>
/// Separação de registros de um arquivo CNAB. A convenção CNAB é CRLF
/// (0D 0A — FEBRABAN V10.9, "Delimitadores do Registro"; Banrisul V10.3,
/// seção 2.2), mas arquivos gerados em sistemas UNIX usam apenas LF e
/// editores antigos podem deixar CR solto. Esta rotina é a única fonte da
/// verdade para as três camadas que precisam separar linhas (motor de
/// leitura de retornos 240/400 e CLI), evitando divergências de comportamento.
/// </summary>
public static class LinhasCnab
{
    /// <summary>
    /// Separa o conteúdo em registros, descartando somente o delimitador final.
    /// Linhas vazias no meio do arquivo são preservadas para que a validação
    /// reporte o comprimento inválido na linha correta.
    /// </summary>
    /// <param name="conteudo">Conteúdo textual do arquivo (Latin-1 decodificado).</param>
    /// <returns>Registros sem delimitadores.</returns>
    /// <exception cref="ArgumentNullException">Conteúdo nulo.</exception>
    public static IReadOnlyList<string> Separar(string conteudo)
    {
        ArgumentNullException.ThrowIfNull(conteudo);

        // CRLF vira LF; CR solto também. Assim um único Split cobre os três
        // delimitadores sem risco de sobrar um '\n' à frente de cada linha.
        var normalizado = conteudo.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        var partes = normalizado.Split('\n');
        return partes.Length > 0 && partes[^1].Length == 0 ? partes[..^1] : partes;
    }
}
