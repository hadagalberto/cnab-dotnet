namespace Cnab.Layouts.Cnab400.Registros;

using Cnab.Arquivo;

/// <summary>
/// Header de remessa CNAB 400 do Bradesco (registro 0). Manual
/// "Cobrança Bradesco — Versão Português" 4008.524.0121 v17 (28/09/2020),
/// seção "Arquivo Remessa — Header". Fonte:
/// https://assets.bradesco.com.br/content/dam/portal-bradesco/assets/pessoajuridica/pdf/4008-524-0121-layout-cobranca-versao-portugues.pdf
/// </summary>
[RegistroCnab(Codigos400.TamanhoRegistro)]
public sealed record HeaderRemessaBradesco400
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

    /// <summary>
    /// Código da empresa (cedente), posições 27-46: campo numérico 9(20)
    /// alinhado à direita com zeros à esquerda (manual 4008-524-0121 v17).
    /// </summary>
    [CampoCnab("Código da Empresa", 27, 20, TipoCampo.Numerico)]
    public long CodigoEmpresa { get; init; }

    /// <summary>Nome da empresa, posições 47-76.</summary>
    [CampoCnab("Nome da Empresa", 47, 30, TipoCampo.Alfanumerico)]
    public string NomeEmpresa { get; init; } = string.Empty;

    /// <summary>Código do banco, posições 77-79: "237".</summary>
    [CampoCnab("Código do Banco", 77, 3, TipoCampo.Numerico)]
    public int Banco { get; init; } = 237;

    /// <summary>Nome do banco, posições 80-94: "Bradesco".</summary>
    [CampoCnab("Nome do Banco", 80, 15, TipoCampo.Alfanumerico)]
    public string NomeBanco { get; init; } = "Bradesco";

    /// <summary>Data da gravação do arquivo, posições 95-100 (DDMMAA).</summary>
    [CampoCnab("Data da Gravação", 95, 6, TipoCampo.DataDdMmAa)]
    public DateOnly DataGravacao { get; init; }

    /// <summary>Complemento, posições 101-108, brancos.</summary>
    [CampoCnab("Complemento 101-108", 101, 8, TipoCampo.Filler)]
    public string ComplementoInicial { get; init; } = string.Empty;

    /// <summary>Identificação do sistema, posições 109-110: "MX".</summary>
    [CampoCnab("Identificação do Sistema", 109, 2, TipoCampo.Alfanumerico)]
    public string IdentificacaoSistema { get; init; } = "MX";

    /// <summary>Número sequencial da remessa, posições 111-117.</summary>
    [CampoCnab("Número Sequencial de Remessa", 111, 7, TipoCampo.Numerico)]
    public int NumeroSequencialRemessa { get; init; }

    /// <summary>Complemento, posições 118-394, brancos.</summary>
    [CampoCnab("Complemento 118-394", 118, 277, TipoCampo.Filler)]
    public string ComplementoFinal { get; init; } = string.Empty;

    /// <summary>Número sequencial do registro, posições 395-400.</summary>
    [CampoCnab("Número Sequencial do Registro", 395, 6, TipoCampo.Numerico)]
    public int NumeroSequencial { get; init; }
}

/// <summary>
/// Registro de transação (emissão de boleto) da remessa CNAB 400 do Bradesco.
/// Manual 4008.524.0121 v17, seção "Arquivo Remessa — Registro de Transação".
/// </summary>
[RegistroCnab(Codigos400.TamanhoRegistro)]
public sealed record DetalheRemessaBradesco400
{
    /// <summary>Identificação do registro, posição 1: "1".</summary>
    [CampoCnab("Identificação do Registro", 1, 1, TipoCampo.Numerico)]
    public int IdentificacaoRegistro { get; init; } = 1;

    /// <summary>Agência do pagador para débito automático, posições 2-6.</summary>
    [CampoCnab("Agência do Pagador", 2, 5, TipoCampo.Numerico)]
    public int AgenciaPagador { get; init; }

    /// <summary>DV da agência do pagador, posição 7.</summary>
    [CampoCnab("DV da Agência do Pagador", 7, 1, TipoCampo.Numerico)]
    public int DvAgenciaPagador { get; init; }

    /// <summary>Razão da conta, posições 8-12.</summary>
    [CampoCnab("Razão da Conta", 8, 5, TipoCampo.Numerico)]
    public int RazaoConta { get; init; }

