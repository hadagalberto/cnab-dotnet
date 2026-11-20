namespace Cnab.Tests;

using System.Text;
using Cnab.Arquivo;
using Cnab.Layouts.Cnab240;
using Cnab.Layouts.Cnab400;
using Cnab.Modelo;
using Xunit;

/// <summary>
/// SPEC: "retorno samples covering every occurrence code family". Cada família
/// do enum é exercitada por uma amostra real de retorno CNAB 240 cujo código
/// C044 é extraído da tabela FEBRABAN V10.9 (citada em docs/layouts.md).
/// </summary>
public sealed class OcorrenciasFamiliaTests
{
    public static TheoryData<FamiliaOcorrencia, string> FamiliasECodigos => new()
    {
        { FamiliaOcorrencia.EntradaConfirmada, "02" },
        { FamiliaOcorrencia.EntradaRejeitada, "03" },
        { FamiliaOcorrencia.Liquidacao, "06" },
        { FamiliaOcorrencia.Baixa, "09" },
        { FamiliaOcorrencia.Tarifa, "28" },
        { FamiliaOcorrencia.Rejeicao, "26" },
        { FamiliaOcorrencia.Cancelamento, "20" },
        { FamiliaOcorrencia.Informativo, "11" },
    };

    // Tabela Itaú CNAB 400 (manual I2): o código 26 significa "tarifa de aviso
    // de cobrança" nesse layout, não "instrução rejeitada" como no 240.
    public static TheoryData<FamiliaOcorrencia, string> FamiliasECodigos400 => new()
    {
        { FamiliaOcorrencia.EntradaConfirmada, "02" },
        { FamiliaOcorrencia.EntradaRejeitada, "03" },
        { FamiliaOcorrencia.Liquidacao, "06" },
        { FamiliaOcorrencia.Baixa, "09" },
        { FamiliaOcorrencia.Tarifa, "26" },
        { FamiliaOcorrencia.Rejeicao, "16" },
        { FamiliaOcorrencia.Cancelamento, "20" },
        { FamiliaOcorrencia.Informativo, "11" },
    };

    [Fact]
    public void TabelaCobranca_ContemPeloMenosUmCodigoDeCadaFamilia()
    {
        var familiasTabela = TabelasOcorrencias.TodasCobranca().Select(o => o.Familia).ToHashSet();
        foreach (var familia in Enum.GetValues<FamiliaOcorrencia>())
        {
            Assert.True(familiasTabela.Contains(familia), $"Família {familia} sem código na tabela de cobrança.");
        }
    }

    [Theory]
    [MemberData(nameof(FamiliasECodigos400))]
    public void Retorno400_ComCodigoDaFamilia_DeveMapearParaFamiliaCorreta(FamiliaOcorrencia familia, string codigo)
    {
        var conteudo = Encoding.Latin1.GetString(
            File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Golden", "341_cobranca_400.ret")));
        var linhas = LinhasCnab.Separar(conteudo).ToArray();

        // Ocorrência do detalhe Itaú CNAB 400: posições 109-110 (manual I2).
        var caracteres = linhas[1].ToCharArray();
        codigo.CopyTo(0, caracteres, 108, 2);
        linhas[1] = new string(caracteres);

        var retorno = LeitorRetorno400.Ler(string.Join("\r\n", linhas) + "\r\n", Bancos.Itau);
        var item = Assert.Single(retorno.Itens);

        Assert.Equal(codigo, item.Ocorrencia!.Codigo);
        Assert.Equal(familia, item.Ocorrencia.Familia);
    }

    [Theory]
    [MemberData(nameof(FamiliasECodigos))]
    public void Retorno240_ComCodigoDaFamilia_DeveMapearParaFamiliaCorreta(FamiliaOcorrencia familia, string codigo)
    {
        var baseFixture = RetornoFixtures.Cnab240(Bancos.Itau.Codigo, 1, 100m);
        var linhas = LinhasCnab.Separar(baseFixture).ToArray();

        // C044 (movimento) fica nas posições 16-17 dos segmentos T e U.
        for (var indice = 2; indice <= 3; indice++)
        {
            var caracteres = linhas[indice].ToCharArray();
            codigo.CopyTo(0, caracteres, 15, 2);
            linhas[indice] = new string(caracteres);
        }

        var retorno = LeitorRetorno240.Ler(string.Join("\r\n", linhas) + "\r\n", Bancos.Itau);
        var item = Assert.Single(Assert.Single(retorno.Lotes).Itens);

        Assert.Equal(codigo, item.Ocorrencia!.Codigo);
        Assert.Equal(familia, item.Ocorrencia.Familia);
    }
}
