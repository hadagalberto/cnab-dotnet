namespace Cnab.Tests;

using System.Diagnostics;
using Cnab.Arquivo;
using Xunit;

/// <summary>
/// Teste de desempenho do motor: 1.000.000 de linhas CNAB 240 precisam ser
/// analisadas dentro de um limite generoso (o SPEC pede apenas que o limite
/// seja generoso). A soma de verificação impede que o compilador elimine o
/// trabalho e garante que todas as linhas foram realmente lidas.
/// </summary>
public class DesempenhoParseUmMilhaoTests
{
    private const int QuantidadeLinhas = 1_000_000;

    // Limite propositalmente generoso: ~60 s para 1M linhas (≈60 µs/linha),
    // muito acima do custo real do leitor compilado (~0,2–2 µs/linha). Serve
    // para detectar regressões grosseiras sem tornar a CI instável.
    private static readonly TimeSpan LimiteGeneroso = TimeSpan.FromSeconds(60);

    [Fact]
    [Trait("Categoria", "Desempenho")]
    public void ParseDeUmMilhaoDeLinhasDentroDoLimiteGeneroso()
    {
        var modelo = new RegistroDesempenho240
        {
            Banco = 33,
            Lote = 1,
            TipoRegistro = 3,
            Inscricao = 12345678901234,
            NomeEmpresa = "EMPRESA MODELO LTDA",
            DataGeracao = new DateOnly(2026, 11, 20),
            HoraGeracao = 101530,
            SequencialArquivo = 1,
            Densidade = 1600,
            ValorTitulo = 1500.50m,
            Vencimento = new DateOnly(2026, 12, 20),
        };
        var linha = RegistroCnab<RegistroDesempenho240>.Escrever(modelo);
        Assert.Equal(240, linha.Length);
        var linhas = new string[QuantidadeLinhas];
        Array.Fill(linhas, linha);

        var cronometro = Stopwatch.StartNew();
        long soma = 0;
        for (var i = 0; i < linhas.Length; i++)
        {
            soma += RegistroCnab<RegistroDesempenho240>.Ler(linhas[i], i + 1).Banco;
        }

        cronometro.Stop();

        Assert.Equal(33L * QuantidadeLinhas, soma);
        Assert.True(
            cronometro.Elapsed < LimiteGeneroso,
            $"Parse de {QuantidadeLinhas:N0} linhas levou {cronometro.Elapsed.TotalSeconds:F2}s; "
            + $"limite generoso: {LimiteGeneroso.TotalSeconds:F0}s.");
    }
}

/// <summary>Registro de 240 posições usado apenas no teste de desempenho.</summary>
[RegistroCnab(240)]
public sealed record RegistroDesempenho240
{
    /// <summary>Código do banco, posições 1-3.</summary>
    [CampoCnab("Código do Banco", 1, 3, TipoCampo.Numerico)]
    public int Banco { get; init; }

    /// <summary>Lote de serviço, posições 4-7.</summary>
    [CampoCnab("Lote de Serviço", 4, 4, TipoCampo.Numerico)]
    public int Lote { get; init; }

    /// <summary>Tipo de registro, posição 8.</summary>
    [CampoCnab("Tipo de Registro", 8, 1, TipoCampo.Numerico)]
    public int TipoRegistro { get; init; }

    /// <summary>Inscrição da empresa, posições 18-32.</summary>
    [CampoCnab("Inscrição da Empresa", 18, 14, TipoCampo.Numerico)]
    public long Inscricao { get; init; }

    /// <summary>Nome da empresa, posições 73-102.</summary>
    [CampoCnab("Nome da Empresa", 73, 30, TipoCampo.Alfanumerico)]
    public string NomeEmpresa { get; init; } = string.Empty;

    /// <summary>Data de geração, posições 144-151.</summary>
    [CampoCnab("Data de Geração", 144, 8, TipoCampo.DataDdMmAaaa)]
    public DateOnly DataGeracao { get; init; }

    /// <summary>Hora de geração, posições 152-157.</summary>
    [CampoCnab("Hora de Geração", 152, 6, TipoCampo.Numerico)]
    public int HoraGeracao { get; init; }

    /// <summary>Sequencial do arquivo, posições 158-163.</summary>
    [CampoCnab("Sequencial do Arquivo", 158, 6, TipoCampo.Numerico)]
    public int SequencialArquivo { get; init; }

    /// <summary>Densidade de gravação, posições 167-171.</summary>
    [CampoCnab("Densidade", 167, 5, TipoCampo.Numerico)]
    public int Densidade { get; init; }

    /// <summary>Valor com duas casas decimais, posições 221-233.</summary>
    [CampoCnab("Valor do Título", 221, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorTitulo { get; init; }

    /// <summary>Vencimento em DDMMAA, posições 180-185.</summary>
    [CampoCnab("Vencimento", 180, 6, TipoCampo.DataDdMmAa)]
    public DateOnly Vencimento { get; init; }
}
