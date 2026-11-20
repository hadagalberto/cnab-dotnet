namespace Cnab.Layouts.Cnab400.Registros;

using Cnab.Arquivo;

/// <summary>
/// Header de remessa CNAB 400 do Sicredi (registro 0). Manual "CNAB 400 —
/// Cobrança Sicredi" v2.1 (atualização novembro/2021), seção "Header".
/// Fonte: https://www.sicredi.com.br/media/produtos/manual_cnab_400_2_1.pdf
/// Datas neste layout usam AAAAMMDD e são gravadas como texto (o motor só
/// conhece DDMMAA/DDMMAAAA); a conversão é feita explicitamente no gerador.
/// </summary>
[RegistroCnab(Codigos400.TamanhoRegistro)]
public sealed record HeaderRemessaSicredi400
{
    /// <summary>Identificação do registro, posição 1: "0".</summary>
    [CampoCnab("Identificação do Registro", 1, 1, TipoCampo.Numerico)]
    public int IdentificacaoRegistro { get; init; }

    /// <summary>Identificação do arquivo remessa, posição 2: "1".</summary>
    [CampoCnab("Identificação do Arquivo-Remessa", 2, 1, TipoCampo.Numerico)]
    public int IdentificacaoRemessa { get; init; } = 1;

    /// <summary>Literal "REMESSA", posições 3-9.</summary>
    [CampoCnab("Literal Remessa", 3, 7, TipoCampo.Alfanumerico)]
    public string LiteralRemessa { get; init; } = Codigos400.LiteralRemessa;

    /// <summary>Código do serviço de cobrança, posições 10-11: "01".</summary>
    [CampoCnab("Código do Serviço", 10, 2, TipoCampo.Numerico)]
    public int CodigoServico { get; init; } = 1;

    /// <summary>Literal "COBRANCA", posições 12-19.</summary>
    [CampoCnab("Literal Serviço", 12, 8, TipoCampo.Alfanumerico)]
    public string LiteralServico { get; init; } = Codigos400.LiteralCobranca;

    /// <summary>Filler, posições 20-26, brancos.</summary>
    [CampoCnab("Filler 20-26", 20, 7, TipoCampo.Filler)]
    public string Filler20A26 { get; init; } = string.Empty;

    /// <summary>Código do beneficiário/cedente, posições 27-31.</summary>
    [CampoCnab("Código do Beneficiário", 27, 5, TipoCampo.Alfanumerico)]
    public string CodigoBeneficiario { get; init; } = string.Empty;

    /// <summary>CPF/CNPJ do beneficiário, posições 32-45.</summary>
    [CampoCnab("CPF/CNPJ do Beneficiário", 32, 14, TipoCampo.Alfanumerico)]
    public string DocumentoBeneficiario { get; init; } = string.Empty;

    /// <summary>Filler, posições 46-76, brancos.</summary>
    [CampoCnab("Filler 46-76", 46, 31, TipoCampo.Filler)]
    public string Filler46A76 { get; init; } = string.Empty;

    /// <summary>Número do Sicredi, posições 77-79: "748".</summary>
    [CampoCnab("Número do Sicredi", 77, 3, TipoCampo.Numerico)]
    public int Banco { get; init; } = 748;

    /// <summary>Literal "SICREDI", posições 80-94 (no retorno é "BANSICREDI").</summary>
    [CampoCnab("Literal Sicredi", 80, 15, TipoCampo.Alfanumerico)]
    public string LiteralBanco { get; init; } = "SICREDI";

    /// <summary>Data de geração (AAAAMMDD), posições 95-102.</summary>
    [CampoCnab("Data de Geração", 95, 8, TipoCampo.Alfanumerico)]
    public string DataGeracao { get; init; } = string.Empty;

    /// <summary>Filler, posições 103-110, brancos.</summary>
    [CampoCnab("Filler 103-110", 103, 8, TipoCampo.Filler)]
    public string Filler103A110 { get; init; } = string.Empty;

    /// <summary>Número da remessa, posições 111-117.</summary>
    [CampoCnab("Número da Remessa", 111, 7, TipoCampo.Numerico)]
    public int NumeroRemessa { get; init; }

