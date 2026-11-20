namespace Cnab.Layouts.Cnab400;

using System.Globalization;
using Cnab.Arquivo;
using Cnab.Bancos;
using Cnab.Layouts.Cnab400.Registros;
using Cnab.Modelo;

/// <summary>
/// Geração de arquivos de remessa CNAB 400 de cobrança. Cada banco tem um
/// conjunto de registros declarativos próprios, com as posições extraídas do
/// manual oficial citado em <c>docs/layouts.md</c>. Nenhum campo é deixado em
/// branco por omissão: os valores fixos publicados são escritos explicitamente.
/// </summary>
public static class GeradorRemessa400
{
    /// <summary>Gera o conteúdo completo do arquivo, com delimitadores de linha.</summary>
    /// <param name="remessa">Dados da remessa.</param>
    /// <returns>Conteúdo do arquivo.</returns>
    public static string Gerar(Remessa400 remessa) =>
        EscritorDeArquivo.Montar(GerarLinhas(remessa), remessa.OpcoesArquivo);

    /// <summary>Gera as linhas de 400 posições, sem delimitadores.</summary>
    /// <param name="remessa">Dados da remessa.</param>
    /// <returns>Linhas na ordem header, títulos, trailer.</returns>
    public static IReadOnlyList<string> GerarLinhas(Remessa400 remessa)
    {
        ArgumentNullException.ThrowIfNull(remessa);
        Validar(remessa);
        return remessa.Banco.Codigo switch
        {
            341 => GerarItau(remessa),
            237 => GerarBradesco(remessa),
            748 => GerarSicredi(remessa),
            77 => GerarInter(remessa),
            _ => throw new ArgumentException(
                $"Banco {remessa.Banco} não tem remessa CNAB 400 implementada neste marco.", nameof(remessa)),
        };
    }

    private static IReadOnlyList<string> GerarItau(Remessa400 remessa)
    {
        var conta = remessa.Cedente.Conta;
        var agencia = Digitos(conta.Agencia, 4, "Agência");
        var numeroConta = Digitos(conta.Conta, 5, "Conta");
        var linhas = new List<string>
        {
            RegistroCnab<HeaderRemessaItau400>.Escrever(new HeaderRemessaItau400
            {
                Agencia = int.Parse(agencia, CultureInfo.InvariantCulture),
                Conta = int.Parse(numeroConta, CultureInfo.InvariantCulture),
                DacAgenciaConta = Itau.CalcularDacAgenciaConta(agencia, numeroConta),
                NomeEmpresa = remessa.Cedente.Nome,
                DataGeracao = remessa.DataGeracao,
                NumeroSequencial = 1,
            }),
        };

        var sequencial = 1;
        foreach (var titulo in remessa.Titulos)
        {
            titulo.Validar();
            sequencial++;
            var carteira = Digitos(titulo.Carteira, 3, "Carteira");
            var nossoNumero = Digitos(titulo.NossoNumero, 8, "Nosso Número");
            var endereco = titulo.Pagador.Endereco;
            linhas.Add(RegistroCnab<DetalheRemessaItau400>.Escrever(new DetalheRemessaItau400
            {
                TipoInscricaoEmpresa = remessa.Cedente.Documento.TipoInscricao,
                NumeroInscricaoEmpresa = remessa.Cedente.Documento.Digitos,
                Agencia = int.Parse(agencia, CultureInfo.InvariantCulture),
                Conta = int.Parse(numeroConta, CultureInfo.InvariantCulture),
                DacAgenciaConta = Itau.CalcularDacAgenciaConta(agencia, numeroConta),
                UsoEmpresa = titulo.NumeroDocumento,
                NossoNumero = nossoNumero,
                NumeroCarteira = int.Parse(carteira, CultureInfo.InvariantCulture),
                CodigoCarteira = CodigoCarteiraItau(carteira),
                CodigoOcorrencia = remessa.CodigoMovimentoEntrada,
                NumeroDocumento = titulo.NumeroDocumento,
                DataVencimento = titulo.DataVencimento,
                Valor = titulo.Valor,
                Especie = Codigos400.Especies[titulo.Especie].ToString("00", CultureInfo.InvariantCulture).PadLeft(2, '0'),
                Aceite = titulo.Aceite ? "A" : "N",
                DataEmissao = titulo.DataEmissao,
                Juros = titulo.Juros?.Valor ?? 0m,
                DataLimiteDesconto = titulo.Desconto?.DataLimite,
                Desconto = titulo.Desconto?.Valor ?? 0m,
                Abatimento = titulo.Abatimento ?? 0m,
                TipoInscricaoPagador = titulo.Pagador.Documento.TipoInscricao,
                NumeroInscricaoPagador = titulo.Pagador.Documento.Digitos,
                NomePagador = titulo.Pagador.Nome,
                LogradouroPagador = endereco?.Logradouro ?? string.Empty,
                BairroPagador = endereco?.Bairro ?? string.Empty,
                CepPagador = endereco is null ? 0 : int.Parse(Digitos(endereco.Cep, 8, "CEP"), CultureInfo.InvariantCulture),
                CidadePagador = endereco?.Cidade ?? string.Empty,
                UfPagador = endereco?.Uf ?? string.Empty,
                SacadorAvalista = titulo.SacadorAvalista?.Nome ?? string.Empty,
                PrazoDias = titulo.Protesto?.Dias ?? 0,
                NumeroSequencial = sequencial,
            }));

            if (titulo.Multa is not null)
            {
                sequencial++;
                linhas.Add(RegistroCnab<RegistroMultaRemessaItau400>.Escrever(new RegistroMultaRemessaItau400
                {
                    CodigoMulta = titulo.Multa.Tipo == TipoMulta.Valor ? 1 : 2,
                    DataMulta = titulo.Multa.DataInicio,
                    ValorMulta = titulo.Multa.Valor,
                    NumeroSequencial = sequencial,
                }));
            }
        }

        linhas.Add(RegistroCnab<TrailerRemessaItau400>.Escrever(new TrailerRemessaItau400
        {
            NumeroSequencial = sequencial + 1,
        }));
        return linhas;
    }

