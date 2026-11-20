namespace Cnab.Tests;

using Cnab.Bancos;
using Cnab.Boleto;
using Cnab.Modelo;
using Xunit;

/// <summary>
/// Vetores oficiais de código de barras e linha digitável, por banco. Todos
/// os valores esperados vêm dos manuais citados ou das respostas de exemplo
/// das APIs oficiais — nenhum foi inventado.
/// </summary>
public class BoletoTests
{
    // Bradesco 4008-524-0121, "Montagem dos Dados para Composição da Linha
    // Digitável": linha digitável de exemplo completa
    // "23790.03102 40031.772003 28009.527905 7 10010000000000" para agência
    // 31, carteira 04, NN 00317720028, conta 95279, fator 1001
    // (vencimento 04/07/2000) e valor 0,00.
    [Fact]
    public void Bradesco_ComporLinhaDigitavelReproduzOExemploDoManual()
    {
        var campoLivre = Bradesco.ComporCampoLivre("31", "04", "00317720028", "95279");
        var codigo = CodigoDeBarras.Compor(Bancos.Bradesco, new DateOnly(2000, 7, 4), 0m, campoLivre);
        Assert.Equal("2379" + "7" + "1001" + "0000000000" + campoLivre, codigo);
        Assert.Equal("23790.03102 40031.772003 28009.527905 7 10010000000000", LinhaDigitavel.Compor(codigo));
    }

    [Fact]
    public void Bradesco_LinhaDigitavelParaBarrasEReversivel()
    {
        var linha = "23790.03102 40031.772003 28009.527905 7 10010000000000";
        var codigo = LinhaDigitavel.ParaCodigoDeBarras(linha);
        Assert.Equal("2379" + "7" + "1001" + "0000000000" + Bradesco.ComporCampoLivre("31", "04", "00317720028", "95279"), codigo);
        Assert.Equal(linha, LinhaDigitavel.Compor(codigo));
        Assert.True(LinhaDigitavel.ConferirDvs(linha));
    }

    // Itaú CNAB 400, Anexo 2: código de barras do exemplo
    // "34196166700000123451101234567880057123457000" (fator 1667 =
    // 01/05/2002, valor 123,45, carteira 110, NN 12345678-8, agência 0057,
    // conta 12345-7); Anexo 3: linha digitável completa
    // "34191.10121 34567.880058 71234.570001 6 16670000012345".
    [Fact]
    public void Itau_ComporCodigoDeBarrasELinhaReproduzemOsAnexos()
    {
        var campoLivre = Itau.ComporCampoLivre("110", "12345678", "0057", "12345");
        Assert.Equal("1101234567880057123457000", campoLivre);
        var codigo = CodigoDeBarras.Compor(Bancos.Itau, new DateOnly(2002, 5, 1), 123.45m, campoLivre);
        Assert.Equal("3419" + "6" + "1667" + "0000012345" + campoLivre, codigo);
        Assert.Equal("34191.10121 34567.880058 71234.570001 6 16670000012345", LinhaDigitavel.Compor(codigo));
    }

    // Santander v34: exemplo do manual (PSK 0282033, NN 566612457800-2,
    // carteira 101, fator 2046, valor 273,71) com DV geral 8; os campos da
    // linha digitável do manual: "033990282.7", "0335666124.3",
    // "5780020101.4" e campo 5 "20460000027371" — os 47 dígitos seguem a
    // composição FEBRABAN (campo 1 = banco + moeda + fixo 9 + PSK4 + DV).
    [Fact]
    public void Santander_ComporCodigoDeBarrasConfereComExemploDoManual()
    {
        var nossoNumero = Santander.ComporNossoNumero("566612457800");
        Assert.Equal("5666124578002", nossoNumero);
        var campoLivre = Santander.ComporCampoLivre(
            "0282033",
            nossoNumero,
            Santander.ModalidadeCarteira.CobrancaRapidaComRegistro);
        var codigo = CodigoDeBarras.Compor(Bancos.Santander, FatorDeVencimento.ParaVencimento(2046), 273.71m, campoLivre);
        Assert.Equal("0339" + "8" + "2046" + "0000027371" + campoLivre, codigo);

        var digitos = LinhaDigitavel.ComporDigitos(codigo);
        Assert.Equal("0339902827", digitos[..10]);
        Assert.Equal("03356661243", digitos.Substring(10, 11));
        Assert.Equal("57800201014", digitos.Substring(21, 11));
        Assert.Equal("8", digitos[32].ToString(System.Globalization.CultureInfo.InvariantCulture));
        Assert.Equal("20460000027371", digitos[33..]);
        Assert.True(LinhaDigitavel.ConferirDvs(LinhaDigitavel.Compor(codigo)));
    }

