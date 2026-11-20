namespace Cnab.Layouts.Cnab400.Registros;

using System.Globalization;
using Cnab.Arquivo;
using Cnab.Layouts.Cnab240;
using Cnab.Modelo;

/// <summary>
/// Header de remessa CNAB 400 do Inter (registro 0). Manual "CNAB400 —
/// Emissão de boletos de cobrança" V2.2 (26/08/2024). Fonte:
/// https://s3.sa-east-1.amazonaws.com/static.bancointer.com.br/openbanking/cnab/manual/manual.pdf
/// </summary>
[RegistroCnab(Codigos400.TamanhoRegistro)]
public sealed record HeaderRemessaInter400
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

    /// <summary>Código do serviço, posições 10-11: "01".</summary>
    [CampoCnab("Código do Serviço", 10, 2, TipoCampo.Numerico)]
    public int CodigoServico { get; init; } = 1;

    /// <summary>Literal "COBRANCA", posições 12-26.</summary>
    [CampoCnab("Literal Serviço", 12, 15, TipoCampo.Alfanumerico)]
    public string LiteralServico { get; init; } = Codigos400.LiteralCobranca;

    /// <summary>Filler, posições 27-46, brancos.</summary>
    [CampoCnab("Filler 27-46", 27, 20, TipoCampo.Filler)]
    public string Filler27A46 { get; init; } = string.Empty;

    /// <summary>Nome da empresa, posições 47-76.</summary>
    [CampoCnab("Nome da Empresa", 47, 30, TipoCampo.Alfanumerico)]
    public string NomeEmpresa { get; init; } = string.Empty;

    /// <summary>Código do banco, posições 77-79: "077".</summary>
    [CampoCnab("Código do Banco", 77, 3, TipoCampo.Numerico)]
    public int Banco { get; init; } = 77;

    /// <summary>Nome do banco, posições 80-94: "INTER".</summary>
    [CampoCnab("Nome do Banco", 80, 15, TipoCampo.Alfanumerico)]
    public string NomeBanco { get; init; } = "INTER";

    /// <summary>Data de gravação, posições 95-100 (DDMMAA).</summary>
    [CampoCnab("Data de Gravação", 95, 6, TipoCampo.DataDdMmAa)]
    public DateOnly DataGravacao { get; init; }

    /// <summary>Filler, posições 101-110, brancos.</summary>
    [CampoCnab("Filler 101-110", 101, 10, TipoCampo.Filler)]
    public string Filler101A110 { get; init; } = string.Empty;

    /// <summary>Número sequencial da remessa, posições 111-117 (7 posições).</summary>
    [CampoCnab("Número Sequencial da Remessa", 111, 7, TipoCampo.Numerico)]
    public int NumeroSequencialRemessa { get; init; }

    /// <summary>Filler, posições 118-394, brancos.</summary>
    [CampoCnab("Filler 118-394", 118, 277, TipoCampo.Filler)]
    public string Filler118A394 { get; init; } = string.Empty;

    /// <summary>Número sequencial do registro, posições 395-400.</summary>
    [CampoCnab("Número Sequencial do Registro", 395, 6, TipoCampo.Numerico)]
    public int NumeroSequencial { get; init; }
}

/// <summary>Registro de transação tipo 1 da remessa CNAB 400 do Inter.</summary>
[RegistroCnab(Codigos400.TamanhoRegistro)]
public sealed record DetalheRemessaInter400
{
    /// <summary>Identificação do registro, posição 1: "1".</summary>
    [CampoCnab("Identificação do Registro", 1, 1, TipoCampo.Numerico)]
    public int IdentificacaoRegistro { get; init; } = 1;

    /// <summary>Filler, posições 2-20, brancos.</summary>
    [CampoCnab("Filler 2-20", 2, 19, TipoCampo.Filler)]
    public string Filler2A20 { get; init; } = string.Empty;

    /// <summary>Carteira, posições 21-23: 110 ou 112.</summary>
    [CampoCnab("Carteira", 21, 3, TipoCampo.Numerico)]
    public int Carteira { get; init; }

    /// <summary>Agência da empresa beneficiária, posições 24-27 ("00001").</summary>
    [CampoCnab("Agência", 24, 4, TipoCampo.Numerico)]
    public int Agencia { get; init; } = 1;

    /// <summary>Conta corrente (9 posições), posições 28-36.</summary>
    [CampoCnab("Conta Corrente", 28, 9, TipoCampo.Numerico)]
    public long Conta { get; init; }

