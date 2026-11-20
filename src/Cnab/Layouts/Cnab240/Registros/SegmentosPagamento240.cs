namespace Cnab.Layouts.Cnab240.Registros;

using Cnab.Arquivo;

/// <summary>
/// Segmento A — crédito em conta / DOC / TED / Pix (obrigatório na remessa e
/// no retorno de pagamentos). FEBRABAN CNAB 240 V10.9/V10.11, seção 3.1.
/// </summary>
[RegistroCnab(Codigos240.TamanhoRegistro)]
public sealed record SegmentoA240
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

    /// <summary>Código do segmento (G039), posição 14; "A".</summary>
    [CampoCnab("Código do Segmento", 14, 1, TipoCampo.Alfanumerico)]
    public string Segmento { get; init; } = "A";

    /// <summary>Tipo de movimento (G060), posição 15; "0" = inclusão.</summary>
    [CampoCnab("Tipo de Movimento", 15, 1, TipoCampo.Numerico)]
    public int TipoMovimento { get; init; }

    /// <summary>Código da instrução para movimento (G061), posições 16-17; "00".</summary>
    [CampoCnab("Código da Instrução para Movimento", 16, 2, TipoCampo.Numerico)]
    public int CodigoInstrucao { get; init; }

    /// <summary>Código da câmara centralizadora (P001), posições 18-20.</summary>
    [CampoCnab("Câmara Centralizadora", 18, 3, TipoCampo.Numerico)]
    public int CamaraCentralizadora { get; init; }

    /// <summary>Código do banco do favorecido (P002), posições 21-23.</summary>
    [CampoCnab("Banco do Favorecido", 21, 3, TipoCampo.Numerico)]
    public int BancoFavorecido { get; init; }

    /// <summary>Agência mantenedora da conta do favorecido (G008), posições 24-28.</summary>
    [CampoCnab("Agência do Favorecido", 24, 5, TipoCampo.Numerico)]
    public int AgenciaFavorecido { get; init; }

    /// <summary>DV da agência do favorecido (G009), posição 29.</summary>
    [CampoCnab("DV da Agência do Favorecido", 29, 1, TipoCampo.Alfanumerico)]
    public string DvAgenciaFavorecido { get; init; } = string.Empty;

    /// <summary>Número da conta corrente do favorecido (G010), posições 30-41.</summary>
    [CampoCnab("Conta do Favorecido", 30, 12, TipoCampo.Numerico)]
    public long ContaFavorecido { get; init; }

    /// <summary>DV da conta do favorecido (G011), posição 42.</summary>
    [CampoCnab("DV da Conta do Favorecido", 42, 1, TipoCampo.Alfanumerico)]
    public string DvContaFavorecido { get; init; } = string.Empty;

    /// <summary>DV da agência/conta do favorecido (G012), posição 43.</summary>
    [CampoCnab("DV da Ag/Conta do Favorecido", 43, 1, TipoCampo.Alfanumerico)]
    public string DvAgenciaContaFavorecido { get; init; } = string.Empty;

    /// <summary>Nome do favorecido (G013), posições 44-73.</summary>
    [CampoCnab("Nome do Favorecido", 44, 30, TipoCampo.Alfanumerico)]
    public string NomeFavorecido { get; init; } = string.Empty;

    /// <summary>Número do documento atribuído pela empresa (G064), posições 74-93.</summary>
    [CampoCnab("Número do Documento da Empresa", 74, 20, TipoCampo.Alfanumerico)]
    public string NumeroDocumentoEmpresa { get; init; } = string.Empty;

    /// <summary>Data do pagamento (P009), posições 94-101.</summary>
    [CampoCnab("Data do Pagamento", 94, 8, TipoCampo.DataDdMmAaaa)]
    public DateOnly DataPagamento { get; init; }

    /// <summary>Tipo da moeda (G040), posições 102-104; "BRL".</summary>
    [CampoCnab("Tipo da Moeda", 102, 3, TipoCampo.Alfanumerico)]
    public string TipoMoeda { get; init; } = Codigos240.MoedaRealAlfa;

    /// <summary>Quantidade da moeda (G041), posições 105-119, 5 decimais.</summary>
    [CampoCnab("Quantidade da Moeda", 105, 15, TipoCampo.NumericoDecimal, decimais: 5)]
    public decimal QuantidadeMoeda { get; init; }

    /// <summary>Valor do pagamento (P010), posições 120-134, 2 decimais.</summary>
    [CampoCnab("Valor do Pagamento", 120, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorPagamento { get; init; }

    /// <summary>Número do documento atribuído pelo banco (G043), posições 135-154.</summary>
    [CampoCnab("Número do Documento do Banco", 135, 20, TipoCampo.Alfanumerico)]
    public string NumeroDocumentoBanco { get; init; } = string.Empty;

    /// <summary>Data real da efetivação do pagamento (P003), posições 155-162.</summary>
    [CampoCnab("Data Real da Efetivação", 155, 8, TipoCampo.DataDdMmAaaa)]
    public DateOnly? DataRealEfetivacao { get; init; }

    /// <summary>Valor real da efetivação do pagamento (P004), posições 163-177, 2 decimais.</summary>
    [CampoCnab("Valor Real da Efetivação", 163, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal? ValorRealEfetivacao { get; init; }

    /// <summary>Outras informações (G031), posições 178-217.</summary>
    [CampoCnab("Outras Informações", 178, 40, TipoCampo.Alfanumerico)]
    public string OutrasInformacoes { get; init; } = string.Empty;

    /// <summary>Complemento do tipo de serviço (P005), posições 218-219.</summary>
    [CampoCnab("Complemento do Tipo de Serviço", 218, 2, TipoCampo.Alfanumerico)]
    public string ComplementoTipoServico { get; init; } = string.Empty;

    /// <summary>Código da finalidade da TED (P011), posições 220-224.</summary>
    [CampoCnab("Finalidade da TED", 220, 5, TipoCampo.Alfanumerico)]
    public string FinalidadeTed { get; init; } = string.Empty;

    /// <summary>Complemento da finalidade de pagamento (P013), posições 225-226.</summary>
    [CampoCnab("Complemento da Finalidade", 225, 2, TipoCampo.Alfanumerico)]
    public string ComplementoFinalidade { get; init; } = string.Empty;

    /// <summary>Uso exclusivo FEBRABAN/CNAB (G004), posições 227-229, brancos.</summary>
    [CampoCnab("Filler CNAB 227-229", 227, 3, TipoCampo.Filler)]
    public string Filler227A229 { get; init; } = string.Empty;

    /// <summary>Aviso ao favorecido (P006), posição 230.</summary>
    [CampoCnab("Aviso ao Favorecido", 230, 1, TipoCampo.Numerico)]
    public int AvisoFavorecido { get; init; }

    /// <summary>Códigos das ocorrências para retorno (G059), posições 231-240.</summary>
    [CampoCnab("Ocorrências para Retorno", 231, 10, TipoCampo.Alfanumerico)]
    public string OcorrenciasRetorno { get; init; } = string.Empty;
}

/// <summary>
/// Segmento B — dados complementares do favorecido, layout genérico FEBRABAN
/// V10.x (Informações 10/11/12). FEBRABAN CNAB 240, seção 3.1.
/// </summary>
[RegistroCnab(Codigos240.TamanhoRegistro)]
public sealed record SegmentoB240
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

    /// <summary>Código do segmento (G039), posição 14; "B".</summary>
    [CampoCnab("Código do Segmento", 14, 1, TipoCampo.Alfanumerico)]
    public string Segmento { get; init; } = "B";

    /// <summary>Forma de iniciação / identificação do favorecido (G100), posições 15-17.</summary>
    [CampoCnab("Forma de Iniciação", 15, 3, TipoCampo.Alfanumerico)]
    public string FormaIniciacao { get; init; } = string.Empty;

    /// <summary>Tipo de inscrição do favorecido (G005), posição 18.</summary>
    [CampoCnab("Tipo de Inscrição do Favorecido", 18, 1, TipoCampo.Numerico)]
    public int TipoInscricaoFavorecido { get; init; }

    /// <summary>Número de inscrição do favorecido (G006), posições 19-32.</summary>
    [CampoCnab("Número de Inscrição do Favorecido", 19, 14, TipoCampo.Alfanumerico)]
    public string NumeroInscricaoFavorecido { get; init; } = string.Empty;

    /// <summary>Informação 10 (G101), posições 33-67.</summary>
    [CampoCnab("Informação 10", 33, 35, TipoCampo.Alfanumerico)]
    public string Informacao10 { get; init; } = string.Empty;

    /// <summary>Informação 11 (G101), posições 68-127.</summary>
    [CampoCnab("Informação 11", 68, 60, TipoCampo.Alfanumerico)]
    public string Informacao11 { get; init; } = string.Empty;

    /// <summary>Informação 12 (G101), posições 128-226.</summary>
    [CampoCnab("Informação 12", 128, 99, TipoCampo.Alfanumerico)]
    public string Informacao12 { get; init; } = string.Empty;

    /// <summary>Código UG centralizadora (P012), posições 227-232.</summary>
    [CampoCnab("Código UG Centralizadora", 227, 6, TipoCampo.Numerico)]
    public int CodigoUgCentralizadora { get; init; }

    /// <summary>Código ISPB / identificação do banco no SPB (P015), posições 233-240.</summary>
    [CampoCnab("Código ISPB", 233, 8, TipoCampo.Numerico)]
    public long CodigoIspb { get; init; }
}

/// <summary>
/// Segmento B em layout de endereço (versão legada ainda exigida por BB,
/// CAIXA, Sicredi e Sicoob): posições 33-127 trazem logradouro, número,
/// complemento, bairro, cidade, CEP e UF; 227-232 é uso do SIAPE.
/// </summary>
[RegistroCnab(Codigos240.TamanhoRegistro)]
public sealed record SegmentoBEndereco240
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

    /// <summary>Código do segmento (G039), posição 14; "B".</summary>
    [CampoCnab("Código do Segmento", 14, 1, TipoCampo.Alfanumerico)]
    public string Segmento { get; init; } = "B";

    /// <summary>Uso exclusivo FEBRABAN/CNAB (G004), posições 15-17, brancos.</summary>
    [CampoCnab("Filler CNAB 15-17", 15, 3, TipoCampo.Filler)]
    public string Filler15A17 { get; init; } = string.Empty;

    /// <summary>Tipo de inscrição do favorecido (G005), posição 18.</summary>
    [CampoCnab("Tipo de Inscrição do Favorecido", 18, 1, TipoCampo.Numerico)]
    public int TipoInscricaoFavorecido { get; init; }

    /// <summary>Número de inscrição do favorecido (G006), posições 19-32.</summary>
    [CampoCnab("Número de Inscrição do Favorecido", 19, 14, TipoCampo.Alfanumerico)]
    public string NumeroInscricaoFavorecido { get; init; } = string.Empty;

    /// <summary>Logradouro (G032), posições 33-62.</summary>
    [CampoCnab("Logradouro", 33, 30, TipoCampo.Alfanumerico)]
    public string Logradouro { get; init; } = string.Empty;

    /// <summary>Número do local (G032), posições 63-67.</summary>
    [CampoCnab("Número do Local", 63, 5, TipoCampo.Numerico)]
    public int NumeroLocal { get; init; }

    /// <summary>Complemento (G032), posições 68-82.</summary>
    [CampoCnab("Complemento", 68, 15, TipoCampo.Alfanumerico)]
    public string Complemento { get; init; } = string.Empty;

    /// <summary>Bairro (G032), posições 83-97.</summary>
    [CampoCnab("Bairro", 83, 15, TipoCampo.Alfanumerico)]
    public string Bairro { get; init; } = string.Empty;

    /// <summary>Cidade (G033), posições 98-117.</summary>
    [CampoCnab("Cidade", 98, 20, TipoCampo.Alfanumerico)]
    public string Cidade { get; init; } = string.Empty;

    /// <summary>CEP (G034), posições 118-122.</summary>
    [CampoCnab("CEP", 118, 5, TipoCampo.Numerico)]
    public int Cep { get; init; }

    /// <summary>Complemento do CEP (G035), posições 123-125.</summary>
    [CampoCnab("Complemento do CEP", 123, 3, TipoCampo.Alfanumerico)]
    public string ComplementoCep { get; init; } = string.Empty;

    /// <summary>Unidade da federação (G036), posições 126-127.</summary>
    [CampoCnab("UF", 126, 2, TipoCampo.Alfanumerico)]
    public string Uf { get; init; } = string.Empty;

    /// <summary>Data de vencimento nominal (G044), posições 128-135.</summary>
    [CampoCnab("Data de Vencimento", 128, 8, TipoCampo.DataDdMmAaaa)]
    public DateOnly? DataVencimento { get; init; }

    /// <summary>Valor do documento nominal (G042), posições 136-150, 2 decimais.</summary>
    [CampoCnab("Valor do Documento", 136, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorDocumento { get; init; }

    /// <summary>Valor do abatimento (G045), posições 151-165, 2 decimais.</summary>
    [CampoCnab("Valor do Abatimento", 151, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorAbatimento { get; init; }

    /// <summary>Valor do desconto (G046), posições 166-180, 2 decimais.</summary>
    [CampoCnab("Valor do Desconto", 166, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorDesconto { get; init; }

    /// <summary>Valor da mora (G047), posições 181-195, 2 decimais.</summary>
    [CampoCnab("Valor da Mora", 181, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorMora { get; init; }

    /// <summary>Valor da multa (G048), posições 196-210, 2 decimais.</summary>
    [CampoCnab("Valor da Multa", 196, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorMulta { get; init; }

    /// <summary>Código/documento do favorecido (P008), posições 211-225.</summary>
    [CampoCnab("Código/Documento do Favorecido", 211, 15, TipoCampo.Alfanumerico)]
    public string CodigoDocumentoFavorecido { get; init; } = string.Empty;

    /// <summary>Aviso ao favorecido (P006), posição 226.</summary>
    [CampoCnab("Aviso ao Favorecido", 226, 1, TipoCampo.Numerico)]
    public int AvisoFavorecido { get; init; }

    /// <summary>Uso exclusivo SIAPE (P012), posições 227-232.</summary>
    [CampoCnab("Código UG Centralizadora", 227, 6, TipoCampo.Numerico)]
    public int CodigoUgCentralizadora { get; init; }

    /// <summary>Uso exclusivo FEBRABAN/CNAB (G004), posições 233-240, brancos.</summary>
    [CampoCnab("Filler CNAB 233-240", 233, 8, TipoCampo.Filler)]
    public string FillerFinal { get; init; } = string.Empty;
}

/// <summary>
/// Segmento J — pagamento de título de cobrança com código de barras.
/// FEBRABAN CNAB 240 V10.9, seção 3.1.3. O código de barras é numérico (44).
/// </summary>
[RegistroCnab(Codigos240.TamanhoRegistro)]
public sealed record SegmentoJ240
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

    /// <summary>Código do segmento (G039), posição 14; "J".</summary>
    [CampoCnab("Código do Segmento", 14, 1, TipoCampo.Alfanumerico)]
    public string Segmento { get; init; } = "J";

    /// <summary>Tipo de movimento (G060), posição 15; "0" = inclusão.</summary>
    [CampoCnab("Tipo de Movimento", 15, 1, TipoCampo.Numerico)]
    public int TipoMovimento { get; init; }

    /// <summary>Código da instrução para movimento (G061), posições 16-17; "00".</summary>
    [CampoCnab("Código da Instrução para Movimento", 16, 2, TipoCampo.Numerico)]
    public int CodigoInstrucao { get; init; }

    /// <summary>Código de barras (G063), posições 18-61 (44 posições).</summary>
    [CampoCnab("Código de Barras", 18, 44, TipoCampo.Alfanumerico)]
    public string CodigoBarras { get; init; } = string.Empty;

    /// <summary>Nome do beneficiário (G013), posições 62-91.</summary>
    [CampoCnab("Nome do Beneficiário", 62, 30, TipoCampo.Alfanumerico)]
    public string NomeBeneficiario { get; init; } = string.Empty;

    /// <summary>Data de vencimento nominal (G044), posições 92-99.</summary>
    [CampoCnab("Data de Vencimento", 92, 8, TipoCampo.DataDdMmAaaa)]
    public DateOnly? DataVencimento { get; init; }

    /// <summary>Valor do título nominal (G042), posições 100-114, 2 decimais.</summary>
    [CampoCnab("Valor do Título", 100, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorTitulo { get; init; }

    /// <summary>Valor do desconto + abatimento (L002), posições 115-129, 2 decimais.</summary>
    [CampoCnab("Desconto + Abatimento", 115, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal DescontoAbatimento { get; init; }

    /// <summary>Valor da mora + multa (L003), posições 130-144, 2 decimais.</summary>
    [CampoCnab("Mora + Multa", 130, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal MoraMulta { get; init; }

    /// <summary>Data do pagamento (P009), posições 145-152.</summary>
    [CampoCnab("Data do Pagamento", 145, 8, TipoCampo.DataDdMmAaaa)]
    public DateOnly DataPagamento { get; init; }

    /// <summary>Valor do pagamento (P010), posições 153-167, 2 decimais.</summary>
    [CampoCnab("Valor do Pagamento", 153, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorPagamento { get; init; }

    /// <summary>Quantidade da moeda (G041), posições 168-182, 5 decimais.</summary>
    [CampoCnab("Quantidade da Moeda", 168, 15, TipoCampo.NumericoDecimal, decimais: 5)]
    public decimal QuantidadeMoeda { get; init; }

    /// <summary>Número do documento atribuído pela empresa (G064), posições 183-202.</summary>
    [CampoCnab("Número do Documento da Empresa", 183, 20, TipoCampo.Alfanumerico)]
    public string NumeroDocumentoEmpresa { get; init; } = string.Empty;

    /// <summary>Número do documento atribuído pelo banco (G043), posições 203-222.</summary>
    [CampoCnab("Número do Documento do Banco", 203, 20, TipoCampo.Alfanumerico)]
    public string NumeroDocumentoBanco { get; init; } = string.Empty;

    /// <summary>Código da moeda (G065), posições 223-224.</summary>
    [CampoCnab("Código da Moeda", 223, 2, TipoCampo.Numerico)]
    public int CodigoMoeda { get; init; }

    /// <summary>Uso exclusivo FEBRABAN/CNAB (G004), posições 225-230, brancos.</summary>
    [CampoCnab("Filler CNAB 225-230", 225, 6, TipoCampo.Filler)]
    public string Filler225A230 { get; init; } = string.Empty;

    /// <summary>Códigos das ocorrências para retorno (G059), posições 231-240.</summary>
    [CampoCnab("Ocorrências para Retorno", 231, 10, TipoCampo.Alfanumerico)]
    public string OcorrenciasRetorno { get; init; } = string.Empty;
}

/// <summary>
/// Segmento J-52 — complemento do J com dados do pagador, beneficiário e
/// sacador/avalista. O identificador de registro opcional é "52" (posições
/// 18-19) e o código de segmento é "J". FEBRABAN V10.9, seção 3.1.3.
/// </summary>
[RegistroCnab(Codigos240.TamanhoRegistro)]
public sealed record SegmentoJ52_240
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

    /// <summary>Código do segmento (G039), posição 14; "J".</summary>
    [CampoCnab("Código do Segmento", 14, 1, TipoCampo.Alfanumerico)]
    public string Segmento { get; init; } = "J";

    /// <summary>Uso exclusivo FEBRABAN/CNAB (G004), posição 15, branco.</summary>
    [CampoCnab("Filler CNAB", 15, 1, TipoCampo.Filler)]
    public string Filler15 { get; init; } = string.Empty;

    /// <summary>Código de movimento remessa (C004), posições 16-17; "00".</summary>
    [CampoCnab("Código de Movimento Remessa", 16, 2, TipoCampo.Numerico)]
    public int CodigoMovimento { get; init; }

    /// <summary>Identificação do registro opcional (G067), posições 18-19; "52".</summary>
    [CampoCnab("Identificação do Registro Opcional", 18, 2, TipoCampo.Numerico)]
    public int IdentificadorRegistroOpcional { get; init; } = Codigos240.IdentificadorJ52;

    /// <summary>Tipo de inscrição do pagador (G005), posição 20.</summary>
    [CampoCnab("Tipo de Inscrição do Pagador", 20, 1, TipoCampo.Numerico)]
    public int TipoInscricaoPagador { get; init; }

    /// <summary>Número de inscrição do pagador (G006), posições 21-35.</summary>
    [CampoCnab("Número de Inscrição do Pagador", 21, 15, TipoCampo.Alfanumerico)]
    public string NumeroInscricaoPagador { get; init; } = string.Empty;

    /// <summary>Nome do pagador (G013), posições 36-75.</summary>
    [CampoCnab("Nome do Pagador", 36, 40, TipoCampo.Alfanumerico)]
    public string NomePagador { get; init; } = string.Empty;

    /// <summary>Tipo de inscrição do beneficiário (G005), posição 76.</summary>
    [CampoCnab("Tipo de Inscrição do Beneficiário", 76, 1, TipoCampo.Numerico)]
    public int TipoInscricaoBeneficiario { get; init; }

    /// <summary>Número de inscrição do beneficiário (G006), posições 77-91.</summary>
    [CampoCnab("Número de Inscrição do Beneficiário", 77, 15, TipoCampo.Alfanumerico)]
    public string NumeroInscricaoBeneficiario { get; init; } = string.Empty;

    /// <summary>Nome do beneficiário (G013), posições 92-131.</summary>
    [CampoCnab("Nome do Beneficiário", 92, 40, TipoCampo.Alfanumerico)]
    public string NomeBeneficiario { get; init; } = string.Empty;

    /// <summary>Tipo de inscrição do sacador/avalista original (G005), posição 132.</summary>
    [CampoCnab("Tipo de Inscrição do Sacador/Avalista", 132, 1, TipoCampo.Numerico)]
    public int TipoInscricaoSacador { get; init; }

    /// <summary>Número de inscrição do sacador/avalista original (G006), posições 133-147.</summary>
    [CampoCnab("Número de Inscrição do Sacador/Avalista", 133, 15, TipoCampo.Alfanumerico)]
    public string NumeroInscricaoSacador { get; init; } = string.Empty;

    /// <summary>Nome do sacador/avalista original (G013), posições 148-187.</summary>
    [CampoCnab("Nome do Sacador/Avalista", 148, 40, TipoCampo.Alfanumerico)]
    public string NomeSacador { get; init; } = string.Empty;

    /// <summary>Uso exclusivo FEBRABAN/CNAB (G004), posições 188-240, brancos.</summary>
    [CampoCnab("Filler CNAB 188-240", 188, 53, TipoCampo.Filler)]
    public string FillerFinal { get; init; } = string.Empty;
}

/// <summary>
/// Segmento O — pagamento de contas e tributos com código de barras.
/// FEBRABAN CNAB 240 V10.9, seção 3.1.4. Aqui o código de barras é X(44).
/// </summary>
[RegistroCnab(Codigos240.TamanhoRegistro)]
public sealed record SegmentoO240
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

    /// <summary>Código do segmento (G039), posição 14; "O".</summary>
    [CampoCnab("Código do Segmento", 14, 1, TipoCampo.Alfanumerico)]
    public string Segmento { get; init; } = "O";

    /// <summary>Tipo de movimento (G060), posição 15; "0" = inclusão.</summary>
    [CampoCnab("Tipo de Movimento", 15, 1, TipoCampo.Numerico)]
    public int TipoMovimento { get; init; }

    /// <summary>Código da instrução de movimento (G061), posições 16-17; "00".</summary>
    [CampoCnab("Código da Instrução de Movimento", 16, 2, TipoCampo.Numerico)]
    public int CodigoInstrucao { get; init; }

    /// <summary>Código de barras (N001), posições 18-61 (44 posições).</summary>
    [CampoCnab("Código de Barras", 18, 44, TipoCampo.Alfanumerico)]
    public string CodigoBarras { get; init; } = string.Empty;

    /// <summary>Nome da concessionária/órgão público (G013), posições 62-91.</summary>
    [CampoCnab("Nome da Concessionária", 62, 30, TipoCampo.Alfanumerico)]
    public string NomeConcessionaria { get; init; } = string.Empty;

    /// <summary>Data de vencimento nominal (G044), posições 92-99.</summary>
    [CampoCnab("Data de Vencimento", 92, 8, TipoCampo.DataDdMmAaaa)]
    public DateOnly? DataVencimento { get; init; }

    /// <summary>Data do pagamento (P009), posições 100-107.</summary>
    [CampoCnab("Data do Pagamento", 100, 8, TipoCampo.DataDdMmAaaa)]
    public DateOnly DataPagamento { get; init; }

    /// <summary>Valor do pagamento (P004), posições 108-122, 2 decimais.</summary>
    [CampoCnab("Valor do Pagamento", 108, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorPagamento { get; init; }

    /// <summary>Número do documento atribuído pela empresa (G064), posições 123-142.</summary>
    [CampoCnab("Número do Documento da Empresa", 123, 20, TipoCampo.Alfanumerico)]
    public string NumeroDocumentoEmpresa { get; init; } = string.Empty;

    /// <summary>Número do documento atribuído pelo banco (G043), posições 143-162.</summary>
    [CampoCnab("Número do Documento do Banco", 143, 20, TipoCampo.Alfanumerico)]
    public string NumeroDocumentoBanco { get; init; } = string.Empty;

    /// <summary>Uso exclusivo FEBRABAN/CNAB (G004), posições 163-230, brancos.</summary>
    [CampoCnab("Filler CNAB 163-230", 163, 68, TipoCampo.Filler)]
    public string Filler163A230 { get; init; } = string.Empty;

    /// <summary>Códigos das ocorrências para retorno (G059), posições 231-240.</summary>
    [CampoCnab("Ocorrências para Retorno", 231, 10, TipoCampo.Alfanumerico)]
    public string OcorrenciasRetorno { get; init; } = string.Empty;
}
