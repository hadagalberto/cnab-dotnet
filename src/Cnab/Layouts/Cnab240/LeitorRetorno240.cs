namespace Cnab.Layouts.Cnab240;

using System.Globalization;
using Cnab.Arquivo;
using Cnab.Layouts.Cnab240.Registros;
using Cnab.Modelo;

/// <summary>Item de retorno: um par segmento T/U (cobrança) ou um segmento de pagamento.</summary>
public sealed record ItemRetorno240
{
    /// <summary>Inicializa um item.</summary>
    /// <param name="linhaOriginal">Linha exatamente como lida do arquivo.</param>
    public ItemRetorno240(string linhaOriginal)
    {
        LinhaOriginal = linhaOriginal;
    }

    /// <summary>Linha original do segmento principal (240 posições).</summary>
    public string LinhaOriginal { get; init; }

    /// <summary>Linha original do segmento U, quando pareado com um T.</summary>
    public string? LinhaValores { get; init; }

    /// <summary>Segmento T, quando presente.</summary>
    public SegmentoT240? Titulo { get; init; }

    /// <summary>Segmento U, quando presente.</summary>
    public SegmentoU240? Valores { get; init; }

    /// <summary>Ocorrência principal (C044, posições 16-17 do T ou U).</summary>
    public OcorrenciaRetorno? Ocorrencia { get; init; }

    /// <summary>Motivos/rejeições (C047, posições 214-223 do T).</summary>
    public IReadOnlyList<OcorrenciaRetorno> Motivos { get; init; } = [];

    /// <summary>Ocorrências do pagador (A001, posições 154-157 do U).</summary>
    public IReadOnlyList<string> OcorrenciasPagador { get; init; } = [];

    /// <summary>
    /// Ocorrências de retorno de pagamentos (posições 231-240 dos segmentos
    /// A/B/J/O). No Inter, esta é a tabela G059 própria do banco.
    /// </summary>
    public IReadOnlyList<OcorrenciaRetorno> OcorrenciasPagamento { get; init; } = [];
}

/// <summary>Lote de retorno com seus itens e trailers.</summary>
public sealed record LoteRetorno240
{
    /// <summary>Linha original do header de lote, preservada para round-trip byte-exato.</summary>
    public string? LinhaHeader { get; init; }

    /// <summary>Linha original do trailer de lote, preservada para round-trip byte-exato.</summary>
    public string? LinhaTrailer { get; init; }

    /// <summary>Header de lote de cobrança (quando o lote é de cobrança).</summary>
    public HeaderLoteCobranca240? HeaderCobranca { get; init; }

    /// <summary>Header de lote de pagamentos (quando o lote é de pagamentos).</summary>
    public HeaderLotePagamento240? HeaderPagamento { get; init; }

    /// <summary>Itens de detalhe do lote.</summary>
    public IList<ItemRetorno240> Itens { get; init; } = new List<ItemRetorno240>();

    /// <summary>Trailer de lote de cobrança.</summary>
    public TrailerLoteCobranca240? TrailerCobranca { get; set; }

    /// <summary>Trailer de lote de pagamentos.</summary>
    public TrailerLotePagamento240? TrailerPagamento { get; set; }
}

/// <summary>Arquivo retorno CNAB 240 parseado.</summary>
public sealed record Retorno240
{
    /// <summary>Banco emissor (lido das posições 1-3).</summary>
    public required Banco Banco { get; init; }

    /// <summary>Header de arquivo.</summary>
    public required HeaderArquivo240 HeaderArquivo { get; init; }

    /// <summary>Linha original do header de arquivo, preservada para round-trip byte-exato.</summary>
    public string? LinhaHeaderArquivo { get; init; }

    /// <summary>Linha original do trailer de arquivo, preservada para round-trip byte-exato.</summary>
    public string? LinhaTrailerArquivo { get; init; }

    /// <summary>Lotes do arquivo.</summary>
    public required IReadOnlyList<LoteRetorno240> Lotes { get; init; }

