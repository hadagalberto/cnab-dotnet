namespace Cnab.Tests;

using System.Globalization;
using Cnab.Arquivo;
using Cnab.Layouts.Cnab240;
using Cnab.Layouts.Cnab240.Registros;
using Cnab.Modelo;

/// <summary>
/// Construtores de retornos sintéticos válidos, usados por testes de CLI e
/// pelas propriedades de round-trip. Todas as posições vêm dos registros
/// declarativos (citados em <c>docs/layouts.md</c>); nenhuma é inventada.
/// </summary>
internal static class RetornoFixtures
{
    private static readonly DateOnly DataBase = new(2026, 11, 20);

    /// <summary>Monta um retorno CNAB 240 com segmentos T/U para um banco.</summary>
    /// <param name="codigoBanco">Código do banco (um dos nove suportados).</param>
    /// <param name="quantidadeItens">Quantidade de pares T/U.</param>
    /// <param name="valor">Valor de cada título.</param>
    /// <returns>Conteúdo CRLF do arquivo.</returns>
    internal static string Cnab240(int codigoBanco, int quantidadeItens, decimal valor)
    {
        var banco = Bancos.Obter(codigoBanco);
        var perfil = PerfilBanco240.Obter(banco);
        var inscricao = new DocumentoFiscal(DocumentoFiscal.CnpjExemplo);
        var linhas = new List<string>
        {
            RegistroCnab<HeaderArquivo240>.Escrever(new HeaderArquivo240
            {
                Banco = codigoBanco,
                Lote = 0,
                TipoRegistro = Codigos240.HeaderArquivo,
                TipoInscricaoEmpresa = inscricao.TipoInscricao,
                NumeroInscricaoEmpresa = inscricao.Digitos,
                NomeEmpresa = "EMPRESA TESTE LTDA",
                NomeBanco = perfil.NomeBanco,
                CodigoRemessaRetorno = Codigos240.Retorno,
                DataGeracao = DataBase,
                HoraGeracao = 101530,
                NumeroSequencialArquivo = 1,
                VersaoLayout = perfil.VersaoLayoutArquivo,
                Densidade = Codigos240.DensidadePadrao,
            }),
            RegistroCnab<HeaderLoteCobranca240>.Escrever(new HeaderLoteCobranca240
            {
                Banco = codigoBanco,
                Lote = 1,
                TipoRegistro = Codigos240.HeaderLote,
                TipoOperacao = perfil.OperacaoCobrancaRetorno.ToString(),
                TipoServico = int.Parse(Codigos240.ServicoCobranca, CultureInfo.InvariantCulture),
                VersaoLayoutLote = perfil.VersaoLayoutLoteCobranca,
                TipoInscricaoEmpresa = inscricao.TipoInscricao,
                NumeroInscricaoEmpresa = inscricao.Digitos,
                NomeEmpresa = "EMPRESA TESTE LTDA",
                NumeroRemessaRetorno = 1,
                DataGravacao = DataBase,
            }),
        };

        for (var i = 0; i < quantidadeItens; i++)
        {
            linhas.Add(RegistroCnab<SegmentoT240>.Escrever(new SegmentoT240
            {
                Banco = codigoBanco,
                Lote = 1,
                TipoRegistro = Codigos240.Detalhe,
                Sequencial = (2 * i) + 1,
                CodigoMovimento = 6, // C044 "06" — liquidação normal (FEBRABAN V10.9)
                NossoNumero = "00000000000000000001",
                DataVencimento = DataBase.AddDays(10),
                Valor = valor,
                CodigoMoeda = Codigos240.MoedaRealCodigo,
                MotivoOcorrencia = new string('0', 10),
            }));
            linhas.Add(RegistroCnab<SegmentoU240>.Escrever(new SegmentoU240
            {
                Banco = codigoBanco,
                Lote = 1,
                TipoRegistro = Codigos240.Detalhe,
                Sequencial = (2 * i) + 2,
                CodigoMovimento = 6,
                ValorPago = valor,
                ValorLiquido = valor,
                DataOcorrencia = DataBase,
                DataCredito = DataBase.AddDays(1),
                CodigoOcorrenciaPagador = new string('0', 4),
            }));
        }

        linhas.Add(RegistroCnab<TrailerLoteCobranca240>.Escrever(new TrailerLoteCobranca240
        {
            Banco = codigoBanco,
            Lote = 1,
            TipoRegistro = Codigos240.TrailerLote,
            QuantidadeSimples = quantidadeItens,
            ValorSimples = valor * quantidadeItens,
        }));
        linhas.Add(RegistroCnab<TrailerArquivo240>.Escrever(new TrailerArquivo240
        {
            Banco = codigoBanco,
            Lote = 9999,
            TipoRegistro = Codigos240.TrailerArquivo,
            QuantidadeLotes = 1,
            QuantidadeRegistros = linhas.Count + 1,
        }));
        return EscritorDeArquivo.Montar(linhas);
    }

    /// <summary>
    /// Monta um retorno CNAB 400 repetindo o detalhe de um golden file e
    /// substituindo o valor pago por um vetor aleatório válido (13 dígitos).
    /// </summary>
    /// <param name="codigoBanco">Código do banco (77, 237, 341 ou 748).</param>
    /// <param name="quantidadeItens">Quantidade de detalhes.</param>
    /// <param name="centavos">Valor pago em centavos.</param>
    /// <returns>Conteúdo CRLF do arquivo.</returns>
    internal static string Cnab400(int codigoBanco, int quantidadeItens, int centavos)
    {
        var nome = $"{codigoBanco:000}_cobranca_400.ret";
        var caminho = Path.Combine(AppContext.BaseDirectory, "Golden", nome);
        var conteudo = ArquivoTextoLeitor.Ler(caminho);
        var linhas = conteudo.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);
        var cabecalho = linhas[0];
        var detalhe = linhas[1];
        var trailer = linhas[^1];

        // Posições publicadas dos manuais: Inter 160-172; Itaú/Bradesco/
        // Sicredi 254-266 (13 posições, valor com 2 decimais implícitas).
        var inicioValor = codigoBanco == 77 ? 159 : 253;
        var digitos = centavos.ToString("D13", CultureInfo.InvariantCulture);
        var detalhes = new List<string>(quantidadeItens);
        for (var i = 0; i < quantidadeItens; i++)
        {
            var caracteres = detalhe.ToCharArray();
            digitos.CopyTo(0, caracteres, inicioValor, digitos.Length);
            detalhes.Add(new string(caracteres));
        }

        return EscritorDeArquivo.Montar([cabecalho, .. detalhes, trailer]);
    }

    private static class ArquivoTextoLeitor
    {
        internal static string Ler(string caminho) => System.Text.Encoding.Latin1.GetString(File.ReadAllBytes(caminho));
    }
}
