namespace Cnab.Tests;

using Cnab.Bancos;
using Cnab.Modelo;
using Xunit;

/// <summary>
/// Vetores dos DVs por banco, extraídos dos manuais oficiais citados em cada
/// teste. Nenhum valor aqui foi inventado: todos vêm de exemplos resolvidos
/// dos manuais (ou do portal oficial do banco).
/// </summary>
public class DvBancosTests
{
    // Bradesco 4008-524-0121, "Identificações do Título no Banco":
    // carteira 19, NN 00000000002 → DV 8; NN 00000000001 → DV "P";
    // NN 00000000006 → DV 0.
    [Theory]
    [InlineData("19", "00000000002", "8")]
    [InlineData("19", "00000000001", "P")]
    [InlineData("19", "00000000006", "0")]
    public void Bradesco_DvNossoNumeroSegueManual(string carteira, string nn, string dv)
    {
        Assert.Equal(dv, Bradesco.CalcularDvNossoNumero(carteira, nn));
    }

    [Fact]
    public void Bradesco_ComporNossoNumeroAdicionaDv()
    {
        Assert.Equal("00000000002" + "8", Bradesco.ComporNossoNumero("19", "00000000002"));
        Assert.Equal("00000000001P", Bradesco.ComporNossoNumero("19", "00000000001"));
    }

    // Bradesco 4008-524-0121, "Montagem dos Dados do Código de Barras":
    // agência 31, carteira 04, NN 00317720028, conta 95279 + "0" final.
    [Fact]
    public void Bradesco_CampoLivreSegueManual()
    {
        var campoLivre = Bradesco.ComporCampoLivre("31", "04", "00317720028", "95279");
        Assert.Equal("0031040031772002800952790", campoLivre);
        Assert.Equal(25, campoLivre.Length);
    }

    // Itaú CNAB 400, Anexo 4: agência 0057, conta 12345, carteira 110,
    // NN 12345678 → DAC 8.
    [Fact]
    public void Itau_DacNossoNumeroSegueAnexo4()
    {
        Assert.Equal(8, Itau.CalcularDacNossoNumero("0057", "12345", "110", "12345678"));
    }

    // Itaú CNAB 400, Anexo 4 (exemplo do cabeçalho): carteira 198 usa só
    // carteira + NN; o DAC do exemplo "0057/12345-7" também confere a
    // agência/conta.
    [Fact]
    public void Itau_DacAgenciaContaConfereComExemploDoManual()
    {
        Assert.Equal(7, Itau.CalcularDacAgenciaConta("0057", "12345"));
    }

    [Fact]
    public void Itau_CampoLivreSegueExemploDoCodigoDeBarras()
    {
        var campoLivre = Itau.ComporCampoLivre("110", "12345678", "0057", "12345");
        Assert.Equal("1101234567880057123457000", campoLivre);
        Assert.Equal(25, campoLivre.Length);
    }

    [Fact]
    public void Itau_CarteirasComDacSoNossoNumero()
    {
        Assert.Contains("126", Itau.CarteirasDacSoNossoNumero);
        Assert.Contains("131", Itau.CarteirasDacSoNossoNumero);
        Assert.Contains("146", Itau.CarteirasDacSoNossoNumero);
        Assert.Contains("150", Itau.CarteirasDacSoNossoNumero);
        Assert.Contains("168", Itau.CarteirasDacSoNossoNumero);
        Assert.DoesNotContain("110", Itau.CarteirasDacSoNossoNumero);
    }

    // Santander, "Código de Barras – Cobrança v34", seção "Nosso Número":
    // NN 566612457800 → resto 9 → DV 2.
    [Fact]
    public void Santander_DvNossoNumeroSegueManual()
    {
        Assert.Equal(2, Santander.CalcularDvNossoNumero("566612457800"));
        Assert.Equal("5666124578002", Santander.ComporNossoNumero("566612457800"));
    }