    /// <summary>Trailer de arquivo.</summary>
    public required TrailerArquivo240 TrailerArquivo { get; init; }

    /// <summary>Perfil do banco usado na leitura.</summary>
    public required PerfilBanco240 Perfil { get; init; }
}

/// <summary>
/// Lê arquivos retorno CNAB 240 (registros T/U de cobrança e segmentos de
/// pagamento), validando comprimento de linha e mapeando os códigos de
/// ocorrência para <see cref="OcorrenciaRetorno"/> com descrição e família.
/// </summary>
public static class LeitorRetorno240
{
    /// <summary>Parseia o conteúdo completo de um arquivo retorno.</summary>
    /// <param name="conteudo">Conteúdo textual do arquivo (CRLF ou LF).</param>
    /// <param name="banco">Banco esperado; quando informado, confere o header.</param>
    /// <returns>Arquivo parseado.</returns>
    /// <exception cref="CampoCnabException">Linha inválida ou campo inconsistente.</exception>
    public static Retorno240 Ler(string conteudo, Banco? banco = null)
    {
        ArgumentNullException.ThrowIfNull(conteudo);
        var linhas = LinhasCnab.Separar(conteudo);
        if (linhas.Count < 3)
        {
            throw new CampoCnabException("Arquivo retorno incompleto: esperado header, lote e trailer.");
        }

        var header = RegistroCnab<HeaderArquivo240>.Ler(linhas[0], 1);
        if (header.TipoRegistro != Codigos240.HeaderArquivo)
        {
            throw new CampoCnabException(
                "Primeira linha não é header de arquivo (tipo 0).", "Tipo de Registro", 1, 8, 1);
        }

        var emissor = banco ?? Bancos.Obter(header.Banco);
        if (header.Banco != emissor.Codigo)
        {
            throw new CampoCnabException(
                $"Banco do header ({header.Banco:000}) difere do esperado ({emissor.Codigo:000}).",
                "Código do Banco", 1, 1, 3);
        }

        var perfil = PerfilBanco240.Obter(emissor);
        var lotes = new List<LoteRetorno240>();
        LoteRetorno240? loteAtual = null;
        TrailerArquivo240? trailerArquivo = null;

        for (var i = 1; i < linhas.Count; i++)
        {
            var linha = linhas[i];
            var numeroLinha = i + 1;
            var tipo = Tipo(linha, numeroLinha);
            switch (tipo)
            {
                case Codigos240.HeaderLote:
                    loteAtual = NovoLote(linha, numeroLinha, emissor, perfil) with { LinhaHeader = linha };
                    lotes.Add(loteAtual);
                    break;
                case Codigos240.Detalhe:
                    if (loteAtual is null)
                    {
                        throw new CampoCnabException("Detalhe fora de lote.", null, numeroLinha);
                    }

                    AdicionarDetalhe(loteAtual, linha, numeroLinha, emissor);
                    break;
                case Codigos240.TrailerLote:
                    if (loteAtual is null)
                    {
                        throw new CampoCnabException("Trailer de lote fora de lote.", null, numeroLinha);
                    }

                    FecharLote(loteAtual, linha, numeroLinha);
                    lotes[^1] = loteAtual with { LinhaTrailer = linha };
                    break;
                case Codigos240.TrailerArquivo:
                    trailerArquivo = RegistroCnab<TrailerArquivo240>.Ler(linha, numeroLinha);
                    break;
                default:
                    throw new CampoCnabException(
                        $"Tipo de registro {tipo} não esperado no retorno.", "Tipo de Registro", numeroLinha, 8, 1);
            }
        }

        if (trailerArquivo is null)
        {
            throw new CampoCnabException("Arquivo retorno sem trailer de arquivo (tipo 9).");
        }

        return new Retorno240
        {
            Banco = emissor,
            HeaderArquivo = header,
            LinhaHeaderArquivo = linhas[0],
            Lotes = lotes,
            TrailerArquivo = trailerArquivo,
            LinhaTrailerArquivo = trailerArquivo is null ? null : linhas[^1],
            Perfil = perfil,
        };
    }