    private static IReadOnlyList<string> GerarBradesco(Remessa400 remessa)
    {
        var conta = remessa.Cedente.Conta;
        var linhas = new List<string>
        {
            RegistroCnab<HeaderRemessaBradesco400>.Escrever(new HeaderRemessaBradesco400
            {
                CodigoEmpresa = long.Parse(
                    DigitosSemDescarte(remessa.CodigoBeneficiario, 20, "Código da Empresa"),
                    CultureInfo.InvariantCulture),
                NomeEmpresa = remessa.Cedente.Nome,
                DataGravacao = remessa.DataGeracao,
                NumeroSequencialRemessa = remessa.NumeroRemessa,
                NumeroSequencial = 1,
            }),
        };

        var sequencial = 1;
        foreach (var titulo in remessa.Titulos)
        {
            titulo.Validar();
            sequencial++;
            var carteira = Digitos(titulo.Carteira, 2, "Carteira");
            var nossoNumero = Bradesco.ComporNossoNumero(carteira, Digitos(titulo.NossoNumero, 11, "Nosso Número"));
            var endereco = titulo.Pagador.Endereco;
            linhas.Add(RegistroCnab<DetalheRemessaBradesco400>.Escrever(new DetalheRemessaBradesco400
            {
                Carteira = int.Parse(carteira, CultureInfo.InvariantCulture),
                AgenciaBeneficiaria = int.Parse(Digitos(conta.Agencia, 5, "Agência"), CultureInfo.InvariantCulture),
                ContaCorrente = int.Parse(Digitos(conta.Conta, 7, "Conta"), CultureInfo.InvariantCulture),
                DvConta = int.Parse(Digito(conta.DvConta), CultureInfo.InvariantCulture),
                ControleParticipante = titulo.NumeroDocumento,
                IdentificativoMulta = titulo.Multa is null ? 0 : 2,
                PercentualMulta = titulo.Multa is { Tipo: TipoMulta.Percentual } multa ? multa.Valor : 0m,
                NossoNumero = nossoNumero,
                CodigoOcorrencia = remessa.CodigoMovimentoEntrada,
                NumeroDocumento = titulo.NumeroDocumento,
                DataVencimento = titulo.DataVencimento,
                Valor = titulo.Valor,
                Especie = Codigos400.Especies[titulo.Especie],
                DataEmissao = titulo.DataEmissao,
                Instrucao1 = titulo.Protesto is not null ? 6 : 0,
                ValorMoraDia = titulo.Juros?.Valor ?? 0m,
                DataLimiteDesconto = titulo.Desconto?.DataLimite,
                Desconto = titulo.Desconto?.Valor ?? 0m,
                Abatimento = titulo.Abatimento ?? 0m,
                TipoInscricaoPagador = titulo.Pagador.Documento.TipoInscricao,
                NumeroInscricaoPagador = titulo.Pagador.Documento.Digitos,
                NomePagador = titulo.Pagador.Nome,
                EnderecoPagador = endereco is null ? string.Empty : $"{endereco.Logradouro}, {endereco.Bairro}",
                CepPagador = endereco?.Cep ?? string.Empty,
                NomeCedente = remessa.Cedente.Nome,
                CnpjCedente = remessa.Cedente.Documento.Digitos,
                NumeroSequencial = sequencial,
            }));

            if (titulo.Instrucoes.Count > 0)
            {
                sequencial++;
                linhas.Add(RegistroCnab<MensagemRemessaBradesco400>.Escrever(new MensagemRemessaBradesco400
                {
                    Mensagem1 = titulo.Instrucoes.Count > 0 ? titulo.Instrucoes[0] : string.Empty,
                    Mensagem2 = titulo.Instrucoes.Count > 1 ? titulo.Instrucoes[1] : string.Empty,
                    Mensagem3 = titulo.Instrucoes.Count > 2 ? titulo.Instrucoes[2] : string.Empty,
                    Mensagem4 = titulo.Instrucoes.Count > 3 ? titulo.Instrucoes[3] : string.Empty,
                    Carteira = carteira,
                    Agencia = Digitos(conta.Agencia, 5, "Agência"),
                    Conta = Digitos(conta.Conta, 7, "Conta"),
                    DvConta = Digito(conta.DvConta),
                    NossoNumero = novoNumeroSemDv(nossoNumero, 11),
                    DvNossoNumero = novoNumeroDv(nossoNumero),
                    NumeroSequencial = sequencial,
                }));
            }
        }

        linhas.Add(RegistroCnab<TrailerRemessaBradesco400>.Escrever(new TrailerRemessaBradesco400
        {
            NumeroSequencial = sequencial + 1,
        }));
        return linhas;
    }

