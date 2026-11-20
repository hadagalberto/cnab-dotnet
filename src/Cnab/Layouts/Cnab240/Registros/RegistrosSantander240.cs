namespace Cnab.Layouts.Cnab240.Registros;

using Cnab.Arquivo;

/// <summary>
/// Header de arquivo de cobrança do Santander (H7815 v6, fevereiro/2023).
/// Diferente do padrão FEBRABAN: 9-16 (8 brancos), inscrição em 18-32 (15) e
/// código de transmissão em 33-47 (15). Versão do layout 040.
/// </summary>
[RegistroCnab(Codigos240.TamanhoRegistro)]
public sealed record HeaderArquivoSantander240
{
    /// <summary>Código do banco na compensação, posições 1-3 ("033").</summary>
    [CampoCnab("Código do Banco", 1, 3, TipoCampo.Numerico)]
    public int Banco { get; init; }

    /// <summary>Lote de serviço, posições 4-7 ("0000").</summary>
    [CampoCnab("Lote de Serviço", 4, 4, TipoCampo.Numerico)]
    public int Lote { get; init; }

    /// <summary>Tipo de registro, posição 8 ("0").</summary>
    [CampoCnab("Tipo de Registro", 8, 1, TipoCampo.Numerico)]
    public int TipoRegistro { get; init; }

    /// <summary>Reservado do banco, posições 9-16, brancos.</summary>
    [CampoCnab("Reservado do Banco 9-16", 9, 8, TipoCampo.Filler)]
    public string Reservado9A16 { get; init; } = string.Empty;

    /// <summary>Tipo de inscrição da empresa, posição 17.</summary>
    [CampoCnab("Tipo de Inscrição da Empresa", 17, 1, TipoCampo.Numerico)]
    public int TipoInscricaoEmpresa { get; init; }

    /// <summary>Número de inscrição da empresa (15), posições 18-32.</summary>
    [CampoCnab("Número de Inscrição da Empresa", 18, 15, TipoCampo.Alfanumerico)]
    public string NumeroInscricaoEmpresa { get; init; } = string.Empty;

    /// <summary>Código de transmissão (15), posições 33-47.</summary>
    [CampoCnab("Código de Transmissão", 33, 15, TipoCampo.Alfanumerico)]
    public string CodigoTransmissao { get; init; } = string.Empty;

    /// <summary>Reservado do banco, posições 48-72, brancos.</summary>
    [CampoCnab("Reservado do Banco 48-72", 48, 25, TipoCampo.Filler)]
    public string Reservado48A72 { get; init; } = string.Empty;

    /// <summary>Nome da empresa, posições 73-102.</summary>
    [CampoCnab("Nome da Empresa", 73, 30, TipoCampo.Alfanumerico)]
    public string NomeEmpresa { get; init; } = string.Empty;

    /// <summary>Nome do banco, posições 103-132 ("Banco Santander").</summary>
    [CampoCnab("Nome do Banco", 103, 30, TipoCampo.Alfanumerico)]
    public string NomeBanco { get; init; } = string.Empty;

    /// <summary>Filler, posições 133-142, brancos.</summary>
    [CampoCnab("Filler 133-142", 133, 10, TipoCampo.Filler)]
    public string Filler133A142 { get; init; } = string.Empty;

    /// <summary>Código remessa/retorno, posição 143.</summary>
    [CampoCnab("Código Remessa/Retorno", 143, 1, TipoCampo.Numerico)]
    public int CodigoRemessaRetorno { get; init; }

    /// <summary>Data de geração, posições 144-151.</summary>
    [CampoCnab("Data de Geração do Arquivo", 144, 8, TipoCampo.DataDdMmAaaa)]
    public DateOnly DataGeracao { get; init; }

    /// <summary>Hora de geração, posições 152-157.</summary>
    [CampoCnab("Hora de Geração do Arquivo", 152, 6, TipoCampo.Numerico)]
    public int HoraGeracao { get; init; }

    /// <summary>Número sequencial do arquivo, posições 158-163.</summary>
    [CampoCnab("Número Sequencial do Arquivo", 158, 6, TipoCampo.Numerico)]
    public int NumeroSequencialArquivo { get; init; }

    /// <summary>Versão do layout do arquivo, posições 164-166 ("040").</summary>
    [CampoCnab("Versão do Layout do Arquivo", 164, 3, TipoCampo.Numerico)]
    public int VersaoLayout { get; init; } = 40;

    /// <summary>Reservado, posições 167-240, brancos.</summary>
    [CampoCnab("Reservado 167-240", 167, 74, TipoCampo.Filler)]
    public string ReservadoFinal { get; init; } = string.Empty;
}