    // Santander v34, layout do código de barras: fixo "9" + PSK 0282033 + NN
    // com DV 5666124578002 + fixo "0" + modalidade 101.
    [Fact]
    public void Santander_CampoLivreSegueLayoutDoManual()
    {
        var campoLivre = Santander.ComporCampoLivre(
            "0282033",
            "5666124578002",
            Santander.ModalidadeCarteira.CobrancaRapidaComRegistro);
        Assert.Equal("9028203356661245780020101", campoLivre);
        Assert.Equal(25, campoLivre.Length);
    }

    [Fact]
    public void Santander_CampoLivreCurtoZeraCincoPrimeirosDoNossoNumero()
    {
        var campoLivre = Santander.ComporCampoLivreNossoNumeroCurto(
            "0282033",
            "24578002",
            Santander.ModalidadeCarteira.CobrancaRapidaComRegistro);
        Assert.Equal("9028203300000245780020101", campoLivre);
    }

    // Caixa SIGCB Anexo IV: NN "1400000000000019" → soma 59, resto 4, DV 7.
    [Fact]
    public void Caixa_DvNossoNumeroSigcbSegueAnexoIv()
    {
        // Anexo IV: nosso número "14/000000000000019" (2 posições de
        // carteira/emissão + 15 livres) → soma 59, resto 4, DV 7.
        var nossoNumero = "14" + "000000000000019";
        Assert.Equal(17, nossoNumero.Length);
        Assert.Equal(7, Caixa.CalcularDvNossoNumero(nossoNumero));
        Assert.Equal(nossoNumero + "7", Caixa.ComporNossoNumero("1", "4", "000000000000019"));
    }

    // Caixa SIGCB Anexo VI: código do beneficiário 005507 → DV 7.
    [Fact]
    public void Caixa_DvCodigoBeneficiarioSegueAnexoVi()
    {
        Assert.Equal(7, Caixa.CalcularDvCodigoBeneficiario("005507"));
    }

    // Caixa SIGCB Anexo III: campo livre do exemplo → DV 1.
    [Fact]
    public void Caixa_CampoLivreSegueAnexoIii()
    {
        var campoLivre = Caixa.ComporCampoLivre("005507", "14222333777777777");
        Assert.Equal("0055077222133347777777771", campoLivre);
        Assert.Equal(25, campoLivre.Length);
    }

    // Caixa CNAB 400, NE051: conta 000000109990 → DV 6; NE052: agência 0161 +
    // conta 000000109990 → DV 5.
    [Fact]
    public void Caixa_DvsDeContaSeguemNotasDoCnab400()
    {
        Assert.Equal(6, Caixa.CalcularDvConta("000000109990"));
        Assert.Equal(5, Caixa.CalcularDvAgenciaConta("0161", "000000109990"));
    }

    // Sicredi, documentação oficial (portal do desenvolvedor, item 4.5 e
    // exemplo do campo livre 10.4): cooperativa 0165, posto 02, beneficiário
    // 00623, ano 07, byte 2, sequencial 00003 → DV 1 (NN "072000031").
    [Fact]
    public void Sicredi_DvNossoNumeroSegueExemploOficial()
    {
        Assert.Equal(1, Sicredi.CalcularDvNossoNumero("0165", "02", "00623", "07200003"));
        var composto = Sicredi.ComporNossoNumero("0165", "02", "00623", "07", 2, "00003");
        Assert.Equal("072000031", composto);
    }

    // Sicredi 10.4: campo livre "1"+"1"+"072000031"+"0165"+"02"+"00623"+"1"+"0"
    // → somatório 223, resto 3, DV 8.
    [Fact]
    public void Sicredi_CampoLivreSegueExemploDoItem104()
    {
        var campoLivre = Sicredi.ComporCampoLivre("072000031", "0165", "02", "00623", temValor: true);
        Assert.Equal("1107200003101650200623" + "10" + "8", campoLivre);
        Assert.Equal(25, campoLivre.Length);
    }

