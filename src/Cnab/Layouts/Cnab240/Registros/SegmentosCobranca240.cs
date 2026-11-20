namespace Cnab.Layouts.Cnab240.Registros;

using Cnab.Arquivo;

/// <summary>
/// Segmento P — dados do título (obrigatório na remessa de cobrança).
/// FEBRABAN CNAB 240 V10.9, seção 3.2, "Registro Detalhe - Segmento P".
/// </summary>
[RegistroCnab(Codigos240.TamanhoRegistro)]
public sealed record SegmentoP240
{
    /// <summary>Código do banco na compensação (G001), posições 1-3.</summary>
    [CampoCnab("Código do Banco", 1, 3, TipoCampo.Numerico)]
    public int Banco { get; init; }

    /// <summary>Número do lote (G002), posições 4-7.</summary>
    [CampoCnab("Lote de Serviço", 4, 4, TipoCampo.Numerico)]
    public int Lote { get; init; }

    /// <summary>Tipo de registro (G003), posição 8; "3".</summary>
    [CampoCnab("Tipo de Registro", 8, 1, TipoCampo.Numerico)]
    public int TipoRegistro { get; init; }

    /// <summary>Número sequencial do registro no lote (G038), posições 9-13.</summary>
    [CampoCnab("Número Sequencial do Registro no Lote", 9, 5, TipoCampo.Numerico)]
    public int Sequencial { get; init; }

    /// <summary>Código do segmento (G039), posição 14; "P".</summary>
    [CampoCnab("Código do Segmento", 14, 1, TipoCampo.Alfanumerico)]
    public string Segmento { get; init; } = "P";

    /// <summary>Uso exclusivo FEBRABAN/CNAB (G004), posição 15, branco.</summary>
    [CampoCnab("Filler CNAB", 15, 1, TipoCampo.Filler)]
    public string Filler15 { get; init; } = string.Empty;

    /// <summary>Código de movimento remessa (C004), posições 16-17.</summary>
    [CampoCnab("Código de Movimento Remessa", 16, 2, TipoCampo.Numerico)]
    public int CodigoMovimento { get; init; }

    /// <summary>Agência mantenedora da conta (G008), posições 18-22.</summary>
    [CampoCnab("Agência Mantenedora da Conta", 18, 5, TipoCampo.Numerico)]
    public int Agencia { get; init; }

    /// <summary>DV da agência (G009), posição 23.</summary>
    [CampoCnab("Dígito Verificador da Agência", 23, 1, TipoCampo.Alfanumerico)]
    public string DvAgencia { get; init; } = string.Empty;

    /// <summary>Número da conta corrente (G010), posições 24-35.</summary>
    [CampoCnab("Número da Conta Corrente", 24, 12, TipoCampo.Numerico)]
    public long Conta { get; init; }

    /// <summary>DV da conta (G011), posição 36.</summary>
    [CampoCnab("Dígito Verificador da Conta", 36, 1, TipoCampo.Alfanumerico)]
    public string DvConta { get; init; } = string.Empty;

    /// <summary>DV da agência/conta (G012), posição 37.</summary>
    [CampoCnab("Dígito Verificador da Ag/Conta", 37, 1, TipoCampo.Alfanumerico)]
    public string DvAgenciaConta { get; init; } = string.Empty;

    /// <summary>Identificação do título no banco / nosso número (G069), posições 38-57.</summary>
    [CampoCnab("Identificação do Título no Banco", 38, 20, TipoCampo.Alfanumerico)]
    public string NossoNumero { get; init; } = string.Empty;

    /// <summary>Código da carteira (C006), posição 58.</summary>
    [CampoCnab("Código da Carteira", 58, 1, TipoCampo.Numerico)]
    public int Carteira { get; init; }

    /// <summary>Forma de cadastramento do título no banco (C007), posição 59.</summary>
    [CampoCnab("Forma de Cadastramento", 59, 1, TipoCampo.Numerico)]
    public int FormaCadastramento { get; init; }

    /// <summary>Tipo de documento (C008), posição 60.</summary>
    [CampoCnab("Tipo de Documento", 60, 1, TipoCampo.Alfanumerico)]
    public string TipoDocumento { get; init; } = string.Empty;

    /// <summary>Identificação da emissão do boleto (C009), posição 61.</summary>
    [CampoCnab("Identificação da Emissão do Boleto", 61, 1, TipoCampo.Numerico)]
    public int IdentificacaoEmissao { get; init; }

