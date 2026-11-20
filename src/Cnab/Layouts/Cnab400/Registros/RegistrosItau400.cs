namespace Cnab.Layouts.Cnab400.Registros;

using Cnab.Arquivo;

/// <summary>
/// Header de remessa CNAB 400 do Itaú (registro 0). Manual "Cobrança Bancária
/// — Layout de Arquivos — CNAB 400", janeiro/2017, seção "ARQUIVO REMESSA —
/// REGISTRO HEADER". Fonte: https://download.itau.com.br/bankline/layout_cobranca_400bytes_cnab_itau.pdf
/// </summary>
[RegistroCnab(Codigos400.TamanhoRegistro)]
public sealed record HeaderRemessaItau400
{
    /// <summary>Identificação do registro, posição 1: "0".</summary>
    [CampoCnab("Identificação do Registro", 1, 1, TipoCampo.Numerico)]
    public int IdentificacaoRegistro { get; init; }

    /// <summary>Operação remessa, posição 2: "1".</summary>
    [CampoCnab("Identificação do Arquivo-Remessa", 2, 1, TipoCampo.Numerico)]
    public int OperacaoRemessa { get; init; } = 1;

    /// <summary>Literal "REMESSA", posições 3-9.</summary>
    [CampoCnab("Literal Remessa", 3, 7, TipoCampo.Alfanumerico)]
    public string LiteralRemessa { get; init; } = Codigos400.LiteralRemessa;

    /// <summary>Código do serviço, posições 10-11: "01".</summary>
    [CampoCnab("Código do Serviço", 10, 2, TipoCampo.Numerico)]
    public int CodigoServico { get; init; } = 1;

    /// <summary>Literal "COBRANCA", posições 12-26.</summary>
    [CampoCnab("Literal Serviço", 12, 15, TipoCampo.Alfanumerico)]
    public string LiteralServico { get; init; } = Codigos400.LiteralCobranca;

    /// <summary>Agência da empresa, posições 27-30.</summary>
    [CampoCnab("Agência", 27, 4, TipoCampo.Numerico)]
    public int Agencia { get; init; }

    /// <summary>Posições 31-32, fixo "00".</summary>
    [CampoCnab("Zeros 31-32", 31, 2, TipoCampo.Filler, ConteudoFiller = "0")]
    public string Zeros31A32 { get; init; } = "00";

    /// <summary>Conta corrente da empresa, posições 33-37.</summary>
    [CampoCnab("Conta", 33, 5, TipoCampo.Numerico)]
    public int Conta { get; init; }

    /// <summary>DAC da agência/conta, posição 38.</summary>
    [CampoCnab("DAC Agência/Conta", 38, 1, TipoCampo.Numerico)]
    public int DacAgenciaConta { get; init; }

    /// <summary>Complemento, posições 39-46, brancos.</summary>
    [CampoCnab("Complemento 39-46", 39, 8, TipoCampo.Filler)]
    public string Complemento { get; init; } = string.Empty;

    /// <summary>Nome da empresa "mãe", posições 47-76.</summary>
    [CampoCnab("Nome da Empresa", 47, 30, TipoCampo.Alfanumerico)]
    public string NomeEmpresa { get; init; } = string.Empty;

    /// <summary>Código do banco, posições 77-79: "341".</summary>
    [CampoCnab("Código do Banco", 77, 3, TipoCampo.Numerico)]
    public int Banco { get; init; } = 341;

    /// <summary>Nome do banco, posições 80-94: "BANCO ITAU SA".</summary>
    [CampoCnab("Nome do Banco", 80, 15, TipoCampo.Alfanumerico)]
    public string NomeBanco { get; init; } = "BANCO ITAU SA";

    /// <summary>Data de geração, posições 95-100 (DDMMAA).</summary>
    [CampoCnab("Data de Geração", 95, 6, TipoCampo.DataDdMmAa)]
    public DateOnly DataGeracao { get; init; }

    /// <summary>Complemento, posições 101-394, brancos.</summary>
    [CampoCnab("Complemento 101-394", 101, 294, TipoCampo.Filler)]
    public string ComplementoFinal { get; init; } = string.Empty;

    /// <summary>Número sequencial do registro no arquivo, posições 395-400.</summary>
    [CampoCnab("Número Sequencial do Registro", 395, 6, TipoCampo.Numerico)]
    public int NumeroSequencial { get; init; }
}

/// <summary>
/// Registro de emissão de boleto CNAB 400 do Itaú (registro 1). Manual
/// janeiro/2017, seção "ARQUIVO REMESSA — REGISTRO EMISSÃO DE BOLETO".
/// </summary>
[RegistroCnab(Codigos400.TamanhoRegistro)]
public sealed record DetalheRemessaItau400
{
    /// <summary>Identificação do registro, posição 1: "1".</summary>
    [CampoCnab("Identificação do Registro", 1, 1, TipoCampo.Numerico)]
    public int IdentificacaoRegistro { get; init; } = 1;