    /// <summary>Conta corrente do pagador, posições 13-19.</summary>
    [CampoCnab("Conta do Pagador", 13, 7, TipoCampo.Numerico)]
    public int ContaPagador { get; init; }

    /// <summary>DV da conta do pagador, posição 20.</summary>
    [CampoCnab("DV da Conta do Pagador", 20, 1, TipoCampo.Alfanumerico)]
    public string DvContaPagador { get; init; } = string.Empty;

    /// <summary>Zero fixo, posição 21.</summary>
    [CampoCnab("Zero Fixo 21", 21, 1, TipoCampo.Filler, ConteudoFiller = "0")]
    public string ZeroFixo21 { get; init; } = "0";

    /// <summary>Carteira, posições 22-24.</summary>
    [CampoCnab("Carteira", 22, 3, TipoCampo.Numerico)]
    public int Carteira { get; init; }

    /// <summary>Agência beneficiária sem DV, posições 25-29.</summary>
    [CampoCnab("Agência Beneficiária", 25, 5, TipoCampo.Numerico)]
    public int AgenciaBeneficiaria { get; init; }

    /// <summary>Conta corrente do beneficiário, posições 30-36.</summary>
    [CampoCnab("Conta Corrente", 30, 7, TipoCampo.Numerico)]
    public int ContaCorrente { get; init; }

    /// <summary>DV da conta corrente, posição 37.</summary>
    [CampoCnab("DV da Conta", 37, 1, TipoCampo.Numerico)]
    public int DvConta { get; init; }

    /// <summary>Número de controle do participante, posições 38-62.</summary>
    [CampoCnab("Controle do Participante", 38, 25, TipoCampo.Alfanumerico)]
    public string ControleParticipante { get; init; } = string.Empty;

    /// <summary>Código do banco para débito (237 ou 000), posições 63-65.</summary>
    [CampoCnab("Banco para Débito", 63, 3, TipoCampo.Numerico)]
    public int BancoDebito { get; init; }

    /// <summary>Identificativo de multa, posição 66: 0 sem, 2 com.</summary>
    [CampoCnab("Identificativo de Multa", 66, 1, TipoCampo.Numerico)]
    public int IdentificativoMulta { get; init; }