    private static IReadOnlyList<string> GerarSicredi(Remessa400 remessa)
    {
        var linhas = new List<string>
        {
            RegistroCnab<HeaderRemessaSicredi400>.Escrever(new HeaderRemessaSicredi400
            {
                CodigoBeneficiario = Digitos(remessa.CodigoBeneficiario, 5, "Código do Beneficiário"),
                DocumentoBeneficiario = remessa.Cedente.Documento.Digitos,
                DataGeracao = DataAnsi(remessa.DataGeracao),
                NumeroRemessa = remessa.NumeroRemessa,
                NumeroSequencial = 1,
            }),
        };

        var sequencial = 1;
        foreach (var titulo in remessa.Titulos)
        {
            titulo.Validar();
            sequencial++;
            var nossoNumero = DigitosAlfa(titulo.NossoNumero, 9, "Nosso Número");
            var endereco = titulo.Pagador.Endereco;
            var protesto = titulo.Protesto is not null
                && titulo.Protesto.Tipo is TipoProtesto.ProtestarDiasCorridos or TipoProtesto.NegativarDiasCorridos;
            linhas.Add(RegistroCnab<DetalheRemessaSicredi400>.Escrever(new DetalheRemessaSicredi400
            {
                DataInicioJuros = titulo.Juros is null ? Zeros(8) : DataAnsi(titulo.Juros.DataInicio),
                DataInicioMulta = titulo.Multa is null ? Zeros(8) : DataAnsi(titulo.Multa.DataInicio),
                NossoNumero = nossoNumero,
                DataInstrucao = DataAnsi(remessa.DataGeracao),
                TipoJuros = titulo.Juros?.Tipo is TipoJuros.ValorDiario ? "A" : "B",
                TipoMulta = titulo.Multa?.Tipo is TipoMulta.Valor ? "A" : "B",
                DescontoAntecipacao = 0m,
                MultaPercentual = titulo.Multa is { Tipo: TipoMulta.Percentual } multa ? multa.Valor : 0m,
                MultaValor = titulo.Multa is { Tipo: TipoMulta.Valor } multaValor ? multaValor.Valor : 0m,
                Instrucao = remessa.CodigoMovimentoEntrada,
                SeuNumero = titulo.NumeroDocumento,
                DataVencimento = titulo.DataVencimento,
                Valor = titulo.Valor,
                Especie = "A",
                Aceite = titulo.Aceite ? "S" : "N",
                DataEmissao = titulo.DataEmissao,
                InstrucaoProtesto = protesto ? 6 : 0,
                DiasProtesto = protesto ? titulo.Protesto!.Dias : 0,
                JurosDia = titulo.Juros?.Valor ?? 0m,
                DataLimiteDesconto = titulo.Desconto?.DataLimite,
                Desconto = titulo.Desconto?.Valor ?? 0m,
                InstrucaoNegativacao = titulo.Protesto?.Tipo is TipoProtesto.NegativarDiasCorridos ? 6 : 0,
                DiasNegativacao = titulo.Protesto?.Tipo is TipoProtesto.NegativarDiasCorridos ? titulo.Protesto.Dias : 0,
                Abatimento = titulo.Abatimento ?? 0m,
                TipoInscricaoPagador = titulo.Pagador.Documento.TipoInscricao,
                DocumentoPagador = titulo.Pagador.Documento.Digitos,
                NomePagador = titulo.Pagador.Nome,
                EnderecoPagador = endereco is null ? string.Empty : $"{endereco.Logradouro} {endereco.Bairro}",
                CepPagador = endereco?.Cep ?? string.Empty,
                CodigoPagadorCooperativa = Zeros(5),
                CodigoPagadorCliente = Zeros(5),
                DocumentoBeneficiarioFinal = titulo.SacadorAvalista?.Documento.Digitos ?? string.Empty,
                NomeBeneficiarioFinal = titulo.SacadorAvalista?.Nome ?? string.Empty,
                NumeroSequencial = sequencial,
            }));
        }

        linhas.Add(RegistroCnab<TrailerRemessaSicredi400>.Escrever(new TrailerRemessaSicredi400
        {
            CodigoBeneficiario = Digitos(remessa.CodigoBeneficiario, 5, "Código do Beneficiário"),
            NumeroSequencial = sequencial + 1,
        }));
        return linhas;
    }