    /// <summary>Filler, posições 118-390, brancos.</summary>
    [CampoCnab("Filler 118-390", 118, 273, TipoCampo.Filler)]
    public string Filler118A390 { get; init; } = string.Empty;

    /// <summary>Versão do sistema, posições 391-394: "2.00".</summary>
    [CampoCnab("Versão do Sistema", 391, 4, TipoCampo.Alfanumerico)]
    public string VersaoSistema { get; init; } = "2.00";

    /// <summary>Número sequencial do registro, posições 395-400.</summary>
    [CampoCnab("Número Sequencial do Registro", 395, 6, TipoCampo.Numerico)]
    public int NumeroSequencial { get; init; }
}

/// <summary>Registro de detalhe (tipo 1) da remessa CNAB 400 do Sicredi.</summary>
[RegistroCnab(Codigos400.TamanhoRegistro)]
public sealed record DetalheRemessaSicredi400
{
    /// <summary>Identificação do registro, posição 1: "1".</summary>
    [CampoCnab("Identificação do Registro", 1, 1, TipoCampo.Numerico)]
    public int IdentificacaoRegistro { get; init; } = 1;

    /// <summary>Tipo de cobrança, posição 2: "A" (com registro).</summary>
    [CampoCnab("Tipo de Cobrança", 2, 1, TipoCampo.Alfanumerico)]
    public string TipoCobranca { get; init; } = "A";

    /// <summary>Tipo de carteira, posição 3: "A" (simples).</summary>
    [CampoCnab("Tipo de Carteira", 3, 1, TipoCampo.Alfanumerico)]
    public string TipoCarteira { get; init; } = "A";

    /// <summary>Tipo de impressão, posição 4: "A" normal, "B" carnê.</summary>
    [CampoCnab("Tipo de Impressão", 4, 1, TipoCampo.Alfanumerico)]
    public string TipoImpressao { get; init; } = "A";

    /// <summary>Filler, posição 5, branco.</summary>
    [CampoCnab("Filler 5", 5, 1, TipoCampo.Filler)]
    public string Filler5 { get; init; } = string.Empty;

    /// <summary>Tipo de boleto, posição 6: "H" híbrido.</summary>
    [CampoCnab("Tipo de Boleto", 6, 1, TipoCampo.Alfanumerico)]
    public string TipoBoleto { get; init; } = string.Empty;

    /// <summary>Filler, posições 7-16, brancos.</summary>
    [CampoCnab("Filler 7-16", 7, 10, TipoCampo.Filler)]
    public string Filler7A16 { get; init; } = string.Empty;

    /// <summary>Tipo de moeda, posição 17: "A" (real).</summary>
    [CampoCnab("Tipo de Moeda", 17, 1, TipoCampo.Alfanumerico)]
    public string TipoMoeda { get; init; } = "A";

    /// <summary>Tipo de desconto, posição 18: "A" valor, "B" percentual.</summary>
    [CampoCnab("Tipo de Desconto", 18, 1, TipoCampo.Alfanumerico)]
    public string TipoDesconto { get; init; } = "A";

    /// <summary>Tipo de juros, posição 19: "A" valor, "B" % diário, "C" % mensal.</summary>
    [CampoCnab("Tipo de Juros", 19, 1, TipoCampo.Alfanumerico)]
    public string TipoJuros { get; init; } = "B";

    /// <summary>Tipo de multa, posição 20: "A" valor, "B" percentual.</summary>
    [CampoCnab("Tipo de Multa", 20, 1, TipoCampo.Alfanumerico)]
    public string TipoMulta { get; init; } = "B";

    /// <summary>Data de início dos juros (AAAAMMDD), posições 21-28.</summary>
    [CampoCnab("Data de Início dos Juros", 21, 8, TipoCampo.Alfanumerico)]
    public string DataInicioJuros { get; init; } = string.Empty;

    /// <summary>Data de início da multa (AAAAMMDD), posições 29-36.</summary>
    [CampoCnab("Data de Início da Multa", 29, 8, TipoCampo.Alfanumerico)]
    public string DataInicioMulta { get; init; } = string.Empty;