    // Caixa SIGCB: exemplo dos Anexos I/III/IV — beneficiário 005507 (DV 7),
    // NN 14/222333777777777 (sem DV no campo livre), fator 3242
    // (23/08/2006), valor 321,12, DV geral 4 e DV do campo livre 1;
    // campo 1 da linha digitável "104900550" com DV 5 (item 5.5.1).
    [Fact]
    public void Caixa_ComporCodigoDeBarrasConfereComAnexos()
    {
        var nossoNumero = "14" + "222333777777777"; // sem DV para o campo livre
        var campoLivre = Caixa.ComporCampoLivre("005507", nossoNumero);
        Assert.Equal("0055077222133347777777771", campoLivre);
        var codigo = CodigoDeBarras.Compor(Bancos.Caixa, new DateOnly(2006, 8, 23), 321.12m, campoLivre);
        Assert.Equal("1049" + "4" + "3242" + "0000032112" + campoLivre, codigo);

        var digitos = LinhaDigitavel.ComporDigitos(codigo);
        Assert.Equal("1049005505", digitos[..10]);
        Assert.Equal(47, digitos.Length);
        Assert.True(LinhaDigitavel.ConferirDvs(LinhaDigitavel.Compor(codigo)));
    }

    // Sicredi: exemplo real da API de Cobrança Sicredi (v3.9.1) —
    // codigoBarras "74891886400000099901125100614205120315335103",
    // linhaDigitavel "74891125110061420512803153351030188640000009990",
    // nossoNumero "251006142", cooperativa 0512, posto 03 (fator 8864 e
    // valor 99,90 implícitos no código de barras).
    [Fact]
    public void Sicredi_ComporCodigoDeBarrasReproduzAmostraOficialDaApi()
    {
        var campoLivre = Sicredi.ComporCampoLivre("251006142", "0512", "03", "15335", temValor: true);
        var fator = FatorDeVencimento.ParaVencimento(8864);
        Assert.Equal(new DateOnly(2022, 1, 13), fator);
        var codigo = CodigoDeBarras.Compor(Bancos.Sicredi, fator, 99.90m, campoLivre);
        Assert.Equal("7489" + "1" + "8864" + "0000009990" + campoLivre, codigo);
        Assert.Equal("74891125110061420512803153351030188640000009990", LinhaDigitavel.ComporDigitos(codigo));
        Assert.Equal("74891.12511 00614.205128 03153.351030 1 88640000009990", LinhaDigitavel.Compor(codigo));
    }

    [Fact]
    public void Sicredi_LinhaDaApiConfereTodosOsDvs()
    {
        var linha = "74891.12511 00614.205128 03153.351030 1 88640000009990";
        Assert.True(LinhaDigitavel.ConferirDvs(linha));
        var codigo = LinhaDigitavel.ParaCodigoDeBarras(linha);
        Assert.Equal("7489" + "1" + "8864" + "0000009990" + Sicredi.ComporCampoLivre("251006142", "0512", "03", "15335", temValor: true), codigo);
        Assert.True(CodigoDeBarras.ConferirDvGeral(codigo));
    }

