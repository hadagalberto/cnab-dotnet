namespace Cnab.Cli;

using System.Text.Encodings.Web;
using System.Text.Json;
using Cnab.Arquivo;
using Cnab.Layouts.Cnab240;
using Cnab.Layouts.Cnab400;
using Cnab.Modelo;

/// <summary>Implementação de <c>cnab parse</c>.</summary>
public static class ComandoParse
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true,
    };

    /// <summary>Parseia um retorno 240/400 e imprime JSON ou resumo legível.</summary>
    /// <param name="opcoes">Opções já interpretadas.</param>
    /// <returns>Saída do comando.</returns>
    public static ResultadoCli Executar(Cli.Opcoes opcoes)
    {
        if (!ArquivoTexto.TentarLer(opcoes.Alvo, out var conteudo, out var erroLeitura))
        {
            return ResultadoCli.Erro(erroLeitura);
        }

        try
        {
            var linhas = ArquivoCnab.SepararLinhas(conteudo);
            var layout = ArquivoCnab.DetectarLayout(linhas);
            if (ArquivoCnab.EhRemessa(linhas[0], layout))
            {
                throw new CliException("O arquivo informado não é um retorno (código remessa/retorno ≠ 2).");
            }

            var detectado = ArquivoCnab.DetectarBanco(linhas[0], layout);
            var banco = opcoes.Banco is null ? detectado : Bancos.Obter(opcoes.Banco.Value);
            if (opcoes.Banco is not null && detectado is not null && detectado.Codigo != banco!.Codigo)
            {
                throw new CliException(
                    $"Banco do header ({detectado.Codigo:000}) difere do informado ({banco!.Codigo:000}).");
            }

            object json;
            string resumo;
            if (layout == LayoutArquivo.Cnab240)
            {
                var retorno = LeitorRetorno240.Ler(conteudo, banco);
                if (retorno.HeaderArquivo.CodigoRemessaRetorno != Codigos240.Retorno)
                {
                    throw new CliException("O arquivo informado não é um retorno (código remessa/retorno ≠ 2).");
                }

                json = Projeto240(retorno);
                resumo = Resumo240(retorno);
            }
            else
            {
                var retorno = LeitorRetorno400.Ler(conteudo, banco);
                if (retorno.HeaderArquivo.CodigoRemessaRetorno != Codigos400.Retorno)
                {
                    throw new CliException("O arquivo informado não é um retorno (código remessa/retorno ≠ 2).");
                }

                json = Projeto400(retorno);
                resumo = Resumo400(retorno);
            }

            return ResultadoCli.Ok(opcoes.Json
                ? JsonSerializer.Serialize(json, JsonOptions) + Environment.NewLine
                : resumo);
        }
        catch (Exception ex) when (ex is CliException or CampoCnabException or TamanhoRegistroException
                                       or ArgumentException or FormatException or InvalidOperationException)
        {
            return ResultadoCli.Erro(ex.Message);
        }
    }

    private static object Projeto240(Retorno240 retorno)
    {
        var itens = new List<object>();
        for (var i = 0; i < retorno.Lotes.Count; i++)
        {
            var lote = retorno.Lotes[i];
            for (var j = 0; j < lote.Itens.Count; j++)
            {
                var item = lote.Itens[j];
                var titulo = item.Titulo;
                var valores = item.Valores;
                itens.Add(new
                {
                    lote = i + 1,
                    ordem = j + 1,
                    nossoNumero = titulo?.NossoNumero?.Trim(),
                    seuNumero = titulo?.NumeroDocumento?.Trim(),
                    ocorrencia = Ocorrencia(item.Ocorrencia),
                    motivos = item.Motivos.Select(Ocorrencia).ToArray(),
                    ocorrenciasPagamento = item.OcorrenciasPagamento.Select(o => o.Codigo).ToArray(),
                    valores = new
                    {
                        valorTitulo = titulo?.Valor,
                        valorPago = valores?.ValorPago,
                        valorLiquido = valores?.ValorLiquido,
                        tarifa = titulo?.ValorTarifa,
                        jurosMulta = valores?.JurosMultaEncargos,
                        desconto = valores?.ValorDesconto,
                        abatimento = valores?.ValorAbatimento,
                        iof = valores?.ValorIof,
                        dataOcorrencia = valores?.DataOcorrencia,
                        dataCredito = valores?.DataCredito,
                        dataVencimento = titulo?.DataVencimento,
                    },
                });
            }
        }

        return new
        {
            layout = 240,
            banco = ProjetoBanco(retorno.Banco),
            header = new
            {
                codigoRemessaRetorno = retorno.HeaderArquivo.CodigoRemessaRetorno,
                dataGeracao = retorno.HeaderArquivo.DataGeracao,
                numeroSequencialArquivo = retorno.HeaderArquivo.NumeroSequencialArquivo,
            },
            quantidadeLotes = retorno.Lotes.Count,
            quantidadeItens = itens.Count,
            itens,
            trailer = new
            {
                quantidadeLotes = retorno.TrailerArquivo.QuantidadeLotes,
                quantidadeRegistros = retorno.TrailerArquivo.QuantidadeRegistros,
            },
        };
    }

    private static object Projeto400(Retorno400 retorno)
    {
        var itens = retorno.Itens.Select((item, indice) => new
        {
            lote = 1,
            ordem = indice + 1,
            nossoNumero = item.NossoNumero?.Trim(),
            seuNumero = item.SeuNumero?.Trim(),
            ocorrencia = Ocorrencia(item.Ocorrencia),
            motivos = item.Motivos.Select(Ocorrencia).ToArray(),
            ocorrenciasPagamento = Array.Empty<string>(),
            valores = new
            {
                valorTitulo = item.Valores?.ValorTitulo,
                valorPago = item.Valores?.ValorPago,
                valorLiquido = item.Valores?.ValorLiquido,
                tarifa = item.Valores?.Tarifa,
                jurosMulta = item.Valores?.JurosMulta,
                desconto = item.Valores?.Desconto,
                abatimento = item.Valores?.Abatimento,
                iof = item.Valores?.Iof,
                dataOcorrencia = item.Valores?.DataOcorrencia,
                dataCredito = item.Valores?.DataCredito,
                dataVencimento = item.Valores?.DataVencimento,
            },
        }).ToArray();

        return new
        {
            layout = 400,
            banco = ProjetoBanco(retorno.Banco),
            header = new
            {
                codigoRemessaRetorno = retorno.HeaderArquivo.CodigoRemessaRetorno,
                dataGeracao = retorno.HeaderArquivo.DataGeracao,
                numeroSequencialArquivo = retorno.HeaderArquivo.NumeroSequencialArquivo,
            },
            quantidadeLotes = 0,
            quantidadeItens = itens.Length,
            itens,
            trailer = new
            {
                tipoRegistro = retorno.TrailerArquivo.TipoRegistro,
                numeroSequencialArquivo = retorno.TrailerArquivo.NumeroSequencialArquivo,
            },
        };
    }

    private static object ProjetoBanco(Banco banco) => new { codigo = banco.Codigo, nome = banco.Nome };

    private static object? Ocorrencia(OcorrenciaRetorno? ocorrencia) => ocorrencia is null
        ? null
        : new { codigo = ocorrencia.Codigo, descricao = ocorrencia.Descricao, familia = ocorrencia.Familia.ToString() };

    private static string Resumo240(Retorno240 retorno)
    {
        var linhas = new List<string>
        {
            $"Retorno CNAB 240 — banco {retorno.Banco.Codigo:000} ({retorno.Banco.Nome})",
            $"Lotes: {retorno.Lotes.Count}",
        };
        foreach (var lote in retorno.Lotes)
        {
            foreach (var item in lote.Itens)
            {
                linhas.Add(ResumirItem(
                    item.Titulo?.NossoNumero,
                    item.Ocorrencia,
                    item.Valores?.ValorPago,
                    item.Valores?.DataCredito));
            }
        }

        return string.Join(Environment.NewLine, linhas) + Environment.NewLine;
    }

    private static string Resumo400(Retorno400 retorno)
    {
        var linhas = new List<string>
        {
            $"Retorno CNAB 400 — banco {retorno.Banco.Codigo:000} ({retorno.Banco.Nome})",
            $"Itens: {retorno.Itens.Count}",
        };
        foreach (var item in retorno.Itens)
        {
            linhas.Add(ResumirItem(item.NossoNumero, item.Ocorrencia, item.Valores?.ValorPago, item.Valores?.DataCredito));
        }

        return string.Join(Environment.NewLine, linhas) + Environment.NewLine;
    }

    private static string ResumirItem(string? nossoNumero, OcorrenciaRetorno? ocorrencia, decimal? valorPago, DateOnly? dataCredito)
    {
        var valor = valorPago is null ? string.Empty : $" — pago {valorPago.Value:0.00}";
        var credito = dataCredito is null ? string.Empty : $" — crédito {dataCredito.Value:dd/MM/yyyy}";
        return $"  {nossoNumero?.Trim()}: {ocorrencia?.Codigo ?? "--"} {ocorrencia?.Descricao}{valor}{credito}";
    }
}