    /// <summary>
    /// Reescreve um retorno parseado em linhas de 240 posições. Os cabeçalhos e
    /// trailers são resserializados a partir dos registros tipados; os
    /// segmentos de detalhe são copiados integralmente. Base para testes de
    /// round-trip byte-exato.
    /// </summary>
    /// <param name="retorno">Retorno parseado.</param>
    /// <returns>Linhas sem delimitadores.</returns>
    public static IReadOnlyList<string> EscreverLinhas(Retorno240 retorno)
    {
        ArgumentNullException.ThrowIfNull(retorno);
        var linhas = new List<string> { retorno.LinhaHeaderArquivo ?? RegistroCnab<HeaderArquivo240>.Escrever(retorno.HeaderArquivo) };
        foreach (var lote in retorno.Lotes)
        {
            if (lote.LinhaHeader is not null)
            {
                linhas.Add(lote.LinhaHeader);
            }
            else if (lote.HeaderCobranca is not null)
            {
                linhas.Add(RegistroCnab<HeaderLoteCobranca240>.Escrever(lote.HeaderCobranca));
            }
            else if (lote.HeaderPagamento is not null)
            {
                linhas.Add(RegistroCnab<HeaderLotePagamento240>.Escrever(lote.HeaderPagamento));
            }

            foreach (var item in lote.Itens)
            {
                linhas.Add(item.LinhaOriginal);
                if (item.LinhaValores is not null)
                {
                    linhas.Add(item.LinhaValores);
                }
            }

            if (lote.LinhaTrailer is not null)
            {
                linhas.Add(lote.LinhaTrailer);
            }
            else if (lote.TrailerCobranca is not null)
            {
                linhas.Add(RegistroCnab<TrailerLoteCobranca240>.Escrever(lote.TrailerCobranca));
            }
            else if (lote.TrailerPagamento is not null)
            {
                linhas.Add(RegistroCnab<TrailerLotePagamento240>.Escrever(lote.TrailerPagamento));
            }
        }

        linhas.Add(retorno.LinhaTrailerArquivo ?? RegistroCnab<TrailerArquivo240>.Escrever(retorno.TrailerArquivo));
        return linhas;
    }

