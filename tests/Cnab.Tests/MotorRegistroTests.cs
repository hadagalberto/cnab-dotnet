namespace Cnab.Tests;

using Cnab.Arquivo;
using Xunit;

public class MotorRegistroTests
{
    private static RegistroExemplo CriarRegistroExemplo() => new()
    {
        Banco = 237,
        Lote = 0,
        TipoRegistro = 0,
        Inscricao = 0,
        NomeEmpresa = "EMPRESA MODELO LTDA",
        DataGeracao = new DateOnly(2026, 11, 20),
        HoraGeracao = 101530,
        SequencialArquivo = 1,
        FillerLayout = string.Empty,
        Densidade = 1600,
        ValorTitulo = 1500.50m,
        VencimentoCurto = new DateOnly(2026, 12, 20),
        DescontoFlag = null,
    };

    [Fact]
    public void Escrever_ProduzLinhaComTamanhoExato()
    {
        var linha = RegistroCnab<RegistroExemplo>.Escrever(CriarRegistroExemplo());
        Assert.Equal(240, linha.Length);
    }

    [Fact]
    public void Escrever_PreencheCamposNasPosicoesCorretas()
    {
        var linha = RegistroCnab<RegistroExemplo>.Escrever(CriarRegistroExemplo());
        Assert.Equal("237", linha[..3]);
        Assert.Equal("0000", linha.Substring(3, 4));
        Assert.Equal("0", linha.Substring(7, 1));
        Assert.Equal(new string(' ', 9), linha.Substring(8, 9)); // lacuna 9-17
        Assert.Equal("EMPRESA MODELO LTDA", linha.Substring(72, 19));
        Assert.Equal(new string(' ', 11), linha.Substring(91, 11)); // resto do alfa
        Assert.Equal("20112026", linha.Substring(143, 8));
        Assert.Equal("101530", linha.Substring(151, 6));
        Assert.Equal("000001", linha.Substring(157, 6));
        Assert.Equal("081", linha.Substring(163, 3)); // filler declarado
        Assert.Equal("01600", linha.Substring(166, 5));
        Assert.Equal("0000000150050", linha.Substring(220, 13));
        Assert.Equal("0", linha.Substring(239, 1));
    }

    [Fact]
    public void EscreverAlfanumerico_NormalizaCharsetETruncaErro()
    {
        var registro = CriarRegistroExemplo() with
        {
            NomeEmpresa = "Açaí & Cia — Brasil",
        };
        var linha = RegistroCnab<RegistroExemplo>.Escrever(registro);
        Assert.Equal("ACAI   CIA - BRASIL", linha.Substring(72, 19));

        var erro = Assert.Throws<CampoCnabException>(
            () => RegistroCnab<RegistroNomeGrande>.Escrever(new RegistroNomeGrande()));
        Assert.Equal("Nome da Empresa", erro.Campo);
        Assert.Equal(73, erro.Coluna);
        Assert.Equal(30, erro.Tamanho);
    }

    [Fact]
    public void EscreverNumerico_RecusaValorMaiorQueCampo()
    {
        var registro = CriarRegistroExemplo() with { Banco = 1234 };
        var erro = Assert.Throws<CampoCnabException>(() => RegistroCnab<RegistroExemplo>.Escrever(registro));
        Assert.Equal("Código do Banco", erro.Campo);
        Assert.Equal(1, erro.Coluna);
        Assert.Equal(3, erro.Tamanho);
    }

    [Fact]
    public void EscreverNumerico_RecusaValorNegativo()
    {
        var registro = CriarRegistroExemplo() with { Densidade = -1 };
        var erro = Assert.Throws<CampoCnabException>(() => RegistroCnab<RegistroExemplo>.Escrever(registro));
        Assert.Equal("Densidade", erro.Campo);
    }