    private static IReadOnlyList<string> GerarInter(Remessa400 remessa)
    {
        // O layout de remessa CNAB 400 do Inter é implementado em
        // RegistrosInter400; o manual oficial é citado em docs/layouts.md.
        return RegistrosInter400.Gerar(remessa);
    }

    private static string CodigoCarteiraItau(string carteira) => carteira switch
    {
        "108" or "180" or "121" or "109" or "104" or "188" or "112" or "115" => "I",
        "150" or "147" => "E",
        _ => throw new ArgumentException(
            $"Carteira Itaú '{carteira}' sem código de carteira publicado na NOTA 5 do manual.", nameof(carteira)),
    };

    private static string novoNumeroSemDv(string nossoNumeroComDv, int tamanho) =>
        novoNumeroDv(nossoNumeroComDv).Length == 0
            ? nossoNumeroComDv
            : nossoNumeroComDv[..^1].PadRight(tamanho, '0');

    private static string novoNumeroDv(string nossoNumeroComDv) =>
        nossoNumeroComDv.Length > 0 && char.IsLetterOrDigit(nossoNumeroComDv[^1])
            ? nossoNumeroComDv[^1..]
            : string.Empty;

    private static string Digito(string? valor) => string.IsNullOrWhiteSpace(valor) ? "0" : valor[..1];

    /// <summary>
    /// Igual a <see cref="Digitos"/>, mas rejeita qualquer caractere não
    /// numérico em vez de descartá-lo. Usado no código da empresa Bradesco:
    /// remover letras silenciosamente enviaria um código diferente do
    /// informado (por exemplo "ABC12" viraria "...0012").
    /// </summary>
    private static string DigitosSemDescarte(string valor, int tamanho, string nome)
    {
        if (valor.Any(c => !char.IsAsciiDigit(c)))
        {
            throw new ArgumentException(
                $"{nome} deve conter apenas dígitos; recebido '{valor}'.", nome);
        }

        if (valor.Length > tamanho)
        {
            throw new ArgumentException($"{nome} excede {tamanho} dígitos.", nome);
        }

        return valor.PadLeft(tamanho, '0');
    }

    private static string Digitos(string valor, int tamanho, string nome)
    {
        var digitos = new string([.. valor.Where(char.IsAsciiDigit)]);
        if (digitos.Length > tamanho)
        {
            throw new ArgumentException($"{nome} excede {tamanho} dígitos.", nome);
        }

        return digitos.PadLeft(tamanho, '0');
    }

    private static string DigitosAlfa(string valor, int tamanho, string nome)
    {
        var resultado = new string([.. valor.Where(char.IsLetterOrDigit)]).ToUpperInvariant();
        if (resultado.Length > tamanho)
        {
            throw new ArgumentException($"{nome} excede {tamanho} posições alfanuméricas.", nome);
        }

        return resultado.PadLeft(tamanho, '0');
    }

    private static string Zeros(int tamanho) => new('0', tamanho);

    private static string DataAnsi(DateOnly data) => data.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

    private static void Validar(Remessa400 remessa)
    {
        if (remessa.NumeroSequencialArquivo is < 0 or > 999999)
        {
            throw new ArgumentOutOfRangeException(nameof(remessa), "Número sequencial do arquivo deve ter até 6 dígitos.");
        }

        if (remessa.Titulos.Count == 0)
        {
            throw new ArgumentException("A remessa CNAB 400 precisa de pelo menos um título.", nameof(remessa));
        }

        remessa.Cedente.Conta.Validar();
    }
}