    /// <summary>Filler, posições 37-47, brancos.</summary>
    [CampoCnab("Filler 37-47", 37, 11, TipoCampo.Filler)]
    public string Filler37A47 { get; init; } = string.Empty;

    /// <summary>Nosso número (AA B XXXXX D), posições 48-56.</summary>
    [CampoCnab("Nosso Número", 48, 9, TipoCampo.Alfanumerico)]
    public string NossoNumero { get; init; } = string.Empty;

    /// <summary>Filler, posições 57-62, brancos.</summary>
    [CampoCnab("Filler 57-62", 57, 6, TipoCampo.Filler)]
    public string Filler57A62 { get; init; } = string.Empty;

    /// <summary>Data da instrução (AAAAMMDD), posições 63-70.</summary>
    [CampoCnab("Data da Instrução", 63, 8, TipoCampo.Alfanumerico)]
    public string DataInstrucao { get; init; } = string.Empty;

    /// <summary>Campo alterado (instrução 31), posição 71.</summary>
    [CampoCnab("Campo Alterado", 71, 1, TipoCampo.Alfanumerico)]
    public string CampoAlterado { get; init; } = string.Empty;

    /// <summary>Postagem do título ("S" Sicredi, "N" beneficiário), posição 72.</summary>
    [CampoCnab("Postagem do Título", 72, 1, TipoCampo.Alfanumerico)]
    public string PostagemTitulo { get; init; } = "N";

    /// <summary>Filler, posição 73, branco.</summary>
    [CampoCnab("Filler 73", 73, 1, TipoCampo.Filler)]
    public string Filler73 { get; init; } = string.Empty;

    /// <summary>Impressão do boleto ("A" Sicredi, "B" beneficiário), posição 74.</summary>
    [CampoCnab("Impressão do Boleto", 74, 1, TipoCampo.Alfanumerico)]
    public string ImpressaoBoleto { get; init; } = "A";

    /// <summary>Número da parcela do carnê, posições 75-76.</summary>
    [CampoCnab("Número da Parcela", 75, 2, TipoCampo.Alfanumerico)]
    public string NumeroParcela { get; init; } = string.Empty;

    /// <summary>Total de parcelas do carnê, posições 77-78.</summary>
    [CampoCnab("Total de Parcelas", 77, 2, TipoCampo.Alfanumerico)]
    public string TotalParcelas { get; init; } = string.Empty;

    /// <summary>Filler, posições 79-82, brancos.</summary>
    [CampoCnab("Filler 79-82", 79, 4, TipoCampo.Filler)]
    public string Filler79A82 { get; init; } = string.Empty;

