using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Cnab.Arquivo;

namespace Cnab.Benchmarks;

/// <summary>
/// Registro de 240 posições usado nos benchmarks do motor. Espelha a forma de
/// um header/detalhe CNAB 240 (numérico, decimal, datas e alfanumérico) sem
/// representar layout de banco específico.
/// </summary>
[RegistroCnab(240)]
public sealed record RegistroBenchmark
{
    [CampoCnab("Código do Banco", 1, 3, TipoCampo.Numerico)]
    public int Banco { get; init; }

    [CampoCnab("Lote de Serviço", 4, 4, TipoCampo.Numerico)]
    public int Lote { get; init; }

    [CampoCnab("Tipo de Registro", 8, 1, TipoCampo.Numerico)]
    public int TipoRegistro { get; init; }

    [CampoCnab("Inscrição da Empresa", 18, 14, TipoCampo.Numerico)]
    public long Inscricao { get; init; }

    [CampoCnab("Nome da Empresa", 73, 30, TipoCampo.Alfanumerico)]
    public string NomeEmpresa { get; init; } = string.Empty;

    [CampoCnab("Data de Geração", 144, 8, TipoCampo.DataDdMmAaaa)]
    public DateOnly DataGeracao { get; init; }

    [CampoCnab("Hora de Geração", 152, 6, TipoCampo.Numerico)]
    public int HoraGeracao { get; init; }

    [CampoCnab("Sequencial do Arquivo", 158, 6, TipoCampo.Numerico)]
    public int SequencialArquivo { get; init; }

    [CampoCnab("Filler Layout", 164, 3, TipoCampo.Filler, ConteudoFiller = "081")]
    public string FillerLayout { get; init; } = string.Empty;

    [CampoCnab("Densidade", 167, 5, TipoCampo.Numerico)]
    public int Densidade { get; init; }

    [CampoCnab("Valor do Título", 221, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal? ValorTitulo { get; init; }

    [CampoCnab("Vencimento", 180, 6, TipoCampo.DataDdMmAa)]
    public DateOnly? VencimentoCurto { get; init; }
}

/// <summary>
/// Benchmarks do motor de registros. O SPEC pede o parse de 1.000.000 de
/// linhas CNAB 240 dentro de um limite generoso; este benchmark roda
/// amostras menores por iteração e reporta o custo por linha — a
/// extração para 1 milhão de linhas é a multiplicação direta dos valores
/// medidos.
/// </summary>
public class MotorDeRegistros
{
    private string[] _linhas = [];

    [Params(1_000, 100_000)]
    public int QuantidadeLinhas { get; set; }

    /// <summary>Prepara as linhas de entrada do benchmark.</summary>
    [GlobalSetup]
    public void Preparar()
    {
        var modelo = new RegistroBenchmark
        {
            Banco = 33,
            Lote = 1,
            TipoRegistro = 3,
            Inscricao = 0,
            NomeEmpresa = "EMPRESA MODELO LTDA",
            DataGeracao = new DateOnly(2026, 11, 20),
            HoraGeracao = 101530,
            SequencialArquivo = 1,
            FillerLayout = string.Empty,
            Densidade = 1600,
            ValorTitulo = 1500.50m,
            VencimentoCurto = new DateOnly(2026, 12, 20),
        };
        var linha = RegistroCnab<RegistroBenchmark>.Escrever(modelo);
        _linhas = new string[QuantidadeLinhas];
        for (var i = 0; i < QuantidadeLinhas; i++)
        {
            _linhas[i] = linha;
        }
    }

    /// <summary>Lê todas as linhas com o leitor compilado.</summary>
    [Benchmark]
    public int LerTodasAsLinhas()
    {
        var total = 0;
        for (var i = 0; i < _linhas.Length; i++)
        {
            var registro = RegistroCnab<RegistroBenchmark>.Ler(_linhas[i], numeroLinha: i + 1);
            total += registro.Banco;
        }

        return total;
    }

    /// <summary>Escreve todas as linhas a partir do modelo lido.</summary>
    [Benchmark]
    public int EscreverTodasAsLinhas()
    {
        var total = 0;
        for (var i = 0; i < _linhas.Length; i++)
        {
            var registro = RegistroCnab<RegistroBenchmark>.Ler(_linhas[i], numeroLinha: i + 1);
            total += RegistroCnab<RegistroBenchmark>.Escrever(registro).Length;
        }

        return total;
    }
}

/// <summary>Ponto de entrada do projeto de benchmarks.</summary>
public static class Program
{
    /// <summary>Executa o BenchmarkDotNet com os benchmarks do projeto.</summary>
    /// <param name="args">Argumentos de linha de comando repassados ao BenchmarkDotNet.</param>
    public static void Main(string[] args) => BenchmarkRunner.Run(typeof(MotorDeRegistros), args: args);
}