    /// <summary>DV da conta, posição 37.</summary>
    [CampoCnab("DV da Conta", 37, 1, TipoCampo.Numerico)]
    public int DvConta { get; init; }

    /// <summary>Número de controle do participante, posições 38-62.</summary>
    [CampoCnab("Controle do Participante", 38, 25, TipoCampo.Alfanumerico)]
    public string ControleParticipante { get; init; } = string.Empty;

    /// <summary>Filler, posições 63-65, brancos.</summary>
    [CampoCnab("Filler 63-65", 63, 3, TipoCampo.Filler)]
    public string Filler63A65 { get; init; } = string.Empty;

    /// <summary>Campo multa, posição 66: 0 sem, 1 valor fixo, 2 percentual.</summary>
    [CampoCnab("Campo Multa", 66, 1, TipoCampo.Numerico)]
    public int CampoMulta { get; init; }

    /// <summary>Valor da multa, posições 67-79 (quando campo multa = 1).</summary>
    [CampoCnab("Valor da Multa", 67, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorMulta { get; init; }

    /// <summary>Percentual da multa, posições 80-83 (2 casas decimais).</summary>
    [CampoCnab("Percentual da Multa", 80, 4, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal PercentualMulta { get; init; }

    /// <summary>Data da multa, posições 84-89 (DDMMAA).</summary>
    [CampoCnab("Data da Multa", 84, 6, TipoCampo.DataDdMmAa)]
    public DateOnly? DataMulta { get; init; }

    /// <summary>Nosso número (11 posições), posições 90-100.</summary>
    [CampoCnab("Nosso Número", 90, 11, TipoCampo.Alfanumerico)]
    public string NossoNumero { get; init; } = string.Empty;

    /// <summary>Filler, posições 101-108, brancos.</summary>
    [CampoCnab("Filler 101-108", 101, 8, TipoCampo.Filler)]
    public string Filler101A108 { get; init; } = string.Empty;

    /// <summary>Identificação da ocorrência, posições 109-110: "01" remessa.</summary>
    [CampoCnab("Identificação da Ocorrência", 109, 2, TipoCampo.Numerico)]
    public int Ocorrencia { get; init; } = 1;

    /// <summary>Seu número, posições 111-120.</summary>
    [CampoCnab("Seu Número", 111, 10, TipoCampo.Alfanumerico)]
    public string SeuNumero { get; init; } = string.Empty;

    /// <summary>Vencimento, posições 121-126 (DDMMAA).</summary>
    [CampoCnab("Vencimento", 121, 6, TipoCampo.DataDdMmAa)]
    public DateOnly? DataVencimento { get; init; }

    /// <summary>Valor do título, posições 127-139 (2 casas decimais).</summary>
    [CampoCnab("Valor do Título", 127, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal Valor { get; init; }

    /// <summary>Data limite de pagamento em dias, posições 140-141.</summary>
    [CampoCnab("Data Limite de Pagamento", 140, 2, TipoCampo.Numerico)]
    public int DataLimitePagamento { get; init; }

    /// <summary>Filler, posições 142-147, brancos.</summary>
    [CampoCnab("Filler 142-147", 142, 6, TipoCampo.Filler)]
    public string Filler142A147 { get; init; } = string.Empty;

    /// <summary>Espécie do título, posições 148-149: "01".</summary>
    [CampoCnab("Espécie do Título", 148, 2, TipoCampo.Numerico)]
    public int Especie { get; init; } = 1;

    /// <summary>Identificação, posição 150: "N".</summary>
    [CampoCnab("Identificação", 150, 1, TipoCampo.Alfanumerico)]
    public string Identificacao { get; init; } = "N";

    /// <summary>Data de emissão, posições 151-156 (DDMMAA).</summary>
    [CampoCnab("Data de Emissão", 151, 6, TipoCampo.DataDdMmAa)]
    public DateOnly DataEmissao { get; init; }

    /// <summary>Filler, posições 157-159, brancos.</summary>
    [CampoCnab("Filler 157-159", 157, 3, TipoCampo.Filler)]
    public string Filler157A159 { get; init; } = string.Empty;

    /// <summary>Campo juros/mora, posição 160: 0, 1 (valor) ou 2 (taxa).</summary>
    [CampoCnab("Campo Juros/Mora", 160, 1, TipoCampo.Numerico)]
    public int CampoJuros { get; init; }

    /// <summary>Valor por dia de atraso, posições 161-173 (2 casas decimais).</summary>
    [CampoCnab("Valor por Dia de Atraso", 161, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorPorDia { get; init; }

    /// <summary>Taxa mensal de juros, posições 174-177 (2 casas decimais).</summary>
    [CampoCnab("Taxa Mensal de Juros", 174, 4, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal TaxaMensalJuros { get; init; }

    /// <summary>Data de mora, posições 178-183 (DDMMAA).</summary>
    [CampoCnab("Data de Mora", 178, 6, TipoCampo.DataDdMmAa)]
    public DateOnly? DataMora { get; init; }

    /// <summary>Campo descontos, posição 184.</summary>
    [CampoCnab("Campo Descontos", 184, 1, TipoCampo.Numerico)]
    public int CampoDescontos { get; init; }

    /// <summary>Valor do desconto 1, posições 185-197 (2 casas decimais).</summary>
    [CampoCnab("Valor do Desconto 1", 185, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorDesconto1 { get; init; }

    /// <summary>Percentual do desconto 1, posições 198-201 (2 casas decimais).</summary>
    [CampoCnab("Percentual do Desconto 1", 198, 4, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal PercentualDesconto1 { get; init; }

    /// <summary>Data limite do desconto 1, posições 202-207 (DDMMAA).</summary>
    [CampoCnab("Data Limite do Desconto 1", 202, 6, TipoCampo.DataDdMmAa)]
    public DateOnly? DataLimiteDesconto1 { get; init; }

    /// <summary>Filler, posições 208-220, brancos.</summary>
    [CampoCnab("Filler 208-220", 208, 13, TipoCampo.Filler)]
    public string Filler208A220 { get; init; } = string.Empty;

    /// <summary>Tipo de inscrição do pagador, posições 221-222: 01 CPF, 02 CNPJ.</summary>
    [CampoCnab("Tipo de Inscrição do Pagador", 221, 2, TipoCampo.Numerico)]
    public int TipoInscricaoPagador { get; init; }

    /// <summary>CPF/CNPJ do pagador, posições 223-236.</summary>
    [CampoCnab("CPF/CNPJ do Pagador", 223, 14, TipoCampo.Alfanumerico)]
    public string DocumentoPagador { get; init; } = string.Empty;

    /// <summary>Nome do pagador, posições 237-276.</summary>
    [CampoCnab("Nome do Pagador", 237, 40, TipoCampo.Alfanumerico)]
    public string NomePagador { get; init; } = string.Empty;

    /// <summary>Endereço completo do pagador, posições 277-314.</summary>
    [CampoCnab("Endereço do Pagador", 277, 38, TipoCampo.Alfanumerico)]
    public string EnderecoPagador { get; init; } = string.Empty;

    /// <summary>UF do pagador, posições 315-316.</summary>
    [CampoCnab("UF do Pagador", 315, 2, TipoCampo.Alfanumerico)]
    public string UfPagador { get; init; } = string.Empty;

    /// <summary>CEP do pagador, posições 317-324.</summary>
    [CampoCnab("CEP do Pagador", 317, 8, TipoCampo.Numerico)]
    public int CepPagador { get; init; }

    /// <summary>Primeira mensagem, posições 325-394.</summary>
    [CampoCnab("Mensagem 1", 325, 70, TipoCampo.Alfanumerico)]
    public string Mensagem1 { get; init; } = string.Empty;

    /// <summary>Número sequencial do registro, posições 395-400.</summary>
    [CampoCnab("Número Sequencial do Registro", 395, 6, TipoCampo.Numerico)]
    public int NumeroSequencial { get; init; }
}

/// <summary>Registro tipo 3 (e-mail / beneficiário final) da remessa Inter.</summary>
[RegistroCnab(Codigos400.TamanhoRegistro)]
public sealed record RegistroEmailRemessaInter400
{
    /// <summary>Identificação do registro, posição 1: "3".</summary>
    [CampoCnab("Identificação do Registro", 1, 1, TipoCampo.Numerico)]
    public int IdentificacaoRegistro { get; init; } = 3;

    /// <summary>E-mail do pagador, posições 2-51.</summary>
    [CampoCnab("E-mail do Pagador", 2, 50, TipoCampo.Alfanumerico)]
    public string Email { get; init; } = string.Empty;

    /// <summary>Filler, posições 52-61, brancos.</summary>
    [CampoCnab("Filler 52-61", 52, 10, TipoCampo.Filler)]
    public string Filler52A61 { get; init; } = string.Empty;

    /// <summary>Tipo de documento do beneficiário final, posições 62-63.</summary>
    [CampoCnab("Tipo de Documento do Beneficiário Final", 62, 2, TipoCampo.Alfanumerico)]
    public string TipoDocumentoBeneficiarioFinal { get; init; } = string.Empty;

    /// <summary>CPF/CNPJ do beneficiário final, posições 64-77.</summary>
    [CampoCnab("Documento do Beneficiário Final", 64, 14, TipoCampo.Alfanumerico)]
    public string DocumentoBeneficiarioFinal { get; init; } = string.Empty;

    /// <summary>Nome do beneficiário final, posições 78-137.</summary>
    [CampoCnab("Nome do Beneficiário Final", 78, 60, TipoCampo.Alfanumerico)]
    public string NomeBeneficiarioFinal { get; init; } = string.Empty;

    /// <summary>Endereço do beneficiário final, posições 138-197.</summary>
    [CampoCnab("Endereço do Beneficiário Final", 138, 60, TipoCampo.Alfanumerico)]
    public string EnderecoBeneficiarioFinal { get; init; } = string.Empty;

    /// <summary>Bairro do beneficiário final, posições 198-242.</summary>
    [CampoCnab("Bairro do Beneficiário Final", 198, 45, TipoCampo.Alfanumerico)]
    public string BairroBeneficiarioFinal { get; init; } = string.Empty;

    /// <summary>CEP do beneficiário final, posições 243-250.</summary>
    [CampoCnab("CEP do Beneficiário Final", 243, 8, TipoCampo.Alfanumerico)]
    public string CepBeneficiarioFinal { get; init; } = string.Empty;

    /// <summary>Cidade do beneficiário final, posições 251-280.</summary>
    [CampoCnab("Cidade do Beneficiário Final", 251, 30, TipoCampo.Alfanumerico)]
    public string CidadeBeneficiarioFinal { get; init; } = string.Empty;

    /// <summary>UF do beneficiário final, posições 281-282.</summary>
    [CampoCnab("UF do Beneficiário Final", 281, 2, TipoCampo.Alfanumerico)]
    public string UfBeneficiarioFinal { get; init; } = string.Empty;

    /// <summary>Agência com DV do beneficiário final, posições 283-287.</summary>
    [CampoCnab("Agência do Beneficiário Final", 283, 5, TipoCampo.Alfanumerico)]
    public string AgenciaBeneficiarioFinal { get; init; } = string.Empty;

    /// <summary>Conta com DV do beneficiário final, posições 288-297.</summary>
    [CampoCnab("Conta do Beneficiário Final", 288, 10, TipoCampo.Alfanumerico)]
    public string ContaBeneficiarioFinal { get; init; } = string.Empty;

    /// <summary>Filler, posições 298-394, brancos.</summary>
    [CampoCnab("Filler 298-394", 298, 97, TipoCampo.Filler)]
    public string FillerFinal { get; init; } = string.Empty;

    /// <summary>Número sequencial do registro, posições 395-400.</summary>
    [CampoCnab("Número Sequencial do Registro", 395, 6, TipoCampo.Numerico)]
    public int NumeroSequencial { get; init; }
}

/// <summary>Trailer de remessa CNAB 400 do Inter.</summary>
[RegistroCnab(Codigos400.TamanhoRegistro)]
public sealed record TrailerRemessaInter400
{
    /// <summary>Identificação do registro, posição 1: "9".</summary>
    [CampoCnab("Identificação do Registro", 1, 1, TipoCampo.Numerico)]
    public int IdentificacaoRegistro { get; init; } = 9;

    /// <summary>Quantidade de boletos, posições 2-7.</summary>
    [CampoCnab("Quantidade de Boletos", 2, 6, TipoCampo.Numerico)]
    public int QuantidadeBoletos { get; init; }

    /// <summary>Filler, posições 8-394, brancos.</summary>
    [CampoCnab("Filler 8-394", 8, 387, TipoCampo.Filler)]
    public string Filler { get; init; } = string.Empty;

    /// <summary>Número sequencial do registro, posições 395-400.</summary>
    [CampoCnab("Número Sequencial do Registro", 395, 6, TipoCampo.Numerico)]
    public int NumeroSequencial { get; init; }
}

/// <summary>Header de retorno CNAB 400 do Inter.</summary>
[RegistroCnab(Codigos400.TamanhoRegistro)]
public sealed record HeaderRetornoInter400
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

    /// <summary>Filler, posições 27-46, brancos.</summary>
    [CampoCnab("Filler 27-46", 27, 20, TipoCampo.Filler)]
    public string Filler27A46 { get; init; } = string.Empty;

    /// <summary>Nome da empresa, posições 47-76.</summary>
    [CampoCnab("Nome da Empresa", 47, 30, TipoCampo.Alfanumerico)]
    public string NomeEmpresa { get; init; } = string.Empty;

    /// <summary>Código do banco, posições 77-79: "077".</summary>
    [CampoCnab("Código do Banco", 77, 3, TipoCampo.Numerico)]
    public int Banco { get; init; } = 77;

    /// <summary>Nome do banco, posições 80-94: "INTER".</summary>
    [CampoCnab("Nome do Banco", 80, 15, TipoCampo.Alfanumerico)]
    public string NomeBanco { get; init; } = "INTER";

    /// <summary>Data de gravação, posições 95-100 (DDMMAA).</summary>
    [CampoCnab("Data de Gravação", 95, 6, TipoCampo.DataDdMmAa)]
    public DateOnly DataGravacao { get; init; }

    /// <summary>Filler, posições 101-394, brancos.</summary>
    [CampoCnab("Filler 101-394", 101, 294, TipoCampo.Filler)]
    public string Filler { get; init; } = string.Empty;

    /// <summary>Número sequencial do registro, posições 395-400.</summary>
    [CampoCnab("Número Sequencial do Registro", 395, 6, TipoCampo.Numerico)]
    public int NumeroSequencial { get; init; }
}

/// <summary>Detalhe de retorno CNAB 400 do Inter (registro tipo 1).</summary>
[RegistroCnab(Codigos400.TamanhoRegistro)]
public sealed record DetalheRetornoInter400
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

    /// <summary>Posições 18-20, zeros.</summary>
    [CampoCnab("Zeros 18-20", 18, 3, TipoCampo.Filler)]
    public string Zeros18A20 { get; init; } = "000";

    /// <summary>Carteira, posições 21-23.</summary>
    [CampoCnab("Carteira", 21, 3, TipoCampo.Numerico)]
    public int Carteira { get; init; }

    /// <summary>Agência da empresa, posições 24-27.</summary>
    [CampoCnab("Agência", 24, 4, TipoCampo.Numerico)]
    public int Agencia { get; init; }

    /// <summary>Conta corrente da empresa, posições 28-37.</summary>
    [CampoCnab("Conta Corrente", 28, 10, TipoCampo.Alfanumerico)]
    public string Conta { get; init; } = string.Empty;

    /// <summary>Controle do participante, posições 38-62.</summary>
    [CampoCnab("Controle do Participante", 38, 25, TipoCampo.Alfanumerico)]
    public string ControleParticipante { get; init; } = string.Empty;

    /// <summary>Posições 63-70, zeros.</summary>
    [CampoCnab("Zeros 63-70", 63, 8, TipoCampo.Filler)]
    public string Zeros63A70 { get; init; } = "00000000";

    /// <summary>Nosso número, posições 71-81.</summary>
    [CampoCnab("Nosso Número", 71, 11, TipoCampo.Alfanumerico)]
    public string NossoNumero { get; init; } = string.Empty;

    /// <summary>Filler, posições 82-86, brancos.</summary>
    [CampoCnab("Filler 82-86", 82, 5, TipoCampo.Filler)]
    public string Filler82A86 { get; init; } = string.Empty;

    /// <summary>Carteira, posições 87-89.</summary>
    [CampoCnab("Carteira 87-89", 87, 3, TipoCampo.Numerico)]
    public int CarteiraConfirmacao { get; init; }

    /// <summary>Identificação da ocorrência, posições 90-91: 02, 03, 06 ou 07.</summary>
    [CampoCnab("Identificação da Ocorrência", 90, 2, TipoCampo.Numerico)]
    public int CodigoOcorrencia { get; init; }

    /// <summary>Data da ocorrência no banco, posições 92-97 (DDMMAA).</summary>
    [CampoCnab("Data da Ocorrência", 92, 6, TipoCampo.DataDdMmAa)]
    public DateOnly? DataOcorrencia { get; init; }

    /// <summary>Seu número, posições 98-107.</summary>
    [CampoCnab("Seu Número", 98, 10, TipoCampo.Alfanumerico)]
    public string SeuNumero { get; init; } = string.Empty;

    /// <summary>Nosso número (segunda ocorrência), posições 108-118.</summary>
    [CampoCnab("Nosso Número 108-118", 108, 11, TipoCampo.Alfanumerico)]
    public string NossoNumeroConfirmado { get; init; } = string.Empty;

    /// <summary>Vencimento, posições 119-124 (DDMMAA).</summary>
    [CampoCnab("Vencimento", 119, 6, TipoCampo.DataDdMmAa)]
    public DateOnly? DataVencimento { get; init; }

    /// <summary>Valor do título, posições 125-137 (2 casas decimais).</summary>
    [CampoCnab("Valor do Título", 125, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorTitulo { get; init; }

    /// <summary>Código do banco, posições 138-140: "077".</summary>
    [CampoCnab("Código do Banco", 138, 3, TipoCampo.Numerico)]
    public int Banco { get; init; }

    /// <summary>Agência da empresa, posições 141-144.</summary>
    [CampoCnab("Agência", 141, 4, TipoCampo.Numerico)]
    public int AgenciaEmpresa { get; init; }

    /// <summary>Espécie, posições 145-146: "01".</summary>
    [CampoCnab("Espécie", 145, 2, TipoCampo.Numerico)]
    public int Especie { get; init; }

    /// <summary>Filler, posições 147-159, brancos.</summary>
    [CampoCnab("Filler 147-159", 147, 13, TipoCampo.Filler)]
    public string Filler147A159 { get; init; } = string.Empty;

    /// <summary>Valor pago, posições 160-172 (2 casas decimais).</summary>
    [CampoCnab("Valor Pago", 160, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorPago { get; init; }

    /// <summary>Data do crédito, posições 173-178 (DDMMAA).</summary>
    [CampoCnab("Data do Crédito", 173, 6, TipoCampo.DataDdMmAa)]
    public DateOnly? DataCredito { get; init; }

    /// <summary>Filler, posições 179-181, brancos.</summary>
    [CampoCnab("Filler 179-181", 179, 3, TipoCampo.Filler)]
    public string Filler179A181 { get; init; } = string.Empty;

    /// <summary>Nome do pagador, posições 182-221.</summary>
    [CampoCnab("Nome do Pagador", 182, 40, TipoCampo.Alfanumerico)]
    public string NomePagador { get; init; } = string.Empty;

    /// <summary>Filler, posições 222-226, brancos.</summary>
    [CampoCnab("Filler 222-226", 222, 5, TipoCampo.Filler)]
    public string Filler222A226 { get; init; } = string.Empty;

    /// <summary>CPF/CNPJ do pagador, posições 227-240.</summary>
    [CampoCnab("CPF/CNPJ do Pagador", 227, 14, TipoCampo.Alfanumerico)]
    public string DocumentoPagador { get; init; } = string.Empty;

    /// <summary>Motivo da rejeição em texto livre, posições 241-380 (somente ocorrência 03).</summary>
    [CampoCnab("Motivo da Rejeição", 241, 140, TipoCampo.Alfanumerico)]
    public string MotivoRejeicao { get; init; } = string.Empty;

    /// <summary>Número de operação, posições 381-394 (campo livre do código de barras).</summary>
    [CampoCnab("Número da Operação", 381, 14, TipoCampo.Alfanumerico)]
    public string NumeroOperacao { get; init; } = string.Empty;

    /// <summary>Número sequencial do registro, posições 395-400.</summary>
    [CampoCnab("Número Sequencial do Registro", 395, 6, TipoCampo.Numerico)]
    public int NumeroSequencial { get; init; }
}

/// <summary>Trailer de retorno CNAB 400 do Inter.</summary>
[RegistroCnab(Codigos400.TamanhoRegistro)]
public sealed record TrailerRetornoInter400
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

    /// <summary>Código do banco, posições 5-7: "077".</summary>
    [CampoCnab("Código do Banco", 5, 3, TipoCampo.Numerico)]
    public int Banco { get; init; } = 77;

    /// <summary>Filler, posições 8-17, brancos.</summary>
    [CampoCnab("Filler 8-17", 8, 10, TipoCampo.Filler)]
    public string Filler8A17 { get; init; } = string.Empty;

    /// <summary>Quantidade da ocorrência 01, posições 18-25.</summary>
    [CampoCnab("Qtde. Ocorrência 01", 18, 8, TipoCampo.Numerico)]
    public int QuantidadeOcorrencia01 { get; init; }

    /// <summary>Filler, posições 26-57, brancos.</summary>
    [CampoCnab("Filler 26-57", 26, 32, TipoCampo.Filler)]
    public string Filler26A57 { get; init; } = string.Empty;

    /// <summary>Quantidade da ocorrência 02, posições 58-62.</summary>
    [CampoCnab("Qtde. Ocorrência 02", 58, 5, TipoCampo.Numerico)]
    public int QuantidadeOcorrencia02 { get; init; }

    /// <summary>Valor da ocorrência 02, posições 63-74 (2 casas decimais).</summary>
    [CampoCnab("Valor Ocorrência 02", 63, 12, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorOcorrencia02 { get; init; }

    /// <summary>Filler, posições 75-86, brancos.</summary>
    [CampoCnab("Filler 75-86", 75, 12, TipoCampo.Filler)]
    public string Filler75A86 { get; init; } = string.Empty;

    /// <summary>Quantidade da ocorrência 03 (rejeitados), posições 87-91.</summary>
    [CampoCnab("Qtde. Ocorrência 03", 87, 5, TipoCampo.Numerico)]
    public int QuantidadeOcorrencia03 { get; init; }

    /// <summary>Filler, posições 92-115, brancos.</summary>
    [CampoCnab("Filler 92-115", 92, 24, TipoCampo.Filler)]
    public string Filler92A115 { get; init; } = string.Empty;

    /// <summary>Quantidade da ocorrência 04 (pagos), posições 116-120.</summary>
    [CampoCnab("Qtde. Ocorrência 04", 116, 5, TipoCampo.Numerico)]
    public int QuantidadeOcorrencia04 { get; init; }

    /// <summary>Valor da ocorrência 04, posições 121-132 (2 casas decimais).</summary>
    [CampoCnab("Valor Ocorrência 04", 121, 12, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorOcorrencia04 { get; init; }

    /// <summary>Filler, posições 133-394, brancos.</summary>
    [CampoCnab("Filler 133-394", 133, 262, TipoCampo.Filler)]
    public string FillerFinal { get; init; } = string.Empty;

    /// <summary>Número sequencial do registro, posições 395-400.</summary>
    [CampoCnab("Número Sequencial do Registro", 395, 6, TipoCampo.Numerico)]
    public int NumeroSequencial { get; init; }
}

/// <summary>Pontos de entrada do Inter CNAB 400 usados pelo gerador e pelo leitor.</summary>
public static class RegistrosInter400
{
    /// <summary>Gera as linhas de remessa CNAB 400 do Inter.</summary>
    /// <param name="remessa">Remessa.</param>
    /// <returns>Linhas sem delimitadores.</returns>
    public static IReadOnlyList<string> Gerar(Remessa400 remessa)
    {
        var conta = remessa.Cedente.Conta;
        var linhas = new List<string>
        {
            RegistroCnab<HeaderRemessaInter400>.Escrever(new HeaderRemessaInter400
            {
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
            var endereco = titulo.Pagador.Endereco;
            linhas.Add(RegistroCnab<DetalheRemessaInter400>.Escrever(new DetalheRemessaInter400
            {
                Carteira = int.Parse(Digitos(titulo.Carteira, 3, "Carteira"), CultureInfo.InvariantCulture),
                Conta = long.Parse(Digitos(conta.Conta, 9, "Conta"), CultureInfo.InvariantCulture),
                DvConta = int.Parse(Digito(conta.DvConta), CultureInfo.InvariantCulture),
                ControleParticipante = titulo.NumeroDocumento,
                CampoMulta = titulo.Multa is null ? 0 : titulo.Multa.Tipo == TipoMulta.Valor ? 1 : 2,
                ValorMulta = titulo.Multa is { Tipo: TipoMulta.Valor } mv ? mv.Valor : 0m,
                PercentualMulta = titulo.Multa is { Tipo: TipoMulta.Percentual } mp ? mp.Valor : 0m,
                DataMulta = titulo.Multa?.DataInicio,
                NossoNumero = DigitosAlfa(titulo.NossoNumero, 11, "Nosso Número"),
                Ocorrencia = remessa.CodigoMovimentoEntrada,
                SeuNumero = titulo.NumeroDocumento,
                DataVencimento = titulo.DataVencimento,
                Valor = titulo.Valor,
                DataEmissao = titulo.DataEmissao,
                CampoJuros = titulo.Juros is null ? 0 : titulo.Juros.Tipo == TipoJuros.ValorDiario ? 1 : 2,
                ValorPorDia = titulo.Juros is { Tipo: TipoJuros.ValorDiario } jv ? jv.Valor : 0m,
                TaxaMensalJuros = titulo.Juros is { Tipo: TipoJuros.PercentualMensal } jm ? jm.Valor : 0m,
                DataMora = titulo.Juros?.DataInicio,
                CampoDescontos = titulo.Desconto is null ? 0 : 1,
                ValorDesconto1 = titulo.Desconto?.Valor ?? 0m,
                DataLimiteDesconto1 = titulo.Desconto?.DataLimite,
                TipoInscricaoPagador = titulo.Pagador.Documento.TipoInscricao,
                DocumentoPagador = titulo.Pagador.Documento.Digitos,
                NomePagador = titulo.Pagador.Nome,
                EnderecoPagador = endereco is null ? string.Empty : $"{endereco.Logradouro} {endereco.Bairro}",
                UfPagador = endereco?.Uf ?? string.Empty,
                CepPagador = endereco is null ? 0 : int.Parse(Digitos(endereco.Cep, 8, "CEP"), CultureInfo.InvariantCulture),
                Mensagem1 = titulo.Instrucoes.Count > 0 ? titulo.Instrucoes[0] : string.Empty,
                NumeroSequencial = sequencial,
            }));

            if (titulo.Pagador.Email is not null || titulo.SacadorAvalista is not null)
            {
                sequencial++;
                linhas.Add(RegistroCnab<RegistroEmailRemessaInter400>.Escrever(new RegistroEmailRemessaInter400
                {
                    Email = titulo.Pagador.Email ?? string.Empty,
                    DocumentoBeneficiarioFinal = titulo.SacadorAvalista?.Documento.Digitos ?? string.Empty,
                    NomeBeneficiarioFinal = titulo.SacadorAvalista?.Nome ?? string.Empty,
                    NumeroSequencial = sequencial,
                }));
            }
        }

        linhas.Add(RegistroCnab<TrailerRemessaInter400>.Escrever(new TrailerRemessaInter400
        {
            QuantidadeBoletos = remessa.Titulos.Count,
            NumeroSequencial = sequencial + 1,
        }));
        return linhas;
    }

    /// <summary>Lê o cabeçalho de retorno do Inter.</summary>
    /// <param name="linha">Linha do header.</param>
    /// <returns>Cabeçalho comum.</returns>
    public static CabecalhoRetorno400 LerCabecalho(string linha)
    {
        var h = RegistroCnab<HeaderRetornoInter400>.Ler(linha, 1);
        return new CabecalhoRetorno400
        {
            Banco = h.Banco,
            CodigoRemessaRetorno = h.CodigoRetorno,
            DataGeracao = h.DataGravacao,
            NumeroSequencialArquivo = h.NumeroSequencial,
            LinhaOriginal = RegistroCnab<HeaderRetornoInter400>.Escrever(h),
        };
    }

    /// <summary>Lê um detalhe de retorno do Inter.</summary>
    /// <param name="linha">Linha de detalhe.</param>
    /// <param name="banco">Banco emissor.</param>
    /// <param name="numeroLinha">Número da linha para erros.</param>
    /// <returns>Item de retorno.</returns>
    public static ItemRetorno400 LerDetalhe(string linha, Banco banco, int numeroLinha)
    {
        var d = RegistroCnab<DetalheRetornoInter400>.Ler(linha, numeroLinha);
        return new ItemRetorno400
        {
            LinhaOriginal = RegistroCnab<DetalheRetornoInter400>.Escrever(d),
            TipoRegistro = d.IdentificacaoRegistro,
            Sequencial = d.NumeroSequencial,
            NossoNumero = d.NossoNumero.Trim(),
            SeuNumero = d.SeuNumero.Trim(),
            Ocorrencia = TabelasOcorrencias400.Obter(banco, d.CodigoOcorrencia.ToString("00", CultureInfo.InvariantCulture)),
            Motivos = d.MotivoRejeicao.Trim().Length == 0 ? [] :
                [new OcorrenciaRetorno("TX", d.MotivoRejeicao.Trim(), FamiliaOcorrencia.Rejeicao)],
            Valores = new ValoresRetorno400
            {
                ValorTitulo = d.ValorTitulo,
                ValorPago = d.ValorPago,
                ValorLiquido = d.ValorPago,
                DataOcorrencia = d.DataOcorrencia,
                DataCredito = d.DataCredito,
                DataVencimento = d.DataVencimento,
            },
        };
    }

    /// <summary>Lê o trailer de retorno do Inter.</summary>
    /// <param name="linha">Linha do trailer.</param>
    /// <returns>Trailer comum.</returns>
    public static TrailerRetorno400 LerTrailer(string linha)
    {
        var t = RegistroCnab<TrailerRetornoInter400>.Ler(linha);
        return new TrailerRetorno400
        {
            TipoRegistro = t.IdentificacaoRegistro,
            CodigoRemessaRetorno = t.CodigoRetorno,
            NumeroSequencialArquivo = t.NumeroSequencial,
            LinhaOriginal = RegistroCnab<TrailerRetornoInter400>.Escrever(t),
        };
    }

    private static string Digito(string? valor) => string.IsNullOrWhiteSpace(valor) ? "0" : valor[..1];

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
            throw new ArgumentException($"{nome} excede {tamanho} posições.", nome);
        }

        return resultado.PadLeft(tamanho, '0');
    }
}