    /// <summary>Desconto por dia de antecipação, posições 83-92 (2 casas decimais).</summary>
    [CampoCnab("Desconto por Dia de Antecipação", 83, 10, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal DescontoAntecipacao { get; init; }

    /// <summary>Multa em percentual, posições 93-96 (2 casas decimais).</summary>
    [CampoCnab("Multa Percentual", 93, 4, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal MultaPercentual { get; init; }

    /// <summary>Multa em valor monetário, posições 97-108 (2 casas decimais).</summary>
    [CampoCnab("Multa em Valor", 97, 12, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal MultaValor { get; init; }

    /// <summary>Instrução, posições 109-110 ("01" cadastro de título).</summary>
    [CampoCnab("Instrução", 109, 2, TipoCampo.Numerico)]
    public int Instrucao { get; init; } = 1;

    /// <summary>Seu número, posições 111-120.</summary>
    [CampoCnab("Seu Número", 111, 10, TipoCampo.Alfanumerico)]
    public string SeuNumero { get; init; } = string.Empty;

    /// <summary>Data de vencimento, posições 121-126 (DDMMAA).</summary>
    [CampoCnab("Data de Vencimento", 121, 6, TipoCampo.DataDdMmAa)]
    public DateOnly? DataVencimento { get; init; }

    /// <summary>Valor do título, posições 127-139 (2 casas decimais).</summary>
    [CampoCnab("Valor do Título", 127, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal Valor { get; init; }

    /// <summary>Filler, posições 140-148, brancos.</summary>
    [CampoCnab("Filler 140-148", 140, 9, TipoCampo.Filler)]
    public string Filler140A148 { get; init; } = string.Empty;

    /// <summary>Espécie de documento, posição 149.</summary>
    [CampoCnab("Espécie de Documento", 149, 1, TipoCampo.Alfanumerico)]
    public string Especie { get; init; } = "A";

    /// <summary>Aceite ("S"/"N"), posição 150.</summary>
    [CampoCnab("Aceite", 150, 1, TipoCampo.Alfanumerico)]
    public string Aceite { get; init; } = "N";

    /// <summary>Data de emissão, posições 151-156 (DDMMAA).</summary>
    [CampoCnab("Data de Emissão", 151, 6, TipoCampo.DataDdMmAa)]
    public DateOnly DataEmissao { get; init; }

    /// <summary>Instrução de protesto automático, posições 157-158.</summary>
    [CampoCnab("Instrução de Protesto", 157, 2, TipoCampo.Numerico)]
    public int InstrucaoProtesto { get; init; }

    /// <summary>Dias para protesto automático, posições 159-160.</summary>
    [CampoCnab("Dias para Protesto", 159, 2, TipoCampo.Numerico)]
    public int DiasProtesto { get; init; }

    /// <summary>Juros por dia de atraso, posições 161-173 (2 casas decimais).</summary>
    [CampoCnab("Juros por Dia de Atraso", 161, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal JurosDia { get; init; }

    /// <summary>Data limite para desconto, posições 174-179 (DDMMAA).</summary>
    [CampoCnab("Data Limite do Desconto", 174, 6, TipoCampo.DataDdMmAa)]
    public DateOnly? DataLimiteDesconto { get; init; }

    /// <summary>Desconto, posições 180-192 (2 casas decimais).</summary>
    [CampoCnab("Desconto", 180, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal Desconto { get; init; }

    /// <summary>Instrução de negativação automática, posições 193-194.</summary>
    [CampoCnab("Instrução de Negativação", 193, 2, TipoCampo.Numerico)]
    public int InstrucaoNegativacao { get; init; }

    /// <summary>Dias para negativação, posições 195-196.</summary>
    [CampoCnab("Dias para Negativação", 195, 2, TipoCampo.Numerico)]
    public int DiasNegativacao { get; init; }

    /// <summary>Filler, posições 197-205, zeros.</summary>
    [CampoCnab("Filler 197-205", 197, 9, TipoCampo.Filler)]
    public string Filler197A205 { get; init; } = "000000000";

    /// <summary>Valor do abatimento, posições 206-218 (2 casas decimais).</summary>
    [CampoCnab("Valor do Abatimento", 206, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal Abatimento { get; init; }

    /// <summary>Tipo de inscrição do pagador, posição 219.</summary>
    [CampoCnab("Tipo de Inscrição do Pagador", 219, 1, TipoCampo.Numerico)]
    public int TipoInscricaoPagador { get; init; }

    /// <summary>Filler, posição 220, zero.</summary>
    [CampoCnab("Filler 220", 220, 1, TipoCampo.Filler)]
    public string Filler220 { get; init; } = "0";

    /// <summary>CPF/CNPJ do pagador, posições 221-234.</summary>
    [CampoCnab("CPF/CNPJ do Pagador", 221, 14, TipoCampo.Alfanumerico)]
    public string DocumentoPagador { get; init; } = string.Empty;

    /// <summary>Nome do pagador, posições 235-274.</summary>
    [CampoCnab("Nome do Pagador", 235, 40, TipoCampo.Alfanumerico)]
    public string NomePagador { get; init; } = string.Empty;

    /// <summary>Endereço do pagador, posições 275-314.</summary>
    [CampoCnab("Endereço do Pagador", 275, 40, TipoCampo.Alfanumerico)]
    public string EnderecoPagador { get; init; } = string.Empty;

    /// <summary>Código do pagador na cooperativa, posições 315-319.</summary>
    [CampoCnab("Código do Pagador na Cooperativa", 315, 5, TipoCampo.Alfanumerico)]
    public string CodigoPagadorCooperativa { get; init; } = string.Empty;

    /// <summary>Filler, posições 320-325, zeros.</summary>
    [CampoCnab("Filler 320-325", 320, 6, TipoCampo.Filler)]
    public string Filler320A325 { get; init; } = "000000";

    /// <summary>Filler, posição 326, branco.</summary>
    [CampoCnab("Filler 326", 326, 1, TipoCampo.Filler)]
    public string Filler326 { get; init; } = string.Empty;

    /// <summary>CEP do pagador, posições 327-334.</summary>
    [CampoCnab("CEP do Pagador", 327, 8, TipoCampo.Alfanumerico)]
    public string CepPagador { get; init; } = string.Empty;

    /// <summary>Código do pagador junto ao cliente, posições 335-339.</summary>
    [CampoCnab("Código do Pagador no Cliente", 335, 5, TipoCampo.Alfanumerico)]
    public string CodigoPagadorCliente { get; init; } = string.Empty;

    /// <summary>CPF/CNPJ do beneficiário final, posições 340-353.</summary>
    [CampoCnab("CPF/CNPJ do Beneficiário Final", 340, 14, TipoCampo.Alfanumerico)]
    public string DocumentoBeneficiarioFinal { get; init; } = string.Empty;

    /// <summary>Nome do beneficiário final, posições 354-394.</summary>
    [CampoCnab("Nome do Beneficiário Final", 354, 41, TipoCampo.Alfanumerico)]
    public string NomeBeneficiarioFinal { get; init; } = string.Empty;

    /// <summary>Número sequencial do registro, posições 395-400.</summary>
    [CampoCnab("Número Sequencial do Registro", 395, 6, TipoCampo.Numerico)]
    public int NumeroSequencial { get; init; }
}

/// <summary>Trailer de remessa CNAB 400 do Sicredi (registro 9).</summary>
[RegistroCnab(Codigos400.TamanhoRegistro)]
public sealed record TrailerRemessaSicredi400
{
    /// <summary>Identificação do registro, posição 1: "9".</summary>
    [CampoCnab("Identificação do Registro", 1, 1, TipoCampo.Numerico)]
    public int IdentificacaoRegistro { get; init; } = 9;

    /// <summary>Identificação do arquivo remessa, posição 2: "1".</summary>
    [CampoCnab("Identificação do Arquivo-Remessa", 2, 1, TipoCampo.Numerico)]
    public int IdentificacaoRemessa { get; init; } = 1;

    /// <summary>Número do Sicredi, posições 3-5: "748".</summary>
    [CampoCnab("Número do Sicredi", 3, 3, TipoCampo.Numerico)]
    public int Banco { get; init; } = 748;

    /// <summary>Código do beneficiário, posições 6-10.</summary>
    [CampoCnab("Código do Beneficiário", 6, 5, TipoCampo.Alfanumerico)]
    public string CodigoBeneficiario { get; init; } = string.Empty;

    /// <summary>Filler, posições 11-394, brancos.</summary>
    [CampoCnab("Filler 11-394", 11, 384, TipoCampo.Filler)]
    public string Filler { get; init; } = string.Empty;

    /// <summary>Número sequencial do registro, posições 395-400.</summary>
    [CampoCnab("Número Sequencial do Registro", 395, 6, TipoCampo.Numerico)]
    public int NumeroSequencial { get; init; }
}

/// <summary>Header de retorno CNAB 400 do Sicredi (registro 0).</summary>
[RegistroCnab(Codigos400.TamanhoRegistro)]
public sealed record HeaderRetornoSicredi400
{
    /// <summary>Identificação do registro, posição 1: "0".</summary>
    [CampoCnab("Identificação do Registro", 1, 1, TipoCampo.Numerico)]
    public int IdentificacaoRegistro { get; init; }

    /// <summary>Identificação do arquivo retorno, posição 2: "2".</summary>
    [CampoCnab("Identificação do Arquivo-Retorno", 2, 1, TipoCampo.Numerico)]
    public int IdentificacaoRetorno { get; init; } = 2;

    /// <summary>Literal "RETORNO", posições 3-9.</summary>
    [CampoCnab("Literal Retorno", 3, 7, TipoCampo.Alfanumerico)]
    public string LiteralRetorno { get; init; } = Codigos400.LiteralRetorno;

    /// <summary>Código do serviço de cobrança, posições 10-11: "01".</summary>
    [CampoCnab("Código do Serviço", 10, 2, TipoCampo.Numerico)]
    public int CodigoServico { get; init; } = 1;

    /// <summary>Literal "COBRANCA", posições 12-26.</summary>
    [CampoCnab("Literal Serviço", 12, 15, TipoCampo.Alfanumerico)]
    public string LiteralServico { get; init; } = Codigos400.LiteralCobranca;

    /// <summary>Código do beneficiário, posições 27-31.</summary>
    [CampoCnab("Código do Beneficiário", 27, 5, TipoCampo.Alfanumerico)]
    public string CodigoBeneficiario { get; init; } = string.Empty;

    /// <summary>CPF/CNPJ do beneficiário, posições 32-45.</summary>
    [CampoCnab("CPF/CNPJ do Beneficiário", 32, 14, TipoCampo.Alfanumerico)]
    public string DocumentoBeneficiario { get; init; } = string.Empty;

    /// <summary>Filler, posições 46-76, brancos.</summary>
    [CampoCnab("Filler 46-76", 46, 31, TipoCampo.Filler)]
    public string Filler46A76 { get; init; } = string.Empty;

    /// <summary>Número do Sicredi, posições 77-79: "748".</summary>
    [CampoCnab("Número do Sicredi", 77, 3, TipoCampo.Numerico)]
    public int Banco { get; init; } = 748;

    /// <summary>Literal "BANSICREDI", posições 80-94.</summary>
    [CampoCnab("Literal BANSICREDI", 80, 15, TipoCampo.Alfanumerico)]
    public string LiteralBanco { get; init; } = "BANSICREDI";

    /// <summary>Data de gravação (AAAAMMDD), posições 95-102.</summary>
    [CampoCnab("Data de Gravação", 95, 8, TipoCampo.Alfanumerico)]
    public string DataGravacao { get; init; } = string.Empty;

    /// <summary>Filler, posições 103-110, brancos.</summary>
    [CampoCnab("Filler 103-110", 103, 8, TipoCampo.Filler)]
    public string Filler103A110 { get; init; } = string.Empty;

    /// <summary>Número do retorno, posições 111-117.</summary>
    [CampoCnab("Número do Retorno", 111, 7, TipoCampo.Numerico)]
    public int NumeroRetorno { get; init; }

    /// <summary>Filler, posições 118-389, brancos.</summary>
    [CampoCnab("Filler 118-389", 118, 272, TipoCampo.Filler)]
    public string Filler118A389 { get; init; } = string.Empty;

    /// <summary>Versão do sistema, posições 390-394: "99.99".</summary>
    [CampoCnab("Versão do Sistema", 390, 5, TipoCampo.Alfanumerico)]
    public string VersaoSistema { get; init; } = "99.99";

    /// <summary>Número sequencial do registro, posições 395-400.</summary>
    [CampoCnab("Número Sequencial do Registro", 395, 6, TipoCampo.Numerico)]
    public int NumeroSequencial { get; init; }
}

/// <summary>Detalhe de retorno CNAB 400 do Sicredi (registro 1, cobrança com registro).</summary>
[RegistroCnab(Codigos400.TamanhoRegistro)]
public sealed record DetalheRetornoSicredi400
{
    /// <summary>Identificação do registro, posição 1: "1".</summary>
    [CampoCnab("Identificação do Registro", 1, 1, TipoCampo.Numerico)]
    public int IdentificacaoRegistro { get; init; } = 1;

    /// <summary>Tipo de carteira, posição 2.</summary>
    [CampoCnab("Tipo de Carteira", 2, 1, TipoCampo.Alfanumerico)]
    public string TipoCarteira { get; init; } = string.Empty;

    /// <summary>Filler, posições 3-13, brancos.</summary>
    [CampoCnab("Filler 3-13", 3, 11, TipoCampo.Filler)]
    public string Filler3A13 { get; init; } = string.Empty;

    /// <summary>Tipo de cobrança, posição 14: "A" (com registro).</summary>
    [CampoCnab("Tipo de Cobrança", 14, 1, TipoCampo.Alfanumerico)]
    public string TipoCobranca { get; init; } = "A";

    /// <summary>Código do pagador na cooperativa, posições 15-19.</summary>
    [CampoCnab("Código do Pagador na Cooperativa", 15, 5, TipoCampo.Alfanumerico)]
    public string CodigoPagadorCooperativa { get; init; } = string.Empty;

    /// <summary>Código do pagador junto ao associado, posições 20-24.</summary>
    [CampoCnab("Código do Pagador no Associado", 20, 5, TipoCampo.Alfanumerico)]
    public string CodigoPagadorAssociado { get; init; } = string.Empty;

    /// <summary>Boleto DDA, posição 25.</summary>
    [CampoCnab("Boleto DDA", 25, 1, TipoCampo.Alfanumerico)]
    public string BoletoDda { get; init; } = string.Empty;

    /// <summary>Filler, posições 26-47, brancos.</summary>
    [CampoCnab("Filler 26-47", 26, 22, TipoCampo.Filler)]
    public string Filler26A47 { get; init; } = string.Empty;

    /// <summary>Nosso número, posições 48-62.</summary>
    [CampoCnab("Nosso Número", 48, 15, TipoCampo.Alfanumerico)]
    public string NossoNumero { get; init; } = string.Empty;

    /// <summary>Filler, posições 63-108, brancos.</summary>
    [CampoCnab("Filler 63-108", 63, 46, TipoCampo.Filler)]
    public string Filler63A108 { get; init; } = string.Empty;

    /// <summary>Código da ocorrência, posições 109-110.</summary>
    [CampoCnab("Código de Ocorrência", 109, 2, TipoCampo.Numerico)]
    public int CodigoOcorrencia { get; init; }

    /// <summary>Data da ocorrência, posições 111-116 (DDMMAA).</summary>
    [CampoCnab("Data da Ocorrência", 111, 6, TipoCampo.DataDdMmAa)]
    public DateOnly? DataOcorrencia { get; init; }

    /// <summary>Seu número, posições 117-126.</summary>
    [CampoCnab("Seu Número", 117, 10, TipoCampo.Alfanumerico)]
    public string SeuNumero { get; init; } = string.Empty;

    /// <summary>Filler, posições 127-146, brancos.</summary>
    [CampoCnab("Filler 127-146", 127, 20, TipoCampo.Filler)]
    public string Filler127A146 { get; init; } = string.Empty;

    /// <summary>Data de vencimento, posições 147-152 (DDMMAA).</summary>
    [CampoCnab("Data de Vencimento", 147, 6, TipoCampo.DataDdMmAa)]
    public DateOnly? DataVencimento { get; init; }

    /// <summary>Valor do título, posições 153-165 (2 casas decimais).</summary>
    [CampoCnab("Valor do Título", 153, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorTitulo { get; init; }

    /// <summary>Filler, posições 166-174, brancos.</summary>
    [CampoCnab("Filler 166-174", 166, 9, TipoCampo.Filler)]
    public string Filler166A174 { get; init; } = string.Empty;

    /// <summary>Espécie de documento, posição 175.</summary>
    [CampoCnab("Espécie de Documento", 175, 1, TipoCampo.Alfanumerico)]
    public string Especie { get; init; } = string.Empty;

    /// <summary>Despesas de cobrança/tarifa, posições 176-188 (2 casas decimais).</summary>
    [CampoCnab("Tarifa de Cobrança", 176, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal Tarifa { get; init; }

    /// <summary>Custas de protesto, posições 189-201 (2 casas decimais).</summary>
    [CampoCnab("Custas de Protesto", 189, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal CustasProtesto { get; init; }

    /// <summary>Filler, posições 202-227, zeros.</summary>
    [CampoCnab("Filler 202-227", 202, 26, TipoCampo.Filler)]
    public string Filler202A227 { get; init; } = "00000000000000000000000000";

    /// <summary>Abatimento concedido, posições 228-240 (2 casas decimais).</summary>
    [CampoCnab("Abatimento Concedido", 228, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal Abatimento { get; init; }

    /// <summary>Desconto concedido, posições 241-253 (2 casas decimais).</summary>
    [CampoCnab("Desconto Concedido", 241, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal Desconto { get; init; }

    /// <summary>Valor efetivamente pago, posições 254-266 (2 casas decimais).</summary>
    [CampoCnab("Valor Pago", 254, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorPago { get; init; }

    /// <summary>Juros de mora, posições 267-279 (2 casas decimais).</summary>
    [CampoCnab("Juros de Mora", 267, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal JurosMora { get; init; }

    /// <summary>Multa/outros recebimentos, posições 280-292 (2 casas decimais).</summary>
    [CampoCnab("Multa/Outros Recebimentos", 280, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal MultaOutros { get; init; }

    /// <summary>Filler, posições 293-294, brancos.</summary>
    [CampoCnab("Filler 293-294", 293, 2, TipoCampo.Filler)]
    public string Filler293A294 { get; init; } = string.Empty;

    /// <summary>Aceito/desprezado (ocorrência 19), posição 295.</summary>
    [CampoCnab("Aceite/Desprezado", 295, 1, TipoCampo.Alfanumerico)]
    public string AceiteDesprezado { get; init; } = string.Empty;

    /// <summary>Filler, posições 296-318, brancos.</summary>
    [CampoCnab("Filler 296-318", 296, 23, TipoCampo.Filler)]
    public string Filler296A318 { get; init; } = string.Empty;

    /// <summary>Motivo da ocorrência, posições 319-328 (até 5 pares de 2 posições).</summary>
    [CampoCnab("Motivo da Ocorrência", 319, 10, TipoCampo.Alfanumerico)]
    public string MotivoOcorrencia { get; init; } = string.Empty;

    /// <summary>Data prevista de lançamento em conta (AAAAMMDD), posições 329-336.</summary>
    [CampoCnab("Data Prevista de Lançamento", 329, 8, TipoCampo.Alfanumerico)]
    public string DataPrevistaCredito { get; init; } = string.Empty;

    /// <summary>Filler, posições 337-394, brancos.</summary>
    [CampoCnab("Filler 337-394", 337, 58, TipoCampo.Filler)]
    public string Filler337A394 { get; init; } = string.Empty;

    /// <summary>Número sequencial do registro, posições 395-400.</summary>
    [CampoCnab("Número Sequencial do Registro", 395, 6, TipoCampo.Numerico)]
    public int NumeroSequencial { get; init; }
}

/// <summary>Trailer de retorno CNAB 400 do Sicredi (registro 9).</summary>
[RegistroCnab(Codigos400.TamanhoRegistro)]
public sealed record TrailerRetornoSicredi400
{
    /// <summary>Identificação do registro, posição 1: "9".</summary>
    [CampoCnab("Identificação do Registro", 1, 1, TipoCampo.Numerico)]
    public int IdentificacaoRegistro { get; init; } = 9;

    /// <summary>Identificação do arquivo retorno, posição 2: "2".</summary>
    [CampoCnab("Identificação do Arquivo-Retorno", 2, 1, TipoCampo.Numerico)]
    public int IdentificacaoRetorno { get; init; } = 2;

    /// <summary>Número do Sicredi, posições 3-5: "748".</summary>
    [CampoCnab("Número do Sicredi", 3, 3, TipoCampo.Numerico)]
    public int Banco { get; init; } = 748;

    /// <summary>Código do beneficiário, posições 6-10.</summary>
    [CampoCnab("Código do Beneficiário", 6, 5, TipoCampo.Alfanumerico)]
    public string CodigoBeneficiario { get; init; } = string.Empty;

    /// <summary>Filler, posições 11-394, brancos.</summary>
    [CampoCnab("Filler 11-394", 11, 384, TipoCampo.Filler)]
    public string Filler { get; init; } = string.Empty;

    /// <summary>Número sequencial do registro, posições 395-400.</summary>
    [CampoCnab("Número Sequencial do Registro", 395, 6, TipoCampo.Numerico)]
    public int NumeroSequencial { get; init; }
}
