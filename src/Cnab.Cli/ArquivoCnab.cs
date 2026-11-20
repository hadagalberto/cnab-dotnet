namespace Cnab.Cli;

using System.Collections.Concurrent;
using System.Globalization;
using Cnab.Arquivo;
using Cnab.Layouts.Cnab240.Registros;
using Cnab.Layouts.Cnab400.Registros;
using Cnab.Modelo;

/// <summary>Layout detectado pelo comprimento dos registros.</summary>
public enum LayoutArquivo
{
    /// <summary>240 posições por registro (FEBRABAN V10.9).</summary>
    Cnab240 = 240,

    /// <summary>400 posições por registro (manuais de cobrança dos bancos).</summary>
    Cnab400 = 400,
}

/// <summary>Leitura e catalogação de arquivos CNAB para as quatro operações da CLI.</summary>
public static class ArquivoCnab
{
    private static readonly ConcurrentDictionary<Type, Func<string, int?, object>> Leitores = new();

    /// <summary>Separa registros por CRLF ou LF, ignorando o delimitador final.</summary>
    /// <param name="conteudo">Conteúdo textual do arquivo.</param>
    /// <returns>Linhas sem delimitadores.</returns>
    public static IReadOnlyList<string> SepararLinhas(string conteudo) => LinhasCnab.Separar(conteudo);

    /// <summary>Detecta o layout pelo comprimento da primeira linha.</summary>
    /// <param name="linhas">Linhas do arquivo.</param>
    /// <returns>Layout detectado.</returns>
    /// <exception cref="CliException">Arquivo vazio ou comprimento não suportado.</exception>
    public static LayoutArquivo DetectarLayout(IReadOnlyList<string> linhas)
    {
        if (linhas.Count == 0)
        {
            throw new CliException("Arquivo vazio: esperado ao menos um registro.");
        }

        return linhas[0].Length switch
        {
            240 => LayoutArquivo.Cnab240,
            400 => LayoutArquivo.Cnab400,
            var tamanho => throw new CliException(
                $"Comprimento de registro {tamanho} na linha 1 não corresponde a CNAB 240 "
                + "(esperado 240) nem CNAB 400 (esperado 400)."),
        };
    }

    /// <summary>
    /// Detecta o banco pelo header. CNAB 240 usa as posições 1-3 (G001);
    /// CNAB 400 usa as posições 77-79 dos headers implementados (ver
    /// <c>docs/layouts.md</c> e <c>LeitorRetorno400</c>).
    /// </summary>
    /// <param name="linha">Primeira linha do arquivo.</param>
    /// <param name="layout">Layout do arquivo.</param>
    /// <returns>Banco detectado ou <see langword="null"/> quando não identificável.</returns>
    public static Banco? DetectarBanco(string linha, LayoutArquivo layout)
    {
        var inicio = layout == LayoutArquivo.Cnab240 ? 0 : 76;
        if (linha.Length < inicio + 3
            || !int.TryParse(linha.Substring(inicio, 3), NumberStyles.None, CultureInfo.InvariantCulture, out var codigo))
        {
            return null;
        }

        return Bancos.TentarObter(codigo, out var banco) ? banco : null;
    }

    /// <summary>
    /// Resolve o tipo de registro de uma linha, incluindo as variantes
    /// proprietárias de cada banco. Os códigos de tipo e segmento vêm das
    /// tabelas G003/G039 do padrão FEBRABAN (CNAB 240) e dos manuais de
    /// cobrança (CNAB 400), citados em <c>docs/layouts.md</c>.
    /// </summary>
    /// <param name="layout">Layout do arquivo.</param>
    /// <param name="linha">Linha a classificar.</param>
    /// <param name="banco">Banco, quando conhecido.</param>
    /// <param name="ehRemessa">
    /// Sentido do arquivo, lido do header. Quando omitido, é inferido da
    /// própria linha (útil apenas para o header).
    /// </param>
    /// <returns>Tipo do registro, ou <see langword="null"/> quando não há registro declarado.</returns>
    public static Type? ObterTipoRegistro(LayoutArquivo layout, string linha, Banco? banco, bool? ehRemessa = null)
    {
        // CNAB 400 usa a posição 1 (G003 do padrão de cobrança); CNAB 240 usa
        // a posição 8 (G003 FEBRABAN V10.9).
        var indiceTipo = layout == LayoutArquivo.Cnab240 ? 7 : 0;
        if (linha.Length <= indiceTipo
            || !int.TryParse(linha.Substring(indiceTipo, 1), NumberStyles.None, CultureInfo.InvariantCulture, out var tipo))
        {
            return null;
        }

        if (layout == LayoutArquivo.Cnab240)
        {
            return Tipo240(tipo, linha, banco);
        }

        return Tipo400(tipo, banco, ehRemessa ?? EhRemessa(linha));
    }

    /// <summary>
    /// Identifica remessa/retorno pelo código do header. No CNAB 400 o código
    /// fica na posição 2 ("1" remessa, "2" retorno); no CNAB 240, na posição
    /// 143 (G015).
    /// </summary>
    /// <param name="primeiraLinha">Header do arquivo.</param>
    /// <param name="layout">Layout do arquivo.</param>
    /// <returns><see langword="true"/> para remessa.</returns>
    public static bool EhRemessa(string primeiraLinha, LayoutArquivo layout) => layout == LayoutArquivo.Cnab400
        ? primeiraLinha.Length > 1 && primeiraLinha[1] == '1'
        : primeiraLinha.Length > 142 && primeiraLinha[142] == '1';

    private static bool EhRemessa(string linha) => linha.Length > 1 && linha[1] == '1';

