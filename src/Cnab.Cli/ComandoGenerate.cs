namespace Cnab.Cli;

using System.Text.Json;
using System.Text.Json.Serialization;
using Cnab.Arquivo;

/// <summary>Implementação de <c>cnab generate</c>.</summary>
public static class ComandoGenerate
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>Gera uma remessa a partir de um JSON de títulos.</summary>
    /// <param name="opcoes">Opções já interpretadas (bank e layout obrigatórios).</param>
    /// <returns>Arquivo gerado (stdout ou --output).</returns>
    public static ResultadoCli Executar(Cli.Opcoes opcoes)
    {
        if (opcoes.Banco is null)
        {
            return ResultadoCli.Uso("'generate' exige --bank <código>.");
        }

        if (opcoes.Layout is null)
        {
            return ResultadoCli.Uso("'generate' exige --layout <240|400>.");
        }

        string conteudoJson;
        try
        {
            conteudoJson = File.ReadAllText(opcoes.Alvo!);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return ResultadoCli.Erro($"Não foi possível ler '{opcoes.Alvo}': {ex.Message}");
        }

        try
        {
            var entrada = JsonSerializer.Deserialize<EntradaGeracao>(conteudoJson, JsonOptions)
                ?? throw new CliException("JSON de entrada vazio.");
            var conteudo = GeradorEntrada.Gerar(entrada, opcoes.Banco.Value, opcoes.Layout.Value, opcoes.Lf);
            var bytes = ArquivoTexto.ParaBytes(conteudo);
            if (opcoes.Saida is null)
            {
                return ResultadoCli.Arquivo(bytes, string.Empty);
            }

            File.WriteAllBytes(opcoes.Saida, bytes);
            return ResultadoCli.Ok(
                $"Remessa CNAB {opcoes.Layout.Value} gerada em '{opcoes.Saida}' "
                + $"({bytes.Length} bytes, {ArquivoCnab.SepararLinhas(conteudo).Count} registros)."
                + Environment.NewLine);
        }
        catch (Exception ex) when (ex is CliException or JsonException or CampoCnabException or TamanhoRegistroException
                                       or ArgumentException or FormatException or InvalidOperationException
                                       or NotSupportedException)
        {
            return ResultadoCli.Erro(ex.Message);
        }
    }
}
