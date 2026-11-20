namespace Cnab.Tests;

using Cnab.Boleto;
using Xunit;

/// <summary>
/// Vetores oficiais do fator de vencimento, publicados em:
/// Bradesco "Layout da Cobrança (4008-524-0121)" — tabela de correlação;
/// Itaú "Cobrança CNAB 400 (jan/2017)" — Anexo 6;
/// Caixa "Especificações técnicas SIGCB (67.119 v014)" — Anexo II;
/// Santander "Código de Barras – Cobrança v34" — fator vencimento;
/// Sicredi "CNAB 240 – Cobrança" — item 10.7 (inclui Comunicados FB-082 e
/// FB-122 da FEBRABAN);
/// Sicoob "Layouts para troca de informações — Especificações do Boleto".
/// </summary>
public class FatorDeVencimentoTests
{
    [Theory]
    [InlineData("2000-07-03", 1000)] // Bradesco/Itaú/Caixa/Sicredi/Sicoob
    [InlineData("2000-07-05", 1002)] // Bradesco/Caixa/Sicredi
    [InlineData("2000-12-06", 1156)] // Santander
    [InlineData("2000-12-15", 1165)] // Santander
    [InlineData("2000-12-30", 1180)] // Santander
    [InlineData("2002-05-01", 1667)] // Bradesco/Itaú/Caixa/Sicredi
    [InlineData("2007-12-20", 3726)] // Sicredi 10.7
    [InlineData("2010-11-17", 4789)] // Bradesco/Itaú/Caixa/Sicredi
    [InlineData("2025-02-21", 9999)] // último do ciclo original
    [InlineData("2025-02-22", 1000)] // reinício FEBRABAN (FB-082/FB-122)
    [InlineData("2025-02-23", 1001)]
    [InlineData("2025-02-24", 1002)]
    [InlineData("2026-12-21", 1667)] // Caixa, ciclo novo
    [InlineData("2035-07-09", 4789)] // Caixa, ciclo novo
    [InlineData("2049-10-13", 9999)] // Caixa, ciclo novo
    public void Calcular_CorrigeComTabelasOficiais(string vencimentoTexto, int fatorEsperado)
    {
        var vencimento = DateOnly.Parse(vencimentoTexto, System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(fatorEsperado, FatorDeVencimento.Calcular(vencimento));
    }

    [Fact]
    public void Calcular_SemVencimentoRetornaZero()
    {
        Assert.Equal(0, FatorDeVencimento.Calcular(null));
    }

    [Fact]
    public void Calcular_VencimentoAnteriorADataBaseERejeitado()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => FatorDeVencimento.Calcular(new DateOnly(1997, 10, 6)));
    }

    [Fact]
    public void CalcularFator_FormatoDeQuatroDigitos()
    {
        // Ocupação de 4 dígitos conferida nas bordas dos dois ciclos.
        Assert.Equal("1000", FatorDeVencimento.Calcular(new DateOnly(2000, 7, 3)).ToString("0000"));
        Assert.Equal("9999", FatorDeVencimento.Calcular(new DateOnly(2025, 2, 21)).ToString("0000"));
        Assert.Equal("0000", FatorDeVencimento.Calcular(null).ToString("0000"));
    }

    [Fact]
    public void ParaVencimento_InverteOCalculoNoCicloOriginal()
    {
        // Fatores de 1000 a 9999 existem nos dois ciclos: a inversão adota o
        // ciclo original (data-base 07/10/1997), documentado aqui. As datas do
        // ciclo novo só são alcançáveis pelo fator na via inversa quando não há
        // colisão (nenhum fator do ciclo novo existe ainda como fator menor que
        // o limite do ciclo original).
        foreach (var (vencimento, fator) in new[]
        {
            (new DateOnly(2000, 7, 3), 1000),
            (new DateOnly(2002, 5, 1), 1667),
            (new DateOnly(2010, 11, 17), 4789),
            (new DateOnly(2025, 2, 21), 9999),
        })
        {
            Assert.Equal(vencimento, FatorDeVencimento.ParaVencimento(fator));
        }

        // A via direta cobre os dois ciclos: 22/02/2025 inicia o ciclo novo.
        Assert.Equal(1000, FatorDeVencimento.Calcular(new DateOnly(2025, 2, 22)));
        Assert.Equal(1667, FatorDeVencimento.Calcular(new DateOnly(2026, 12, 21)));
    }

    [Fact]
    public void ParaVencimento_FatorZeroRetornaNulo()
    {
        Assert.Null(FatorDeVencimento.ParaVencimento(0));
    }

    [Theory]
    [InlineData(999)]
    [InlineData(10000)]
    [InlineData(-1)]
    public void ParaVencimento_FatoresForaDoDominioSaoRejeitados(int fator)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FatorDeVencimento.ParaVencimento(fator));
    }
}