    private static int Tipo(string linha, int numeroLinha)
    {
        if (linha.Length != Codigos240.TamanhoRegistro)
        {
            throw new TamanhoRegistroException(
                "Linha de retorno com tamanho incorreto", Codigos240.TamanhoRegistro, linha.Length, numeroLinha);
        }

        var texto = linha.Substring(7, 1);
        return int.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out var tipo)
            ? tipo
            : throw new CampoCnabException("Tipo de registro não numérico", "Tipo de Registro", numeroLinha, 8, 1);
    }

    private static LoteRetorno240 NovoLote(string linha, int numeroLinha, Banco banco, PerfilBanco240 perfil)
    {
        var operacao = linha.Substring(8, 1);
        var servico = linha.Substring(9, 2);
        if (operacao == perfil.OperacaoCobrancaRetorno.ToString()
            || servico == Codigos240.ServicoCobranca)
        {
            return new LoteRetorno240
            {
                HeaderCobranca = RegistroCnab<HeaderLoteCobranca240>.Ler(linha, numeroLinha),
            };
        }

        return new LoteRetorno240
        {
            HeaderPagamento = RegistroCnab<HeaderLotePagamento240>.Ler(linha, numeroLinha),
        };
    }

    private static void AdicionarDetalhe(
        LoteRetorno240 lote, string linha, int numeroLinha, Banco banco)
    {
        var segmento = linha.Substring(13, 1);
        if (segmento == Codigos240.SegmentoT.ToString())
        {
            var titulo = RegistroCnab<SegmentoT240>.Ler(linha, numeroLinha);
            var codigo = titulo.CodigoMovimento.ToString("00", CultureInfo.InvariantCulture);
            lote.Itens.Add(new ItemRetorno240(linha)
            {
                Titulo = titulo,
                Ocorrencia = TabelasOcorrencias.Obter(banco, codigo),
                Motivos = InterpretarMotivos(titulo.MotivoOcorrencia, banco),
            });
            return;
        }

        if (segmento == Codigos240.SegmentoU.ToString())
        {
            var valores = RegistroCnab<SegmentoU240>.Ler(linha, numeroLinha);
            var codigo = valores.CodigoMovimento.ToString("00", CultureInfo.InvariantCulture);
            var itemAnterior = lote.Itens.Count > 0 ? lote.Itens[^1] : null;
            if (itemAnterior is not null && itemAnterior.Titulo is not null && itemAnterior.Valores is null)
            {
                lote.Itens.RemoveAt(lote.Itens.Count - 1);
                lote.Itens.Add(itemAnterior with
                {
                    LinhaValores = linha,
                    Valores = valores,
                    OcorrenciasPagador = InterpretarOcorrenciasPagador(valores.CodigoOcorrenciaPagador),
                });
                return;
            }

            lote.Itens.Add(new ItemRetorno240(linha)
            {
                Valores = valores,
                Ocorrencia = TabelasOcorrencias.Obter(banco, codigo),
                OcorrenciasPagador = InterpretarOcorrenciasPagador(valores.CodigoOcorrenciaPagador),
            });
            return;
        }

        lote.Itens.Add(new ItemRetorno240(linha)
        {
            OcorrenciasPagamento = InterpretarOcorrenciasPagamento(linha, banco),
        });
    }

    private static IReadOnlyList<OcorrenciaRetorno> InterpretarOcorrenciasPagamento(string linha, Banco banco)
    {
        if (linha.Length < 240)
        {
            return [];
        }

        var codigos = linha.Substring(230, 10);
        var ocorrencias = new List<OcorrenciaRetorno>();
        for (var i = 0; i + 1 < codigos.Length; i += 2)
        {
            var codigo = codigos.Substring(i, 2).Trim();
            if (codigo.Length == 0 || codigo == "00" || !codigo.All(char.IsLetterOrDigit))
            {
                continue;
            }

            try
            {
                ocorrencias.Add(TabelasOcorrencias.Obter(banco, codigo));
            }
            catch (ArgumentException)
            {
                try
                {
                    ocorrencias.Add(TabelasOcorrencias.ObterPagamento(codigo));
                }
                catch (ArgumentException)
                {
                    // Código não publicado nas fontes consultadas: não inventar.
                }
            }
        }

        return ocorrencias;
    }

    private static void FecharLote(LoteRetorno240 lote, string linha, int numeroLinha)
    {
        if (lote.HeaderCobranca is not null)
        {
            lote.TrailerCobranca = RegistroCnab<TrailerLoteCobranca240>.Ler(linha, numeroLinha);
        }
        else
        {
            lote.TrailerPagamento = RegistroCnab<TrailerLotePagamento240>.Ler(linha, numeroLinha);
        }
    }

    private static IReadOnlyList<OcorrenciaRetorno> InterpretarMotivos(string conteudo, Banco banco)
    {
        var motivos = new List<OcorrenciaRetorno>();
        for (var i = 0; i + 1 < conteudo.Length; i += 2)
        {
            var codigo = conteudo.Substring(i, 2).Trim();
            if (codigo.Length == 0 || codigo == "00")
            {
                continue;
            }

            motivos.Add(new OcorrenciaRetorno(
                codigo,
                TabelasOcorrencias.ObterMotivo(banco, codigo),
                FamiliaOcorrencia.Rejeicao));
        }

        return motivos;
    }

    private static IReadOnlyList<string> InterpretarOcorrenciasPagador(string conteudo)
    {
        var codigo = conteudo.Trim();
        return codigo.Length == 0 || codigo == "0000" ? [] : [codigo];
    }
}
