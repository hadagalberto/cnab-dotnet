namespace Cnab.Cli;

using System.Text;

/// <summary>Leitura e escrita de arquivos CNAB em Latin-1 (sem BOM).</summary>
public static class ArquivoTexto
{
    /// <summary>Lê um arquivo como texto Latin-1.</summary>
    /// <param name="caminho">Caminho do arquivo.</param>
    /// <returns>Conteúdo textual.</returns>
    public static string Ler(string caminho) => Encoding.Latin1.GetString(File.ReadAllBytes(caminho));

    /// <summary>Converte conteúdo CNAB em bytes Latin-1, sem BOM.</summary>
    /// <param name="conteudo">Conteúdo do arquivo.</param>
    /// <returns>Bytes prontos para gravação.</returns>
    public static byte[] ParaBytes(string conteudo) => Encoding.Latin1.GetBytes(conteudo);
}
