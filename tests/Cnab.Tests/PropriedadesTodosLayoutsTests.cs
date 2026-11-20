namespace Cnab.Tests;

using Cnab.Arquivo;
using Cnab.Cli;
using Cnab.Layouts.Cnab240;
using Cnab.Layouts.Cnab240.Registros;
using Cnab.Layouts.Cnab400;
using Cnab.Layouts.Cnab400.Registros;
using FsCheck;
using FsCheck.Fluent;
using Xunit;

/// <summary>
/// Propriedades (FsCheck) aplicáveis a todos os layouts implementados:
/// comprimento exato das linhas, <c>parse(write(x)) == x</c> nos retornos e
/// totais de trailer derivados dos modelos. Nenhum teste é pulado e todas as
/// propriedades usam geradores de dados válidos (bancos e carteiras reais).
/// </summary>
public class PropriedadesTodosLayoutsTests
{
    [Fact]
    public void Remessa240_TodaLinhaTemExatamente240Posicoes()
    {
        Prop.ForAll(ArbitrariosLayouts.Remessas240(), remessa =>
        {
            var linhas = GeradorRemessa240.GerarLinhas(remessa);
            return (!linhas.Any(string.IsNullOrEmpty) && linhas.All(l => l.Length == Codigos240.TamanhoRegistro))
                .Label($"banco {remessa.Banco.Codigo:000}, {linhas.Count} linhas");
        }).QuickCheckThrowOnFailure();
    }

    [Fact]
    public void Remessa240_Trailers_RefletemQuantidadesESomas()
    {
        Prop.ForAll(ArbitrariosLayouts.Remessas240(), remessa =>
        {
            var linhas = GeradorRemessa240.GerarLinhas(remessa);
            var trailerArquivo = RegistroCnab<TrailerArquivo240>.Ler(linhas[^1], linhas.Count);
            var corretoArquivo = trailerArquivo.QuantidadeRegistros == linhas.Count
                && trailerArquivo.QuantidadeLotes == remessa.LotesCobranca.Count;

            var trailersLote = linhas.Where(l => l.Length == 240 && l[7] == '5').ToArray();
            var corretosLotes = trailersLote.Length == remessa.LotesCobranca.Count;
            for (var i = 0; i < trailersLote.Length && corretosLotes; i++)
            {
                var trailer = RegistroCnab<TrailerLoteCobranca240>.Ler(trailersLote[i]);
                var lote = remessa.LotesCobranca[i];
                corretosLotes = trailer.QuantidadeSimples == lote.Titulos.Count
                    && trailer.ValorSimples == lote.Titulos.Sum(t => t.Valor);
            }

            return (corretoArquivo && corretosLotes)
                .Label($"banco {remessa.Banco.Codigo:000}; arquivo={corretoArquivo}, lotes={corretosLotes}");
        }).QuickCheckThrowOnFailure();
    }

    [Fact]
    public void Remessa240Pagamento_TrailerDoLote_SomaTodosOsValores()
    {
        Prop.ForAll(ArbitrariosLayouts.Remessas240Pagamento(), remessa =>
        {
            var linhas = GeradorRemessa240.GerarLinhas(remessa);
            var trailer = RegistroCnab<TrailerLotePagamento240>.Ler(linhas[^2], linhas.Count - 1);
            var esperado = remessa.LotesPagamento[0].Pagamentos.Sum(p => p.Valor);
            return (trailer.SomatoriaValores == esperado && trailer.QuantidadeRegistros > 2)
                .Label($"banco {remessa.Banco.Codigo:000}: {trailer.SomatoriaValores} ≠ {esperado}");
        }).QuickCheckThrowOnFailure();
    }

    [Fact]
    public void Remessa400_TodaLinhaTemExatamente400PosicoesETrailerBate()
    {
        Prop.ForAll(ArbitrariosLayouts.Remessas400(), remessa =>
        {
            var linhas = GeradorRemessa400.GerarLinhas(remessa);
            var corretoTamanho = linhas.All(l => l.Length == Codigos400.TamanhoRegistro);
            var ultima = linhas[^1];
            var corretoTrailer = remessa.Banco.Codigo switch
            {
                341 => RegistroCnab<TrailerRemessaItau400>.Ler(ultima).NumeroSequencial == linhas.Count,
                237 => RegistroCnab<TrailerRemessaBradesco400>.Ler(ultima).NumeroSequencial == linhas.Count,
                748 => RegistroCnab<TrailerRemessaSicredi400>.Ler(ultima).NumeroSequencial == linhas.Count,
                77 => RegistroCnab<TrailerRemessaInter400>.Ler(ultima).NumeroSequencial == linhas.Count
                    && RegistroCnab<TrailerRemessaInter400>.Ler(ultima).QuantidadeBoletos == remessa.Titulos.Count,
                _ => false,
            };

            return (corretoTamanho && corretoTrailer)
                .Label($"banco {remessa.Banco.Codigo:000}: tamanho={corretoTamanho}, trailer={corretoTrailer}");
        }).QuickCheckThrowOnFailure();
    }

    [Fact]
    public void Retorno240_ParseDeWrite_EhIdenticoEMantemDados()
    {
        Prop.ForAll(ArbitrariosLayouts.Retornos240(), dados =>
        {
            var conteudo = RetornoFixtures.Cnab240(dados.CodigoBanco, dados.QuantidadeItens, dados.Valor);
            var linhas = ArquivoCnab.SepararLinhas(conteudo);
            var retorno = LeitorRetorno240.Ler(conteudo);
            var reescrito = LeitorRetorno240.EscreverLinhas(retorno);
            var itens = retorno.Lotes.Count == 1 ? retorno.Lotes[0].Itens : [];

            return (linhas.SequenceEqual(reescrito)
                && retorno.Banco.Codigo == dados.CodigoBanco
                && retorno.Lotes.Count == 1
                && itens.Count == dados.QuantidadeItens
                && itens.All(i => i.Ocorrencia?.Codigo == "06" && i.Valores?.ValorPago == dados.Valor))
                .Label($"banco {dados.CodigoBanco:000}, itens {itens.Count}/{dados.QuantidadeItens}");
        }).QuickCheckThrowOnFailure();
    }

    [Fact]
    public void Retorno400_ParseDeWrite_EhIdenticoEMantemDados()
    {
        Prop.ForAll(ArbitrariosLayouts.Retornos400(), dados =>
        {
            var conteudo = RetornoFixtures.Cnab400(dados.CodigoBanco, dados.QuantidadeItens, dados.Centavos);
            var linhas = ArquivoCnab.SepararLinhas(conteudo);
            var retorno = LeitorRetorno400.Ler(conteudo);
            var reescrito = LeitorRetorno400.EscreverLinhas(retorno);
            var esperado = dados.Centavos / 100m;

            return (linhas.All(l => l.Length == Codigos400.TamanhoRegistro)
                && linhas.SequenceEqual(reescrito)
                && retorno.Banco.Codigo == dados.CodigoBanco
                && retorno.Itens.Count == dados.QuantidadeItens
                && retorno.Itens.All(i => i.Valores?.ValorPago == esperado))
                .Label($"banco {dados.CodigoBanco:000}, itens {retorno.Itens.Count}/{dados.QuantidadeItens}");
        }).QuickCheckThrowOnFailure();
    }
}
