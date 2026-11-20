namespace Cnab.Tests;

using Cnab.Arquivo;
using Cnab.Bancos;
using Cnab.Boleto;
using Cnab.Modelo;
using Xunit;

/// <summary>
/// Vetores adicionais obtidos de manuais oficiais buscados durante o marco 1:
/// Anexo XI do manual de boleto do BB, seção 7.3 do manual CNAB 400 do Inter
/// e seção "Código de Barras" do portal do desenvolvedor BTG.
/// </summary>
public class AlgoritmosAdicionaisTests
{
    // BB – Doc5175Bloqueto.pdf (maio/2019), Anexo XI:
    // "Nosso-Número: 05009401448-? ... Soma 221 ... Resto 1 ... DV = 1".
    [Theory]
    [InlineData("05009401448", "1")]
    [InlineData("12341234567", "1")] // Particularidades BB CNAB 240, campo 13.3P
    public void Bb_DvNossoNumeroSegueAnexoXi(string baseCalculo, string dvEsperado)
    {
        Assert.Equal(dvEsperado, BancoDoBrasil.CalcularDvNossoNumero(baseCalculo));
    }

    [Fact]
    public void Bb_CompoeNossoNumeroComDvCalculado()
    {
        Assert.Equal("050094014481", BancoDoBrasil.ComporNossoNumeroConvenio4("0500", "9401448"));
        Assert.Equal("123412345671", BancoDoBrasil.ComporNossoNumeroConvenio6("123412", "34567"));
    }

    [Fact]
    public void Bb_DvAgenciaSegueOMesmoAnexoXi()
    {
        // Anexo XI: agência 1234 → 4*9 + 3*8 + 2*7 + 1*6 = 80; 80 % 11 = 3.
        Assert.Equal("3", BancoDoBrasil.CalcularDvAgencia("1234"));
    }

    [Fact]
    public void Bb_CampoLivreConvenio7SegueAnexoIx()
    {
        // Anexo IX: zeros(6) + convênio(7) + sequencial(10) + carteira(2).
        var campoLivre = BancoDoBrasil.ComporCampoLivreConvenio7("1234567", "0000000123", "18");
        Assert.Equal("0000001234567000000012318", campoLivre);
        Assert.Equal(25, campoLivre.Length);

        var codigo = CodigoDeBarras.Compor(Bancos.BancoDoBrasil, new DateOnly(2026, 8, 15), 127.50m, campoLivre);
        Assert.Equal(44, codigo.Length);
        Assert.True(CodigoDeBarras.ConferirDvGeral(codigo));
    }

    [Fact]
    public void Bb_CampoLivreConvenio4SegueAnexoVii()
    {
        var campoLivre = BancoDoBrasil.ComporCampoLivreConvenio4("0500", "9401448", "1234", "12345678", "17");
        Assert.Equal("0500940144812341234567817", campoLivre);
        Assert.Equal(25, campoLivre.Length);
    }

    // Inter – manual CNAB400 V9 (06/07/2026), seção 7.3:
    // agência 0001, carteira 110, NN 0004309540 → base "00011100004309540",
    // soma 29, resto 9, DV = 10 - 9 = 1 ("0004309540-1").
    [Fact]
    public void Inter_DvNossoNumeroSegueManual()
    {
        Assert.Equal(1, Inter.CalcularDvNossoNumero("0001", "110", "0004309540"));
        Assert.Equal("00043095401", Inter.ComporNossoNumero("0001", "110", "0004309540"));
    }

    [Fact]
    public void Inter_CampoLivreSegueSecao713()
    {
        var nn = Inter.ComporNossoNumero("0001", "110", "0004309540");
        var campoLivre = Inter.ComporCampoLivre("0001", "110", "0000123", nn);
        // 0001 (agência) + 110 (carteira) + 0000123 (operação) + 00043095401 (NN com DV).
        Assert.Equal("0001110000012300043095401", campoLivre);
        Assert.Equal(25, campoLivre.Length);

        var codigo = CodigoDeBarras.Compor(Bancos.Inter, new DateOnly(2026, 12, 21), 100m, campoLivre);
        Assert.True(CodigoDeBarras.ConferirDvGeral(codigo));
        Assert.True(LinhaDigitavel.ConferirDvs(LinhaDigitavel.Compor(codigo)));
    }

    // BTG – portal do desenvolvedor, seção "Código de Barras":
    // agência(4) + carteira(2) + NN(11, sem DV) + conta(7) + zero fixo.
    [Fact]
    public void Btg_CampoLivreSegueManualPublicado()
    {
        var campoLivre = Btg.ComporCampoLivre("1234", "01", "00000012345", "1234567");
        Assert.Equal("1234010000001234512345670", campoLivre);
        Assert.Equal(25, campoLivre.Length);

        var codigo = CodigoDeBarras.Compor(Bancos.Btg, new DateOnly(2026, 12, 21), 50m, campoLivre);
        Assert.True(CodigoDeBarras.ConferirDvGeral(codigo));
    }

    [Fact]
    public void Bb_DvRejeitaBaseInvalida()
    {
        Assert.Throws<ArgumentException>(() => BancoDoBrasil.CalcularDvNossoNumero("0500940144"));
        Assert.Throws<ArgumentException>(() => BancoDoBrasil.CalcularDvNossoNumero("0500940144A"));
        Assert.Throws<ArgumentException>(() => Inter.CalcularDvNossoNumero("001", "110", "0004309540"));
    }

    [Fact]
    public void Ler_NumericoIntComValorAcimaDaFaixaGeraErroPreciso()
    {
        var erro = Assert.Throws<CampoCnabException>(
            () => RegistroCnab<RegistroIntGrande>.Ler(new string('9', 10), numeroLinha: 3));
        Assert.Equal("Valor Grande", erro.Campo);
        Assert.Equal(3, erro.Linha);
        Assert.Equal(1, erro.Coluna);
        Assert.Contains("faixa do tipo int", erro.Message, StringComparison.Ordinal);
    }

    [RegistroCnab(10)]
    private sealed record RegistroIntGrande
    {
        [CampoCnab("Valor Grande", 1, 10, TipoCampo.Numerico)]
        public int Valor { get; init; }
    }
}