/// <summary>
/// Header de lote de cobrança do Santander (H7815 v6): versão do lote 030,
/// código de transmissão em 54-68 e inscrição em 19-33 (15 posições).
/// </summary>
[RegistroCnab(Codigos240.TamanhoRegistro)]
public sealed record HeaderLoteCobrancaSantander240
{
    /// <summary>Código do banco na compensação, posições 1-3.</summary>
    [CampoCnab("Código do Banco", 1, 3, TipoCampo.Numerico)]
    public int Banco { get; init; }

    /// <summary>Número do lote, posições 4-7.</summary>
    [CampoCnab("Lote de Serviço", 4, 4, TipoCampo.Numerico)]
    public int Lote { get; init; }

    /// <summary>Tipo de registro, posição 8 ("1").</summary>
    [CampoCnab("Tipo de Registro", 8, 1, TipoCampo.Numerico)]
    public int TipoRegistro { get; init; }

    /// <summary>Tipo de operação, posição 9 ("R" remessa / "T" retorno).</summary>
    [CampoCnab("Tipo de Operação", 9, 1, TipoCampo.Alfanumerico)]
    public string TipoOperacao { get; init; } = "R";

    /// <summary>Tipo de serviço, posições 10-11 ("01").</summary>
    [CampoCnab("Tipo de Serviço", 10, 2, TipoCampo.Numerico)]
    public int TipoServico { get; init; } = 1;

    /// <summary>Filler, posições 12-13, brancos.</summary>
    [CampoCnab("Filler 12-13", 12, 2, TipoCampo.Filler)]
    public string Filler12A13 { get; init; } = string.Empty;

    /// <summary>Versão do layout do lote, posições 14-16 ("030").</summary>
    [CampoCnab("Versão do Layout do Lote", 14, 3, TipoCampo.Numerico)]
    public int VersaoLayoutLote { get; init; } = 30;

    /// <summary>Filler, posição 17.</summary>
    [CampoCnab("Filler 17", 17, 1, TipoCampo.Filler)]
    public string Filler17 { get; init; } = string.Empty;

    /// <summary>Tipo de inscrição da empresa, posição 18.</summary>
    [CampoCnab("Tipo de Inscrição da Empresa", 18, 1, TipoCampo.Numerico)]
    public int TipoInscricaoEmpresa { get; init; }

    /// <summary>Número de inscrição da empresa (15), posições 19-33.</summary>
    [CampoCnab("Número de Inscrição da Empresa", 19, 15, TipoCampo.Alfanumerico)]
    public string NumeroInscricaoEmpresa { get; init; } = string.Empty;

    /// <summary>Reservado, posições 34-53, brancos.</summary>
    [CampoCnab("Reservado 34-53", 34, 20, TipoCampo.Filler)]
    public string Reservado34A53 { get; init; } = string.Empty;

    /// <summary>Código de transmissão, posições 54-68.</summary>
    [CampoCnab("Código de Transmissão", 54, 15, TipoCampo.Alfanumerico)]
    public string CodigoTransmissao { get; init; } = string.Empty;

    /// <summary>Reservado, posições 69-73, brancos.</summary>
    [CampoCnab("Reservado 69-73", 69, 5, TipoCampo.Filler)]
    public string Reservado69A73 { get; init; } = string.Empty;

    /// <summary>Nome do beneficiário, posições 74-103.</summary>
    [CampoCnab("Nome do Beneficiário", 74, 30, TipoCampo.Alfanumerico)]
    public string NomeBeneficiario { get; init; } = string.Empty;

    /// <summary>Mensagem 1, posições 104-143.</summary>
    [CampoCnab("Mensagem 1", 104, 40, TipoCampo.Alfanumerico)]
    public string Mensagem1 { get; init; } = string.Empty;

    /// <summary>Mensagem 2, posições 144-183.</summary>
    [CampoCnab("Mensagem 2", 144, 40, TipoCampo.Alfanumerico)]
    public string Mensagem2 { get; init; } = string.Empty;

    /// <summary>Número remessa/retorno, posições 184-191.</summary>
    [CampoCnab("Número Remessa/Retorno", 184, 8, TipoCampo.Numerico)]
    public long NumeroRemessaRetorno { get; init; }

    /// <summary>Data de gravação, posições 192-199.</summary>
    [CampoCnab("Data de Gravação", 192, 8, TipoCampo.DataDdMmAaaa)]
    public DateOnly DataGravacao { get; init; }

    /// <summary>Filler final, posições 200-240, brancos.</summary>
    [CampoCnab("Filler 200-240", 200, 41, TipoCampo.Filler)]
    public string FillerFinal { get; init; } = string.Empty;
}