    /// <summary>Percentual de multa por atraso, posições 67-70 (2 casas decimais).</summary>
    [CampoCnab("Percentual de Multa", 67, 4, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal PercentualMulta { get; init; }

    /// <summary>Nosso número (11 dígitos + DV), posições 71-82.</summary>
    [CampoCnab("Nosso Número", 71, 12, TipoCampo.Alfanumerico)]
    public string NossoNumero { get; init; } = string.Empty;

    /// <summary>Desconto bonificação por dia, posições 83-92.</summary>
    [CampoCnab("Desconto Bonificação por Dia", 83, 10, TipoCampo.Numerico)]
    public long DescontoBonificacaoDia { get; init; }

    /// <summary>Condição para emissão do boleto, posição 93.</summary>
    [CampoCnab("Condição de Emissão", 93, 1, TipoCampo.Numerico)]
    public int CondicaoEmissao { get; init; }

    /// <summary>Emissão de boleto para débito automático, posição 94.</summary>
    [CampoCnab("Emissão para Débito Automático", 94, 1, TipoCampo.Alfanumerico)]
    public string EmissaoDebitoAutomatico { get; init; } = "N";

    /// <summary>Identificação da operação do banco, posições 95-104, brancos.</summary>
    [CampoCnab("Identificação da Operação do Banco", 95, 10, TipoCampo.Filler)]
    public string IdentificacaoOperacaoBanco { get; init; } = string.Empty;

    /// <summary>Indicador de rateio de crédito, posição 105.</summary>
    [CampoCnab("Indicador de Rateio", 105, 1, TipoCampo.Alfanumerico)]
    public string IndicadorRateio { get; init; } = string.Empty;

    /// <summary>Endereçamento para aviso de débito automático, posição 106.</summary>
    [CampoCnab("Endereçamento de Aviso", 106, 1, TipoCampo.Alfanumerico)]
    public string EnderecamentoAviso { get; init; } = string.Empty;

    /// <summary>Pagamento parcial/quantidade de parcelas, posições 107-108.</summary>
    [CampoCnab("Pagamento Parcial", 107, 2, TipoCampo.Numerico)]
    public int PagamentoParcial { get; init; }

    /// <summary>Código de ocorrência de remessa, posições 109-110.</summary>
    [CampoCnab("Código de Ocorrência", 109, 2, TipoCampo.Numerico)]
    public int CodigoOcorrencia { get; init; }

    /// <summary>Número do documento ("seu número"), posições 111-120.</summary>
    [CampoCnab("Número do Documento", 111, 10, TipoCampo.Alfanumerico)]
    public string NumeroDocumento { get; init; } = string.Empty;

    /// <summary>Data de vencimento, posições 121-126 (DDMMAA).</summary>
    [CampoCnab("Data de Vencimento", 121, 6, TipoCampo.DataDdMmAa)]
    public DateOnly? DataVencimento { get; init; }

    /// <summary>Valor do título, posições 127-139 (2 casas decimais).</summary>
    [CampoCnab("Valor do Título", 127, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal Valor { get; init; }

    /// <summary>Banco encarregado da cobrança, posições 140-142.</summary>
    [CampoCnab("Banco Encarregado da Cobrança", 140, 3, TipoCampo.Numerico)]
    public int BancoCobrador { get; init; }

    /// <summary>Agência depositária, posições 143-147.</summary>
    [CampoCnab("Agência Depositária", 143, 5, TipoCampo.Numerico)]
    public int AgenciaDepositaria { get; init; }

    /// <summary>Espécie do título, posições 148-149.</summary>
    [CampoCnab("Espécie do Título", 148, 2, TipoCampo.Numerico)]
    public int Especie { get; init; }

    /// <summary>Identificação, posição 150.</summary>
    [CampoCnab("Identificação", 150, 1, TipoCampo.Alfanumerico)]
    public string Identificacao { get; init; } = "N";

    /// <summary>Data da emissão do título, posições 151-156 (DDMMAA).</summary>
    [CampoCnab("Data de Emissão", 151, 6, TipoCampo.DataDdMmAa)]
    public DateOnly DataEmissao { get; init; }

    /// <summary>Primeira instrução, posições 157-158.</summary>
    [CampoCnab("Instrução 1", 157, 2, TipoCampo.Numerico)]
    public int Instrucao1 { get; init; }

    /// <summary>Segunda instrução, posições 159-160.</summary>
    [CampoCnab("Instrução 2", 159, 2, TipoCampo.Numerico)]
    public int Instrucao2 { get; init; }

    /// <summary>Valor a ser cobrado por dia de atraso, posições 161-173 (2 casas decimais).</summary>
    [CampoCnab("Valor de Mora por Dia", 161, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorMoraDia { get; init; }

    /// <summary>Data limite para concessão de desconto, posições 174-179 (DDMMAA).</summary>
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

    /// <summary>Nome do pagador, posições 235-274.</summary>
    [CampoCnab("Nome do Pagador", 235, 40, TipoCampo.Alfanumerico)]
    public string NomePagador { get; init; } = string.Empty;

    /// <summary>Endereço completo do pagador, posições 275-314.</summary>
    [CampoCnab("Endereço do Pagador", 275, 40, TipoCampo.Alfanumerico)]
    public string EnderecoPagador { get; init; } = string.Empty;

    /// <summary>Primeira mensagem impressa no boleto, posições 315-326.</summary>
    [CampoCnab("Mensagem 1", 315, 12, TipoCampo.Alfanumerico)]
    public string Mensagem1 { get; init; } = string.Empty;

    /// <summary>CEP do sacado, posições 327-334.</summary>
    [CampoCnab("CEP do Pagador", 327, 8, TipoCampo.Alfanumerico)]
    public string CepPagador { get; init; } = string.Empty;

    /// <summary>Nome do cedente/beneficiário final, posições 335-380.</summary>
    [CampoCnab("Nome do Cedente", 335, 46, TipoCampo.Alfanumerico)]
    public string NomeCedente { get; init; } = string.Empty;

    /// <summary>CNPJ do cedente, posições 381-394.</summary>
    [CampoCnab("CNPJ do Cedente", 381, 14, TipoCampo.Alfanumerico)]
    public string CnpjCedente { get; init; } = string.Empty;

    /// <summary>Número sequencial do registro, posições 395-400.</summary>
    [CampoCnab("Número Sequencial do Registro", 395, 6, TipoCampo.Numerico)]
    public int NumeroSequencial { get; init; }
}

/// <summary>Registro de mensagem (tipo 2) da remessa CNAB 400 do Bradesco.</summary>
[RegistroCnab(Codigos400.TamanhoRegistro)]
public sealed record MensagemRemessaBradesco400
{
    /// <summary>Identificação do registro, posição 1: "2".</summary>
    [CampoCnab("Identificação do Registro", 1, 1, TipoCampo.Numerico)]
    public int IdentificacaoRegistro { get; init; } = 2;

    /// <summary>Mensagem 1, posições 2-81.</summary>
    [CampoCnab("Mensagem 1", 2, 80, TipoCampo.Alfanumerico)]
    public string Mensagem1 { get; init; } = string.Empty;

    /// <summary>Mensagem 2, posições 82-161.</summary>
    [CampoCnab("Mensagem 2", 82, 80, TipoCampo.Alfanumerico)]
    public string Mensagem2 { get; init; } = string.Empty;

    /// <summary>Mensagem 3, posições 162-241.</summary>
    [CampoCnab("Mensagem 3", 162, 80, TipoCampo.Alfanumerico)]
    public string Mensagem3 { get; init; } = string.Empty;

    /// <summary>Mensagem 4, posições 242-321.</summary>
    [CampoCnab("Mensagem 4", 242, 80, TipoCampo.Alfanumerico)]
    public string Mensagem4 { get; init; } = string.Empty;

    /// <summary>Data limite do desconto 2, posições 322-327 (DDMMAA).</summary>
    [CampoCnab("Data Limite do Desconto 2", 322, 6, TipoCampo.DataDdMmAa)]
    public DateOnly? DataLimiteDesconto2 { get; init; }

    /// <summary>Valor do desconto 2, posições 328-340 (2 casas decimais).</summary>
    [CampoCnab("Valor do Desconto 2", 328, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorDesconto2 { get; init; }

    /// <summary>Data limite do desconto 3, posições 341-346 (DDMMAA).</summary>
    [CampoCnab("Data Limite do Desconto 3", 341, 6, TipoCampo.DataDdMmAa)]
    public DateOnly? DataLimiteDesconto3 { get; init; }

    /// <summary>Valor do desconto 3, posições 347-359 (2 casas decimais).</summary>
    [CampoCnab("Valor do Desconto 3", 347, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorDesconto3 { get; init; }

    /// <summary>Reservado, posições 360-366, brancos.</summary>
    [CampoCnab("Reservado 360-366", 360, 7, TipoCampo.Filler)]
    public string Reservado { get; init; } = string.Empty;

    /// <summary>Carteira, posições 367-369.</summary>
    [CampoCnab("Carteira", 367, 3, TipoCampo.Alfanumerico)]
    public string Carteira { get; init; } = string.Empty;

    /// <summary>Agência, posições 370-374.</summary>
    [CampoCnab("Agência", 370, 5, TipoCampo.Alfanumerico)]
    public string Agencia { get; init; } = string.Empty;

    /// <summary>Conta, posições 375-381.</summary>
    [CampoCnab("Conta", 375, 7, TipoCampo.Alfanumerico)]
    public string Conta { get; init; } = string.Empty;

    /// <summary>DV da conta, posição 382.</summary>
    [CampoCnab("DV da Conta", 382, 1, TipoCampo.Alfanumerico)]
    public string DvConta { get; init; } = string.Empty;

    /// <summary>Nosso número, posições 383-393.</summary>
    [CampoCnab("Nosso Número", 383, 11, TipoCampo.Alfanumerico)]
    public string NossoNumero { get; init; } = string.Empty;

    /// <summary>DV do nosso número, posição 394.</summary>
    [CampoCnab("DV do Nosso Número", 394, 1, TipoCampo.Alfanumerico)]
    public string DvNossoNumero { get; init; } = string.Empty;

    /// <summary>Número sequencial do registro, posições 395-400.</summary>
    [CampoCnab("Número Sequencial do Registro", 395, 6, TipoCampo.Numerico)]
    public int NumeroSequencial { get; init; }
}

/// <summary>Trailer de remessa CNAB 400 do Bradesco (sem totalizadores).</summary>
[RegistroCnab(Codigos400.TamanhoRegistro)]
public sealed record TrailerRemessaBradesco400
{
    /// <summary>Identificação do registro, posição 1: "9".</summary>
    [CampoCnab("Identificação do Registro", 1, 1, TipoCampo.Numerico)]
    public int IdentificacaoRegistro { get; init; } = 9;

    /// <summary>Complemento, posições 2-394, brancos.</summary>
    [CampoCnab("Complemento 2-394", 2, 393, TipoCampo.Filler)]
    public string Complemento { get; init; } = string.Empty;

    /// <summary>Número sequencial do registro, posições 395-400.</summary>
    [CampoCnab("Número Sequencial do Registro", 395, 6, TipoCampo.Numerico)]
    public int NumeroSequencial { get; init; }
}

/// <summary>Header de retorno CNAB 400 do Bradesco. Manual 4008.524.0121 v17.</summary>
[RegistroCnab(Codigos400.TamanhoRegistro)]
public sealed record HeaderRetornoBradesco400
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

    /// <summary>Código do serviço, posições 10-11: "01".</summary>
    [CampoCnab("Código do Serviço", 10, 2, TipoCampo.Numerico)]
    public int CodigoServico { get; init; } = 1;

    /// <summary>Literal "COBRANCA", posições 12-26.</summary>
    [CampoCnab("Literal Serviço", 12, 15, TipoCampo.Alfanumerico)]
    public string LiteralServico { get; init; } = Codigos400.LiteralCobranca;

    /// <summary>
    /// Código da empresa, posições 27-46. No retorno o Bradesco grava o código
    /// alinhado à esquerda, com brancos à direita (por exemplo "00623" +
    /// 15 brancos), diferente do padrão FEBRABAN de numérico alinhado à direita
    /// com zeros. Declarar X(20) aqui preserva a leitura de arquivos reais —
    /// o gerador de remessa continua escrevendo o campo preenchido com zeros.
    /// </summary>
    [CampoCnab("Código da Empresa", 27, 20, TipoCampo.Alfanumerico)]
    public string CodigoEmpresa { get; init; } = string.Empty;

    /// <summary>Nome da empresa, posições 47-76.</summary>
    [CampoCnab("Nome da Empresa", 47, 30, TipoCampo.Alfanumerico)]
    public string NomeEmpresa { get; init; } = string.Empty;

    /// <summary>Código do banco, posições 77-79: "237".</summary>
    [CampoCnab("Código do Banco", 77, 3, TipoCampo.Numerico)]
    public int Banco { get; init; } = 237;

    /// <summary>Nome do banco, posições 80-94: "Bradesco".</summary>
    [CampoCnab("Nome do Banco", 80, 15, TipoCampo.Alfanumerico)]
    public string NomeBanco { get; init; } = "Bradesco";

    /// <summary>Data da gravação, posições 95-100 (DDMMAA).</summary>
    [CampoCnab("Data da Gravação", 95, 6, TipoCampo.DataDdMmAa)]
    public DateOnly DataGravacao { get; init; }

    /// <summary>Densidade de gravação, posições 101-108: "01600000".</summary>
    [CampoCnab("Densidade de Gravação", 101, 8, TipoCampo.Alfanumerico)]
    public string Densidade { get; init; } = "01600000";

    /// <summary>Número do aviso bancário, posições 109-113.</summary>
    [CampoCnab("Número do Aviso Bancário", 109, 5, TipoCampo.Numerico)]
    public int NumeroAvisoBancario { get; init; }

    /// <summary>Complemento, posições 114-379, brancos.</summary>
    [CampoCnab("Complemento 114-379", 114, 266, TipoCampo.Filler)]
    public string Complemento { get; init; } = string.Empty;

    /// <summary>Data do crédito, posições 380-385 (DDMMAA).</summary>
    [CampoCnab("Data do Crédito", 380, 6, TipoCampo.DataDdMmAa)]
    public DateOnly? DataCredito { get; init; }

    /// <summary>Complemento, posições 386-394, brancos.</summary>
    [CampoCnab("Complemento 386-394", 386, 9, TipoCampo.Filler)]
    public string ComplementoFinal { get; init; } = string.Empty;

    /// <summary>Número sequencial do registro, posições 395-400.</summary>
    [CampoCnab("Número Sequencial do Registro", 395, 6, TipoCampo.Numerico)]
    public int NumeroSequencial { get; init; }
}

/// <summary>
/// Detalhe de retorno CNAB 400 do Bradesco (registro de transação, tipo 1).
/// Motivos de rejeição nas posições 319-328; data de crédito em 296-301.
/// </summary>
[RegistroCnab(Codigos400.TamanhoRegistro)]
public sealed record DetalheRetornoBradesco400
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

    /// <summary>Complemento, posições 18-20, brancos.</summary>
    [CampoCnab("Complemento 18-20", 18, 3, TipoCampo.Filler)]
    public string ComplementoInicial { get; init; } = string.Empty;

    /// <summary>Identificação da empresa beneficiária no banco, posições 21-37.</summary>
    [CampoCnab("Identificação da Empresa no Banco", 21, 17, TipoCampo.Alfanumerico)]
    public string IdentificacaoEmpresaBanco { get; init; } = string.Empty;

    /// <summary>Número de controle do participante, posições 38-62.</summary>
    [CampoCnab("Controle do Participante", 38, 25, TipoCampo.Alfanumerico)]
    public string ControleParticipante { get; init; } = string.Empty;

    /// <summary>Posições 63-70, zeros.</summary>
    [CampoCnab("Zeros 63-70", 63, 8, TipoCampo.Filler, ConteudoFiller = "0")]
    public string Zeros63A70 { get; init; } = "00000000";

    /// <summary>Nosso número, posições 71-82.</summary>
    [CampoCnab("Nosso Número", 71, 12, TipoCampo.Alfanumerico)]
    public string NossoNumero { get; init; } = string.Empty;

    /// <summary>Uso do banco, posições 83-104.</summary>
    [CampoCnab("Uso do Banco 83-104", 83, 22, TipoCampo.Filler)]
    public string UsoBanco { get; init; } = string.Empty;

    /// <summary>Indicador de rateio, posição 105.</summary>
    [CampoCnab("Indicador de Rateio", 105, 1, TipoCampo.Alfanumerico)]
    public string IndicadorRateio { get; init; } = string.Empty;

    /// <summary>Pagamento parcial, posições 106-107.</summary>
    [CampoCnab("Pagamento Parcial", 106, 2, TipoCampo.Numerico)]
    public int PagamentoParcial { get; init; }

    /// <summary>Carteira, posição 108.</summary>
    [CampoCnab("Carteira", 108, 1, TipoCampo.Alfanumerico)]
    public string Carteira { get; init; } = string.Empty;

    /// <summary>Código de ocorrência, posições 109-110.</summary>
    [CampoCnab("Código de Ocorrência", 109, 2, TipoCampo.Numerico)]
    public int CodigoOcorrencia { get; init; }

    /// <summary>Data da ocorrência no banco, posições 111-116 (DDMMAA).</summary>
    [CampoCnab("Data da Ocorrência", 111, 6, TipoCampo.DataDdMmAa)]
    public DateOnly? DataOcorrencia { get; init; }

    /// <summary>Número do documento ("seu número"), posições 117-126.</summary>
    [CampoCnab("Número do Documento", 117, 10, TipoCampo.Alfanumerico)]
    public string NumeroDocumento { get; init; } = string.Empty;

    /// <summary>Identificação do título no banco, posições 127-146.</summary>
    [CampoCnab("Identificação do Título no Banco", 127, 20, TipoCampo.Alfanumerico)]
    public string IdentificacaoTituloBanco { get; init; } = string.Empty;

    /// <summary>Data de vencimento, posições 147-152 (DDMMAA).</summary>
    [CampoCnab("Data de Vencimento", 147, 6, TipoCampo.DataDdMmAa)]
    public DateOnly? DataVencimento { get; init; }

    /// <summary>Valor do título, posições 153-165 (2 casas decimais).</summary>
    [CampoCnab("Valor do Título", 153, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorTitulo { get; init; }

    /// <summary>Banco cobrador (CIP), posições 166-168.</summary>
    [CampoCnab("Banco Cobrador", 166, 3, TipoCampo.Numerico)]
    public int BancoCobrador { get; init; }

    /// <summary>Agência depositária, posições 169-173.</summary>
    [CampoCnab("Agência Depositária", 169, 5, TipoCampo.Numerico)]
    public int AgenciaDepositaria { get; init; }

    /// <summary>Espécie do título, posições 174-175.</summary>
    [CampoCnab("Espécie do Título", 174, 2, TipoCampo.Alfanumerico)]
    public string Especie { get; init; } = string.Empty;

    /// <summary>Despesas de cobrança/tarifa de registro, posições 176-188 (2 casas decimais).</summary>
    [CampoCnab("Tarifa de Cobrança", 176, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal Tarifa { get; init; }

    /// <summary>Outras despesas — custas de protesto, posições 189-201 (2 casas decimais).</summary>
    [CampoCnab("Custas de Protesto", 189, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal CustasProtesto { get; init; }

    /// <summary>Juros da operação em atraso, posições 202-214 (2 casas decimais).</summary>
    [CampoCnab("Juros da Operação", 202, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal JurosOperacao { get; init; }

    /// <summary>IOF devido, posições 215-227 (2 casas decimais).</summary>
    [CampoCnab("IOF Devido", 215, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal Iof { get; init; }

    /// <summary>Abatimento concedido, posições 228-240 (2 casas decimais).</summary>
    [CampoCnab("Abatimento Concedido", 228, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal Abatimento { get; init; }

    /// <summary>Desconto concedido, posições 241-253 (2 casas decimais).</summary>
    [CampoCnab("Desconto Concedido", 241, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal Desconto { get; init; }

    /// <summary>Valor pago, posições 254-266 (2 casas decimais).</summary>
    [CampoCnab("Valor Pago", 254, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorPago { get; init; }

    /// <summary>Juros de mora, posições 267-279 (2 casas decimais).</summary>
    [CampoCnab("Juros de Mora", 267, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal JurosMora { get; init; }

    /// <summary>Outros créditos, posições 280-292 (2 casas decimais).</summary>
    [CampoCnab("Outros Créditos", 280, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal OutrosCreditos { get; init; }

    /// <summary>Complemento, posições 293-294, brancos.</summary>
    [CampoCnab("Complemento 293-294", 293, 2, TipoCampo.Filler)]
    public string Complemento293A294 { get; init; } = string.Empty;

    /// <summary>Aceito/desprezado (ocorrências 19/25), posição 295.</summary>
    [CampoCnab("Aceite/Desprezado", 295, 1, TipoCampo.Alfanumerico)]
    public string AceiteDesprezado { get; init; } = string.Empty;

    /// <summary>Data do crédito, posições 296-301 (DDMMAA).</summary>
    [CampoCnab("Data do Crédito", 296, 6, TipoCampo.DataDdMmAa)]
    public DateOnly? DataCredito { get; init; }

    /// <summary>Origem do pagamento, posições 302-304.</summary>
    [CampoCnab("Origem do Pagamento", 302, 3, TipoCampo.Numerico)]
    public int OrigemPagamento { get; init; }

    /// <summary>Complemento, posições 305-314, brancos.</summary>
    [CampoCnab("Complemento 305-314", 305, 10, TipoCampo.Filler)]
    public string Complemento305A314 { get; init; } = string.Empty;

    /// <summary>Código do banco, posições 315-318.</summary>
    [CampoCnab("Código do Banco", 315, 4, TipoCampo.Alfanumerico)]
    public string CodigoBanco { get; init; } = string.Empty;

    /// <summary>Motivos das rejeições, posições 319-328 (até 5 códigos de 2 posições).</summary>
    [CampoCnab("Motivos das Rejeições", 319, 10, TipoCampo.Alfanumerico)]
    public string MotivosRejeicao { get; init; } = string.Empty;

    /// <summary>Complemento, posições 329-368, brancos.</summary>
    [CampoCnab("Complemento 329-368", 329, 40, TipoCampo.Filler)]
    public string Complemento329A368 { get; init; } = string.Empty;

    /// <summary>Número do cartório, posições 369-370.</summary>
    [CampoCnab("Número do Cartório", 369, 2, TipoCampo.Numerico)]
    public int NumeroCartorio { get; init; }

    /// <summary>Número do protocolo, posições 371-380.</summary>
    [CampoCnab("Número do Protocolo", 371, 10, TipoCampo.Alfanumerico)]
    public string NumeroProtocolo { get; init; } = string.Empty;

    /// <summary>Complemento, posições 381-394, brancos.</summary>
    [CampoCnab("Complemento 381-394", 381, 14, TipoCampo.Filler)]
    public string ComplementoFinal { get; init; } = string.Empty;

    /// <summary>Número sequencial do registro, posições 395-400.</summary>
    [CampoCnab("Número Sequencial do Registro", 395, 6, TipoCampo.Numerico)]
    public int NumeroSequencial { get; init; }
}

/// <summary>Trailer de retorno CNAB 400 do Bradesco. Manual 4008.524.0121 v17.</summary>
[RegistroCnab(Codigos400.TamanhoRegistro)]
public sealed record TrailerRetornoBradesco400
{
    /// <summary>Identificação do registro, posição 1: "9".</summary>
    [CampoCnab("Identificação do Registro", 1, 1, TipoCampo.Numerico)]
    public int IdentificacaoRegistro { get; init; } = 9;

    /// <summary>Identificação do arquivo retorno, posição 2: "2".</summary>
    [CampoCnab("Identificação do Arquivo-Retorno", 2, 1, TipoCampo.Numerico)]
    public int IdentificacaoRetorno { get; init; } = 2;

    /// <summary>Código do serviço, posições 3-4: "01".</summary>
    [CampoCnab("Código do Serviço", 3, 2, TipoCampo.Numerico)]
    public int CodigoServico { get; init; } = 1;

    /// <summary>Código do banco, posições 5-7: "237".</summary>
    [CampoCnab("Código do Banco", 5, 3, TipoCampo.Numerico)]
    public int Banco { get; init; } = 237;

    /// <summary>Complemento, posições 8-17, brancos.</summary>
    [CampoCnab("Complemento 8-17", 8, 10, TipoCampo.Filler)]
    public string Complemento8A17 { get; init; } = string.Empty;

    /// <summary>Quantidade de títulos em cobrança, posições 18-25.</summary>
    [CampoCnab("Qtde. de Títulos em Cobrança", 18, 8, TipoCampo.Numerico)]
    public int QuantidadeTitulos { get; init; }

    /// <summary>Valor total em cobrança, posições 26-39 (2 casas decimais).</summary>
    [CampoCnab("Valor Total em Cobrança", 26, 14, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorTotal { get; init; }

    /// <summary>Número do aviso bancário, posições 40-47.</summary>
    [CampoCnab("Número do Aviso Bancário", 40, 8, TipoCampo.Alfanumerico)]
    public string NumeroAvisoBancario { get; init; } = string.Empty;

    /// <summary>Complemento, posições 48-57, brancos.</summary>
    [CampoCnab("Complemento 48-57", 48, 10, TipoCampo.Filler)]
    public string Complemento48A57 { get; init; } = string.Empty;

    /// <summary>Quantidade de ocorrências 02, posições 58-62.</summary>
    [CampoCnab("Qtde. Ocorrência 02", 58, 5, TipoCampo.Numerico)]
    public int QuantidadeEntradasConfirmadas { get; init; }

    /// <summary>Valor das ocorrências 02, posições 63-74 (2 casas decimais).</summary>
    [CampoCnab("Valor Ocorrência 02", 63, 12, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorEntradasConfirmadas { get; init; }

    /// <summary>Valor das liquidações (ocorrência 06), posições 75-86 (2 casas decimais).</summary>
    [CampoCnab("Valor Liquidações", 75, 12, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorLiquidacoes { get; init; }

    /// <summary>Quantidade de liquidações (ocorrência 06), posições 87-91.</summary>
    [CampoCnab("Qtde. Liquidações", 87, 5, TipoCampo.Numerico)]
    public int QuantidadeLiquidacoes { get; init; }

    /// <summary>Valor das baixas, posições 92-103 (2 casas decimais).</summary>
    [CampoCnab("Valor Baixas", 92, 12, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorBaixas { get; init; }

    /// <summary>Complemento, posições 104-362, brancos.</summary>
    [CampoCnab("Complemento 104-362", 104, 259, TipoCampo.Filler)]
    public string ComplementoCentral { get; init; } = string.Empty;

    /// <summary>Valor total dos rateios, posições 363-377 (2 casas decimais).</summary>
    [CampoCnab("Valor Total dos Rateios", 363, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorTotalRateios { get; init; }

    /// <summary>Quantidade total de rateios, posições 378-385.</summary>
    [CampoCnab("Qtde. Total de Rateios", 378, 8, TipoCampo.Numerico)]
    public int QuantidadeTotalRateios { get; init; }

    /// <summary>Complemento, posições 386-394, brancos.</summary>
    [CampoCnab("Complemento 386-394", 386, 9, TipoCampo.Filler)]
    public string ComplementoFinal { get; init; } = string.Empty;

    /// <summary>Número sequencial do registro, posições 395-400.</summary>
    [CampoCnab("Número Sequencial do Registro", 395, 6, TipoCampo.Numerico)]
    public int NumeroSequencial { get; init; }
}