    [Fact]
    public void EscreverDecimal_RecusaPrecisaoAcimaDoCampo()
    {
        var registro = CriarRegistroExemplo() with { ValorTitulo = 10.123m };
        var erro = Assert.Throws<CampoCnabException>(() => RegistroCnab<RegistroExemplo>.Escrever(registro));
        Assert.Equal("Valor do Título", erro.Campo);
        Assert.Contains("decimal", erro.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void EscreverDecimal_RecusaValorQueNaoCabe()
    {
        var registro = CriarRegistroExemplo() with { ValorTitulo = 99_999_999_999_999.99m };
        var erro = Assert.Throws<CampoCnabException>(() => RegistroCnab<RegistroExemplo>.Escrever(registro));
        Assert.Equal("Valor do Título", erro.Campo);
    }

    [Fact]
    public void LerERecreer_RetornamRegistroIdentico()
    {
        var original = CriarRegistroExemplo();
        var linha = RegistroCnab<RegistroExemplo>.Escrever(original);
        var lido = RegistroCnab<RegistroExemplo>.Ler(linha, numeroLinha: 1);
        Assert.Equal(original, lido);
    }

    [Fact]
    public void Ler_RejeitaLinhaCurta()
    {
        var erro = Assert.Throws<TamanhoRegistroException>(
            () => RegistroCnab<RegistroExemplo>.Ler(new string(' ', 239), numeroLinha: 7));
        Assert.Equal(240, erro.Esperado);
        Assert.Equal(239, erro.Encontrado);
        Assert.Equal(7, erro.Linha);
        Assert.Contains("linha 7", erro.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Ler_RejeitaLinhaLonga()
    {
        Assert.Throws<TamanhoRegistroException>(
            () => RegistroCnab<RegistroExemplo>.Ler(new string(' ', 241)));
    }

    [Fact]
    public void Ler_CaractereNaoNumericoGeraErroComCampoLinhaEColuna()
    {
        var linha = RegistroCnab<RegistroExemplo>.Escrever(CriarRegistroExemplo());
        var linhaComErro = string.Concat(linha.AsSpan(0, 143), "X", linha.AsSpan(144));
        var erro = Assert.Throws<CampoCnabException>(
            () => RegistroCnab<RegistroExemplo>.Ler(linhaComErro, numeroLinha: 12));
        Assert.Equal("Data de Geração", erro.Campo);
        Assert.Equal(12, erro.Linha);
        Assert.Equal(144, erro.Coluna);
        // O erro aponta o fatiamento lido (2 dígitos do dia), com o campo e a linha.
        Assert.Contains("linha 12, colunas 144-145, campo 'Data de Geração'", erro.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Ler_DataInvalidaGeraErroPreciso()
    {
        var linha = RegistroCnab<RegistroExemplo>.Escrever(CriarRegistroExemplo());
        // 32/12/26: data inexistente no campo DDMMAAAA (posição 144).
        var linhaComErro = string.Concat(linha.AsSpan(0, 143), "32122026", linha.AsSpan(151));
        var erro = Assert.Throws<CampoCnabException>(
            () => RegistroCnab<RegistroExemplo>.Ler(linhaComErro));
        Assert.Equal("Data de Geração", erro.Campo);
        Assert.Contains("inválida", erro.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Ler_CampoZeradoEMarcadoComoNulo_LeComoNulo()
    {
        var registro = CriarRegistroExemplo() with { ValorTitulo = null, VencimentoCurto = null };
        var linha = RegistroCnab<RegistroExemplo>.Escrever(registro);
        Assert.Equal(new string('0', 13), linha.Substring(220, 13));
        var lido = RegistroCnab<RegistroExemplo>.Ler(linha);
        Assert.Null(lido.ValorTitulo);
        Assert.Null(lido.VencimentoCurto);
    }

    [Fact]
    public void Ler_DataDDMMAA_AssumeSeculo2000()
    {
        var registro = CriarRegistroExemplo() with { VencimentoCurto = new DateOnly(2026, 12, 20) };
        var linha = RegistroCnab<RegistroExemplo>.Escrever(registro);
        Assert.Equal("201226", linha.Substring(179, 6));
        var lido = RegistroCnab<RegistroExemplo>.Ler(linha);
        Assert.Equal(new DateOnly(2026, 12, 20), lido.VencimentoCurto);
    }

    [Fact]
    public void EscreverArquivo_UsaCrLfPorPadrao()
    {
        var r1 = RegistroCnab<RegistroExemplo>.Escrever(CriarRegistroExemplo());
        var r2 = RegistroCnab<RegistroExemplo>.Escrever(CriarRegistroExemplo() with { SequencialArquivo = 2 });
        var arquivo = EscritorDeArquivo.Montar([r1, r2]);
        Assert.Equal(r1 + "\r\n" + r2 + "\r\n", arquivo);
    }

    [Fact]
    public void EscreverArquivo_AceitaOpcaoPorLf()
    {
        var r1 = RegistroCnab<RegistroExemplo>.Escrever(CriarRegistroExemplo());
        var r2 = RegistroCnab<RegistroExemplo>.Escrever(CriarRegistroExemplo() with { SequencialArquivo = 2 });
        var arquivo = EscritorDeArquivo.Montar([r1, r2], new OpcoesArquivo { Fim = OpcoesArquivo.FimDeLinha.Lf });
        Assert.Equal(r1 + "\n" + r2 + "\n", arquivo);
    }

    [Fact]
    public void EscreverArquivo_SemDelimitadorFinal()
    {
        var r1 = RegistroCnab<RegistroExemplo>.Escrever(CriarRegistroExemplo());
        var r2 = RegistroCnab<RegistroExemplo>.Escrever(CriarRegistroExemplo() with { SequencialArquivo = 2 });
        var arquivo = EscritorDeArquivo.Montar(
            [r1, r2],
            new OpcoesArquivo { DelimitadorFinal = false });
        Assert.Equal(r1 + "\r\n" + r2, arquivo);
    }

    [Fact]
    public void Layout_CamposSobrepostosSaoRejeitados()
    {
        var erro = Assert.Throws<TypeInitializationException>(
            () => LayoutBuilderInvalido.Acessar());
        Assert.IsType<InvalidOperationException>(erro.InnerException);
    }

    [Fact]
    public void Layout_TipoSemAtributoERejeitado()
    {
        var erro = Assert.Throws<TypeInitializationException>(() => SemAtributo.Acessar());
        Assert.IsType<InvalidOperationException>(erro.InnerException);
    }

    // Tipos auxiliares para validar as guarda-limite do motor.

    private static class LayoutBuilderInvalido
    {
        public static string Acessar() => RegistroCnab<RegistroSobreposto>.Escrever(new RegistroSobreposto());
    }

    private static class SemAtributo
    {
        public static string Acessar() => RegistroCnab<SemAtributoRegistro>.Escrever(new SemAtributoRegistro());
    }

    [RegistroCnab(240)]
    private sealed record RegistroSobreposto
    {
        [CampoCnab("A", 1, 10, TipoCampo.Numerico)]
        public int A { get; init; }

        [CampoCnab("B", 5, 10, TipoCampo.Numerico)]
        public int B { get; init; }
    }

    private sealed record SemAtributoRegistro
    {
        [CampoCnab("A", 1, 10, TipoCampo.Numerico)]
        public int A { get; init; }
    }

    [RegistroCnab(240)]
    private sealed record RegistroNomeGrande
    {
        [CampoCnab("Nome da Empresa", 73, 30, TipoCampo.Alfanumerico)]
        public string NomeEmpresa { get; init; } = new string('A', 31);
    }
}
