namespace Cnab.Arquivo;

using System.Text;

/// <summary>
/// Opções de montagem do arquivo CNAB. O arquivo é textual, um registro por
/// linha, e cada registro termina com CR/LF (0D 0A) — convenção CNAB
/// (Banrisul, <c>Leiaute Padrão Febraban CNAB 240 V10.3</c>, seção 2.2
/// "Delimitadores do Registro"; FEBRABAN V10.9). O SPEC pede tratamento
/// explícito de CRLF com opção por LF.
/// </summary>
public sealed record OpcoesArquivo
{
    /// <summary>Sequência de fim de linha entre os registros.</summary>
    public enum FimDeLinha
    {
        /// <summary>CR + LF (0D 0A) — padrão CNAB.</summary>
        CrLf,

        /// <summary>LF (0A) — opção para integrações que exigem apenas LF.</summary>
        Lf,
    }

    /// <summary>Fim de linha entre registros. Padrão: <see cref="FimDeLinha.CrLf"/>.</summary>
    public FimDeLinha Fim { get; init; } = FimDeLinha.CrLf;

    /// <summary>
    /// Gravar o delimitador também após o último registro. Padrão
    /// <see langword="true"/> (cada registro é encerrado pelo delimitador).
    /// </summary>
    public bool DelimitadorFinal { get; init; } = true;

    /// <summary>
    /// Codificação do arquivo. Padrão: ISO-8859-1 (Latin-1), superconjunto do
    /// ASCII aceito pelos bancos; o conteúdo já é normalizado para o charset
    /// CNAB antes da escrita.
    /// </summary>
    public Encoding Codificacao { get; init; } = Encoding.Latin1;

    /// <summary>Instância com as opções padrão (CRLF, delimitador final, Latin-1).</summary>
    public static OpcoesArquivo Padrao { get; } = new();
}

/// <summary>
/// Monta o conteúdo de um arquivo CNAB a partir de linhas de registro,
/// aplicando o delimitador de fim de linha escolhido.
/// </summary>
public static class EscritorDeArquivo
{
    /// <summary>Monta o arquivo completo a partir das linhas já escritas.</summary>
    /// <param name="linhas">Linhas (registros) sem delimitadores.</param>
    /// <param name="opcoes">Opções de montagem; padrão CRLF.</param>
    /// <returns>Conteúdo completo do arquivo.</returns>
    /// <exception cref="ArgumentException">Nenhuma linha informada.</exception>
    public static string Montar(IEnumerable<string> linhas, OpcoesArquivo? opcoes = null)
    {
        ArgumentNullException.ThrowIfNull(linhas);
        opcoes ??= OpcoesArquivo.Padrao;

        var fim = opcoes.Fim == OpcoesArquivo.FimDeLinha.CrLf ? "\r\n" : "\n";
        var lista = linhas as IReadOnlyList<string> ?? linhas.ToArray();
        if (lista.Count == 0)
        {
            throw new ArgumentException("O arquivo precisa de pelo menos um registro.", nameof(linhas));
        }

        var construtor = new StringBuilder();
        for (var i = 0; i < lista.Count; i++)
        {
            construtor.Append(lista[i]);
            if (opcoes.DelimitadorFinal || i < lista.Count - 1)
            {
                construtor.Append(fim);
            }
        }

        return construtor.ToString();
    }
}