    // Sicoob: composição do campo livre conforme "Especificações do Boleto"
    // (item 3.29) com consistência interna integral (DV geral e DVs de
    // bloco conferem sobre o código composto).
    [Fact]
    public void Sicoob_ComporCodigoDeBarrasEConsistente()
    {
        var campoLivre = Sicoob.ComporCampoLivre("1", "3071", "02", "0315559", "00000218", "001");
        var codigo = CodigoDeBarras.Compor(Bancos.Sicoob, new DateOnly(2026, 12, 21), 1250.00m, campoLivre);
        Assert.Equal(44, codigo.Length);
        Assert.Equal("756", codigo[..3]);
        Assert.Equal("9", codigo.Substring(3, 1));
        Assert.Equal("1667", codigo.Substring(5, 4));
        Assert.Equal("0000125000", codigo.Substring(9, 10));
        Assert.True(CodigoDeBarras.ConferirDvGeral(codigo));
        Assert.True(LinhaDigitavel.ConferirDvs(LinhaDigitavel.Compor(codigo)));

        // Formato "AAABC.DDDDE" do campo 1 da linha digitável Sicoob:
        // banco + moeda + carteira + agência (4) + DV.
        var linha = LinhaDigitavel.Compor(codigo);
        Assert.Equal("75691.3071", linha[..10]);
    }

    [Fact]
    public void CodigoDeBarras_LerCamposDeVolta()
    {
        const string codigo = "34196166700000123451101234567880057123457000";
        Assert.Equal(Bancos.Itau, CodigoDeBarras.LerBanco(codigo));
        Assert.Equal(new DateOnly(2002, 5, 1), CodigoDeBarras.LerVencimento(codigo));
        Assert.Equal(123.45m, CodigoDeBarras.LerValor(codigo));
        Assert.Equal("1101234567880057123457000", CodigoDeBarras.LerCampoLivre(codigo));
        Assert.True(CodigoDeBarras.ConferirDvGeral(codigo));
    }

    [Fact]
    public void CodigoDeBarras_RejeitaEntradasInvalidas()
    {
        Assert.Throws<ArgumentException>(() => CodigoDeBarras.Compor(
            Bancos.Bradesco,
            new DateOnly(2026, 1, 1),
            1m,
            "123"));
        Assert.Throws<ArgumentException>(() => CodigoDeBarras.Compor(
            Bancos.Bradesco,
            new DateOnly(2026, 1, 1),
            1.005m,
            new string('0', 25)));
        Assert.Throws<ArgumentException>(() => CodigoDeBarras.Compor(
            Bancos.Bradesco,
            new DateOnly(2026, 1, 1),
            -1m,
            new string('0', 25)));
        Assert.Throws<ArgumentException>(() => CodigoDeBarras.Compor(
            Bancos.Bradesco,
            new DateOnly(2026, 1, 1),
            100_000_000m,
            new string('0', 25)));
    }

    [Fact]
    public void CodigoDeBarras_SemVencimentoUsaFatorZero()
    {
        var codigo = CodigoDeBarras.Compor(Bancos.Bradesco, null, 10m, new string('0', 25));
        Assert.Equal("0000", codigo.Substring(5, 4));
        Assert.Null(CodigoDeBarras.LerVencimento(codigo));
    }

    [Fact]
    public void LinhaDigitavel_RejeitaTamanhoErrado()
    {
        Assert.Throws<ArgumentException>(() => LinhaDigitavel.Formatar("12345"));
        Assert.Throws<ArgumentException>(() => LinhaDigitavel.SomenteDigitos("23790.03102"));
    }

    [Fact]
    public void LinhaDigitavel_CompoeAPartirDeBarrasComDvErrado()
    {
        const string codigo = "34196166700000123451101234567880057123457000";
        var codigoCorrompido = codigo.TrocarCaractere(4, '9');
        Assert.Throws<InvalidOperationException>(() => LinhaDigitavel.ComporDigitos(codigoCorrompido));
    }
}

file static class ExtensaoDeTeste
{
    public static string TrocarCaractere(this string valor, int indice, char novo)
    {
        var caracteres = valor.ToCharArray();
        caracteres[indice] = novo;
        return new string(caracteres);
    }
}
