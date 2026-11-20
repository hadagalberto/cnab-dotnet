namespace Cnab.Layouts.Cnab400;

using System.Globalization;
using Cnab.Arquivo;
using Cnab.Layouts.Cnab240;
using Cnab.Layouts.Cnab400.Registros;
using Cnab.Modelo;

/// <summary>
/// Leitura de arquivos retorno CNAB 400 de cobrança. Os registros de detalhe
/// são tipados por banco e produzem <see cref="ItemRetorno400"/> com ocorrência,
/// motivos, valores e datas publicados no manual correspondente. As linhas
/// originais são preservadas para round-trip byte-exato.
/// </summary>
public static class LeitorRetorno400
{
    /// <summary>Parseia um arquivo retorno CNAB 400.</summary>
    /// <param name="conteudo">Conteúdo textual do arquivo (CRLF ou LF).</param>
    /// <param name="banco">Banco esperado; quando informado, confere o cabeçalho.</param>
    /// <returns>Arquivo parseado.</returns>
    public static Retorno400 Ler(string conteudo, Banco? banco = null)
    {
        ArgumentNullException.ThrowIfNull(conteudo);
        var linhas = SepararLinhas(conteudo);
        if (linhas.Count < 3)
        {
            throw new CampoCnabException("Arquivo retorno CNAB 400 incompleto: esperado header, detalhe e trailer.");
        }

        ValidarTamanhos(linhas);
        var emissor = banco ?? DetectarBanco(linhas[0]);
        var cabecalho = LerCabecalho(linhas[0], emissor) with { LinhaOriginal = linhas[0] };
        var itens = new List<ItemRetorno400>();
        for (var i = 1; i < linhas.Count - 1; i++)
        {
            var linha = linhas[i];
            var tipo = TipoRegistro(linha, i + 1);
            if (tipo is Codigos400.Detalhe or Codigos400.Mensagem)
            {
                itens.Add(LerDetalhe(linha, emissor, i + 1) with { LinhaOriginal = linha });
            }
        }

        var trailer = LerTrailer(linhas[^1], emissor) with { LinhaOriginal = linhas[^1] };
        return new Retorno400
        {
            Banco = emissor,
            HeaderArquivo = cabecalho,
            Itens = itens,
            TrailerArquivo = trailer,
        };
    }

    /// <summary>Reescreve o retorno parseado em linhas de 400 posições.</summary>
    /// <param name="retorno">Retorno parseado.</param>
    /// <returns>Linhas sem delimitadores, byte-idênticas às linhas lidas.</returns>
    public static IReadOnlyList<string> EscreverLinhas(Retorno400 retorno)
    {
        ArgumentNullException.ThrowIfNull(retorno);
        var linhas = new List<string> { retorno.HeaderArquivo.LinhaOriginal };
        foreach (var item in retorno.Itens)
        {
            linhas.Add(item.LinhaOriginal);
        }

        linhas.Add(retorno.TrailerArquivo.LinhaOriginal);
        return linhas;
    }

    private static IReadOnlyList<string> SepararLinhas(string conteudo)
    {
        var partes = conteudo.Split(["\r\n", "\n"], StringSplitOptions.None);
        var linhas = new List<string>(partes.Length);
        foreach (var parte in partes)
        {
            if (parte.Length == 0 && ReferenceEquals(parte, partes[^1]))
            {
                continue;
            }

            linhas.Add(parte);
        }

        return linhas;
    }

    private static void ValidarTamanhos(IReadOnlyList<string> linhas)
    {
        for (var i = 0; i < linhas.Count; i++)
        {
            if (linhas[i].Length != Codigos400.TamanhoRegistro)
            {
                throw new TamanhoRegistroException(
                    "Linha de retorno CNAB 400 com tamanho incorreto",
                    Codigos400.TamanhoRegistro,
                    linhas[i].Length,
                    i + 1);
            }
        }
    }

    private static Banco DetectarBanco(string linha)
    {
        // Os quatro layouts implementados gravam o código do banco nas
        // posições 77-79 do header (Itaú, Bradesco, Sicredi e Inter).
        var codigo = linha.Substring(76, 3);
        if (int.TryParse(codigo, NumberStyles.None, CultureInfo.InvariantCulture, out var numero))
        {
            if (Bancos.TentarObter(numero, out var banco))
            {
                return banco!;
            }
        }

        throw new CampoCnabException(
            "Não foi possível identificar o banco pelo header; informe o banco esperado no parâmetro 'banco'.");
    }

