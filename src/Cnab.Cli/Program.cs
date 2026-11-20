namespace Cnab.Cli;

/// <summary>Ponto de entrada do dotnet tool <c>cnab</c>.</summary>
public static class Program
{
    /// <summary>
    /// Executa a ferramenta e escreve a saída: texto no console, ou bytes
    /// Latin-1 diretamente no stdout quando um arquivo foi gerado.
    /// </summary>
    /// <param name="args">Argumentos de linha de comando.</param>
    /// <returns>Código de saída (0 sucesso, 1 erro de dados, 2 erro de uso).</returns>
    public static int Main(string[] args)
    {
        var resultado = Cli.Executar(args);
        if (resultado.ConteudoBinario is not null)
        {
            using var stdout = Console.OpenStandardOutput();
            stdout.Write(resultado.ConteudoBinario, 0, resultado.ConteudoBinario.Length);
            stdout.Flush();
        }

        if (resultado.Texto.Length > 0)
        {
            Console.Out.Write(resultado.Texto);
        }

        return resultado.CodigoSaida;
    }
}
