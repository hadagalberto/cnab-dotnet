namespace Cnab.Tests;

using Cnab.Arquivo;
using Xunit;

/// <summary>
/// Snapshot (Verify) do arquivo de exemplo montado pelo motor: congela o
/// conteúdo byte-exato (CR/LF visível) para detectar regressões de layout.
/// </summary>
public class SnapshotTests
{
    [Fact]
    public Task ArquivoDeExemploComDoisRegistros()
    {
        var primeiro = new RegistroExemplo
        {
            Banco = 33,
            Lote = 0,
            TipoRegistro = 0,
            Inscricao = 0,
            NomeEmpresa = "EMPRESA MODELO LTDA",
            DataGeracao = new DateOnly(2026, 11, 20),
            HoraGeracao = 101530,
            SequencialArquivo = 1,
            FillerLayout = string.Empty,
            Densidade = 1600,
            ValorTitulo = 1500.50m,
            VencimentoCurto = new DateOnly(2026, 12, 20),
            DescontoFlag = null,
        };
        var segundo = primeiro with
        {
            Banco = 33,
            TipoRegistro = 1,
            NomeEmpresa = "ÇAKI & FRUTAS DO CERRADO",
            ValorTitulo = 99.99m,
            DescontoFlag = 1m,
        };

        var linha1 = RegistroCnab<RegistroExemplo>.Escrever(primeiro);
        var linha2 = RegistroCnab<RegistroExemplo>.Escrever(segundo);
        var arquivo = EscritorDeArquivo.Montar([linha1, linha2]);
        var arquivoVisivel = arquivo
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal);

        return Verify(arquivoVisivel);
    }
}