    /// <summary>Tipo de inscrição da empresa, posições 2-3.</summary>
    [CampoCnab("Tipo de Inscrição da Empresa", 2, 2, TipoCampo.Numerico)]
    public int TipoInscricaoEmpresa { get; init; }

    /// <summary>Número de inscrição da empresa, posições 4-17.</summary>
    [CampoCnab("Número de Inscrição da Empresa", 4, 14, TipoCampo.Alfanumerico)]
    public string NumeroInscricaoEmpresa { get; init; } = string.Empty;

    /// <summary>Agência da empresa, posições 18-21.</summary>
    [CampoCnab("Agência", 18, 4, TipoCampo.Numerico)]
    public int Agencia { get; init; }

    /// <summary>Posições 22-23, fixo "00".</summary>
    [CampoCnab("Zeros 22-23", 22, 2, TipoCampo.Filler, ConteudoFiller = "0")]
    public string Zeros22A23 { get; init; } = "00";

    /// <summary>Conta corrente da empresa, posições 24-28.</summary>
    [CampoCnab("Conta", 24, 5, TipoCampo.Numerico)]
    public int Conta { get; init; }

    /// <summary>DAC da agência/conta, posição 29.</summary>
    [CampoCnab("DAC Agência/Conta", 29, 1, TipoCampo.Numerico)]
    public int DacAgenciaConta { get; init; }

    /// <summary>Complemento, posições 30-33, brancos.</summary>
    [CampoCnab("Complemento 30-33", 30, 4, TipoCampo.Filler)]
    public string ComplementoInicial { get; init; } = string.Empty;

    /// <summary>Instrução/alegação a cancelar, posições 34-37.</summary>
    [CampoCnab("Instrução a Cancelar", 34, 4, TipoCampo.Numerico)]
    public int InstrucaoCancelar { get; init; }

    /// <summary>Uso da empresa ("seu número"), posições 38-62.</summary>
    [CampoCnab("Uso da Empresa", 38, 25, TipoCampo.Alfanumerico)]
    public string UsoEmpresa { get; init; } = string.Empty;

    /// <summary>Nosso número (8 dígitos), posições 63-70.</summary>
    [CampoCnab("Nosso Número", 63, 8, TipoCampo.Alfanumerico)]
    public string NossoNumero { get; init; } = string.Empty;

    /// <summary>Quantidade de moeda variável, posições 71-83 (5 casas decimais).</summary>
    [CampoCnab("Quantidade de Moeda", 71, 13, TipoCampo.NumericoDecimal, decimais: 5)]
    public decimal QuantidadeMoeda { get; init; }

    /// <summary>Número da carteira, posições 84-86.</summary>
    [CampoCnab("Número da Carteira", 84, 3, TipoCampo.Numerico)]
    public int NumeroCarteira { get; init; }

    /// <summary>Uso do banco, posições 87-107, brancos.</summary>
    [CampoCnab("Uso do Banco", 87, 21, TipoCampo.Filler)]
    public string UsoBanco { get; init; } = string.Empty;

    /// <summary>Código da carteira (I/U/D/E), posição 108.</summary>
    [CampoCnab("Código da Carteira", 108, 1, TipoCampo.Alfanumerico)]
    public string CodigoCarteira { get; init; } = string.Empty;

    /// <summary>Código de ocorrência de remessa, posições 109-110.</summary>
    [CampoCnab("Código de Ocorrência", 109, 2, TipoCampo.Numerico)]
    public int CodigoOcorrencia { get; init; }

    /// <summary>Número do documento de cobrança, posições 111-120.</summary>
    [CampoCnab("Número do Documento", 111, 10, TipoCampo.Alfanumerico)]
    public string NumeroDocumento { get; init; } = string.Empty;

    /// <summary>Data de vencimento, posições 121-126 (DDMMAA).</summary>
    [CampoCnab("Data de Vencimento", 121, 6, TipoCampo.DataDdMmAa)]
    public DateOnly? DataVencimento { get; init; }

