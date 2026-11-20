namespace Cnab.Tests;

using Cnab.Digitos;
using Xunit;

/// <summary>
/// Vetores dos algoritmos de módulo 10/11, extraídos dos manuais oficiais
/// citados em cada caso.
/// </summary>
public class DigitosTests
{
    // Módulo 10 — Itaú, Anexo 3 (manual jan/2017): campo 1 "341911012" →
    // soma 29 → resto 9 → DAC 1; campo 2 "3456788005" → soma 42 → DAC 8;
    // campo 3 "7123457000" → soma 29 → DAC 1.
    [Theory]
    [InlineData("341911012", 1)]
    [InlineData("3456788005", 8)]
    [InlineData("7123457000", 1)]
    public void Modulo10_CorrigeComAnexo3DoItau(string campo, int dvEsperado)
    {
        Assert.Equal(dvEsperado, Modulo.Modulo10([.. campo]));
    }

    // Módulo 10 — Bradesco, "Cálculo dos DACs dos Campos da Linha Digitável":
    // campo 1 "237900310" → total 28 → DV 2; campo 2 "4003177200" → 27 → 3;
    // campo 3 "2800952790" → 35 → 5.
    [Theory]
    [InlineData("237900310", 2)]
    [InlineData("4003177200", 3)]
    [InlineData("2800952790", 5)]
    public void Modulo10_CorrigeComBradesco(string campo, int dvEsperado)
    {
        Assert.Equal(dvEsperado, Modulo.Modulo10([.. campo]));
    }

    // Módulo 10 — Caixa SIGCB 5.5.1: campo 1 "104900550" → soma 25 → DV 5.
    [Fact]
    public void Modulo10_CorrigeComCaixa()
    {
        Assert.Equal(5, Modulo.Modulo10("104900550".ToCharArray()));
    }

    // Módulo 11 — Itaú Anexo 2: código de barras sem DV
    // "3419166700000123451101234567880057123457000" → soma 742 → resto 5 →
    // DAC 6.
    [Fact]
    public void RestoModulo11_CorrigeComAnexo2DoItau()
    {
        Assert.Equal(5, Modulo.RestoModulo11("3419166700000123451101234567880057123457000".ToCharArray(), 9));
        Assert.Equal(6, Modulo.DvCodigoDeBarras("3419166700000123451101234567880057123457000".ToCharArray()));
    }

    // Módulo 11 — Bradesco "Módulo 11 com base 7": carteira 19 + nosso número
    // 00000000002 → soma 69 → resto 3 → DV 8; NN 00000000001 → soma 67 →
    // resto 1 → DV "P"; NN 00000000006 → soma 77 → resto 0 → DV 0.
    [Theory]
    [InlineData("1900000000002", 3)]
    [InlineData("1900000000001", 1)]
    [InlineData("1900000000006", 0)]
    public void RestoModulo11_Base7_CorrigeComBradesco(string digitos, int restoEsperado)
    {
        Assert.Equal(restoEsperado, Modulo.RestoModulo11([.. digitos], 7));
    }

    // DV geral — Bradesco: 43 posições do exemplo do manual (banco 237, fator
    // 1001, valor 0, campo livre 0031040031772002800952790) → DV 7.
    [Fact]
    public void DvCodigoDeBarras_CorrigeComExemploBradesco()
    {
        var semDv = "2379" + "1001" + "0000000000" + "0031040031772002800952790";
        Assert.Equal(7, Modulo.DvCodigoDeBarras([.. semDv]));
    }

    // DV geral — Santander v34: exemplo do manual (PSK 0282033, NN
    // 5666124578002, carteira 101, fator 2046, valor 273,71) → DV 8
    // (somatório 696; 696 x 10 / 11 com resto 8 — variante equivalente à
    // regra FEBRABAN).
    [Fact]
    public void DvCodigoDeBarras_CorrigeComExemploSantander()
    {
        var semDv = "0339" + "2046" + "0000027371" + "9028203356661245780020101";
        Assert.Equal(8, Modulo.DvCodigoDeBarras([.. semDv]));
    }

    // DV geral — Caixa SIGCB Anexo I: banco 104, moeda 9, fator 3242, valor
    // 321,12, beneficiário 005507 DV 7, NN 14/222333777777777, DV do campo
    // livre 1 → soma 788 → resto 7 → DV 4 ("não admite 0").
    [Fact]
    public void DvCodigoDeBarras_CorrigeComExemploCaixa()
    {
        var semDv = "1049" + "3242" + "0000032112" + "0055077" + "222" + "1" + "333" + "4" + "777777777" + "1";
        Assert.Equal(4, Modulo.DvCodigoDeBarras([.. semDv]));
    }

    [Theory]
    [InlineData("1a2")]
    [InlineData("12!")]
    public void Modulo10_RejeitaCaracteresNaoNumericos(string digitos)
    {
        Assert.Throws<ArgumentException>(() => Modulo.Modulo10([.. digitos]));
    }

    [Fact]
    public void RestoModulo11_RejeitaBaseInvalida()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Modulo.RestoModulo11("11".ToCharArray(), 10));
    }
}
