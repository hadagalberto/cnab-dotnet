namespace Cnab.Cli;

using System.Security;
using System.Text;

/// <summary>Leitura e escrita de arquivos CNAB em Latin-1 (sem BOM).</summary>
public static class ArquivoTexto
{
    private static readonly byte[] BomUtf8 = [0xEF, 0xBB, 0xBF];

    /// <summary>
    /// Lê um arquivo como texto Latin-1. Um BOM UTF-8 no início é descartado:
    /// editores e ferramentas de texto costumam inseri-lo, mas o padrão CNAB
    /// começa com o código do banco na posição 1 e não admite prefixos.
    /// </summary>
    /// <param name="caminho">Caminho do arquivo.</param>
    /// <returns>Conteúdo textual.</returns>
    public static string Ler(string caminho)
    {
        var bytes = File.ReadAllBytes(caminho);
        var inicio = bytes.AsSpan().StartsWith(BomUtf8) ? BomUtf8.Length : 0;
        return Encoding.Latin1.GetString(bytes, inicio, bytes.Length - inicio);
    }

    /// <summary>Converte conteúdo CNAB em bytes Latin-1, sem BOM.</summary>
    /// <param name="conteudo">Conteúdo do arquivo.</param>
    /// <returns>Bytes prontos para gravação.</returns>
    public static byte[] ParaBytes(string conteudo) => Encoding.Latin1.GetBytes(conteudo);

    /// <summary>
    /// Lê um arquivo convertendo qualquer falha de E/S ou de caminho em erro
    /// de dados da CLI (código 1), nunca em exceção não capturada.
    /// </summary>
    /// <param name="caminho">Caminho informado pelo usuário.</param>
    /// <param name="conteudo">Conteúdo lido quando bem-sucedido.</param>
    /// <param name="erro">Mensagem amigável quando falha.</param>
    /// <returns><see langword="true"/> quando a leitura termina com sucesso.</returns>
    public static bool TentarLer(string? caminho, out string conteudo, out string erro)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(caminho))
            {
                throw new ArgumentException("O caminho não pode ser vazio.", nameof(caminho));
            }

            conteudo = Ler(caminho);
            erro = string.Empty;
            return true;
        }
        catch (Exception ex) when (EhFalhaDeArquivo(ex))
        {
            conteudo = string.Empty;
            erro = $"Não foi possível ler '{caminho}': {ex.Message}";
            return false;
        }
    }

    /// <summary>
    /// Grava bytes convertendo qualquer falha de E/S ou de caminho em erro de
    /// dados da CLI (código 1), nunca em exceção não capturada.
    /// </summary>
    /// <param name="caminho">Caminho de destino.</param>
    /// <param name="bytes">Bytes a gravar.</param>
    /// <param name="erro">Mensagem amigável quando falha.</param>
    /// <returns><see langword="true"/> quando a gravação termina com sucesso.</returns>
    public static bool TentarGravar(string? caminho, byte[] bytes, out string erro)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(caminho))
            {
                throw new ArgumentException("O caminho não pode ser vazio.", nameof(caminho));
            }

            File.WriteAllBytes(caminho, bytes);
            erro = string.Empty;
            return true;
        }
        catch (Exception ex) when (EhFalhaDeArquivo(ex))
        {
            erro = $"Não foi possível gravar '{caminho}': {ex.Message}";
            return false;
        }
    }

    private static bool EhFalhaDeArquivo(Exception ex) =>
        ex is IOException or UnauthorizedAccessException or ArgumentException
            or NotSupportedException or SecurityException;
}