/// <summary>
/// Segmento P de cobrança do Santander (H7815 v6): nosso número de 13
/// posições em 45-57, carteira em 58, "seu número" de 15 em 63-77 e zero
/// fixo na posição 225.
/// </summary>
[RegistroCnab(Codigos240.TamanhoRegistro)]
public sealed record SegmentoPSantander240
{
    /// <summary>Código do banco, posições 1-3.</summary>
    [CampoCnab("Código do Banco", 1, 3, TipoCampo.Numerico)]
    public int Banco { get; init; }

    /// <summary>Lote de serviço, posições 4-7.</summary>
    [CampoCnab("Lote de Serviço", 4, 4, TipoCampo.Numerico)]
    public int Lote { get; init; }

    /// <summary>Tipo de registro, posição 8 ("3").</summary>
    [CampoCnab("Tipo de Registro", 8, 1, TipoCampo.Numerico)]
    public int TipoRegistro { get; init; }

    /// <summary>Número sequencial do registro no lote, posições 9-13.</summary>
    [CampoCnab("Número Sequencial do Registro no Lote", 9, 5, TipoCampo.Numerico)]
    public int Sequencial { get; init; }

    /// <summary>Código do segmento, posição 14 ("P").</summary>
    [CampoCnab("Código do Segmento", 14, 1, TipoCampo.Alfanumerico)]
    public string Segmento { get; init; } = "P";

    /// <summary>Filler, posição 15.</summary>
    [CampoCnab("Filler 15", 15, 1, TipoCampo.Filler)]
    public string Filler15 { get; init; } = string.Empty;

    /// <summary>Código de movimento remessa, posições 16-17.</summary>
    [CampoCnab("Código de Movimento Remessa", 16, 2, TipoCampo.Numerico)]
    public int CodigoMovimento { get; init; }

    /// <summary>Agência do destinatário (4), posições 18-21.</summary>
    [CampoCnab("Agência do Destinatário", 18, 4, TipoCampo.Numerico)]
    public int Agencia { get; init; }

    /// <summary>DV da agência, posição 22.</summary>
    [CampoCnab("DV da Agência", 22, 1, TipoCampo.Alfanumerico)]
    public string DvAgencia { get; init; } = string.Empty;

    /// <summary>Conta corrente (9), posições 23-31.</summary>
    [CampoCnab("Conta Corrente", 23, 9, TipoCampo.Numerico)]
    public long Conta { get; init; }

    /// <summary>DV da conta, posição 32.</summary>
    [CampoCnab("DV da Conta", 32, 1, TipoCampo.Alfanumerico)]
    public string DvConta { get; init; } = string.Empty;

    /// <summary>Conta cobrança destinatária FIDC (9), posições 33-41.</summary>
    [CampoCnab("Conta Cobrança FIDC", 33, 9, TipoCampo.Numerico)]
    public long ContaFidc { get; init; }

    /// <summary>DV da conta FIDC, posição 42.</summary>
    [CampoCnab("DV da Conta FIDC", 42, 1, TipoCampo.Alfanumerico)]
    public string DvContaFidc { get; init; } = string.Empty;

    /// <summary>Filler, posições 43-44.</summary>
    [CampoCnab("Filler 43-44", 43, 2, TipoCampo.Filler)]
    public string Filler43A44 { get; init; } = string.Empty;

    /// <summary>Nosso número (13), posições 45-57.</summary>
    [CampoCnab("Nosso Número", 45, 13, TipoCampo.Alfanumerico)]
    public string NossoNumero { get; init; } = string.Empty;

    /// <summary>Tipo de cobrança/carteira, posição 58.</summary>
    [CampoCnab("Carteira", 58, 1, TipoCampo.Numerico)]
    public int Carteira { get; init; }

    /// <summary>Forma de cadastramento, posição 59.</summary>
    [CampoCnab("Forma de Cadastramento", 59, 1, TipoCampo.Numerico)]
    public int FormaCadastramento { get; init; }

    /// <summary>Tipo de documento, posição 60.</summary>
    [CampoCnab("Tipo de Documento", 60, 1, TipoCampo.Alfanumerico)]
    public string TipoDocumento { get; init; } = string.Empty;

    /// <summary>Filler, posições 61-62.</summary>
    [CampoCnab("Filler 61-62", 61, 2, TipoCampo.Filler)]
    public string Filler61A62 { get; init; } = string.Empty;

    /// <summary>Seu número (15), posições 63-77.</summary>
    [CampoCnab("Número do Documento de Cobrança", 63, 15, TipoCampo.Alfanumerico)]
    public string NumeroDocumento { get; init; } = string.Empty;

    /// <summary>Data de vencimento, posições 78-85.</summary>
    [CampoCnab("Data de Vencimento", 78, 8, TipoCampo.DataDdMmAaaa)]
    public DateOnly? DataVencimento { get; init; }

