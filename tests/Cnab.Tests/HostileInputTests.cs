namespace Cnab.Tests;

using System.Text;
using System.Text.Json.Nodes;
using Cnab.Cli;
using Xunit;

/// <summary>
/// Testes de hardening (marco 5): entradas maliciosas e de borda que a CLI e o
/// motor precisam tratar sem exceções não capturadas, aceitando apenas vetores
/// válidos. Cada teste aqui nasceu de uma sondagem real executada contra a
/// ferramenta; a correção de produção correspondente é permanente.
/// </summary>
public sealed class HostileInputTests : IDisposable
{
    private readonly string _diretorioTemporario;

    public HostileInputTests()
    {
        _diretorioTemporario = Path.Combine(Path.GetTempPath(), "cnab-hostile-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_diretorioTemporario);
    }

    public void Dispose()
    {
        if (Directory.Exists(_diretorioTemporario))
        {
            Directory.Delete(_diretorioTemporario, recursive: true);
        }
    }

    [Fact]
    public void ComandoDesconhecidoSemArquivo_DeveSerErroDeUso()
    {
        var resultado = Cli.Executar(["inventar"]);

        Assert.Equal(2, resultado.CodigoSaida);
        Assert.Contains("Comando desconhecido", resultado.Texto, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("validate")]
    [InlineData("parse")]
    [InlineData("inspect")]
    public void CaminhoVazio_DeveRetornarErroDeDadosSemLancar(string comando)
    {
        var resultado = Cli.Executar([comando, string.Empty]);

        Assert.Equal(1, resultado.CodigoSaida);
        Assert.Contains("Não foi possível ler", resultado.Texto, StringComparison.Ordinal);
    }

    [Fact]
    public void Generate_OutputEmDiretorioInexistente_DeveRetornarErroDeDadosSemLancar()
    {
        var destino = Path.Combine(_diretorioTemporario, "inexistente", "remessa.rem");

        var resultado = Cli.Executar([
            "generate", Exemplo(), "--bank", "237", "--layout", "400", "--output", destino,
        ]);

        Assert.Equal(1, resultado.CodigoSaida);
        Assert.Contains("Não foi possível gravar", resultado.Texto, StringComparison.Ordinal);
    }

    [Fact]
    public void BancoNaoSuportado_DeveSerErroDeUso()
    {
        var resultado = Cli.Executar(["validate", Golden("341_cobranca_400.rem"), "--bank", "999"]);

        Assert.Equal(2, resultado.CodigoSaida);
        Assert.Contains("Banco 999 não é suportado", resultado.Texto, StringComparison.Ordinal);
    }

    [Fact]
    public void Generate_HoraInvalida_DeveExplicarOCampo()
    {
        var json = JsonNode.Parse(File.ReadAllText(Exemplo()))!;
        json["horaGeracao"] = "00:60:00";
        var caminho = GravarTexto("hora-invalida.json", json.ToJsonString());

        var resultado = Cli.Executar(["generate", caminho, "--bank", "237", "--layout", "400"]);

        Assert.Equal(1, resultado.CodigoSaida);
        Assert.Contains("horaGeracao inválida", resultado.Texto, StringComparison.Ordinal);
    }

    [Fact]
    public void Generate_ComLf_DeveGravarSomenteLfESemBom()
    {
        var destino = Path.Combine(_diretorioTemporario, "remessa-lf.rem");

        var resultado = Cli.Executar([
            "generate", Exemplo(), "--bank", "237", "--layout", "400", "--lf", "--output", destino,
        ]);

        Assert.Equal(0, resultado.CodigoSaida);
        var bytes = File.ReadAllBytes(destino);
        Assert.DoesNotContain((byte)0x0D, bytes);
        Assert.Equal(0x0A, bytes[400]);
        Assert.NotEqual(0xEF, bytes[0]); // nenhum BOM
    }

    [Fact]
    public void Generate_SemOutput_DeveEscreverBytesLatin1NoStdout()
    {
        var resultado = Cli.Executar(["generate", Exemplo(), "--bank", "237", "--layout", "400"]);

        Assert.Equal(0, resultado.CodigoSaida);
        Assert.NotNull(resultado.ConteudoBinario);
        Assert.Equal(1206, resultado.ConteudoBinario.Length);
        Assert.Equal((byte)'0', resultado.ConteudoBinario[0]);
        Assert.Equal(0x0D, resultado.ConteudoBinario[400]);
        Assert.Equal(0x0A, resultado.ConteudoBinario[401]);
        Assert.Equal(string.Empty, resultado.Texto);
    }

    [Fact]
    public void Generate_CaminhoDoWindowsComEspacosEAcentos_DeveGravarEValidar()
    {
        var diretorio = Path.Combine(_diretorioTemporario, "pasta com espaços e Ação");
        Directory.CreateDirectory(diretorio);
        var destino = Path.Combine(diretorio, "remessa de teste.rem");

        var gerado = Cli.Executar([
            "generate", Exemplo(), "--bank", "341", "--layout", "240", "--output", destino,
        ]);
        Assert.Equal(0, gerado.CodigoSaida);
        Assert.True(File.Exists(destino));

        var validado = Cli.Executar(["validate", destino, "--bank", "341"]);
        Assert.Equal(0, validado.CodigoSaida);
    }

    [Fact]
    public void Generate_MuitosTitulos_DeveGerarEValidarTodosOsRegistros()
    {
        const int quantidade = 1000;
        var json = JsonNode.Parse(File.ReadAllText(Exemplo()))!;
        var titulos = (JsonArray)json["titulos"]!;
        var modelo = titulos[0]!.DeepClone();
        var muitos = new JsonArray();
        for (var i = 0; i < quantidade; i++)
        {
            muitos.Add(modelo.DeepClone());
        }

        json["titulos"] = muitos;
        var caminho = GravarTexto("muitos-titulos.json", json.ToJsonString());

        var gerado = Cli.Executar(["generate", caminho, "--bank", "237", "--layout", "400"]);
        Assert.Equal(0, gerado.CodigoSaida);
        Assert.NotNull(gerado.ConteudoBinario);

        var conteudo = Encoding.Latin1.GetString(gerado.ConteudoBinario);
        var arquivo = GravarTexto("muitos-titulos.rem", conteudo);
        var validado = Cli.Executar(["validate", arquivo, "--bank", "237"]);
        Assert.Equal(0, validado.CodigoSaida);
        Assert.Contains($"{quantidade + 2} registro(s)", validado.Texto, StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_ArquivoComBomUtf8_DeveValidarNormalmente()
    {
        var original = File.ReadAllBytes(Golden("341_cobranca_400.rem"));
        var comBom = new byte[original.Length + 3];
        comBom[0] = 0xEF;
        comBom[1] = 0xBB;
        comBom[2] = 0xBF;
        original.CopyTo(comBom, 3);
        var caminho = Path.Combine(_diretorioTemporario, "com-bom.rem");
        File.WriteAllBytes(caminho, comBom);

        var resultado = Cli.Executar(["validate", caminho]);

        Assert.Equal(0, resultado.CodigoSaida);
        Assert.Contains("OK: 4 registro(s)", resultado.Texto, StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_SeparadorCrSolto_DeveSepararRegistros()
    {
        var conteudo = Encoding.Latin1.GetString(File.ReadAllBytes(Golden("341_cobranca_400.rem")))
            .Replace("\r\n", "\r", StringComparison.Ordinal);
        var caminho = GravarTexto("apenas-cr.rem", conteudo);

        var resultado = Cli.Executar(["validate", caminho]);

        Assert.Equal(0, resultado.CodigoSaida);
        Assert.Contains("OK: 4 registro(s)", resultado.Texto, StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_NumericoComEspacoNoMeio_DeveSerRejeitadoComCampo()
    {
        var conteudo = Encoding.Latin1.GetString(File.ReadAllBytes(Golden("341_cobranca_400.rem")));
        var caracteres = conteudo.ToCharArray();
        "0 57".CopyTo(0, caracteres, 26, 4); // posições 27-30 (agência)
        var caminho = GravarTexto("espaco-numerico.rem", new string(caracteres));

        var resultado = Cli.Executar(["validate", caminho, "--bank", "341"]);

        Assert.Equal(1, resultado.CodigoSaida);
        Assert.Contains("colunas 27-30", resultado.Texto, StringComparison.Ordinal);
        Assert.Contains("campo 'Agência'", resultado.Texto, StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_TrailerDeArquivoAusente_DeveSerRejeitado()
    {
        var linhas = ArquivoCnab.SepararLinhas(
            Encoding.Latin1.GetString(File.ReadAllBytes(Golden("341_cobranca_400.rem"))));
        var caminho = GravarTexto("sem-trailer.rem", string.Join("\r\n", linhas.Take(linhas.Count - 1)) + "\r\n");

        var resultado = Cli.Executar(["validate", caminho, "--bank", "341"]);

        Assert.Equal(1, resultado.CodigoSaida);
        Assert.Contains("trailer de arquivo ausente", resultado.Texto, StringComparison.Ordinal);
    }

    [Fact]
    public void Parse_RetornoComCampoNumericoCorrompido_DeveApontarLinhaECampo()
    {
        var conteudo = Encoding.Latin1.GetString(File.ReadAllBytes(Golden("341_cobranca_400.ret")));
        var caracteres = conteudo.ToCharArray();
        var inicioDoDetalhe = conteudo.IndexOf("\r\n", StringComparison.Ordinal) + 2;
        "ABCD".CopyTo(0, caracteres, inicioDoDetalhe + 26, 4); // agência no detalhe
        var caminho = GravarTexto("retorno-corrompido.ret", new string(caracteres));

        var resultado = Cli.Executar(["parse", caminho, "--json"]);

        Assert.Equal(1, resultado.CodigoSaida);
        Assert.Contains("linha 2", resultado.Texto, StringComparison.Ordinal);
        Assert.Contains("campo", resultado.Texto, StringComparison.Ordinal);
    }

    private static string Golden(string nome) =>
        Path.Combine(AppContext.BaseDirectory, "Golden", nome);

    private static string Exemplo() =>
        Path.Combine(AppContext.BaseDirectory, "examples", "titulos.json");

    private string GravarTexto(string nome, string conteudo)
    {
        var caminho = Path.Combine(_diretorioTemporario, nome);
        File.WriteAllBytes(caminho, Encoding.Latin1.GetBytes(conteudo));
        return caminho;
    }
}
