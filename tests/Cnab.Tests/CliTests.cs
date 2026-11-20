namespace Cnab.Tests;

using System.Text;
using System.Text.Json;
using Cnab.Cli;
using Xunit;

/// <summary>
/// Testes da ferramenta <c>cnab</c> invocada como biblioteca: mesmas rotas
/// usadas pelo <c>Main</c>, com saída capturada e arquivos temporários.
/// </summary>
public sealed class CliTests : IDisposable
{
    private readonly string _diretorioTemporario;

    public CliTests()
    {
        _diretorioTemporario = Path.Combine(Path.GetTempPath(), "cnab-cli-" + Guid.NewGuid().ToString("N"));
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
    public void SemArgumentos_MostraAjudaECodigoZero()
    {
        var resultado = Cli.Executar([]);

        Assert.Equal(0, resultado.CodigoSaida);
        Assert.Contains("Uso: cnab <comando>", resultado.Texto, StringComparison.Ordinal);
        Assert.Contains("validate", resultado.Texto, StringComparison.Ordinal);
        Assert.Null(resultado.ConteudoBinario);
    }

    [Fact]
    public void ComandoDesconhecido_EhErroDeUso()
    {
        var resultado = Cli.Executar(["inventar", "arquivo.rem"]);

        Assert.Equal(2, resultado.CodigoSaida);
        Assert.Contains("Comando desconhecido", resultado.Texto, StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_Remessa400Valida_RetornaZero()
    {
        var resultado = Cli.Executar(["validate", Golden("341_cobranca_400.rem")]);

        Assert.Equal(0, resultado.CodigoSaida);
        Assert.Contains("OK: 4 registro(s) CNAB 400", resultado.Texto, StringComparison.Ordinal);
        Assert.Contains("341 - Itaú", resultado.Texto, StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_Retorno240SinteticoValido_RetornaZero()
    {
        var caminho = Gravar("retorno240.ret", RetornoFixtures.Cnab240(208, 2, 1234.56m));

        var resultado = Cli.Executar(["validate", caminho]);

        Assert.Equal(0, resultado.CodigoSaida);
        Assert.Contains("CNAB 240", resultado.Texto, StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_LinhaComTamanhoErrado_RelataLinhaEComprimento()
    {
        var caminho = Gravar("curto.rem", new string('0', 239) + "\r\n");

        var resultado = Cli.Executar(["validate", caminho]);

        Assert.Equal(1, resultado.CodigoSaida);
        Assert.Contains("linha 1", resultado.Texto, StringComparison.Ordinal);
        Assert.Contains("esperado 240", resultado.Texto, StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_CampoNumericoInvalido_RelataColunaENomeDoCampo()
    {
        var original = ArquivoTexto.Ler(Golden("341_cobranca_400.rem"));
        var caracteres = original.ToCharArray();
        "ABCD".CopyTo(0, caracteres, 26, 4); // posições 27-30: agência
        var caminho = Gravar("invalido.rem", new string(caracteres));

        var resultado = Cli.Executar(["validate", caminho]);

        Assert.Equal(1, resultado.CodigoSaida);
        Assert.Contains("colunas 27-30", resultado.Texto, StringComparison.Ordinal);
        Assert.Contains("campo 'Agência'", resultado.Texto, StringComparison.Ordinal);
    }

    [Fact]
    public void Parse_Retorno400Json_ContemOcorrenciaMotivoEValores()
    {
        var resultado = Cli.Executar(["parse", Golden("341_cobranca_400.ret"), "--json"]);

        Assert.Equal(0, resultado.CodigoSaida);
        using var json = JsonDocument.Parse(resultado.Texto);
        var raiz = json.RootElement;
        Assert.Equal(400, raiz.GetProperty("layout").GetInt32());
        Assert.Equal(341, raiz.GetProperty("banco").GetProperty("codigo").GetInt32());
        var item = raiz.GetProperty("itens")[0];
        Assert.Equal("06", item.GetProperty("ocorrencia").GetProperty("codigo").GetString());
        Assert.Equal("03", item.GetProperty("motivos")[0].GetProperty("codigo").GetString());
        Assert.Equal(1500.50m, item.GetProperty("valores").GetProperty("valorPago").GetDecimal());
        Assert.Equal("2026-11-20", raiz.GetProperty("header").GetProperty("dataGeracao").GetString());
    }

    [Fact]
    public void Parse_Retorno240SinteticoJson_ContemDoisItens()
    {
        var caminho = Gravar("retorno240.ret", RetornoFixtures.Cnab240(748, 2, 999.99m));

        var resultado = Cli.Executar(["parse", caminho, "--json"]);

        Assert.Equal(0, resultado.CodigoSaida);
        using var json = JsonDocument.Parse(resultado.Texto);
        Assert.Equal(240, json.RootElement.GetProperty("layout").GetInt32());
        Assert.Equal(2, json.RootElement.GetProperty("quantidadeItens").GetInt32());
        var itens = json.RootElement.GetProperty("itens");
        Assert.Equal("06", itens[0].GetProperty("ocorrencia").GetProperty("codigo").GetString());
        Assert.Equal(999.99m, itens[1].GetProperty("valores").GetProperty("valorPago").GetDecimal());
    }

    [Fact]
    public void Parse_SemJson_ImprimeResumoLegivel()
    {
        var resultado = Cli.Executar(["parse", Golden("341_cobranca_400.ret")]);

        Assert.Equal(0, resultado.CodigoSaida);
        Assert.Contains("Retorno CNAB 400", resultado.Texto, StringComparison.Ordinal);
        Assert.Contains("98712345", resultado.Texto, StringComparison.Ordinal);
    }

    [Fact]
    public void Parse_ArquivoDeRemessa_Rejeitado()
    {
        var resultado = Cli.Executar(["parse", Golden("341_cobranca_400.rem")]);

        Assert.Equal(1, resultado.CodigoSaida);
        Assert.Contains("não é um retorno", resultado.Texto, StringComparison.Ordinal);
    }

    [Fact]
    public void Inspect_Remessa400_MostraPosicoesEValores()
    {
        var resultado = Cli.Executar(["inspect", Golden("341_cobranca_400.rem"), "--bank", "341"]);

        Assert.Equal(0, resultado.CodigoSaida);
        Assert.Contains("HeaderRemessaItau400", resultado.Texto, StringComparison.Ordinal);
        Assert.Contains("077-079", resultado.Texto, StringComparison.Ordinal);
        Assert.Contains("Código do Banco", resultado.Texto, StringComparison.Ordinal);
        Assert.Contains("341", resultado.Texto, StringComparison.Ordinal);
    }

    [Fact]
    public void Inspect_Retorno240_MostraSegmentosT_U()
    {
        var caminho = Gravar("retorno240.ret", RetornoFixtures.Cnab240(341, 1, 100m));

        var resultado = Cli.Executar(["inspect", caminho]);

        Assert.Equal(0, resultado.CodigoSaida);
        Assert.Contains("SegmentoT240", resultado.Texto, StringComparison.Ordinal);
        Assert.Contains("SegmentoU240", resultado.Texto, StringComparison.Ordinal);
        Assert.Contains("016-017", resultado.Texto, StringComparison.Ordinal);
    }

    [Fact]
    public void Generate_Bradesco400_ProduzArquivoValido()
    {
        var resultado = Cli.Executar([
            "generate",
            Exemplo(),
            "--bank", "237",
            "--layout", "400",
        ]);

        Assert.Equal(0, resultado.CodigoSaida);
        Assert.NotNull(resultado.ConteudoBinario);
        var conteudo = Encoding.Latin1.GetString(resultado.ConteudoBinario);
        var linhas = conteudo.Split("\r\n", StringSplitOptions.None);
        Assert.Equal(4, linhas.Length); // CRLF final gera entrada vazia
        Assert.All(linhas.Take(3), linha => Assert.Equal(400, linha.Length));
        Assert.Contains("BRADESCO", conteudo, StringComparison.Ordinal);

        var caminho = Gravar("gerado.rem", Encoding.Latin1.GetString(resultado.ConteudoBinario));
        var validacao = Cli.Executar(["validate", caminho, "--bank", "237"]);
        Assert.Equal(0, validacao.CodigoSaida);
    }

    [Fact]
    public void Generate_Itau240_ProduzArquivoValido()
    {
        var resultado = Cli.Executar([
            "generate",
            Exemplo(),
            "--bank", "341",
            "--layout", "240",
        ]);

        Assert.Equal(0, resultado.CodigoSaida);
        Assert.NotNull(resultado.ConteudoBinario);
        var conteudo = Encoding.Latin1.GetString(resultado.ConteudoBinario);
        var linhas = conteudo.Split("\r\n", StringSplitOptions.None);
        Assert.All(linhas.Take(6), linha => Assert.Equal(240, linha.Length));

        var caminho = Gravar("gerado240.rem", conteudo);
        Assert.Equal(0, Cli.Executar(["validate", caminho, "--bank", "341"]).CodigoSaida);
    }

    [Fact]
    public void Generate_ComOutput_GravaArquivoEmLatin1()
    {
        var destino = Path.Combine(_diretorioTemporario, "saida.rem");
        var resultado = Cli.Executar([
            "generate", Exemplo(), "--bank", "237", "--layout", "400", "--output", destino,
        ]);

        Assert.Equal(0, resultado.CodigoSaida);
        Assert.Null(resultado.ConteudoBinario);
        Assert.True(File.Exists(destino));
        var bytes = File.ReadAllBytes(destino);
        Assert.Equal(0x0D, bytes[400]);
        Assert.Equal(0x0A, bytes[401]);
        Assert.Contains("remessa", resultado.Texto, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Generate_SemBankOuLayout_ErroDeUso()
    {
        Assert.Equal(2, Cli.Executar(["generate", Exemplo(), "--layout", "400"]).CodigoSaida);
        Assert.Equal(2, Cli.Executar(["generate", Exemplo(), "--bank", "237"]).CodigoSaida);
        Assert.Equal(2, Cli.Executar(["generate", Exemplo(), "--bank", "237", "--layout", "500"]).CodigoSaida);
    }

    [Fact]
    public void Generate_JSONIncompleto_RetornaErroDeDados()
    {
        var caminho = Gravar("incompleto.json", "{\"dataGeracao\":\"2026-11-20\"}");

        var resultado = Cli.Executar(["generate", caminho, "--bank", "237", "--layout", "400"]);

        Assert.Equal(1, resultado.CodigoSaida);
        Assert.Contains("obrigatório", resultado.Texto, StringComparison.Ordinal);
    }

    private static string Golden(string nome) =>
        Path.Combine(AppContext.BaseDirectory, "Golden", nome);

    private static string Exemplo() =>
        Path.Combine(AppContext.BaseDirectory, "examples", "titulos.json");

    private string Gravar(string nome, string conteudo)
    {
        var caminho = Path.Combine(_diretorioTemporario, nome);
        File.WriteAllText(caminho, conteudo, Encoding.Latin1);
        return caminho;
    }
}
