namespace Cnab.Tests;

using Cnab.Arquivo;
using Cnab.Texto;
using FsCheck;
using FsCheck.Fluent;
using Xunit;

/// <summary>
/// Testes de propriedade (FsCheck) do motor de registros: todo registro válido
/// escreve linha do tamanho exato do layout, parse(write(x)) == x, e o
/// conteúdo alfanumérico sempre pertence ao charset CNAB.
/// </summary>
public class MotorRegistroPropriedadeTests
{
    [Fact]
    public void TodoRegistroEscreveLinhaDoTamanhoExato()
    {
        Prop.ForAll(Arbitrarios.Registros(), registro =>
        {
            var linha = RegistroCnab<RegistroExemplo>.Escrever(registro);
            return linha.Length == 240;
        }).QuickCheckThrowOnFailure();
    }

    [Fact]
    public void EscreverELer_RetornaRegistroIdentico()
    {
        Prop.ForAll(Arbitrarios.Registros(), registro =>
        {
            var linha = RegistroCnab<RegistroExemplo>.Escrever(registro);
            var lido = RegistroCnab<RegistroExemplo>.Ler(linha, numeroLinha: 1);
            return lido.Equals(registro).Label($"esperado {registro}, lido {lido}");
        }).QuickCheckThrowOnFailure();
    }

    [Fact]
    public void ConteudoAlfanumericoSemprePertenceAoCharsetCnab()
    {
        Prop.ForAll(Arbitrarios.TextosArbitrarios(), texto =>
        {
            var normalizado = CharsetCnab.Normalizar(texto);
            return CharsetCnab.EstaNoCharset(normalizado).Label($"'{normalizado}' fora do charset");
        }).QuickCheckThrowOnFailure();
    }

    [Fact]
    public void NormalizarEIdempotenteParaQualquerTexto()
    {
        Prop.ForAll(Arbitrarios.TextosArbitrarios(), texto =>
        {
            var primeira = CharsetCnab.Normalizar(texto);
            return CharsetCnab.Normalizar(primeira) == primeira;
        }).QuickCheckThrowOnFailure();
    }

    [Fact]
    public void TodoArquivoMontadoTemLinhasDoTamanhoExato()
    {
        Prop.ForAll(
            Arbitrarios.Registros(),
            Arbitrarios.Quantidades(),
            (registro, quantidade) =>
            {
                var linhas = Enumerable.Range(1, quantidade)
                    .Select(i => RegistroCnab<RegistroExemplo>.Escrever(registro with { SequencialArquivo = i }))
                    .ToArray();
                var arquivo = EscritorDeArquivo.Montar(linhas);
                var semDelimitadores = arquivo.Replace("\r\n", string.Empty, StringComparison.Ordinal);
                return semDelimitadores.Length == quantidade * 240
                    && arquivo.EndsWith("\r\n", StringComparison.Ordinal);
            }).QuickCheckThrowOnFailure();
    }
}

/// <summary>Geradores FsCheck para os tipos de teste.</summary>
public static class Arbitrarios
{
    private static readonly char[] CharsetPermitido =
        " !*-()[]{},.;:/%+=0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

    private static readonly DateTime BaseDeDatas = new(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static Arbitrary<RegistroExemplo> Registros() =>
        (from banco in Gen.Choose(1, 999)
         from lote in Gen.Choose(0, 9999)
         from inscricaoAlta in Gen.Choose(0, 999_999)
         from inscricaoBaixa in Gen.Choose(0, 999_999)
         from nome in TextoSemEspacoFinal()
         from diaGeracao in Gen.Choose(0, 36_000)
         from hora in Gen.Choose(0, 235_959)
         from sequencial in Gen.Choose(1, 999_999)
         from densidade in Gen.Choose(0, 99_999)
         from reais in Gen.Choose(0, 99_999_999)
         from centavos in Gen.Choose(0, 99)
         from temValor in Gen.Elements(true, false)
         from diaVencimento in Gen.Choose(0, 36_000)
         from temVencimento in Gen.Elements(true, false)
         from temDesconto in Gen.Elements(true, false)
         from desconto in Gen.Choose(1, 9)
         select new RegistroExemplo
         {
             Banco = banco,
             Lote = lote,
             TipoRegistro = 0,
             Inscricao = (inscricaoAlta * 1_000_000L) + inscricaoBaixa,
             NomeEmpresa = nome,
             DataGeracao = DateOnly.FromDateTime(BaseDeDatas.AddDays(diaGeracao)),
             HoraGeracao = hora,
             SequencialArquivo = sequencial,
             FillerLayout = string.Empty,
             Densidade = densidade,
             ValorTitulo = temValor ? ((reais * 100L) + centavos) / 100m : null,
             VencimentoCurto = temVencimento ? DateOnly.FromDateTime(BaseDeDatas.AddDays(diaVencimento)) : null,
             DescontoFlag = temDesconto ? desconto : null,
         }).ToArbitrary();

    public static Arbitrary<int> Quantidades() => Gen.Choose(1, 20).ToArbitrary();

    public static Arbitrary<string> TextosArbitrarios() =>
        (from tamanho in Gen.Choose(0, 40)
         from caracteres in Gen.ListOf(CaractereUnicode())
         select new string([.. caracteres.Take(tamanho)])).ToArbitrary();

    private static Gen<string> TextoSemEspacoFinal() =>
        from tamanho in Gen.Choose(0, 30)
        from chars in Gen.ListOf(Gen.Elements(CharsetPermitido))
        select new string([.. chars.Take(tamanho)]).TrimEnd(' ');

    private static Gen<char> CaractereUnicode()
    {
        Gen<char> comum = Gen.Choose(32, 0xD7FF).Select(CodigoParaChar);
        Gen<char> raro = Gen.Choose(0xE000, 0xFFFD).Select(CodigoParaChar);
        return Gen.Frequency((9, comum), (1, raro));
    }

    private static char CodigoParaChar(int codigo) => (char)codigo;
}
