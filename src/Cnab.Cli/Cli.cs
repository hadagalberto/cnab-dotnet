namespace Cnab.Cli;

using System.Globalization;
using Cnab.Modelo;

/// <summary>Resultado de uma invocação da ferramenta, sem depender do console.</summary>
public sealed record ResultadoCli
{
    private ResultadoCli(int codigoSaida, string texto, byte[]? conteudoBinario)
    {
        CodigoSaida = codigoSaida;
        Texto = texto;
        ConteudoBinario = conteudoBinario;
    }

    /// <summary>0 = sucesso, 1 = erro de dados/validação, 2 = uso inválido.</summary>
    public int CodigoSaida { get; }

    /// <summary>Saída textual ( relatórios, erros e ajuda ).</summary>
    public string Texto { get; }

    /// <summary>Conteúdo binário de um arquivo gerado, quando aplicável.</summary>
    public byte[]? ConteudoBinario { get; }

    /// <summary>Resultado bem-sucedido com texto.</summary>
    /// <param name="texto">Texto a imprimir.</param>
    /// <returns>Resultado com código 0.</returns>
    public static ResultadoCli Ok(string texto) => new(0, texto, null);

    /// <summary>Resultado bem-sucedido com arquivo binário (Latin-1, sem BOM).</summary>
    /// <param name="conteudo">Bytes do arquivo.</param>
    /// <param name="mensagem">Mensagem curta impressa quando não há redirecionamento.</param>
    /// <returns>Resultado com código 0.</returns>
    public static ResultadoCli Arquivo(byte[] conteudo, string mensagem) => new(0, mensagem, conteudo);

    /// <summary>Erro de validação/dados.</summary>
    /// <param name="mensagem">Descrição do erro.</param>
    /// <returns>Resultado com código 1.</returns>
    public static ResultadoCli Erro(string mensagem) => new(1, mensagem + Environment.NewLine, null);

    /// <summary>Erro de uso (argumentos inválidos).</summary>
    /// <param name="mensagem">Descrição do erro.</param>
    /// <returns>Resultado com código 2.</returns>
    public static ResultadoCli Uso(string mensagem) => new(2, mensagem + Environment.NewLine + Cli.UsoGeral, null);
}

/// <summary>Ponto de entrada lógico da ferramenta <c>cnab</c>.</summary>
public static class Cli
{
    /// <summary>Texto de ajuda geral.</summary>
    public const string UsoGeral = """
        Uso: cnab <comando> [opções]

        Comandos:
          validate <arquivo>                 Valida linhas, campos e trailers.
          parse <arquivo> [--json]            Lê um retorno (240 ou 400).
          inspect <arquivo> [--bank <código>] Mostra campos, posições e valores.
          generate <titulos.json> --bank <código> --layout <240|400>
                                             Gera uma remessa (padrão: stdout).

        Opções:
          --json                 Saída JSON em 'parse'.
          --bank <código>        Código do banco (001, 033, 104, 237, 341, 748, 756, 077, 208).
          --layout <240|400>     Layout da remessa em 'generate'.
          --output <arquivo>     Grava a remessa gerada em arquivo.
          --lf                   Usa LF em vez de CRLF na remessa gerada.
          --help, -h             Mostra esta ajuda.
          --version              Mostra a versão.
        """;

    /// <summary>
    /// Executa um comando e devolve o resultado, sem imprimir no console —
    /// isso permite testar a CLI como biblioteca e garante saída byte-exata.
    /// </summary>
    /// <param name="args">Argumentos, no formato usado por <c>dotnet run -- ...</c>.</param>
    /// <returns>Resultado com código de saída, texto e/ou arquivo gerado.</returns>
    public static ResultadoCli Executar(IReadOnlyList<string> args)
    {
        ArgumentNullException.ThrowIfNull(args);
        if (args.Count == 0 || args[0] is "--help" or "-h" or "help")
        {
            return ResultadoCli.Ok(UsoGeral + Environment.NewLine);
        }

        if (args[0] == "--version")
        {
            return ResultadoCli.Ok("cnab 4.0.0" + Environment.NewLine);
        }

        var comando = args[0].ToLowerInvariant();
        if (comando is not ("validate" or "parse" or "inspect" or "generate"))
        {
            return ResultadoCli.Uso($"Comando desconhecido: '{args[0]}'.");
        }

        var opcoes = Opcoes.Ler(args);
        if (opcoes.Erro is not null)
        {
            return ResultadoCli.Uso(opcoes.Erro);
        }

        if (opcoes.Alvo is null)
        {
            return ResultadoCli.Uso($"Arquivo de entrada ausente para '{args[0]}'.");
        }

        return comando switch
        {
            "validate" => ComandoValidate.Executar(opcoes),
            "parse" => ComandoParse.Executar(opcoes),
            "inspect" => ComandoInspect.Executar(opcoes),
            _ => ComandoGenerate.Executar(opcoes),
        };
    }

