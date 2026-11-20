namespace Cnab.Cli;

using Cnab.Arquivo;
using Cnab.Layouts.Cnab240.Registros;
using Cnab.Layouts.Cnab400.Registros;
using Cnab.Modelo;

/// <summary>Implementação de <c>cnab validate</c>.</summary>
public static class ComandoValidate
{
    /// <summary>Valida comprimento, tipo e campos de todas as linhas do arquivo.</summary>
    /// <param name="opcoes">Opções já interpretadas.</param>
    /// <returns>Relatório de validação.</returns>
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
            var banco = ResolverBanco(linhas[0], layout, opcoes.Banco);
            var ehRemessa = ArquivoCnab.EhRemessa(linhas[0], layout);
            var erros = new List<string>();
            var avisos = new List<string>();

            for (var i = 0; i < linhas.Count; i++)
            {
                var numeroLinha = i + 1;
                var linha = linhas[i];
                if (linha.Length != (int)layout)
                {
                    erros.Add(
                        $"linha {numeroLinha}: comprimento {linha.Length}, esperado {(int)layout} "
                        + $"(TamanhoRegistroException)");
                    continue;
                }

                var tipoRegistro = ArquivoCnab.ObterTipoRegistro(layout, linha, banco, ehRemessa);
                if (tipoRegistro is null)
                {
                    erros.Add($"linha {numeroLinha}: tipo de registro não reconhecido (posição 1).");
                    continue;
                }

                try
                {
                    _ = ArquivoCnab.LerRegistro(tipoRegistro, linha, numeroLinha);
                }
                catch (Exception ex) when (ex is CampoCnabException or TamanhoRegistroException or ArgumentException)
                {
                    erros.Add($"linha {numeroLinha}: {ex.Message}");
                }
            }

            ValidarTrailers(linhas, layout, banco, ehRemessa, erros, avisos);

            if (erros.Count > 0)
            {
                var detalhes = erros.Take(20).Select(e => "  - " + e);
                var resumo = erros.Count > 20 ? $"\n  ... e mais {erros.Count - 20} erro(s)." : string.Empty;
                return ResultadoCli.Erro(
                    $"{erros.Count} erro(s) em {linhas.Count} registro(s) CNAB {(int)layout}:\n"
                    + string.Join("\n", detalhes) + resumo);
            }

            var cabecalho = new List<string>
            {
                $"OK: {linhas.Count} registro(s) CNAB {(int)layout} validado(s)"
                    + (banco is null ? "." : $" (banco {banco.Codigo:000} - {banco.Nome})."),
            };
            cabecalho.AddRange(avisos.Select(a => "AVISO: " + a));
            return ResultadoCli.Ok(string.Join(Environment.NewLine, cabecalho) + Environment.NewLine);
        }
        catch (Exception ex) when (ex is CliException or CampoCnabException or TamanhoRegistroException or ArgumentException
                                       or FormatException or InvalidOperationException)
        {
            return ResultadoCli.Erro(ex.Message);
        }
    }

    private static Banco? ResolverBanco(string primeiraLinha, LayoutArquivo layout, int? informado)
    {
        var detectado = ArquivoCnab.DetectarBanco(primeiraLinha, layout);
        if (informado is not null)
        {
            var banco = Bancos.Obter(informado.Value);
            if (detectado is not null && detectado.Codigo != banco.Codigo)
            {
                throw new CliException(
                    $"Banco do header ({detectado.Codigo:000}) difere do informado ({banco.Codigo:000}).");
            }

            return banco;
        }

        return detectado;
    }

    private static void ValidarTrailers(
        IReadOnlyList<string> linhas,
        LayoutArquivo layout,
        Banco? banco,
        bool ehRemessa,
        List<string> erros,
        List<string> avisos)
    {
        var ultima = linhas[^1];
        var tipoUltima = ArquivoCnab.ObterTipoRegistro(layout, ultima, banco, ehRemessa);
        var ehTrailerArquivo = layout == LayoutArquivo.Cnab240
            ? tipoUltima == typeof(TrailerArquivo240)
            : tipoUltima is not null && tipoUltima.Name.StartsWith("Trailer", StringComparison.Ordinal);
        if (!ehTrailerArquivo)
        {
            // Um remessa sem trailer de arquivo está incompleta mesmo que todas
            // as linhas presentes tenham comprimento e campos válidos.
            if (ehRemessa && (layout == LayoutArquivo.Cnab240 || banco is not null))
            {
                erros.Add("trailer de arquivo ausente: a última linha não é um trailer de arquivo.");
            }

            return;
        }

        if (layout == LayoutArquivo.Cnab240)
        {
            var trailer = RegistroCnab<TrailerArquivo240>.Ler(ultima, linhas.Count);
            var quantidadeLotes = linhas.Count(l => l.Length > 7 && l[7] == '1');
            if (trailer.QuantidadeRegistros != linhas.Count)
            {
                var mensagem = $"trailer de arquivo informa {trailer.QuantidadeRegistros} registros, "
                    + $"arquivo tem {linhas.Count}.";
                if (ehRemessa)
                {
                    erros.Add(mensagem);
                }
                else
                {
                    avisos.Add(mensagem);
                }
            }

            if (trailer.QuantidadeLotes != quantidadeLotes)
            {
                avisos.Add($"trailer de arquivo informa {trailer.QuantidadeLotes} lotes, "
                    + $"encontrados {quantidadeLotes}.");
            }

            return;
        }

        if (banco is null || !ehRemessa)
        {
            return;
        }

        if (banco.Codigo is 341 or 237 or 748)
        {
            var sequencial = banco.Codigo switch
            {
                341 => RegistroCnab<TrailerRemessaItau400>.Ler(ultima).NumeroSequencial,
                237 => RegistroCnab<TrailerRemessaBradesco400>.Ler(ultima).NumeroSequencial,
                _ => RegistroCnab<TrailerRemessaSicredi400>.Ler(ultima).NumeroSequencial,
            };
            if (sequencial != linhas.Count)
            {
                erros.Add($"trailer de arquivo informa sequencial {sequencial}, arquivo tem {linhas.Count} registros.");
            }
        }
    }
}