    private static int TipoRegistro(string linha, int numeroLinha)
    {
        var texto = linha[..1];
        return int.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out var tipo)
            ? tipo
            : throw new CampoCnabException(
                "Tipo de registro não numérico", "Tipo de Registro", numeroLinha, 1, 1);
    }

    private static CabecalhoRetorno400 LerCabecalho(string linha, Banco banco) => banco.Codigo switch
    {
        341 => CabecalhoItau(RegistroCnab<HeaderRetornoItau400>.Ler(linha, 1)),
        237 => CabecalhoBradesco(RegistroCnab<HeaderRetornoBradesco400>.Ler(linha, 1)),
        748 => CabecalhoSicredi(RegistroCnab<HeaderRetornoSicredi400>.Ler(linha, 1)),
        77 => RegistrosInter400.LerCabecalho(linha),
        _ => throw new ArgumentException($"Banco {banco} sem leitor de retorno CNAB 400.", nameof(banco)),
    };

    private static CabecalhoRetorno400 CabecalhoItau(HeaderRetornoItau400 h) => new()
    {
        Banco = h.Banco,
        CodigoRemessaRetorno = h.CodigoRetorno,
        DataGeracao = h.DataGeracao,
        DataCredito = h.DataCredito,
        NumeroSequencialArquivo = h.NumeroSequencialRetorno,
        LinhaOriginal = RegistroCnab<HeaderRetornoItau400>.Escrever(h),
    };

    private static CabecalhoRetorno400 CabecalhoBradesco(HeaderRetornoBradesco400 h) => new()
    {
        Banco = h.Banco,
        CodigoRemessaRetorno = h.IdentificacaoRetorno,
        DataGeracao = h.DataGravacao,
        DataCredito = h.DataCredito,
        NumeroSequencialArquivo = int.Parse(h.NumeroAvisoBancario.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture),
        LinhaOriginal = RegistroCnab<HeaderRetornoBradesco400>.Escrever(h),
    };

    private static CabecalhoRetorno400 CabecalhoSicredi(HeaderRetornoSicredi400 h) => new()
    {
        Banco = h.Banco,
        CodigoRemessaRetorno = h.IdentificacaoRetorno,
        DataGeracao = DataAnsi(h.DataGravacao),
        NumeroSequencialArquivo = h.NumeroRetorno,
        LinhaOriginal = RegistroCnab<HeaderRetornoSicredi400>.Escrever(h),
    };

    private static ItemRetorno400 LerDetalhe(string linha, Banco banco, int numeroLinha) => banco.Codigo switch
    {
        341 => ItemItau(RegistroCnab<DetalheRetornoItau400>.Ler(linha, numeroLinha), banco),
        237 => ItemBradesco(RegistroCnab<DetalheRetornoBradesco400>.Ler(linha, numeroLinha), banco),
        748 => ItemSicredi(RegistroCnab<DetalheRetornoSicredi400>.Ler(linha, numeroLinha), banco),
        77 => RegistrosInter400.LerDetalhe(linha, banco, numeroLinha),
        _ => throw new ArgumentException($"Banco {banco} sem leitor de detalhe CNAB 400.", nameof(banco)),
    };

    private static ItemRetorno400 ItemItau(DetalheRetornoItau400 d, Banco banco) => new()
    {
        LinhaOriginal = RegistroCnab<DetalheRetornoItau400>.Escrever(d),
        TipoRegistro = d.IdentificacaoRegistro,
        Sequencial = d.NumeroSequencial,
        NossoNumero = d.NossoNumero.Trim(),
        SeuNumero = d.NumeroDocumento.Trim(),
        Ocorrencia = TabelasOcorrencias400.Obter(banco, d.CodigoOcorrencia.ToString("00", CultureInfo.InvariantCulture)),
        Motivos = InterpretarMotivos(d.ErrosMensagem, banco),
        Valores = new ValoresRetorno400
        {
            ValorTitulo = d.ValorTitulo,
            ValorPago = d.ValorPago,
            ValorLiquido = d.ValorPago + d.OutrosCreditos,
            Tarifa = d.Tarifa,
            JurosMulta = d.JurosMulta,
            Desconto = d.Desconto,
            Abatimento = d.Abatimento,
            Iof = d.Iof,
            DataOcorrencia = d.DataOcorrencia,
            DataCredito = d.DataCredito,
            DataVencimento = d.DataVencimento,
        },
    };

    private static ItemRetorno400 ItemBradesco(DetalheRetornoBradesco400 d, Banco banco) => new()
    {
        LinhaOriginal = RegistroCnab<DetalheRetornoBradesco400>.Escrever(d),
        TipoRegistro = d.IdentificacaoRegistro,
        Sequencial = d.NumeroSequencial,
        NossoNumero = d.NossoNumero.Trim(),
        SeuNumero = d.NumeroDocumento.Trim(),
        Ocorrencia = TabelasOcorrencias400.Obter(banco, d.CodigoOcorrencia.ToString("00", CultureInfo.InvariantCulture)),
        Motivos = InterpretarMotivos(d.MotivosRejeicao, banco),
        Valores = new ValoresRetorno400
        {
            ValorTitulo = d.ValorTitulo,
            ValorPago = d.ValorPago,
            ValorLiquido = d.ValorPago + d.OutrosCreditos - d.Tarifa - d.CustasProtesto,
            Tarifa = d.Tarifa + d.CustasProtesto,
            JurosMulta = d.JurosMora + d.JurosOperacao,
            Desconto = d.Desconto,
            Abatimento = d.Abatimento,
            Iof = d.Iof,
            DataOcorrencia = d.DataOcorrencia,
            DataCredito = d.DataCredito,
            DataVencimento = d.DataVencimento,
        },
    };

    private static ItemRetorno400 ItemSicredi(DetalheRetornoSicredi400 d, Banco banco) => new()
    {
        LinhaOriginal = RegistroCnab<DetalheRetornoSicredi400>.Escrever(d),
        TipoRegistro = d.IdentificacaoRegistro,
        Sequencial = d.NumeroSequencial,
        NossoNumero = d.NossoNumero.Trim(),
        SeuNumero = d.SeuNumero.Trim(),
        Ocorrencia = TabelasOcorrencias400.Obter(banco, d.CodigoOcorrencia.ToString("00", CultureInfo.InvariantCulture)),
        Motivos = InterpretarMotivos(d.MotivoOcorrencia, banco),
        Valores = new ValoresRetorno400
        {
            ValorTitulo = d.ValorTitulo,
            ValorPago = d.ValorPago,
            ValorLiquido = d.ValorPago - d.Tarifa - d.CustasProtesto,
            Tarifa = d.Tarifa + d.CustasProtesto,
            JurosMulta = d.JurosMora + d.MultaOutros,
            Desconto = d.Desconto,
            Abatimento = d.Abatimento,
            DataOcorrencia = d.DataOcorrencia,
            DataCredito = DataAnsiNullable(d.DataPrevistaCredito),
            DataVencimento = d.DataVencimento,
        },
    };

    private static TrailerRetorno400 LerTrailer(string linha, Banco banco)
    {
        var tipo = TipoRegistro(linha, 0);
        if (tipo != Codigos400.TrailerArquivo)
        {
            throw new CampoCnabException("Última linha não é trailer de arquivo (tipo 9).", "Tipo de Registro", null, 1, 1);
        }

        return banco.Codigo switch
        {
            341 => TrailerItau(RegistroCnab<TrailerRetornoItau400>.Ler(linha)),
            237 => TrailerBradesco(RegistroCnab<TrailerRetornoBradesco400>.Ler(linha)),
            748 => TrailerSicredi(RegistroCnab<TrailerRetornoSicredi400>.Ler(linha)),
            77 => RegistrosInter400.LerTrailer(linha),
            _ => throw new ArgumentException($"Banco {banco} sem leitor de trailer CNAB 400.", nameof(banco)),
        };
    }

    private static TrailerRetorno400 TrailerItau(TrailerRetornoItau400 t) => new()
    {
        TipoRegistro = t.IdentificacaoRegistro,
        CodigoRemessaRetorno = t.CodigoRetorno,
        NumeroSequencialArquivo = t.NumeroSequencial,
        LinhaOriginal = RegistroCnab<TrailerRetornoItau400>.Escrever(t),
    };

    private static TrailerRetorno400 TrailerBradesco(TrailerRetornoBradesco400 t) => new()
    {
        TipoRegistro = t.IdentificacaoRegistro,
        CodigoRemessaRetorno = t.IdentificacaoRetorno,
        NumeroSequencialArquivo = t.NumeroSequencial,
        LinhaOriginal = RegistroCnab<TrailerRetornoBradesco400>.Escrever(t),
    };

    private static TrailerRetorno400 TrailerSicredi(TrailerRetornoSicredi400 t) => new()
    {
        TipoRegistro = t.IdentificacaoRegistro,
        CodigoRemessaRetorno = t.IdentificacaoRetorno,
        NumeroSequencialArquivo = t.NumeroSequencial,
        LinhaOriginal = RegistroCnab<TrailerRetornoSicredi400>.Escrever(t),
    };

    private static IReadOnlyList<OcorrenciaRetorno> InterpretarMotivos(string conteudo, Banco banco)
    {
        var motivos = new List<OcorrenciaRetorno>();
        for (var i = 0; i + 1 < conteudo.Length; i += 2)
        {
            var codigo = conteudo.Substring(i, 2).Trim();
            if (codigo.Length == 0 || codigo == "00" || !codigo.All(char.IsLetterOrDigit))
            {
                continue;
            }

            motivos.Add(new OcorrenciaRetorno(
                codigo,
                TabelasOcorrencias400.ObterMotivo(banco, codigo),
                FamiliaOcorrencia.Rejeicao));
        }

        return motivos;
    }

    private static DateOnly? DataAnsi(string valor) =>
        DateOnly.TryParseExact(valor, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var data)
            && data != default
                ? data
                : null;

    private static DateOnly? DataAnsiNullable(string valor) =>
        valor.Trim('0').Length == 0 ? null : DataAnsi(valor);
}