    // Sicoob, "Especificações do Boleto" (item 3.13): cooperativa 0001,
    // código do cliente 0000000019, NN 0000021 → somatório 36, resto 3,
    // DV 8 ("21-8").
    [Fact]
    public void Sicoob_DvNossoNumeroConst3197SegueExemplo()
    {
        Assert.Equal(8, Sicoob.CalcularDvNossoNumero("0001", "0000000019", "0000021"));
        Assert.Equal("0000021" + "8", Sicoob.ComporNossoNumero("0001", "0000000019", "0000021"));
    }

    // Sicoob, item 3.29: carteira + agência + modalidade + código do cliente +
    // NN com DV + parcela "001".
    [Fact]
    public void Sicoob_CampoLivreSegueEspecificacao()
    {
        var campoLivre = Sicoob.ComporCampoLivre("1", "0001", "01", "0000001", "00000218", "001");
        Assert.Equal("1000101000000100000218001", campoLivre);
        Assert.Equal(25, campoLivre.Length);
    }

    // BB, "Particularidades BB – Leiaute CNAB 240 (Junho/2019)": convênios de
    // 4/6 posições → 11 + DV; convênio de 7 → 17 sem DV; DV pode ser "X".
    [Fact]
    public void BancoDoBrasil_ComposicaoDoNossoNumeroPorConvenio()
    {
        Assert.Equal(12, BancoDoBrasil.ComporNossoNumeroConvenio4("1234", "1234567", "X").Length);
        Assert.Equal("12341234567X", BancoDoBrasil.ComporNossoNumeroConvenio4("1234", "1234567", "X"));
        Assert.Equal("123456123459", BancoDoBrasil.ComporNossoNumeroConvenio6("123456", "12345", "9"));
        Assert.Equal("12345671234567890", BancoDoBrasil.ComporNossoNumeroConvenio7("1234567", "1234567890"));
        Assert.Equal(12, BancoDoBrasil.TamanhoNossoNumeroComDv);
        Assert.Equal(17, BancoDoBrasil.TamanhoNossoNumeroSemDv);
    }

    [Fact]
    public void BancoDoBrasil_DvInvalidoERejeitado()
    {
        Assert.Throws<ArgumentException>(() => BancoDoBrasil.ComporNossoNumeroConvenio4("1234", "1234567", "x"));
        Assert.Throws<ArgumentException>(() => BancoDoBrasil.ComporNossoNumeroConvenio4("1234", "1234567", "12"));
    }

    // Inter: nosso número atribuído pelo banco com 11 dígitos.
    [Fact]
    public void Inter_ValidaNossoNumeroAtribuidoPeloBanco()
    {
        Inter.ValidarNossoNumero("00000001234");
        Assert.Throws<ArgumentException>(() => Inter.ValidarNossoNumero("1234"));
        Assert.Throws<ArgumentException>(() => Inter.ValidarNossoNumero("0000000123A"));
    }

    // BTG: nosso número atribuído pelo banco com 11 dígitos.
    [Fact]
    public void Btg_ValidaNossoNumeroAtribuidoPeloBanco()
    {
        Btg.ValidarNossoNumero("00000001234");
        Assert.Throws<ArgumentException>(() => Btg.ValidarNossoNumero("123"));
    }

    [Theory]
    [InlineData(1, "Banco do Brasil")]
    [InlineData(33, "Santander")]
    [InlineData(104, "Caixa Econômica Federal")]
    [InlineData(237, "Bradesco")]
    [InlineData(341, "Itaú")]
    [InlineData(748, "Sicredi")]
    [InlineData(756, "Sicoob")]
    [InlineData(77, "Inter")]
    [InlineData(208, "BTG Pactual")]
    public void CatalogoDeBancos_CorrigeOsCodigosDoSpec(int codigo, string nome)
    {
        var banco = Bancos.Obter(codigo);
        Assert.Equal(nome, banco.Nome);
        Assert.Equal(codigo, banco.Codigo);
        Assert.True(Bancos.TentarObter(codigo, out var encontrado));
        Assert.Equal(banco, encontrado);
    }

    [Fact]
    public void CatalogoDeBancos_CodigoNaoSuportadoERejeitado()
    {
        Assert.Throws<ArgumentException>(() => Bancos.Obter(999));
        Assert.False(Bancos.TentarObter(999, out _));
    }
}