    /// <summary>Valor nominal do título, posições 127-139 (2 casas decimais).</summary>
    [CampoCnab("Valor do Título", 127, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal Valor { get; init; }

    /// <summary>Código do banco (341), posições 140-142.</summary>
    [CampoCnab("Código do Banco", 140, 3, TipoCampo.Numerico)]
    public int Banco { get; init; } = 341;

    /// <summary>Agência cobradora, posições 143-147 (zeros na remessa).</summary>
    [CampoCnab("Agência Cobradora", 143, 5, TipoCampo.Numerico)]
    public int AgenciaCobradora { get; init; }

    /// <summary>Espécie do título, posições 148-149.</summary>
    [CampoCnab("Espécie do Título", 148, 2, TipoCampo.Alfanumerico)]
    public string Especie { get; init; } = string.Empty;

    /// <summary>Aceite ("A"/"N"), posição 150.</summary>
    [CampoCnab("Aceite", 150, 1, TipoCampo.Alfanumerico)]
    public string Aceite { get; init; } = "N";

    /// <summary>Data de emissão, posições 151-156 (DDMMAA).</summary>
    [CampoCnab("Data de Emissão", 151, 6, TipoCampo.DataDdMmAa)]
    public DateOnly DataEmissao { get; init; }

    /// <summary>Primeira instrução, posições 157-158.</summary>
    [CampoCnab("Instrução 1", 157, 2, TipoCampo.Alfanumerico)]
    public string Instrucao1 { get; init; } = string.Empty;

    /// <summary>Segunda instrução, posições 159-160.</summary>
    [CampoCnab("Instrução 2", 159, 2, TipoCampo.Alfanumerico)]
    public string Instrucao2 { get; init; } = string.Empty;

    /// <summary>Juros de um dia, posições 161-173 (2 casas decimais).</summary>
    [CampoCnab("Juros de Mora", 161, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal Juros { get; init; }

    /// <summary>Data limite do desconto, posições 174-179 (DDMMAA).</summary>
    [CampoCnab("Data Limite do Desconto", 174, 6, TipoCampo.DataDdMmAa)]
    public DateOnly? DataLimiteDesconto { get; init; }

    /// <summary>Valor do desconto, posições 180-192 (2 casas decimais).</summary>
    [CampoCnab("Valor do Desconto", 180, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal Desconto { get; init; }

    /// <summary>Valor do IOF, posições 193-205 (2 casas decimais).</summary>
    [CampoCnab("Valor do IOF", 193, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal Iof { get; init; }

    /// <summary>Valor do abatimento, posições 206-218 (2 casas decimais).</summary>
    [CampoCnab("Valor do Abatimento", 206, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal Abatimento { get; init; }

    /// <summary>Tipo de inscrição do pagador, posições 219-220.</summary>
    [CampoCnab("Tipo de Inscrição do Pagador", 219, 2, TipoCampo.Numerico)]
    public int TipoInscricaoPagador { get; init; }

    /// <summary>Número de inscrição do pagador, posições 221-234.</summary>
    [CampoCnab("Número de Inscrição do Pagador", 221, 14, TipoCampo.Alfanumerico)]
    public string NumeroInscricaoPagador { get; init; } = string.Empty;

    /// <summary>Nome do pagador, posições 235-264.</summary>
    [CampoCnab("Nome do Pagador", 235, 30, TipoCampo.Alfanumerico)]
    public string NomePagador { get; init; } = string.Empty;

    /// <summary>Complemento, posições 265-274, brancos.</summary>
    [CampoCnab("Complemento 265-274", 265, 10, TipoCampo.Filler)]
    public string ComplementoNome { get; init; } = string.Empty;

    /// <summary>Logradouro do pagador, posições 275-314.</summary>
    [CampoCnab("Logradouro do Pagador", 275, 40, TipoCampo.Alfanumerico)]
    public string LogradouroPagador { get; init; } = string.Empty;

    /// <summary>Bairro do pagador, posições 315-326.</summary>
    [CampoCnab("Bairro do Pagador", 315, 12, TipoCampo.Alfanumerico)]
    public string BairroPagador { get; init; } = string.Empty;

    /// <summary>CEP do pagador, posições 327-334.</summary>
    [CampoCnab("CEP do Pagador", 327, 8, TipoCampo.Numerico)]
    public int CepPagador { get; init; }

    /// <summary>Cidade do pagador, posições 335-349.</summary>
    [CampoCnab("Cidade do Pagador", 335, 15, TipoCampo.Alfanumerico)]
    public string CidadePagador { get; init; } = string.Empty;

    /// <summary>UF do pagador, posições 350-351.</summary>
    [CampoCnab("UF do Pagador", 350, 2, TipoCampo.Alfanumerico)]
    public string UfPagador { get; init; } = string.Empty;

    /// <summary>Sacador/avalista, posições 352-381.</summary>
    [CampoCnab("Sacador/Avalista", 352, 30, TipoCampo.Alfanumerico)]
    public string SacadorAvalista { get; init; } = string.Empty;

    /// <summary>Complemento, posições 382-385, brancos.</summary>
    [CampoCnab("Complemento 382-385", 382, 4, TipoCampo.Filler)]
    public string ComplementoAvalista { get; init; } = string.Empty;

    /// <summary>Data de mora, posições 386-391 (DDMMAA).</summary>
    [CampoCnab("Data de Mora", 386, 6, TipoCampo.DataDdMmAa)]
    public DateOnly? DataMora { get; init; }

    /// <summary>Prazo/quantidade de dias, posições 392-393.</summary>
    [CampoCnab("Prazo de Dias", 392, 2, TipoCampo.Numerico)]
    public int PrazoDias { get; init; }

    /// <summary>Complemento, posição 394, branco.</summary>
    [CampoCnab("Complemento 394", 394, 1, TipoCampo.Filler)]
    public string Complemento394 { get; init; } = string.Empty;

    /// <summary>Número sequencial do registro no arquivo, posições 395-400.</summary>
    [CampoCnab("Número Sequencial do Registro", 395, 6, TipoCampo.Numerico)]
    public int NumeroSequencial { get; init; }
}

/// <summary>
/// Registro 2 (complemento de multa) da remessa CNAB 400 do Itaú. Manual
/// janeiro/2017, seção "REGISTRO EMISSÃO DE BOLETO — REGISTRO 2".
/// </summary>
[RegistroCnab(Codigos400.TamanhoRegistro)]
public sealed record RegistroMultaRemessaItau400
{
    /// <summary>Identificação do registro, posição 1: "2".</summary>
    [CampoCnab("Identificação do Registro", 1, 1, TipoCampo.Numerico)]
    public int IdentificacaoRegistro { get; init; } = 2;

    /// <summary>Código da multa, posição 2: 0, 1 (valor) ou 2 (percentual).</summary>
    [CampoCnab("Código da Multa", 2, 1, TipoCampo.Numerico)]
    public int CodigoMulta { get; init; }

    /// <summary>Data da multa, posições 3-10 (DDMMAAAA).</summary>
    [CampoCnab("Data da Multa", 3, 8, TipoCampo.DataDdMmAaaa)]
    public DateOnly DataMulta { get; init; }

    /// <summary>Valor ou percentual da multa, posições 11-23 (2 casas decimais).</summary>
    [CampoCnab("Valor da Multa", 11, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorMulta { get; init; }

    /// <summary>Complemento, posições 24-394, brancos.</summary>
    [CampoCnab("Complemento 24-394", 24, 371, TipoCampo.Filler)]
    public string Complemento { get; init; } = string.Empty;

    /// <summary>Número sequencial do registro no arquivo, posições 395-400.</summary>
    [CampoCnab("Número Sequencial do Registro", 395, 6, TipoCampo.Numerico)]
    public int NumeroSequencial { get; init; }
}

/// <summary>Trailer de remessa CNAB 400 do Itaú: sem totalizadores.</summary>
[RegistroCnab(Codigos400.TamanhoRegistro)]
public sealed record TrailerRemessaItau400
{
    /// <summary>Identificação do registro, posição 1: "9".</summary>
    [CampoCnab("Identificação do Registro", 1, 1, TipoCampo.Numerico)]
    public int IdentificacaoRegistro { get; init; } = 9;

    /// <summary>Complemento, posições 2-394, brancos.</summary>
    [CampoCnab("Complemento 2-394", 2, 393, TipoCampo.Filler)]
    public string Complemento { get; init; } = string.Empty;

    /// <summary>Número sequencial do registro no arquivo, posições 395-400.</summary>
    [CampoCnab("Número Sequencial do Registro", 395, 6, TipoCampo.Numerico)]
    public int NumeroSequencial { get; init; }
}

/// <summary>Header de retorno CNAB 400 do Itaú. Manual janeiro/2017, seção "ARQUIVO RETORNO".</summary>
[RegistroCnab(Codigos400.TamanhoRegistro)]
public sealed record HeaderRetornoItau400
{
    /// <summary>Identificação do registro, posição 1: "0".</summary>
    [CampoCnab("Identificação do Registro", 1, 1, TipoCampo.Numerico)]
    public int IdentificacaoRegistro { get; init; }

    /// <summary>Código de retorno, posição 2: "2".</summary>
    [CampoCnab("Código de Retorno", 2, 1, TipoCampo.Numerico)]
    public int CodigoRetorno { get; init; } = 2;

    /// <summary>Literal "RETORNO", posições 3-9.</summary>
    [CampoCnab("Literal Retorno", 3, 7, TipoCampo.Alfanumerico)]
    public string LiteralRetorno { get; init; } = Codigos400.LiteralRetorno;

    /// <summary>Código do serviço, posições 10-11: "01".</summary>
    [CampoCnab("Código do Serviço", 10, 2, TipoCampo.Numerico)]
    public int CodigoServico { get; init; } = 1;

    /// <summary>Literal "COBRANCA", posições 12-26.</summary>
    [CampoCnab("Literal Serviço", 12, 15, TipoCampo.Alfanumerico)]
    public string LiteralServico { get; init; } = Codigos400.LiteralCobranca;

    /// <summary>Agência, posições 27-30.</summary>
    [CampoCnab("Agência", 27, 4, TipoCampo.Numerico)]
    public int Agencia { get; init; }

    /// <summary>Posições 31-32, fixo "00".</summary>
    [CampoCnab("Zeros 31-32", 31, 2, TipoCampo.Filler, ConteudoFiller = "0")]
    public string Zeros31A32 { get; init; } = "00";

    /// <summary>Conta, posições 33-37.</summary>
    [CampoCnab("Conta", 33, 5, TipoCampo.Numerico)]
    public int Conta { get; init; }

    /// <summary>DAC da agência/conta, posição 38.</summary>
    [CampoCnab("DAC Agência/Conta", 38, 1, TipoCampo.Numerico)]
    public int DacAgenciaConta { get; init; }

    /// <summary>Complemento, posições 39-46, brancos.</summary>
    [CampoCnab("Complemento 39-46", 39, 8, TipoCampo.Filler)]
    public string Complemento { get; init; } = string.Empty;

    /// <summary>Nome da empresa, posições 47-76.</summary>
    [CampoCnab("Nome da Empresa", 47, 30, TipoCampo.Alfanumerico)]
    public string NomeEmpresa { get; init; } = string.Empty;

    /// <summary>Código do banco, posições 77-79: "341".</summary>
    [CampoCnab("Código do Banco", 77, 3, TipoCampo.Numerico)]
    public int Banco { get; init; } = 341;

    /// <summary>Nome do banco, posições 80-94.</summary>
    [CampoCnab("Nome do Banco", 80, 15, TipoCampo.Alfanumerico)]
    public string NomeBanco { get; init; } = "BANCO ITAU SA";

    /// <summary>Data de geração, posições 95-100 (DDMMAA).</summary>
    [CampoCnab("Data de Geração", 95, 6, TipoCampo.DataDdMmAa)]
    public DateOnly DataGeracao { get; init; }

    /// <summary>Densidade de gravação, posições 101-105.</summary>
    [CampoCnab("Densidade de Gravação", 101, 5, TipoCampo.Numerico)]
    public int Densidade { get; init; }

    /// <summary>Literal "BPI", posições 106-108.</summary>
    [CampoCnab("Unidade de Densidade", 106, 3, TipoCampo.Alfanumerico)]
    public string UnidadeDensidade { get; init; } = "BPI";

    /// <summary>Número sequencial do arquivo retorno, posições 109-113.</summary>
    [CampoCnab("Número Sequencial do Retorno", 109, 5, TipoCampo.Numerico)]
    public int NumeroSequencialRetorno { get; init; }

    /// <summary>Data de crédito dos lançamentos, posições 114-119 (DDMMAA).</summary>
    [CampoCnab("Data de Crédito", 114, 6, TipoCampo.DataDdMmAa)]
    public DateOnly DataCredito { get; init; }

    /// <summary>Complemento, posições 120-394, brancos.</summary>
    [CampoCnab("Complemento 120-394", 120, 275, TipoCampo.Filler)]
    public string ComplementoFinal { get; init; } = string.Empty;

    /// <summary>Número sequencial do registro, posições 395-400.</summary>
    [CampoCnab("Número Sequencial do Registro", 395, 6, TipoCampo.Numerico)]
    public int NumeroSequencial { get; init; }
}

/// <summary>
/// Detalhe de retorno CNAB 400 do Itaú (registro 1). Manual janeiro/2017,
/// seção "ARQUIVO RETORNO — REGISTRO DE TRANSAÇÃO". Motivos de rejeição nas
/// posições 378-385 (diferente da convenção Bradesco em 319-328).
/// </summary>
[RegistroCnab(Codigos400.TamanhoRegistro)]
public sealed record DetalheRetornoItau400
{
    /// <summary>Identificação do registro, posição 1: "1".</summary>
    [CampoCnab("Identificação do Registro", 1, 1, TipoCampo.Numerico)]
    public int IdentificacaoRegistro { get; init; } = 1;

    /// <summary>Tipo de inscrição da empresa, posições 2-3.</summary>
    [CampoCnab("Tipo de Inscrição da Empresa", 2, 2, TipoCampo.Numerico)]
    public int TipoInscricaoEmpresa { get; init; }

    /// <summary>Número de inscrição da empresa, posições 4-17.</summary>
    [CampoCnab("Número de Inscrição da Empresa", 4, 14, TipoCampo.Alfanumerico)]
    public string NumeroInscricaoEmpresa { get; init; } = string.Empty;

    /// <summary>Agência, posições 18-21.</summary>
    [CampoCnab("Agência", 18, 4, TipoCampo.Numerico)]
    public int Agencia { get; init; }

    /// <summary>Posições 22-23, fixo "00".</summary>
    [CampoCnab("Zeros 22-23", 22, 2, TipoCampo.Filler, ConteudoFiller = "0")]
    public string Zeros22A23 { get; init; } = "00";

    /// <summary>Conta, posições 24-28.</summary>
    [CampoCnab("Conta", 24, 5, TipoCampo.Numerico)]
    public int Conta { get; init; }

    /// <summary>DAC da agência/conta, posição 29.</summary>
    [CampoCnab("DAC Agência/Conta", 29, 1, TipoCampo.Numerico)]
    public int DacAgenciaConta { get; init; }

    /// <summary>Complemento, posições 30-37, brancos.</summary>
    [CampoCnab("Complemento 30-37", 30, 8, TipoCampo.Filler)]
    public string ComplementoInicial { get; init; } = string.Empty;

    /// <summary>Uso da empresa ("seu número"), posições 38-62.</summary>
    [CampoCnab("Uso da Empresa", 38, 25, TipoCampo.Alfanumerico)]
    public string UsoEmpresa { get; init; } = string.Empty;

    /// <summary>Nosso número, posições 63-70.</summary>
    [CampoCnab("Nosso Número", 63, 8, TipoCampo.Alfanumerico)]
    public string NossoNumero { get; init; } = string.Empty;

    /// <summary>Complemento, posições 71-82, brancos.</summary>
    [CampoCnab("Complemento 71-82", 71, 12, TipoCampo.Filler)]
    public string Complemento71A82 { get; init; } = string.Empty;

    /// <summary>Número da carteira, posições 83-85.</summary>
    [CampoCnab("Número da Carteira", 83, 3, TipoCampo.Numerico)]
    public int NumeroCarteira { get; init; }

    /// <summary>Nosso número (confirmação), posições 86-93.</summary>
    [CampoCnab("Nosso Número Confirmado", 86, 8, TipoCampo.Alfanumerico)]
    public string NossoNumeroConfirmado { get; init; } = string.Empty;

    /// <summary>DAC do nosso número, posição 94.</summary>
    [CampoCnab("DAC do Nosso Número", 94, 1, TipoCampo.Numerico)]
    public int DacNossoNumero { get; init; }

    /// <summary>Complemento, posições 95-107, brancos.</summary>
    [CampoCnab("Complemento 95-107", 95, 13, TipoCampo.Filler)]
    public string Complemento95A107 { get; init; } = string.Empty;

    /// <summary>Código da carteira, posição 108.</summary>
    [CampoCnab("Código da Carteira", 108, 1, TipoCampo.Alfanumerico)]
    public string CodigoCarteira { get; init; } = string.Empty;

    /// <summary>Código de ocorrência, posições 109-110.</summary>
    [CampoCnab("Código de Ocorrência", 109, 2, TipoCampo.Numerico)]
    public int CodigoOcorrencia { get; init; }

    /// <summary>Data da ocorrência no banco, posições 111-116 (DDMMAA).</summary>
    [CampoCnab("Data da Ocorrência", 111, 6, TipoCampo.DataDdMmAa)]
    public DateOnly? DataOcorrencia { get; init; }

    /// <summary>Número do documento, posições 117-126.</summary>
    [CampoCnab("Número do Documento", 117, 10, TipoCampo.Alfanumerico)]
    public string NumeroDocumento { get; init; } = string.Empty;

    /// <summary>Confirmação do nosso número, posições 127-134.</summary>
    [CampoCnab("Confirmação do Nosso Número", 127, 8, TipoCampo.Alfanumerico)]
    public string ConfirmacaoNossoNumero { get; init; } = string.Empty;

    /// <summary>Complemento, posições 135-146, brancos.</summary>
    [CampoCnab("Complemento 135-146", 135, 12, TipoCampo.Filler)]
    public string Complemento135A146 { get; init; } = string.Empty;

    /// <summary>Data de vencimento, posições 147-152 (DDMMAA).</summary>
    [CampoCnab("Data de Vencimento", 147, 6, TipoCampo.DataDdMmAa)]
    public DateOnly? DataVencimento { get; init; }

    /// <summary>Valor nominal do título, posições 153-165 (2 casas decimais).</summary>
    [CampoCnab("Valor do Título", 153, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorTitulo { get; init; }

    /// <summary>Código do banco, posições 166-168.</summary>
    [CampoCnab("Código do Banco", 166, 3, TipoCampo.Numerico)]
    public int Banco { get; init; }

    /// <summary>Agência cobradora, posições 169-172.</summary>
    [CampoCnab("Agência Cobradora", 169, 4, TipoCampo.Numerico)]
    public int AgenciaCobradora { get; init; }

    /// <summary>DAC da agência cobradora, posição 173.</summary>
    [CampoCnab("DAC da Agência Cobradora", 173, 1, TipoCampo.Numerico)]
    public int DacAgenciaCobradora { get; init; }

    /// <summary>Espécie do título, posições 174-175.</summary>
    [CampoCnab("Espécie do Título", 174, 2, TipoCampo.Numerico)]
    public int Especie { get; init; }

    /// <summary>Tarifa/despesas de cobrança, posições 176-188 (2 casas decimais).</summary>
    [CampoCnab("Tarifa de Cobrança", 176, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal Tarifa { get; init; }

    /// <summary>Complemento, posições 189-214, brancos.</summary>
    [CampoCnab("Complemento 189-214", 189, 26, TipoCampo.Filler)]
    public string Complemento189A214 { get; init; } = string.Empty;

    /// <summary>Valor do IOF recolhido, posições 215-227 (2 casas decimais).</summary>
    [CampoCnab("IOF Recolhido", 215, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal Iof { get; init; }

    /// <summary>Abatimento concedido, posições 228-240 (2 casas decimais).</summary>
    [CampoCnab("Abatimento Concedido", 228, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal Abatimento { get; init; }

    /// <summary>Desconto concedido, posições 241-253 (2 casas decimais).</summary>
    [CampoCnab("Desconto Concedido", 241, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal Desconto { get; init; }

    /// <summary>Valor principal creditado, posições 254-266 (2 casas decimais).</summary>
    [CampoCnab("Valor Pago", 254, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorPago { get; init; }

    /// <summary>Juros de mora/multa, posições 267-279 (2 casas decimais).</summary>
    [CampoCnab("Juros de Mora/Multa", 267, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal JurosMulta { get; init; }

    /// <summary>Outros créditos, posições 280-292 (2 casas decimais).</summary>
    [CampoCnab("Outros Créditos", 280, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal OutrosCreditos { get; init; }

    /// <summary>Boleto DDA, posição 293.</summary>
    [CampoCnab("Boleto DDA", 293, 1, TipoCampo.Alfanumerico)]
    public string BoletoDda { get; init; } = string.Empty;

    /// <summary>Complemento, posições 294-295, brancos.</summary>
    [CampoCnab("Complemento 294-295", 294, 2, TipoCampo.Filler)]
    public string Complemento294A295 { get; init; } = string.Empty;

    /// <summary>Data de crédito da liquidação, posições 296-301 (DDMMAA).</summary>
    [CampoCnab("Data de Crédito", 296, 6, TipoCampo.DataDdMmAa)]
    public DateOnly? DataCredito { get; init; }

    /// <summary>Instrução cancelada/complemento da ocorrência, posições 302-305.</summary>
    [CampoCnab("Instrução Cancelada/Complemento", 302, 4, TipoCampo.Numerico)]
    public int ComplementoOcorrencia { get; init; }

    /// <summary>Data complementar da ocorrência do pagador, posições 306-311.</summary>
    [CampoCnab("Data Complementar da Ocorrência", 306, 6, TipoCampo.DataDdMmAa)]
    public DateOnly? DataComplementarOcorrencia { get; init; }

    /// <summary>Valor complementar da ocorrência do pagador, posições 312-324.</summary>
    [CampoCnab("Valor Complementar da Ocorrência", 312, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorComplementarOcorrencia { get; init; }

    /// <summary>Nome do pagador, posições 325-354.</summary>
    [CampoCnab("Nome do Pagador", 325, 30, TipoCampo.Alfanumerico)]
    public string NomePagador { get; init; } = string.Empty;

    /// <summary>Complemento, posições 355-377, brancos.</summary>
    [CampoCnab("Complemento 355-377", 355, 23, TipoCampo.Filler)]
    public string Complemento355A377 { get; init; } = string.Empty;

    /// <summary>Erros/mensagem informativa, posições 378-385 (até 4 códigos de 2 dígitos).</summary>
    [CampoCnab("Erros/Mensagem Informativa", 378, 8, TipoCampo.Alfanumerico)]
    public string ErrosMensagem { get; init; } = string.Empty;

    /// <summary>Complemento, posições 386-392, brancos.</summary>
    [CampoCnab("Complemento 386-392", 386, 7, TipoCampo.Filler)]
    public string Complemento386A392 { get; init; } = string.Empty;

    /// <summary>
    /// Código de liquidação, posições 393-394. O Itaú envia códigos
    /// alfanuméricos (por exemplo "AA" e "BC"), não apenas dois dígitos.
    /// </summary>
    [CampoCnab("Código de Liquidação", 393, 2, TipoCampo.Alfanumerico)]
    public string CodigoLiquidacao { get; init; } = string.Empty;

    /// <summary>Número sequencial do registro, posições 395-400.</summary>
    [CampoCnab("Número Sequencial do Registro", 395, 6, TipoCampo.Numerico)]
    public int NumeroSequencial { get; init; }
}

/// <summary>Trailer de retorno CNAB 400 do Itaú. Manual janeiro/2017.</summary>
[RegistroCnab(Codigos400.TamanhoRegistro)]
public sealed record TrailerRetornoItau400
{
    /// <summary>Identificação do registro, posição 1: "9".</summary>
    [CampoCnab("Identificação do Registro", 1, 1, TipoCampo.Numerico)]
    public int IdentificacaoRegistro { get; init; } = 9;

    /// <summary>Código de retorno, posição 2: "2".</summary>
    [CampoCnab("Código de Retorno", 2, 1, TipoCampo.Numerico)]
    public int CodigoRetorno { get; init; } = 2;

    /// <summary>Código do serviço, posições 3-4: "01".</summary>
    [CampoCnab("Código do Serviço", 3, 2, TipoCampo.Numerico)]
    public int CodigoServico { get; init; } = 1;

    /// <summary>Código do banco, posições 5-7: "341".</summary>
    [CampoCnab("Código do Banco", 5, 3, TipoCampo.Numerico)]
    public int Banco { get; init; } = 341;

    /// <summary>Complemento, posições 8-17, brancos.</summary>
    [CampoCnab("Complemento 8-17", 8, 10, TipoCampo.Filler)]
    public string Complemento8A17 { get; init; } = string.Empty;

    /// <summary>Quantidade de títulos em cobrança simples, posições 18-25.</summary>
    [CampoCnab("Qtde. Títulos Cobrança Simples", 18, 8, TipoCampo.Numerico)]
    public int QuantidadeSimples { get; init; }

    /// <summary>Valor total em cobrança simples, posições 26-39 (2 casas decimais).</summary>
    [CampoCnab("Valor Total Cobrança Simples", 26, 14, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorSimples { get; init; }

    /// <summary>Aviso bancário da cobrança simples, posições 40-47.</summary>
    [CampoCnab("Aviso Bancário Simples", 40, 8, TipoCampo.Alfanumerico)]
    public string AvisoSimples { get; init; } = string.Empty;

    /// <summary>Complemento, posições 48-57, brancos.</summary>
    [CampoCnab("Complemento 48-57", 48, 10, TipoCampo.Filler)]
    public string Complemento48A57 { get; init; } = string.Empty;

    /// <summary>Quantidade de títulos em cobrança vinculada, posições 58-65.</summary>
    [CampoCnab("Qtde. Títulos Cobrança Vinculada", 58, 8, TipoCampo.Numerico)]
    public int QuantidadeVinculada { get; init; }

    /// <summary>Valor total em cobrança vinculada, posições 66-79 (2 casas decimais).</summary>
    [CampoCnab("Valor Total Cobrança Vinculada", 66, 14, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorVinculada { get; init; }

    /// <summary>Aviso bancário da cobrança vinculada, posições 80-87.</summary>
    [CampoCnab("Aviso Bancário Vinculada", 80, 8, TipoCampo.Alfanumerico)]
    public string AvisoVinculada { get; init; } = string.Empty;

    /// <summary>Complemento, posições 88-177, brancos.</summary>
    [CampoCnab("Complemento 88-177", 88, 90, TipoCampo.Filler)]
    public string Complemento88A177 { get; init; } = string.Empty;

    /// <summary>Quantidade de títulos em cobrança direta/escritural, posições 178-185.</summary>
    [CampoCnab("Qtde. Títulos Direta/Escritural", 178, 8, TipoCampo.Numerico)]
    public int QuantidadeDireta { get; init; }

    /// <summary>Valor total em cobrança direta/escritural, posições 186-199 (2 casas decimais).</summary>
    [CampoCnab("Valor Total Direta/Escritural", 186, 14, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorDireta { get; init; }

    /// <summary>Aviso bancário da cobrança direta/escritural, posições 200-207.</summary>
    [CampoCnab("Aviso Bancário Direta", 200, 8, TipoCampo.Alfanumerico)]
    public string AvisoDireta { get; init; } = string.Empty;

    /// <summary>Controle do arquivo, posições 208-212.</summary>
    [CampoCnab("Controle do Arquivo", 208, 5, TipoCampo.Numerico)]
    public int ControleArquivo { get; init; }

    /// <summary>Quantidade de registros de transação, posições 213-220.</summary>
    [CampoCnab("Qtde. de Detalhes", 213, 8, TipoCampo.Numerico)]
    public int QuantidadeDetalhes { get; init; }

    /// <summary>Valor total informado, posições 221-234 (2 casas decimais).</summary>
    [CampoCnab("Valor Total Informado", 221, 14, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorTotalInformado { get; init; }

    /// <summary>Complemento, posições 235-394, brancos.</summary>
    [CampoCnab("Complemento 235-394", 235, 160, TipoCampo.Filler)]
    public string ComplementoFinal { get; init; } = string.Empty;

    /// <summary>Número sequencial do registro, posições 395-400.</summary>
    [CampoCnab("Número Sequencial do Registro", 395, 6, TipoCampo.Numerico)]
    public int NumeroSequencial { get; init; }
}