    /// <summary>Valor nominal, posições 86-100, 2 decimais.</summary>
    [CampoCnab("Valor Nominal do Título", 86, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal Valor { get; init; }

    /// <summary>Agência encarregada da cobrança FIDC (4), posições 101-104.</summary>
    [CampoCnab("Agência Encarregada FIDC", 101, 4, TipoCampo.Numerico)]
    public int AgenciaCobradora { get; init; }

    /// <summary>DV da agência FIDC, posição 105.</summary>
    [CampoCnab("DV da Agência FIDC", 105, 1, TipoCampo.Alfanumerico)]
    public string DvAgenciaCobradora { get; init; } = string.Empty;

    /// <summary>Filler, posição 106.</summary>
    [CampoCnab("Filler 106", 106, 1, TipoCampo.Filler)]
    public string Filler106 { get; init; } = string.Empty;

    /// <summary>Espécie do título, posições 107-108.</summary>
    [CampoCnab("Espécie do Título", 107, 2, TipoCampo.Numerico)]
    public int Especie { get; init; }

    /// <summary>Aceite, posição 109 ("N" na maioria das carteiras).</summary>
    [CampoCnab("Aceite", 109, 1, TipoCampo.Alfanumerico)]
    public string Aceite { get; init; } = "N";

    /// <summary>Data de emissão, posições 110-117.</summary>
    [CampoCnab("Data de Emissão", 110, 8, TipoCampo.DataDdMmAaaa)]
    public DateOnly DataEmissao { get; init; }

    /// <summary>Código do juros de mora, posição 118.</summary>
    [CampoCnab("Código do Juros de Mora", 118, 1, TipoCampo.Numerico)]
    public int CodigoJurosMora { get; init; }

    /// <summary>Data do juros de mora, posições 119-126.</summary>
    [CampoCnab("Data do Juros de Mora", 119, 8, TipoCampo.DataDdMmAaaa)]
    public DateOnly? DataJurosMora { get; init; }

    /// <summary>Juros de mora, posições 127-141, 2 decimais.</summary>
    [CampoCnab("Juros de Mora", 127, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal? ValorJurosMora { get; init; }

    /// <summary>Código do desconto, posição 142.</summary>
    [CampoCnab("Código do Desconto", 142, 1, TipoCampo.Numerico)]
    public int CodigoDesconto { get; init; }

    /// <summary>Data do desconto, posições 143-150.</summary>
    [CampoCnab("Data do Desconto", 143, 8, TipoCampo.DataDdMmAaaa)]
    public DateOnly? DataDesconto { get; init; }

    /// <summary>Desconto, posições 151-165, 2 decimais.</summary>
    [CampoCnab("Desconto", 151, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal? ValorDesconto { get; init; }

    /// <summary>Valor do IOF, posições 166-180, 2 decimais.</summary>
    [CampoCnab("Valor do IOF", 166, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal? ValorIof { get; init; }

    /// <summary>Valor do abatimento, posições 181-195, 2 decimais.</summary>
    [CampoCnab("Valor do Abatimento", 181, 15, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal? ValorAbatimento { get; init; }

    /// <summary>Identificação do título na empresa, posições 196-220.</summary>
    [CampoCnab("Identificação do Título na Empresa", 196, 25, TipoCampo.Alfanumerico)]
    public string IdentificacaoTituloEmpresa { get; init; } = string.Empty;

    /// <summary>Código para protesto, posição 221.</summary>
    [CampoCnab("Código para Protesto", 221, 1, TipoCampo.Numerico)]
    public int CodigoProtesto { get; init; }

    /// <summary>Dias para protesto, posições 222-223.</summary>
    [CampoCnab("Dias para Protesto", 222, 2, TipoCampo.Numerico)]
    public int DiasProtesto { get; init; }

    /// <summary>Código para baixa/devolução, posição 224.</summary>
    [CampoCnab("Código para Baixa/Devolução", 224, 1, TipoCampo.Numerico)]
    public int CodigoBaixaDevolucao { get; init; }

    /// <summary>Zero fixo, posição 225.</summary>
    [CampoCnab("Zero Fixo", 225, 1, TipoCampo.Filler, ConteudoFiller = "0")]
    public string ZeroFixo { get; init; } = string.Empty;

    /// <summary>Filler, posições 226-229.</summary>
    [CampoCnab("Filler 226-229", 226, 4, TipoCampo.Filler)]
    public string Filler226A229 { get; init; } = string.Empty;

    /// <summary>Filler final, posições 230-240, brancos.</summary>
    [CampoCnab("Filler 230-240", 230, 11, TipoCampo.Filler)]
    public string FillerFinal { get; init; } = string.Empty;
}
