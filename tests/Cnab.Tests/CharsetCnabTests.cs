namespace Cnab.Tests;

using Cnab.Texto;
using Xunit;

public class CharsetCnabTests
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("empresa ltda", "EMPRESA LTDA")]
    [InlineData("José Açaí", "JOSE ACAI")]
    [InlineData("ÇÃÃO de Ç", "CAAO DE C")]
    [InlineData("caixa alta forçada pelo banco", "CAIXA ALTA FORCADA PELO BANCO")]
    [InlineData("100% garantido!", "100% GARANTIDO!")]
    [InlineData("com pontos, virgulas; dois-pontos: e barra/", "COM PONTOS, VIRGULAS; DOIS-PONTOS: E BARRA/")]
    [InlineData("traco - igual = mais + abre ( fecha )", "TRACO - IGUAL = MAIS + ABRE ( FECHA )")]
    [InlineData("colchetes [x] e chaves {y}", "COLCHETES [X] E CHAVES {Y}")]
    [InlineData("aspas “curvas” viram espaco", "ASPAS  CURVAS  VIRAM ESPACO")]
    [InlineData("apostrofo curvo ’ vira espaco", "APOSTROFO CURVO   VIRA ESPACO")]
    [InlineData("travessão longo — vira hífen", "TRAVESSAO LONGO - VIRA HIFEN")]
    [InlineData("ampersand & nao existe", "AMPERSAND   NAO EXISTE")]
    [InlineData("ç", "C")]
    [InlineData("Ç", "C")]
    public void Normalizar_AplicaRegrasDoCharsetCnab(string? entrada, string esperado)
    {
        Assert.Equal(esperado, CharsetCnab.Normalizar(entrada));
    }

    [Fact]
    public void Normalizar_EmojiViraEspacoPorCaractere()
    {
        // O emoji ocupa dois chars (par substituto) e cada um vira um espaço.
        var entrada = "emoji \U0001F600 vira espaco";
        Assert.Equal("EMOJI    VIRA ESPACO", CharsetCnab.Normalizar(entrada));
    }

    [Fact]
    public void Normalizar_EIdempotente()
    {
        const string texto = "José & Cia Ltda — 100% Çedilha";
        var primeira = CharsetCnab.Normalizar(texto);
        var segunda = CharsetCnab.Normalizar(primeira);
        Assert.Equal(primeira, segunda);
    }

    [Theory]
    [InlineData("ABC 123", true)]
    [InlineData("SEM ACENTO", true)]
    [InlineData("áéíóú", false)]
    [InlineData("a&b", false)]
    [InlineData("com tab\t", false)]
    public void EstaNoCharset_VerificaConjunto(string texto, bool esperado)
    {
        Assert.Equal(esperado, CharsetCnab.EstaNoCharset(texto));
    }

    [Fact]
    public void Normalizar_MantemSoh64CaracteresAsciiOuEspeciais()
    {
        var entrada = "ÁÉÍÓÚÀÈÌÒÙÂÊÔÃÕÇ áéíóúâêôãõç 0-9 !*-()[]{},.;:/%+=";
        var resultado = CharsetCnab.Normalizar(entrada);
        Assert.All(resultado, c => Assert.True(
            char.IsAsciiLetterOrDigit(c) || " !*-()[]{},.;:/%+=".Contains(c, StringComparison.Ordinal),
            $"caractere '{c}' fora do charset"));
    }
}