    /// <summary>Identificação da distribuição do boleto, posição 62.</summary>
    [CampoCnab("Identificação da Distribuição", 62, 1, TipoCampo.Alfanumerico)]
    public string IdentificacaoDistribuicao { get; init; } = string.Empty;

    /// <summary>Número do documento de cobrança (C011), posições 63-77.</summary>
    [CampoCnab("Número do Documento de Cobrança", 63, 15, TipoCampo.Alfanumerico)]
    public string NumeroDocumento { get; init; } = string.Empty;

    /// <summary>Data de vencimento do título (C012), posições 78-85.</summary>
    [CampoCnab("Data de Vencimento do Título", 78, 8, TipoCampo.DataDdMmAaaa)]
    public DateOnly? DataVencimento { get; init; }

    /// <summary>Valor nominal do título (G070), posições 86-100, 2 decimais.</summary>
    [CampoCnab("Valor Nominal do Título", 86, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal Valor { get; init; }

    /// <summary>Agência encarregada da cobrança (G008), posições 101-105.</summary>
    [CampoCnab("Agência Encarregada da Cobrança", 101, 5, TipoCampo.Numerico)]
    public int AgenciaCobradora { get; init; }

    /// <summary>DV da agência cobradora (G009), posição 106.</summary>
    [CampoCnab("Dígito Verificador da Agência Cobradora", 106, 1, TipoCampo.Alfanumerico)]
    public string DvAgenciaCobradora { get; init; } = string.Empty;

    /// <summary>Espécie do título, posições 107-108.</summary>
    [CampoCnab("Espécie do Título", 107, 2, TipoCampo.Numerico)]
    public int Especie { get; init; }

    /// <summary>Aceite do título, posição 109.</summary>
    [CampoCnab("Aceite do Título", 109, 1, TipoCampo.Alfanumerico)]
    public string Aceite { get; init; } = string.Empty;

    /// <summary>Data da emissão do título (G071), posições 110-117.</summary>
    [CampoCnab("Data de Emissão do Título", 110, 8, TipoCampo.DataDdMmAaaa)]
    public DateOnly DataEmissao { get; init; }

    /// <summary>Código do juros de mora, posição 118.</summary>
    [CampoCnab("Código do Juros de Mora", 118, 1, TipoCampo.Numerico)]
    public int CodigoJurosMora { get; init; }

    /// <summary>Data do juros de mora, posições 119-126.</summary>
    [CampoCnab("Data do Juros de Mora", 119, 8, TipoCampo.DataDdMmAaaa)]
    public DateOnly? DataJurosMora { get; init; }

    /// <summary>Juros de mora por dia/taxa, posições 127-141, 2 decimais.</summary>
    [CampoCnab("Juros de Mora por Dia/Taxa", 127, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal? ValorJurosMora { get; init; }

    /// <summary>Código do desconto 1, posição 142.</summary>
    [CampoCnab("Código do Desconto 1", 142, 1, TipoCampo.Numerico)]
    public int CodigoDesconto { get; init; }

    /// <summary>Data do desconto 1, posições 143-150.</summary>
    [CampoCnab("Data do Desconto 1", 143, 8, TipoCampo.DataDdMmAaaa)]
    public DateOnly? DataDesconto { get; init; }

    /// <summary>Valor/percentual do desconto 1, posições 151-165, 2 decimais.</summary>
    [CampoCnab("Desconto 1", 151, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal? ValorDesconto { get; init; }

    /// <summary>Valor do IOF a ser recolhido, posições 166-180, 2 decimais.</summary>
    [CampoCnab("Valor do IOF", 166, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal? ValorIof { get; init; }

    /// <summary>Valor do abatimento, posições 181-195, 2 decimais.</summary>
    [CampoCnab("Valor do Abatimento", 181, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal? ValorAbatimento { get; init; }

    /// <summary>Identificação do título na empresa (G072), posições 196-220.</summary>
    [CampoCnab("Identificação do Título na Empresa", 196, 25, TipoCampo.Alfanumerico)]
    public string IdentificacaoTituloEmpresa { get; init; } = string.Empty;

    /// <summary>Código para protesto, posição 221.</summary>
    [CampoCnab("Código para Protesto", 221, 1, TipoCampo.Numerico)]
    public int CodigoProtesto { get; init; }

    /// <summary>Número de dias para protesto, posições 222-223.</summary>
    [CampoCnab("Número de Dias para Protesto", 222, 2, TipoCampo.Numerico)]
    public int DiasProtesto { get; init; }

    /// <summary>Código para baixa/devolução, posição 224.</summary>
    [CampoCnab("Código para Baixa/Devolução", 224, 1, TipoCampo.Numerico)]
    public int CodigoBaixaDevolucao { get; init; }

    /// <summary>Número de dias para baixa/devolução, posições 225-227.</summary>
    [CampoCnab("Número de Dias para Baixa/Devolução", 225, 3, TipoCampo.Alfanumerico)]
    public string DiasBaixaDevolucao { get; init; } = string.Empty;

    /// <summary>Código da moeda, posições 228-229.</summary>
    [CampoCnab("Código da Moeda", 228, 2, TipoCampo.Numerico)]
    public int CodigoMoeda { get; init; }

    /// <summary>Número do contrato da operação de crédito (C030), posições 230-239.</summary>
    [CampoCnab("Número do Contrato da Operação de Crédito", 230, 10, TipoCampo.Alfanumerico)]
    public string NumeroContrato { get; init; } = string.Empty;

    /// <summary>Uso livre banco/empresa ou autorização de pagamento parcial (C077), posição 240.</summary>
    [CampoCnab("Uso Livre Banco/Empresa", 240, 1, TipoCampo.Alfanumerico)]
    public string UsoLivre { get; init; } = string.Empty;
}

/// <summary>
/// Segmento Q — pagador e sacador/avalista (obrigatório na remessa de cobrança).
/// FEBRABAN CNAB 240 V10.9, seção 3.2.
/// </summary>
[RegistroCnab(Codigos240.TamanhoRegistro)]
public sealed record SegmentoQ240
{
    /// <summary>Código do banco na compensação (G001), posições 1-3.</summary>
    [CampoCnab("Código do Banco", 1, 3, TipoCampo.Numerico)]
    public int Banco { get; init; }

    /// <summary>Número do lote (G002), posições 4-7.</summary>
    [CampoCnab("Lote de Serviço", 4, 4, TipoCampo.Numerico)]
    public int Lote { get; init; }

    /// <summary>Tipo de registro (G003), posição 8; "3".</summary>
    [CampoCnab("Tipo de Registro", 8, 1, TipoCampo.Numerico)]
    public int TipoRegistro { get; init; }

    /// <summary>Número sequencial do registro no lote (G038), posições 9-13.</summary>
    [CampoCnab("Número Sequencial do Registro no Lote", 9, 5, TipoCampo.Numerico)]
    public int Sequencial { get; init; }

    /// <summary>Código do segmento (G039), posição 14; "Q".</summary>
    [CampoCnab("Código do Segmento", 14, 1, TipoCampo.Alfanumerico)]
    public string Segmento { get; init; } = "Q";

    /// <summary>Uso exclusivo FEBRABAN/CNAB (G004), posição 15, branco.</summary>
    [CampoCnab("Filler CNAB", 15, 1, TipoCampo.Filler)]
    public string Filler15 { get; init; } = string.Empty;

    /// <summary>Código de movimento remessa (C004), posições 16-17.</summary>
    [CampoCnab("Código de Movimento Remessa", 16, 2, TipoCampo.Numerico)]
    public int CodigoMovimento { get; init; }

    /// <summary>Tipo de inscrição do pagador (G005), posição 18.</summary>
    [CampoCnab("Tipo de Inscrição do Pagador", 18, 1, TipoCampo.Numerico)]
    public int TipoInscricaoPagador { get; init; }

    /// <summary>Número de inscrição do pagador (G006), posições 19-33.</summary>
    [CampoCnab("Número de Inscrição do Pagador", 19, 15, TipoCampo.Alfanumerico)]
    public string NumeroInscricaoPagador { get; init; } = string.Empty;

    /// <summary>Nome do pagador (G013), posições 34-73.</summary>
    [CampoCnab("Nome do Pagador", 34, 40, TipoCampo.Alfanumerico)]
    public string NomePagador { get; init; } = string.Empty;

    /// <summary>Endereço do pagador (G032), posições 74-113.</summary>
    [CampoCnab("Endereço do Pagador", 74, 40, TipoCampo.Alfanumerico)]
    public string EnderecoPagador { get; init; } = string.Empty;

    /// <summary>Bairro do pagador (G032), posições 114-128.</summary>
    [CampoCnab("Bairro do Pagador", 114, 15, TipoCampo.Alfanumerico)]
    public string BairroPagador { get; init; } = string.Empty;

    /// <summary>CEP do pagador (G034), posições 129-133.</summary>
    [CampoCnab("CEP do Pagador", 129, 5, TipoCampo.Numerico)]
    public int CepPagador { get; init; }

    /// <summary>Sufixo do CEP (G035), posições 134-136.</summary>
    [CampoCnab("Sufixo do CEP", 134, 3, TipoCampo.Numerico)]
    public int SufixoCep { get; init; }

    /// <summary>Cidade do pagador (G033), posições 137-151.</summary>
    [CampoCnab("Cidade do Pagador", 137, 15, TipoCampo.Alfanumerico)]
    public string CidadePagador { get; init; } = string.Empty;

    /// <summary>Unidade da federação do pagador (G036), posições 152-153.</summary>
    [CampoCnab("UF do Pagador", 152, 2, TipoCampo.Alfanumerico)]
    public string UfPagador { get; init; } = string.Empty;

    /// <summary>Tipo de inscrição do sacador/avalista (G005), posição 154.</summary>
    [CampoCnab("Tipo de Inscrição do Sacador/Avalista", 154, 1, TipoCampo.Numerico)]
    public int TipoInscricaoSacador { get; init; }

    /// <summary>Número de inscrição do sacador/avalista (G006), posições 155-169.</summary>
    [CampoCnab("Número de Inscrição do Sacador/Avalista", 155, 15, TipoCampo.Alfanumerico)]
    public string NumeroInscricaoSacador { get; init; } = string.Empty;

    /// <summary>Nome do sacador/avalista (G013), posições 170-209.</summary>
    [CampoCnab("Nome do Sacador/Avalista", 170, 40, TipoCampo.Alfanumerico)]
    public string NomeSacador { get; init; } = string.Empty;

    /// <summary>Código do banco correspondente (C031), posições 210-212.</summary>
    [CampoCnab("Código do Banco Correspondente", 210, 3, TipoCampo.Numerico)]
    public int BancoCorrespondente { get; init; }

    /// <summary>Nosso número no banco correspondente (C032), posições 213-232.</summary>
    [CampoCnab("Nosso Número no Banco Correspondente", 213, 20, TipoCampo.Alfanumerico)]
    public string NossoNumeroCorrespondente { get; init; } = string.Empty;

    /// <summary>Uso exclusivo FEBRABAN/CNAB (G004), posições 233-240, brancos.</summary>
    [CampoCnab("Filler CNAB 233-240", 233, 8, TipoCampo.Filler)]
    public string FillerFinal { get; init; } = string.Empty;
}

/// <summary>
/// Segmento R — descontos 2/3, multa, mensagens e débito automático (opcional
/// na remessa de cobrança). FEBRABAN CNAB 240 V10.9, seção 3.2.
/// </summary>
[RegistroCnab(Codigos240.TamanhoRegistro)]
public sealed record SegmentoR240
{
    /// <summary>Código do banco na compensação (G001), posições 1-3.</summary>
    [CampoCnab("Código do Banco", 1, 3, TipoCampo.Numerico)]
    public int Banco { get; init; }

    /// <summary>Número do lote (G002), posições 4-7.</summary>
    [CampoCnab("Lote de Serviço", 4, 4, TipoCampo.Numerico)]
    public int Lote { get; init; }

    /// <summary>Tipo de registro (G003), posição 8; "3".</summary>
    [CampoCnab("Tipo de Registro", 8, 1, TipoCampo.Numerico)]
    public int TipoRegistro { get; init; }

    /// <summary>Número sequencial do registro no lote (G038), posições 9-13.</summary>
    [CampoCnab("Número Sequencial do Registro no Lote", 9, 5, TipoCampo.Numerico)]
    public int Sequencial { get; init; }

    /// <summary>Código do segmento (G039), posição 14; "R".</summary>
    [CampoCnab("Código do Segmento", 14, 1, TipoCampo.Alfanumerico)]
    public string Segmento { get; init; } = "R";

    /// <summary>Uso exclusivo FEBRABAN/CNAB (G004), posição 15, branco.</summary>
    [CampoCnab("Filler CNAB", 15, 1, TipoCampo.Filler)]
    public string Filler15 { get; init; } = string.Empty;

    /// <summary>Código de movimento remessa (C004), posições 16-17.</summary>
    [CampoCnab("Código de Movimento Remessa", 16, 2, TipoCampo.Numerico)]
    public int CodigoMovimento { get; init; }

    /// <summary>Código do desconto 2 (C021), posição 18.</summary>
    [CampoCnab("Código do Desconto 2", 18, 1, TipoCampo.Numerico)]
    public int CodigoDesconto2 { get; init; }

    /// <summary>Data do desconto 2 (C022), posições 19-26.</summary>
    [CampoCnab("Data do Desconto 2", 19, 8, TipoCampo.DataDdMmAaaa)]
    public DateOnly? DataDesconto2 { get; init; }

    /// <summary>Valor/percentual do desconto 2 (C023), posições 27-41, 2 decimais.</summary>
    [CampoCnab("Desconto 2", 27, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal? ValorDesconto2 { get; init; }

    /// <summary>Código do desconto 3 (C021), posição 42.</summary>
    [CampoCnab("Código do Desconto 3", 42, 1, TipoCampo.Numerico)]
    public int CodigoDesconto3 { get; init; }

    /// <summary>Data do desconto 3 (C022), posições 43-50.</summary>
    [CampoCnab("Data do Desconto 3", 43, 8, TipoCampo.DataDdMmAaaa)]
    public DateOnly? DataDesconto3 { get; init; }

    /// <summary>Valor/percentual do desconto 3 (C023), posições 51-65, 2 decimais.</summary>
    [CampoCnab("Desconto 3", 51, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal? ValorDesconto3 { get; init; }

    /// <summary>Código da multa (G073), posição 66.</summary>
    [CampoCnab("Código da Multa", 66, 1, TipoCampo.Alfanumerico)]
    public string CodigoMulta { get; init; } = string.Empty;

    /// <summary>Data da multa (G074), posições 67-74.</summary>
    [CampoCnab("Data da Multa", 67, 8, TipoCampo.DataDdMmAaaa)]
    public DateOnly? DataMulta { get; init; }

    /// <summary>Valor/percentual da multa (G075), posições 75-89, 2 decimais.</summary>
    [CampoCnab("Valor da Multa", 75, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal? ValorMulta { get; init; }

    /// <summary>Informação ao pagador (C036), posições 90-99.</summary>
    [CampoCnab("Informação ao Pagador", 90, 10, TipoCampo.Alfanumerico)]
    public string InformacaoPagador { get; init; } = string.Empty;

    /// <summary>Mensagem 3 (C037), posições 100-139.</summary>
    [CampoCnab("Mensagem 3", 100, 40, TipoCampo.Alfanumerico)]
    public string Mensagem3 { get; init; } = string.Empty;

    /// <summary>Mensagem 4 (C037), posições 140-179.</summary>
    [CampoCnab("Mensagem 4", 140, 40, TipoCampo.Alfanumerico)]
    public string Mensagem4 { get; init; } = string.Empty;

    /// <summary>Uso exclusivo FEBRABAN/CNAB (G004), posições 180-199, brancos.</summary>
    [CampoCnab("Filler CNAB 180-199", 180, 20, TipoCampo.Filler)]
    public string Filler180A199 { get; init; } = string.Empty;

    /// <summary>Código da ocorrência do pagador (C038), posições 200-207.</summary>
    [CampoCnab("Código da Ocorrência do Pagador", 200, 8, TipoCampo.Alfanumerico)]
    public string CodigoOcorrenciaPagador { get; init; } = string.Empty;

    /// <summary>Código do banco na conta do débito (G001), posições 208-210.</summary>
    [CampoCnab("Banco para Débito", 208, 3, TipoCampo.Numerico)]
    public int BancoDebito { get; init; }

    /// <summary>Código da agência do débito (G008), posições 211-215.</summary>
    [CampoCnab("Agência para Débito", 211, 5, TipoCampo.Numerico)]
    public int AgenciaDebito { get; init; }

    /// <summary>DV da agência do débito (G009), posição 216.</summary>
    [CampoCnab("DV da Agência para Débito", 216, 1, TipoCampo.Alfanumerico)]
    public string DvAgenciaDebito { get; init; } = string.Empty;

    /// <summary>Conta corrente para débito (G010), posições 217-228.</summary>
    [CampoCnab("Conta para Débito", 217, 12, TipoCampo.Numerico)]
    public long ContaDebito { get; init; }

    /// <summary>DV da conta do débito (G011), posição 229.</summary>
    [CampoCnab("DV da Conta para Débito", 229, 1, TipoCampo.Alfanumerico)]
    public string DvContaDebito { get; init; } = string.Empty;

    /// <summary>DV da agência/conta do débito (G012), posição 230.</summary>
    [CampoCnab("DV da Ag/Conta para Débito", 230, 1, TipoCampo.Alfanumerico)]
    public string DvAgenciaContaDebito { get; init; } = string.Empty;

    /// <summary>Aviso para débito automático (C039), posição 231.</summary>
    [CampoCnab("Aviso para Débito Automático", 231, 1, TipoCampo.Numerico)]
    public int AvisoDebitoAutomatico { get; init; }

    /// <summary>Uso exclusivo FEBRABAN/CNAB (G004), posições 232-240, brancos.</summary>
    [CampoCnab("Filler CNAB 232-240", 232, 9, TipoCampo.Filler)]
    public string FillerFinal { get; init; } = string.Empty;
}

/// <summary>
/// Segmento T —identificação do título no retorno de cobrança (obrigatório).
/// FEBRABAN CNAB 240 V10.9, seção 3.2. O código de movimento retorno (C044)
/// fica nas posições 16-17 e o motivo da ocorrência (C047) em 214-223.
/// </summary>
[RegistroCnab(Codigos240.TamanhoRegistro)]
public sealed record SegmentoT240
{
    /// <summary>Código do banco na compensação (G001), posições 1-3.</summary>
    [CampoCnab("Código do Banco", 1, 3, TipoCampo.Numerico)]
    public int Banco { get; init; }

    /// <summary>Número do lote (G002), posições 4-7.</summary>
    [CampoCnab("Lote de Serviço", 4, 4, TipoCampo.Numerico)]
    public int Lote { get; init; }

    /// <summary>Tipo de registro (G003), posição 8; "3".</summary>
    [CampoCnab("Tipo de Registro", 8, 1, TipoCampo.Numerico)]
    public int TipoRegistro { get; init; }

    /// <summary>Número sequencial do registro no lote (G038), posições 9-13.</summary>
    [CampoCnab("Número Sequencial do Registro no Lote", 9, 5, TipoCampo.Numerico)]
    public int Sequencial { get; init; }

    /// <summary>Código do segmento (G039), posição 14; "T".</summary>
    [CampoCnab("Código do Segmento", 14, 1, TipoCampo.Alfanumerico)]
    public string Segmento { get; init; } = "T";

    /// <summary>Uso exclusivo FEBRABAN/CNAB (G004), posição 15, branco.</summary>
    [CampoCnab("Filler CNAB", 15, 1, TipoCampo.Filler)]
    public string Filler15 { get; init; } = string.Empty;

    /// <summary>Código de movimento retorno (C044), posições 16-17.</summary>
    [CampoCnab("Código de Movimento Retorno", 16, 2, TipoCampo.Numerico)]
    public int CodigoMovimento { get; init; }

    /// <summary>Agência mantenedora da conta (G008), posições 18-22.</summary>
    [CampoCnab("Agência Mantenedora da Conta", 18, 5, TipoCampo.Numerico)]
    public int Agencia { get; init; }

    /// <summary>DV da agência (G009), posição 23.</summary>
    [CampoCnab("Dígito Verificador da Agência", 23, 1, TipoCampo.Alfanumerico)]
    public string DvAgencia { get; init; } = string.Empty;

    /// <summary>Número da conta corrente (G010), posições 24-35.</summary>
    [CampoCnab("Número da Conta Corrente", 24, 12, TipoCampo.Numerico)]
    public long Conta { get; init; }

    /// <summary>DV da conta (G011), posição 36.</summary>
    [CampoCnab("Dígito Verificador da Conta", 36, 1, TipoCampo.Alfanumerico)]
    public string DvConta { get; init; } = string.Empty;

    /// <summary>DV da agência/conta (G012), posição 37.</summary>
    [CampoCnab("Dígito Verificador da Ag/Conta", 37, 1, TipoCampo.Alfanumerico)]
    public string DvAgenciaConta { get; init; } = string.Empty;

    /// <summary>Identificação do título no banco / nosso número (G069), posições 38-57.</summary>
    [CampoCnab("Identificação do Título no Banco", 38, 20, TipoCampo.Alfanumerico)]
    public string NossoNumero { get; init; } = string.Empty;

    /// <summary>Código da carteira (C006), posição 58.</summary>
    [CampoCnab("Código da Carteira", 58, 1, TipoCampo.Numerico)]
    public int Carteira { get; init; }

    /// <summary>Número do documento de cobrança (C011), posições 59-73.</summary>
    [CampoCnab("Número do Documento de Cobrança", 59, 15, TipoCampo.Alfanumerico)]
    public string NumeroDocumento { get; init; } = string.Empty;

    /// <summary>Data de vencimento do título (C012), posições 74-81.</summary>
    [CampoCnab("Data de Vencimento do Título", 74, 8, TipoCampo.DataDdMmAaaa)]
    public DateOnly? DataVencimento { get; init; }

    /// <summary>Valor nominal do título (G070), posições 82-96, 2 decimais.</summary>
    [CampoCnab("Valor Nominal do Título", 82, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal Valor { get; init; }

    /// <summary>Número do banco cobrador/recebedor (C045), posições 97-99.</summary>
    [CampoCnab("Banco Cobrador/Recebedor", 97, 3, TipoCampo.Numerico)]
    public int BancoCobrador { get; init; }

    /// <summary>Agência cobradora/recebedora (G008), posições 100-104.</summary>
    [CampoCnab("Agência Cobradora/Recebedora", 100, 5, TipoCampo.Numerico)]
    public int AgenciaCobradora { get; init; }

    /// <summary>DV da agência cobradora (G009), posição 105.</summary>
    [CampoCnab("Dígito Verificador da Agência Cobradora", 105, 1, TipoCampo.Alfanumerico)]
    public string DvAgenciaCobradora { get; init; } = string.Empty;

    /// <summary>Identificação do título na empresa (G072), posições 106-130.</summary>
    [CampoCnab("Identificação do Título na Empresa", 106, 25, TipoCampo.Alfanumerico)]
    public string IdentificacaoTituloEmpresa { get; init; } = string.Empty;

    /// <summary>Código da moeda (G065), posições 131-132.</summary>
    [CampoCnab("Código da Moeda", 131, 2, TipoCampo.Numerico)]
    public int CodigoMoeda { get; init; }

    /// <summary>Tipo de inscrição do pagador (G005), posição 133.</summary>
    [CampoCnab("Tipo de Inscrição do Pagador", 133, 1, TipoCampo.Numerico)]
    public int TipoInscricaoPagador { get; init; }

    /// <summary>Número de inscrição do pagador (G006), posições 134-148.</summary>
    [CampoCnab("Número de Inscrição do Pagador", 134, 15, TipoCampo.Alfanumerico)]
    public string NumeroInscricaoPagador { get; init; } = string.Empty;

    /// <summary>Nome do pagador (G013), posições 149-188.</summary>
    [CampoCnab("Nome do Pagador", 149, 40, TipoCampo.Alfanumerico)]
    public string NomePagador { get; init; } = string.Empty;

    /// <summary>Número do contrato da operação de crédito (C030), posições 189-198.</summary>
    [CampoCnab("Número do Contrato da Operação de Crédito", 189, 10, TipoCampo.Alfanumerico)]
    public string NumeroContrato { get; init; } = string.Empty;

    /// <summary>Valor da tarifa/custas (G076), posições 199-213, 2 decimais.</summary>
    [CampoCnab("Valor da Tarifa/Custas", 199, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorTarifa { get; init; }

    /// <summary>Motivo da ocorrência (C047), posições 214-223; até 5 códigos de 2 posições.</summary>
    [CampoCnab("Motivo da Ocorrência", 214, 10, TipoCampo.Alfanumerico)]
    public string MotivoOcorrencia { get; init; } = string.Empty;

    /// <summary>Uso exclusivo FEBRABAN/CNAB (G004), posições 224-240, brancos.</summary>
    [CampoCnab("Filler CNAB 224-240", 224, 17, TipoCampo.Filler)]
    public string FillerFinal { get; init; } = string.Empty;
}

/// <summary>
/// Segmento U — valores e encargos do retorno de cobrança (obrigatório).
/// FEBRABAN CNAB 240 V10.9, seção 3.2.
/// </summary>
[RegistroCnab(Codigos240.TamanhoRegistro)]
public sealed record SegmentoU240
{
    /// <summary>Código do banco na compensação (G001), posições 1-3.</summary>
    [CampoCnab("Código do Banco", 1, 3, TipoCampo.Numerico)]
    public int Banco { get; init; }

    /// <summary>Número do lote (G002), posições 4-7.</summary>
    [CampoCnab("Lote de Serviço", 4, 4, TipoCampo.Numerico)]
    public int Lote { get; init; }

    /// <summary>Tipo de registro (G003), posição 8; "3".</summary>
    [CampoCnab("Tipo de Registro", 8, 1, TipoCampo.Numerico)]
    public int TipoRegistro { get; init; }

    /// <summary>Número sequencial do registro no lote (G038), posições 9-13.</summary>
    [CampoCnab("Número Sequencial do Registro no Lote", 9, 5, TipoCampo.Numerico)]
    public int Sequencial { get; init; }

    /// <summary>Código do segmento (G039), posição 14; "U".</summary>
    [CampoCnab("Código do Segmento", 14, 1, TipoCampo.Alfanumerico)]
    public string Segmento { get; init; } = "U";

    /// <summary>Uso exclusivo FEBRABAN/CNAB (G004), posição 15, branco.</summary>
    [CampoCnab("Filler CNAB", 15, 1, TipoCampo.Filler)]
    public string Filler15 { get; init; } = string.Empty;

    /// <summary>Código de movimento retorno (C044), posições 16-17.</summary>
    [CampoCnab("Código de Movimento Retorno", 16, 2, TipoCampo.Numerico)]
    public int CodigoMovimento { get; init; }

    /// <summary>Juros/multa/encargos (C048), posições 18-32, 2 decimais.</summary>
    [CampoCnab("Juros/Multa/Encargos", 18, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal JurosMultaEncargos { get; init; }

    /// <summary>Valor do desconto concedido (C049), posições 33-47, 2 decimais.</summary>
    [CampoCnab("Valor do Desconto Concedido", 33, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorDesconto { get; init; }

    /// <summary>Valor do abatimento concedido/cancelado (C050), posições 48-62, 2 decimais.</summary>
    [CampoCnab("Valor do Abatimento", 48, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorAbatimento { get; init; }

    /// <summary>Valor do IOF recolhido (G077), posições 63-77, 2 decimais.</summary>
    [CampoCnab("Valor do IOF Recolhido", 63, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorIof { get; init; }

    /// <summary>Valor pago pelo pagador (C052), posições 78-92, 2 decimais.</summary>
    [CampoCnab("Valor Pago pelo Pagador", 78, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorPago { get; init; }

    /// <summary>Valor líquido a ser creditado (G078), posições 93-107, 2 decimais.</summary>
    [CampoCnab("Valor Líquido a ser Creditado", 93, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorLiquido { get; init; }

    /// <summary>Valor de outras despesas (C054), posições 108-122, 2 decimais.</summary>
    [CampoCnab("Outras Despesas", 108, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal OutrasDespesas { get; init; }

    /// <summary>Valor de outros créditos (C055), posições 123-137, 2 decimais.</summary>
    [CampoCnab("Outros Créditos", 123, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal OutrosCreditos { get; init; }

    /// <summary>Data da ocorrência (C056), posições 138-145.</summary>
    [CampoCnab("Data da Ocorrência", 138, 8, TipoCampo.DataDdMmAaaa)]
    public DateOnly? DataOcorrencia { get; init; }

    /// <summary>Data da efetivação do crédito (C057), posições 146-153.</summary>
    [CampoCnab("Data da Efetivação do Crédito", 146, 8, TipoCampo.DataDdMmAaaa)]
    public DateOnly? DataCredito { get; init; }

    /// <summary>Código da ocorrência do pagador/sacado (A001), posições 154-157.</summary>
    [CampoCnab("Código da Ocorrência do Pagador", 154, 4, TipoCampo.Alfanumerico)]
    public string CodigoOcorrenciaPagador { get; init; } = string.Empty;

    /// <summary>Data da ocorrência complementar (A001), posições 158-165.</summary>
    [CampoCnab("Data da Ocorrência Complementar", 158, 8, TipoCampo.Alfanumerico)]
    public string DataOcorrenciaComplementar { get; init; } = string.Empty;

    /// <summary>Valor da ocorrência (A001), posições 166-180, 2 decimais.</summary>
    [CampoCnab("Valor da Ocorrência", 166, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorOcorrencia { get; init; }

    /// <summary>Complemento da ocorrência (A001), posições 181-210.</summary>
    [CampoCnab("Complemento da Ocorrência", 181, 30, TipoCampo.Alfanumerico)]
    public string ComplementoOcorrencia { get; init; } = string.Empty;

    /// <summary>Código do banco correspondente (C031), posições 211-213.</summary>
    [CampoCnab("Banco Correspondente", 211, 3, TipoCampo.Numerico)]
    public int BancoCorrespondente { get; init; }

    /// <summary>Nosso número no banco correspondente (C032), posições 214-233.</summary>
    [CampoCnab("Nosso Número no Banco Correspondente", 214, 20, TipoCampo.Alfanumerico)]
    public string NossoNumeroCorrespondente { get; init; } = string.Empty;

    /// <summary>Uso exclusivo FEBRABAN/CNAB (G004), posições 234-240, brancos.</summary>
    [CampoCnab("Filler CNAB 234-240", 234, 7, TipoCampo.Filler)]
    public string FillerFinal { get; init; } = string.Empty;
}
