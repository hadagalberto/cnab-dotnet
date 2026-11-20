namespace Cnab.Cli;

using System.Globalization;
using Cnab.Arquivo;
using Cnab.Modelo;

/// <summary>Implementação de <c>cnab inspect</c>.</summary>
public static class ComandoInspect
{
    /// <summary>Mostra, para cada tipo de registro presente, uma tabela campo a campo.</summary>
    /// <param name="opcoes">Opções já interpretadas.</param>
    /// <returns>Tabela formatada.</returns>
    public static ResultadoCli Executar(Cli.Opcoes opcoes)
    {
        string conteudo;
        try
        {
            conteudo = ArquivoTexto.Ler(opcoes.Alvo!);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return ResultadoCli.Erro($"Não foi possível ler '{opcoes.Alvo}': {ex.Message}");
        }

        try
        {
            var linhas = ArquivoCnab.SepararLinhas(conteudo);
            var layout = ArquivoCnab.DetectarLayout(linhas);
            var ehRemessa = ArquivoCnab.EhRemessa(linhas[0], layout);
            var detectado = ArquivoCnab.DetectarBanco(linhas[0], layout);
            var banco = opcoes.Banco is null ? detectado : Bancos.Obter(opcoes.Banco.Value);
            if (opcoes.Banco is not null && detectado is not null && detectado.Codigo != banco!.Codigo)
            {
                throw new CliException(
                    $"Banco do header ({detectado.Codigo:000}) difere do informado ({banco!.Codigo:000}).");
            }

            var saida = new List<string>
            {
                $"Arquivo: {opcoes.Alvo}",
                $"Layout: CNAB {(int)layout} — {linhas.Count} registro(s)",
                $"Banco: {(banco is null ? "não identificado" : $"{banco.Codigo:000} - {banco.Nome}")}",
            };

            var vistos = new HashSet<Type>();
            for (var i = 0; i < linhas.Count; i++)
            {
                var linha = linhas[i];
                var tipo = ArquivoCnab.ObterTipoRegistro(layout, linha, banco, ehRemessa);
                if (tipo is null || !vistos.Add(tipo))
                {
                    continue;
                }

                saida.Add(string.Empty);
                saida.Add(new string('=', 78));
                saida.Add($"{tipo.Name} — primeira ocorrência na linha {i + 1} — {linha.Length} posições");
                saida.Add(new string('=', 78));
                saida.Add($"{"POSIÇÃO",-11} {"TIPO",-12} {"CAMPO",-34} VALOR");
                var registro = ArquivoCnab.LerRegistro(tipo, linha, i + 1);
                foreach (var campo in MetadadosRegistro.ObterCamposComValores(registro))
                {
                    var atributo = campo.Atributo;
                    var posicao = atributo.Tamanho == 1
                        ? $"{atributo.Posicao:000}"
                        : $"{atributo.Posicao:000}-{atributo.Posicao + atributo.Tamanho - 1:000}";
                    var tipoCampo = atributo.Tipo == TipoCampo.NumericoDecimal
                        ? $"Num({atributo.Decimais})"
                        : atributo.Tipo.ToString();
                    saida.Add(string.Format(
                        CultureInfo.InvariantCulture,
                        "{0,-11} {1,-12} {2,-34} {3}",
                        posicao,
                        tipoCampo,
                        Truncar(atributo.Nome, 34),
                        Formatar(campo.Valor)));
                }
            }

            return ResultadoCli.Ok(string.Join(Environment.NewLine, saida) + Environment.NewLine);
        }
        catch (Exception ex) when (ex is CliException or CampoCnabException or TamanhoRegistroException
                                       or ArgumentException or FormatException or InvalidOperationException)
        {
            return ResultadoCli.Erro(ex.Message);
        }
    }

    private static string Truncar(string texto, int tamanho) => texto.Length <= tamanho ? texto : texto[..tamanho];

    private static string Formatar(object? valor) => valor switch
    {
        null => string.Empty,
        DateOnly data => data.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
        decimal numero => numero.ToString("0.00", CultureInfo.InvariantCulture),
        string texto => texto.TrimEnd(),
        _ => Convert.ToString(valor, CultureInfo.InvariantCulture) ?? string.Empty,
    };
}