    /// <summary>Argumentos já interpretados, comuns a todos os comandos.</summary>
    public sealed class Opcoes
    {
        private Opcoes()
        {
        }

        /// <summary>Primeiro argumento não-opção: caminho do arquivo de entrada.</summary>
        public string? Alvo { get; private init; }

        /// <summary>Alternância --json.</summary>
        public bool Json { get; private init; }

        /// <summary>Alternância --lf.</summary>
        public bool Lf { get; private init; }

        /// <summary>Código do banco informado em --bank, quando houver.</summary>
        public int? Banco { get; private init; }

        /// <summary>Layout informado em --layout, quando houver.</summary>
        public int? Layout { get; private init; }

        /// <summary>Caminho de saída informado em --output, quando houver.</summary>
        public string? Saida { get; private init; }

        /// <summary>Erro de interpretação dos argumentos, quando houver.</summary>
        public string? Erro { get; private init; }

        /// <summary>Interpreta os argumentos após o nome do comando.</summary>
        /// <param name="args">Argumentos completos.</param>
        /// <returns>Opções interpretadas.</returns>
        public static Opcoes Ler(IReadOnlyList<string> args)
        {
            string? alvo = null;
            var json = false;
            var lf = false;
            int? banco = null;
            int? layout = null;
            string? saida = null;
            string? erro = null;

            for (var i = 1; i < args.Count && erro is null; i++)
            {
                var argumento = args[i];
                if (argumento is "--json" or "-j")
                {
                    json = true;
                }
                else if (argumento == "--lf")
                {
                    lf = true;
                }
                else if (argumento.StartsWith("--", StringComparison.Ordinal))
                {
                    var nome = argumento;
                    string? valor = null;
                    var igual = argumento.IndexOf('=', StringComparison.Ordinal);
                    if (igual >= 0)
                    {
                        nome = argumento[..igual];
                        valor = argumento[(igual + 1)..];
                    }

                    switch (nome)
                    {
                        case "--bank":
                        case "--layout":
                        case "--output":
                            valor ??= i + 1 < args.Count ? args[++i] : null;
                            if (string.IsNullOrEmpty(valor))
                            {
                                erro = $"Opção {nome} exige um valor.";
                                break;
                            }

                            if (nome == "--bank")
                            {
                                if (int.TryParse(valor, NumberStyles.None, CultureInfo.InvariantCulture, out var codigoBanco)
                                    && Bancos.TentarObter(codigoBanco, out _))
                                {
                                    banco = codigoBanco;
                                }
                                else if (int.TryParse(valor, NumberStyles.None, CultureInfo.InvariantCulture, out codigoBanco)
                                         && codigoBanco is >= 1 and <= 999)
                                {
                                    erro = $"Banco {codigoBanco:000} não é suportado pelo Cnab.NET.";
                                }
                                else
                                {
                                    erro = $"Código de banco inválido: '{valor}'.";
                                }
                            }
                            else if (nome == "--layout")
                            {
                                if (valor is "240" or "400")
                                {
                                    layout = int.Parse(valor, CultureInfo.InvariantCulture);
                                }
                                else
                                {
                                    erro = $"Layout inválido: '{valor}' (use 240 ou 400).";
                                }
                            }
                            else
                            {
                                saida = valor;
                            }

                            break;
                        default:
                            erro = $"Opção desconhecida: '{argumento}'.";
                            break;
                    }
                }
                else if (alvo is null)
                {
                    alvo = argumento;
                }
                else
                {
                    erro = $"Argumento inesperado: '{argumento}'.";
                }
            }

            return new Opcoes
            {
                Alvo = alvo,
                Json = json,
                Lf = lf,
                Banco = banco,
                Layout = layout,
                Saida = saida,
                Erro = erro,
            };
        }
    }
}