    /// <summary>Metadados declarados de um registro (nome, posição, tamanho, tipo).</summary>
    /// <param name="tipo">Tipo anotado com <see cref="RegistroCnabAttribute"/>.</param>
    /// <returns>Campos ordenados por posição.</returns>
    public static IReadOnlyList<CampoCnabAttribute> Campos(Type tipo) => MetadadosRegistro.ObterCampos(tipo);

    /// <summary>Lê uma linha com o leitor compilado do tipo de registro.</summary>
    /// <param name="tipo">Tipo do registro.</param>
    /// <param name="linha">Linha sem delimitadores.</param>
    /// <param name="numeroLinha">Número da linha para mensagens de erro.</param>
    /// <returns>Instância preenchida.</returns>
    public static object LerRegistro(Type tipo, string linha, int numeroLinha) =>
        Leitores.GetOrAdd(tipo, CriarLeitor)(linha, numeroLinha);

    /// <summary>Número de posições do registro, lido do atributo declarativo.</summary>
    /// <param name="tipo">Tipo do registro.</param>
    /// <returns>Quantidade de posições.</returns>
    public static int Tamanho(Type tipo) => MetadadosRegistro.ObterTamanho(tipo);

    private static Type? Tipo240(int tipo, string linha, Banco? banco)
    {
        var segmento = linha.Length > 13 ? linha[13] : ' ';
        return (tipo, segmento) switch
        {
            (0, _) => banco?.Codigo == 33 ? typeof(HeaderArquivoSantander240) : typeof(HeaderArquivo240),
            (1, _) => Operacao(linha) == 'C'
                ? typeof(HeaderLotePagamento240)
                : banco?.Codigo == 33
                    ? typeof(HeaderLoteCobrancaSantander240)
                    : typeof(HeaderLoteCobranca240),
            (3, 'P') => banco?.Codigo == 33 ? typeof(SegmentoPSantander240) : typeof(SegmentoP240),
            (3, 'Q') => typeof(SegmentoQ240),
            (3, 'R') => typeof(SegmentoR240),
            (3, 'T') => typeof(SegmentoT240),
            (3, 'U') => typeof(SegmentoU240),
            (3, 'A') => typeof(SegmentoA240),
            (3, 'B') => typeof(SegmentoB240),
            (3, 'J') => typeof(SegmentoJ240),
            (3, 'O') => typeof(SegmentoO240),
            (5, _) => Operacao(linha) == 'C' ? typeof(TrailerLotePagamento240) : typeof(TrailerLoteCobranca240),
            (9, _) => typeof(TrailerArquivo240),
            _ => null,
        };
    }

    private static char Operacao(string linha)
    {
        var operacao = linha.Length > 8 ? linha[8] : ' ';
        // Nos headers/trailers de lote, a operação fica na posição 9 (índice 8)
        // conforme G028. Em registros de configuração nunca é usada.
        return operacao;
    }

    private static Type? Tipo400(int tipo, Banco? banco, bool remessa)
    {
        var codigo = banco?.Codigo;
        return (codigo, tipo, remessa) switch
        {
            (341, 0, true) => typeof(HeaderRemessaItau400),
            (341, 1, true) => typeof(DetalheRemessaItau400),
            (341, 2, true) => typeof(RegistroMultaRemessaItau400),
            (341, 9, true) => typeof(TrailerRemessaItau400),
            (341, 0, false) => typeof(HeaderRetornoItau400),
            (341, 1, false) => typeof(DetalheRetornoItau400),
            (341, 9, false) => typeof(TrailerRetornoItau400),
            (237, 0, true) => typeof(HeaderRemessaBradesco400),
            (237, 1, true) => typeof(DetalheRemessaBradesco400),
            (237, 2, true) => typeof(MensagemRemessaBradesco400),
            (237, 9, true) => typeof(TrailerRemessaBradesco400),
            (237, 0, false) => typeof(HeaderRetornoBradesco400),
            (237, 1, false) => typeof(DetalheRetornoBradesco400),
            (237, 9, false) => typeof(TrailerRetornoBradesco400),
            (748, 0, true) => typeof(HeaderRemessaSicredi400),
            (748, 1, true) => typeof(DetalheRemessaSicredi400),
            (748, 9, true) => typeof(TrailerRemessaSicredi400),
            (748, 0, false) => typeof(HeaderRetornoSicredi400),
            (748, 1, false) => typeof(DetalheRetornoSicredi400),
            (748, 9, false) => typeof(TrailerRetornoSicredi400),
            (77, 0, true) => typeof(HeaderRemessaInter400),
            (77, 1, true) => typeof(DetalheRemessaInter400),
            (77, 2, true) => typeof(RegistroEmailRemessaInter400),
            (77, 9, true) => typeof(TrailerRemessaInter400),
            (77, 0, false) => typeof(HeaderRetornoInter400),
            (77, 1, false) => typeof(DetalheRetornoInter400),
            (77, 9, false) => typeof(TrailerRetornoInter400),
            _ => null,
        };
    }

    private static Func<string, int?, object> CriarLeitor(Type tipo)
    {
        var metodo = typeof(RegistroCnab<>).MakeGenericType(tipo)
            .GetMethod(nameof(RegistroCnab<object>.Ler), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)!;
        return (Func<string, int?, object>)metodo.CreateDelegate(typeof(Func<string, int?, object>));
    }
}

/// <summary>Erro de uso ou de dados reportado pela CLI.</summary>
public sealed class CliException : Exception
{
    /// <summary>Inicializa a exceção.</summary>
    /// <param name="mensagem">Mensagem amigável.</param>
    public CliException(string mensagem)
        : base(mensagem)
    {
    }
}
