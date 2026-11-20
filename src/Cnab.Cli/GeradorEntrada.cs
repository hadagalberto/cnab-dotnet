namespace Cnab.Cli;

using System.Globalization;
using Cnab.Arquivo;
using Cnab.Layouts.Cnab240;
using Cnab.Layouts.Cnab400;
using Cnab.Modelo;

/// <summary>Converte o JSON de entrada em modelos de remessa do domínio.</summary>
public static class GeradorEntrada
{
    /// <summary>Gera o conteúdo de uma remessa a partir do JSON validado.</summary>
    /// <param name="entrada">Dados lidos do JSON.</param>
    /// <param name="codigoBanco">Código do banco informado em --bank.</param>
    /// <param name="layout">Layout informado em --layout (240 ou 400).</param>
    /// <param name="usarLf">Usar LF em vez de CRLF.</param>
    /// <returns>Conteúdo do arquivo CNAB.</returns>
    /// <exception cref="CliException">Dados de entrada incompletos ou inválidos.</exception>
    public static string Gerar(EntradaGeracao entrada, int codigoBanco, int layout, bool usarLf)
    {
        ArgumentNullException.ThrowIfNull(entrada);
        var banco = Bancos.Obter(codigoBanco);
        var cedente = MontarCedente(entrada.Cedente, banco);
        var titulos = MontarTitulos(entrada.Titulos);
        var opcoesArquivo = new OpcoesArquivo
        {
            Fim = usarLf ? OpcoesArquivo.FimDeLinha.Lf : OpcoesArquivo.FimDeLinha.CrLf,
        };

        if (entrada.DataGeracao is null)
        {
            throw new CliException("Campo obrigatório ausente: dataGeracao.");
        }

        var hora = InterpretarHora(entrada.HoraGeracao);
        var sequencial = entrada.NumeroSequencialArquivo ?? 1;
        if (sequencial is < 0 or > 999999)
        {
            throw new CliException("numeroSequencialArquivo deve ter no máximo 6 dígitos.");
        }

        return layout switch
        {
            240 => GeradorRemessa240.Gerar(new Remessa240
            {
                Banco = banco,
                Cedente = cedente,
                NumeroSequencialArquivo = sequencial,
                DataGeracao = entrada.DataGeracao.Value,
                HoraGeracao = hora,
                Convenio = entrada.Convenio,
                CodigoBeneficiario = entrada.CodigoBeneficiario,
                CodigoProduto = entrada.CodigoProduto,
                LotesCobranca =
                [
                    new LoteCobranca
                    {
                        Cedente = cedente,
                        Numero = 1,
                        Titulos = titulos,
                    },
                ],
                OpcoesArquivo = opcoesArquivo,
            }),
            400 => GeradorRemessa400.Gerar(new Remessa400
            {
                Banco = banco,
                Cedente = cedente,
                NumeroSequencialArquivo = sequencial,
                DataGeracao = entrada.DataGeracao.Value,
                NumeroRemessa = entrada.NumeroRemessa,
                CodigoBeneficiario = entrada.CodigoBeneficiario,
                CodigoProduto = entrada.CodigoProduto,
                Titulos = titulos,
                OpcoesArquivo = opcoesArquivo,
            }),
            _ => throw new CliException($"Layout {layout} não suportado em 'generate' (use 240 ou 400)."),
        };
    }

    private static Cedente MontarCedente(EntradaCedente? entrada, Banco banco)
    {
        if (entrada is null)
        {
            throw new CliException("Campo obrigatório ausente: cedente.");
        }

        var documento = Exigir(entrada.Documento, "cedente.documento");
        return new Cedente
        {
            Nome = Exigir(entrada.Nome, "cedente.nome"),
            Documento = new DocumentoFiscal(documento),
            Conta = new ContaBancaria
            {
                Banco = banco,
                Agencia = Exigir(entrada.Agencia, "cedente.agencia"),
                DvAgencia = entrada.DvAgencia ?? string.Empty,
                Conta = Exigir(entrada.Conta, "cedente.conta"),
                DvConta = entrada.DvConta ?? string.Empty,
                DvAgenciaConta = entrada.DvAgenciaConta ?? string.Empty,
            },
        };
    }

    private static IReadOnlyList<Titulo> MontarTitulos(IReadOnlyList<EntradaTitulo>? entrada)
    {
        if (entrada is null || entrada.Count == 0)
        {
            throw new CliException("Campo obrigatório ausente: titulos (ao menos um título).");
        }

        var titulos = new List<Titulo>(entrada.Count);
        for (var i = 0; i < entrada.Count; i++)
        {
            var atual = entrada[i];
            var prefixo = $"titulos[{i}]";
            var pagador = atual.Pagador ?? throw new CliException($"Campo obrigatório ausente: {prefixo}.pagador.");
            var endereco = pagador.Endereco;
            titulos.Add(new Titulo
            {
                NossoNumero = Exigir(atual.NossoNumero, $"{prefixo}.nossoNumero"),
                NumeroDocumento = Exigir(atual.NumeroDocumento, $"{prefixo}.numeroDocumento"),
                Especie = atual.Especie,
                DataEmissao = atual.DataEmissao ?? throw new CliException($"Campo obrigatório ausente: {prefixo}.dataEmissao."),
                DataVencimento = atual.DataVencimento,
                Valor = atual.Valor,
                Carteira = Exigir(atual.Carteira, $"{prefixo}.carteira"),
                Pagador = new Pagador
                {
                    Nome = Exigir(pagador.Nome, $"{prefixo}.pagador.nome"),
                    Documento = new DocumentoFiscal(Exigir(pagador.Documento, $"{prefixo}.pagador.documento")),
                    Endereco = endereco is null ? null : new Endereco
                    {
                        Logradouro = endereco.Logradouro,
                        Bairro = endereco.Bairro,
                        Cidade = endereco.Cidade,
                        Uf = endereco.Uf,
                        Cep = endereco.Cep,
                    },
                },
            });
        }

        return titulos;
    }

    private static TimeOnly InterpretarHora(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return new TimeOnly(0, 0, 0);
        }

        var digitos = texto.Replace(":", string.Empty, StringComparison.Ordinal);
        if (digitos.Length == 6
            && int.TryParse(digitos, NumberStyles.None, CultureInfo.InvariantCulture, out var hora)
            && hora is >= 0 and <= 235959)
        {
            var horas = hora / 10000;
            var minutos = (hora / 100) % 100;
            var segundos = hora % 100;
            if (minutos <= 59 && segundos <= 59)
            {
                return new TimeOnly(horas, minutos, segundos);
            }
        }

        throw new CliException($"horaGeracao inválida: '{texto}' (use HH:mm:ss ou HHmmss).");
    }

    private static string Exigir(string? valor, string nome) =>
        string.IsNullOrWhiteSpace(valor)
            ? throw new CliException($"Campo obrigatório ausente: {nome}.")
            : valor;
}
