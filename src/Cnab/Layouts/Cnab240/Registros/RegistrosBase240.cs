namespace Cnab.Layouts.Cnab240.Registros;

using Cnab.Arquivo;

/// <summary>
/// Header de arquivo FEBRABAN CNAB 240 (registro tipo 0). Posições extraídas
/// do "Layout Padrão CNAB 240 V10.9" (14/10/2021), seção 2.2 — ver
/// <c>docs/layouts.md</c>. O campo convênio ocupa 33-52 (20 posições) na V10.x.
/// </summary>
[RegistroCnab(Codigos240.TamanhoRegistro)]
public sealed record HeaderArquivo240
{
    /// <summary>Código do banco na compensação (G001), posições 1-3.</summary>
    [CampoCnab("Código do Banco", 1, 3, TipoCampo.Numerico)]
    public int Banco { get; init; }

    /// <summary>Lote de serviço (G002), posições 4-7; "0000" no header de arquivo.</summary>
    [CampoCnab("Lote de Serviço", 4, 4, TipoCampo.Numerico)]
    public int Lote { get; init; }

    /// <summary>Tipo de registro (G003), posição 8; "0".</summary>
    [CampoCnab("Tipo de Registro", 8, 1, TipoCampo.Numerico)]
    public int TipoRegistro { get; init; }

    /// <summary>Uso exclusivo FEBRABAN/CNAB (G004), posições 9-17, brancos.</summary>
    [CampoCnab("Filler CNAB", 9, 9, TipoCampo.Filler)]
    public string FillerInicial { get; init; } = string.Empty;

    /// <summary>Tipo de inscrição da empresa (G005), posição 18: 1=CPF, 2=CNPJ.</summary>
    [CampoCnab("Tipo de Inscrição da Empresa", 18, 1, TipoCampo.Numerico)]
    public int TipoInscricaoEmpresa { get; init; }

    /// <summary>Número de inscrição da empresa (G006), posições 19-32.</summary>
    [CampoCnab("Número de Inscrição da Empresa", 19, 14, TipoCampo.Alfanumerico)]
    public string NumeroInscricaoEmpresa { get; init; } = string.Empty;

    /// <summary>Código do convênio no banco (G007), posições 33-52.</summary>
    [CampoCnab("Código do Convênio no Banco", 33, 20, TipoCampo.Alfanumerico)]
    public string Convenio { get; init; } = string.Empty;

    /// <summary>Agência mantenedora da conta (G008), posições 53-57.</summary>
    [CampoCnab("Agência Mantenedora da Conta", 53, 5, TipoCampo.Numerico)]
    public int Agencia { get; init; }

    /// <summary>Dígito verificador da agência (G009), posição 58.</summary>
    [CampoCnab("Dígito Verificador da Agência", 58, 1, TipoCampo.Alfanumerico)]
    public string DvAgencia { get; init; } = string.Empty;

    /// <summary>Número da conta corrente (G010), posições 59-70.</summary>
    [CampoCnab("Número da Conta Corrente", 59, 12, TipoCampo.Numerico)]
    public long Conta { get; init; }

    /// <summary>Dígito verificador da conta (G011), posição 71.</summary>
    [CampoCnab("Dígito Verificador da Conta", 71, 1, TipoCampo.Alfanumerico)]
    public string DvConta { get; init; } = string.Empty;

    /// <summary>Dígito verificador da agência/conta (G012), posição 72.</summary>
    [CampoCnab("Dígito Verificador da Ag/Conta", 72, 1, TipoCampo.Alfanumerico)]
    public string DvAgenciaConta { get; init; } = string.Empty;

    /// <summary>Nome da empresa (G013), posições 73-102.</summary>
    [CampoCnab("Nome da Empresa", 73, 30, TipoCampo.Alfanumerico)]
    public string NomeEmpresa { get; init; } = string.Empty;

    /// <summary>Nome do banco (G014), posições 103-132.</summary>
    [CampoCnab("Nome do Banco", 103, 30, TipoCampo.Alfanumerico)]
    public string NomeBanco { get; init; } = string.Empty;

    /// <summary>Uso exclusivo FEBRABAN/CNAB (G004), posições 133-142, brancos.</summary>
    [CampoCnab("Filler CNAB 133-142", 133, 10, TipoCampo.Filler)]
    public string Filler133A142 { get; init; } = string.Empty;

    /// <summary>Código remessa/retorno (G015), posição 143: 1=remessa, 2=retorno.</summary>
    [CampoCnab("Código Remessa/Retorno", 143, 1, TipoCampo.Numerico)]
    public int CodigoRemessaRetorno { get; init; }

    /// <summary>Data de geração do arquivo (G016), posições 144-151 (DDMMAAAA).</summary>
    [CampoCnab("Data de Geração do Arquivo", 144, 8, TipoCampo.DataDdMmAaaa)]
    public DateOnly DataGeracao { get; init; }

    /// <summary>Hora de geração do arquivo (G017), posições 152-157 (HHMMSS).</summary>
    [CampoCnab("Hora de Geração do Arquivo", 152, 6, TipoCampo.Numerico)]
    public int HoraGeracao { get; init; }

    /// <summary>Número sequencial do arquivo — NSA (G018), posições 158-163.</summary>
    [CampoCnab("Número Sequencial do Arquivo", 158, 6, TipoCampo.Numerico)]
    public int NumeroSequencialArquivo { get; init; }

    /// <summary>Número da versão do layout (G019), posições 164-166.</summary>
    [CampoCnab("Número da Versão do Layout", 164, 3, TipoCampo.Numerico)]
    public int VersaoLayout { get; init; }

    /// <summary>Densidade de gravação (G020), posições 167-171.</summary>
    [CampoCnab("Densidade de Gravação do Arquivo", 167, 5, TipoCampo.Numerico)]
    public int Densidade { get; init; }

    /// <summary>Uso reservado do banco (G021), posições 172-191.</summary>
    [CampoCnab("Uso Reservado do Banco", 172, 20, TipoCampo.Alfanumerico)]
    public string ReservadoBanco { get; init; } = string.Empty;

    /// <summary>Uso reservado da empresa (G022), posições 192-211.</summary>
    [CampoCnab("Uso Reservado da Empresa", 192, 20, TipoCampo.Alfanumerico)]
    public string ReservadoEmpresa { get; init; } = string.Empty;

    /// <summary>Uso exclusivo FEBRABAN/CNAB (G004), posições 212-240, brancos.</summary>
    [CampoCnab("Filler CNAB 212-240", 212, 29, TipoCampo.Filler)]
    public string FillerFinal { get; init; } = string.Empty;
}

/// <summary>
/// Header de lote de cobrança (tipo 1), FEBRABAN CNAB 240 V10.9, seção 3.2.2.
/// A inscrição da empresa tem 15 posições (19-33) e o convênio 20 (34-53) neste
/// serviço — diferença documentada em relação ao header de arquivo.
/// </summary>
[RegistroCnab(Codigos240.TamanhoRegistro)]
public sealed record HeaderLoteCobranca240
{
    /// <summary>Código do banco na compensação (G001), posições 1-3.</summary>
    [CampoCnab("Código do Banco", 1, 3, TipoCampo.Numerico)]
    public int Banco { get; init; }

    /// <summary>Número do lote (G002), posições 4-7.</summary>
    [CampoCnab("Lote de Serviço", 4, 4, TipoCampo.Numerico)]
    public int Lote { get; init; }

    /// <summary>Tipo de registro (G003), posição 8; "1".</summary>
    [CampoCnab("Tipo de Registro", 8, 1, TipoCampo.Numerico)]
    public int TipoRegistro { get; init; }

    /// <summary>Tipo de operação (G028), posição 9; definido pelo manual do banco.</summary>
    [CampoCnab("Tipo de Operação", 9, 1, TipoCampo.Alfanumerico)]
    public string TipoOperacao { get; init; } = string.Empty;

    /// <summary>Tipo de serviço (G025), posições 10-11; "01" para cobrança.</summary>
    [CampoCnab("Tipo de Serviço", 10, 2, TipoCampo.Numerico)]
    public int TipoServico { get; init; }

    /// <summary>Uso exclusivo FEBRABAN/CNAB (G004), posições 12-13, brancos.</summary>
    [CampoCnab("Filler CNAB 12-13", 12, 2, TipoCampo.Filler)]
    public string Filler12A13 { get; init; } = string.Empty;

    /// <summary>Número da versão do layout do lote (G030), posições 14-16.</summary>
    [CampoCnab("Versão do Layout do Lote", 14, 3, TipoCampo.Numerico)]
    public int VersaoLayoutLote { get; init; }

    /// <summary>Uso exclusivo FEBRABAN/CNAB (G004), posição 17, branco.</summary>
    [CampoCnab("Filler CNAB 17", 17, 1, TipoCampo.Filler)]
    public string Filler17 { get; init; } = string.Empty;

    /// <summary>Tipo de inscrição da empresa (G005), posição 18.</summary>
    [CampoCnab("Tipo de Inscrição da Empresa", 18, 1, TipoCampo.Numerico)]
    public int TipoInscricaoEmpresa { get; init; }

    /// <summary>Número de inscrição da empresa (G006), posições 19-33.</summary>
    [CampoCnab("Número de Inscrição da Empresa", 19, 15, TipoCampo.Alfanumerico)]
    public string NumeroInscricaoEmpresa { get; init; } = string.Empty;

    /// <summary>Código do convênio no banco (G007), posições 34-53.</summary>
    [CampoCnab("Código do Convênio no Banco", 34, 20, TipoCampo.Alfanumerico)]
    public string Convenio { get; init; } = string.Empty;

    /// <summary>Agência mantenedora da conta (G008), posições 54-58.</summary>
    [CampoCnab("Agência Mantenedora da Conta", 54, 5, TipoCampo.Numerico)]
    public int Agencia { get; init; }

    /// <summary>DV da agência (G009), posição 59.</summary>
    [CampoCnab("Dígito Verificador da Agência", 59, 1, TipoCampo.Alfanumerico)]
    public string DvAgencia { get; init; } = string.Empty;

    /// <summary>Número da conta corrente (G010), posições 60-71.</summary>
    [CampoCnab("Número da Conta Corrente", 60, 12, TipoCampo.Numerico)]
    public long Conta { get; init; }

    /// <summary>DV da conta (G011), posição 72.</summary>
    [CampoCnab("Dígito Verificador da Conta", 72, 1, TipoCampo.Alfanumerico)]
    public string DvConta { get; init; } = string.Empty;

    /// <summary>DV da agência/conta (G012), posição 73.</summary>
    [CampoCnab("Dígito Verificador da Ag/Conta", 73, 1, TipoCampo.Alfanumerico)]
    public string DvAgenciaConta { get; init; } = string.Empty;

    /// <summary>Nome da empresa (G013), posições 74-103.</summary>
    [CampoCnab("Nome da Empresa", 74, 30, TipoCampo.Alfanumerico)]
    public string NomeEmpresa { get; init; } = string.Empty;

    /// <summary>Mensagem 1 (C073), posições 104-143.</summary>
    [CampoCnab("Mensagem 1", 104, 40, TipoCampo.Alfanumerico)]
    public string Mensagem1 { get; init; } = string.Empty;

    /// <summary>Mensagem 2 (C073), posições 144-183.</summary>
    [CampoCnab("Mensagem 2", 144, 40, TipoCampo.Alfanumerico)]
    public string Mensagem2 { get; init; } = string.Empty;

    /// <summary>Número remessa/retorno (G079), posições 184-191.</summary>
    [CampoCnab("Número Remessa/Retorno", 184, 8, TipoCampo.Numerico)]
    public long NumeroRemessaRetorno { get; init; }

    /// <summary>Data de gravação da remessa/retorno (G068), posições 192-199.</summary>
    [CampoCnab("Data de Gravação Remessa/Retorno", 192, 8, TipoCampo.DataDdMmAaaa)]
    public DateOnly DataGravacao { get; init; }

    /// <summary>Data do crédito (C003), posições 200-207; opcional na remessa.</summary>
    [CampoCnab("Data do Crédito", 200, 8, TipoCampo.DataDdMmAaaa)]
    public DateOnly? DataCredito { get; init; }

    /// <summary>Uso exclusivo FEBRABAN/CNAB (G004), posições 208-240, brancos.</summary>
    [CampoCnab("Filler CNAB 208-240", 208, 33, TipoCampo.Filler)]
    public string FillerFinal { get; init; } = string.Empty;
}

/// <summary>
/// Header de lote de pagamentos (tipo 1), FEBRABAN CNAB 240 V10.9, seção 3.1.2.
/// Aqui a inscrição da empresa tem 14 posições (19-32) e o convênio 20 (33-52).
/// </summary>
[RegistroCnab(Codigos240.TamanhoRegistro)]
public sealed record HeaderLotePagamento240
{
    /// <summary>Código do banco na compensação (G001), posições 1-3.</summary>
    [CampoCnab("Código do Banco", 1, 3, TipoCampo.Numerico)]
    public int Banco { get; init; }

    /// <summary>Número do lote (G002), posições 4-7.</summary>
    [CampoCnab("Lote de Serviço", 4, 4, TipoCampo.Numerico)]
    public int Lote { get; init; }

    /// <summary>Tipo de registro (G003), posição 8; "1".</summary>
    [CampoCnab("Tipo de Registro", 8, 1, TipoCampo.Numerico)]
    public int TipoRegistro { get; init; }

    /// <summary>Tipo de operação (G028), posição 9; "C" para crédito/pagamento.</summary>
    [CampoCnab("Tipo de Operação", 9, 1, TipoCampo.Alfanumerico)]
    public string TipoOperacao { get; init; } = string.Empty;

    /// <summary>Tipo de serviço (G025), posições 10-11.</summary>
    [CampoCnab("Tipo de Serviço", 10, 2, TipoCampo.Numerico)]
    public int TipoServico { get; init; }

    /// <summary>Forma de lançamento (G029), posições 12-13.</summary>
    [CampoCnab("Forma de Lançamento", 12, 2, TipoCampo.Numerico)]
    public int FormaLancamento { get; init; }

    /// <summary>Versão do layout do lote (G030), posições 14-16; "046" no padrão.</summary>
    [CampoCnab("Versão do Layout do Lote", 14, 3, TipoCampo.Numerico)]
    public int VersaoLayoutLote { get; init; }

    /// <summary>Uso exclusivo FEBRABAN/CNAB (G004), posição 17, branco.</summary>
    [CampoCnab("Filler CNAB 17", 17, 1, TipoCampo.Filler)]
    public string Filler17 { get; init; } = string.Empty;

    /// <summary>Tipo de inscrição da empresa (G005), posição 18.</summary>
    [CampoCnab("Tipo de Inscrição da Empresa", 18, 1, TipoCampo.Numerico)]
    public int TipoInscricaoEmpresa { get; init; }

    /// <summary>Número de inscrição da empresa (G006), posições 19-32.</summary>
    [CampoCnab("Número de Inscrição da Empresa", 19, 14, TipoCampo.Alfanumerico)]
    public string NumeroInscricaoEmpresa { get; init; } = string.Empty;

    /// <summary>Código do convênio no banco (G007), posições 33-52.</summary>
    [CampoCnab("Código do Convênio no Banco", 33, 20, TipoCampo.Alfanumerico)]
    public string Convenio { get; init; } = string.Empty;

    /// <summary>Agência mantenedora da conta (G008), posições 53-57.</summary>
    [CampoCnab("Agência Mantenedora da Conta", 53, 5, TipoCampo.Numerico)]
    public int Agencia { get; init; }

    /// <summary>DV da agência (G009), posição 58.</summary>
    [CampoCnab("Dígito Verificador da Agência", 58, 1, TipoCampo.Alfanumerico)]
    public string DvAgencia { get; init; } = string.Empty;

    /// <summary>Número da conta corrente (G010), posições 59-70.</summary>
    [CampoCnab("Número da Conta Corrente", 59, 12, TipoCampo.Numerico)]
    public long Conta { get; init; }

    /// <summary>DV da conta (G011), posição 71.</summary>
    [CampoCnab("Dígito Verificador da Conta", 71, 1, TipoCampo.Alfanumerico)]
    public string DvConta { get; init; } = string.Empty;

    /// <summary>DV da agência/conta (G012), posição 72.</summary>
    [CampoCnab("Dígito Verificador da Ag/Conta", 72, 1, TipoCampo.Alfanumerico)]
    public string DvAgenciaConta { get; init; } = string.Empty;

    /// <summary>Nome da empresa (G013), posições 73-102.</summary>
    [CampoCnab("Nome da Empresa", 73, 30, TipoCampo.Alfanumerico)]
    public string NomeEmpresa { get; init; } = string.Empty;

    /// <summary>Mensagem / informação 1 (C073), posições 103-142.</summary>
    [CampoCnab("Mensagem 1", 103, 40, TipoCampo.Alfanumerico)]
    public string Mensagem1 { get; init; } = string.Empty;

    /// <summary>Logradouro da empresa (G032), posições 143-172.</summary>
    [CampoCnab("Logradouro", 143, 30, TipoCampo.Alfanumerico)]
    public string Logradouro { get; init; } = string.Empty;

    /// <summary>Número do local (G032), posições 173-177.</summary>
    [CampoCnab("Número do Local", 173, 5, TipoCampo.Numerico)]
    public int NumeroLocal { get; init; }

    /// <summary>Complemento do endereço (G032), posições 178-192.</summary>
    [CampoCnab("Complemento", 178, 15, TipoCampo.Alfanumerico)]
    public string Complemento { get; init; } = string.Empty;

    /// <summary>Cidade da empresa (G033), posições 193-212.</summary>
    [CampoCnab("Cidade", 193, 20, TipoCampo.Alfanumerico)]
    public string Cidade { get; init; } = string.Empty;

    /// <summary>CEP da empresa (G034), posições 213-217.</summary>
    [CampoCnab("CEP", 213, 5, TipoCampo.Numerico)]
    public int Cep { get; init; }

    /// <summary>Complemento do CEP (G035), posições 218-220.</summary>
    [CampoCnab("Complemento do CEP", 218, 3, TipoCampo.Alfanumerico)]
    public string ComplementoCep { get; init; } = string.Empty;

    /// <summary>Sigla do estado (G036), posições 221-222.</summary>
    [CampoCnab("Estado", 221, 2, TipoCampo.Alfanumerico)]
    public string Estado { get; init; } = string.Empty;

    /// <summary>Indicativo da forma de pagamento do serviço (P014), posições 223-224.</summary>
    [CampoCnab("Indicativo da Forma de Pagamento", 223, 2, TipoCampo.Numerico)]
    public int IndicativoFormaPagamento { get; init; }

    /// <summary>Uso exclusivo FEBRABAN/CNAB (G004), posições 225-230, brancos.</summary>
    [CampoCnab("Filler CNAB 225-230", 225, 6, TipoCampo.Filler)]
    public string FillerFinal { get; init; } = string.Empty;

    /// <summary>Códigos das ocorrências para retorno (G059), posições 231-240.</summary>
    [CampoCnab("Ocorrências para Retorno", 231, 240 - 231 + 1, TipoCampo.Alfanumerico)]
    public string OcorrenciasRetorno { get; init; } = string.Empty;
}

/// <summary>
/// Trailer de lote de PAGAMENTOS (tipo 5), FEBRABAN CNAB 240 V10.9, seção 3.1.
/// Contadores e somatórias são preenchidos automaticamente pelo gerador.
/// Para cobrança existe um trailer próprio (<see cref="TrailerLoteCobranca240"/>).
/// </summary>
[RegistroCnab(Codigos240.TamanhoRegistro)]
public sealed record TrailerLotePagamento240
{
    /// <summary>Código do banco na compensação (G001), posições 1-3.</summary>
    [CampoCnab("Código do Banco", 1, 3, TipoCampo.Numerico)]
    public int Banco { get; init; }

    /// <summary>Número do lote (G002), posições 4-7.</summary>
    [CampoCnab("Lote de Serviço", 4, 4, TipoCampo.Numerico)]
    public int Lote { get; init; }

    /// <summary>Tipo de registro (G003), posição 8; "5".</summary>
    [CampoCnab("Tipo de Registro", 8, 1, TipoCampo.Numerico)]
    public int TipoRegistro { get; init; }

    /// <summary>Uso exclusivo FEBRABAN/CNAB (G004), posições 9-17, brancos.</summary>
    [CampoCnab("Filler CNAB", 9, 9, TipoCampo.Filler)]
    public string FillerInicial { get; init; } = string.Empty;

    /// <summary>Quantidade de registros do lote (G057), posições 18-23.</summary>
    [CampoCnab("Quantidade de Registros do Lote", 18, 6, TipoCampo.Numerico)]
    public int QuantidadeRegistros { get; init; }

    /// <summary>Somatória dos valores do lote (P007/B002), posições 24-41, 2 decimais.</summary>
    [CampoCnab("Somatória dos Valores", 24, 18, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal SomatoriaValores { get; init; }

    /// <summary>Somatória da quantidade de moedas (G058), posições 42-59, 5 decimais.</summary>
    [CampoCnab("Somatória de Quantidade de Moedas", 42, 18, TipoCampo.NumericoDecimal, decimais: 5)]
    public decimal SomatoriaQuantidadeMoedas { get; init; }

    /// <summary>Número do aviso de débito (G066), posições 60-65.</summary>
    [CampoCnab("Número Aviso de Débito", 60, 6, TipoCampo.Numerico)]
    public int NumeroAvisoDebito { get; init; }

    /// <summary>Uso exclusivo FEBRABAN/CNAB (G004), posições 66-230, brancos.</summary>
    [CampoCnab("Filler CNAB 66-230", 66, 165, TipoCampo.Filler)]
    public string FillerCentral { get; init; } = string.Empty;

    /// <summary>Códigos das ocorrências para retorno (G059), posições 231-240.</summary>
    [CampoCnab("Ocorrências para Retorno", 231, 10, TipoCampo.Alfanumerico)]
    public string OcorrenciasRetorno { get; init; } = string.Empty;
}

/// <summary>
/// Trailer de lote de COBRANÇA (tipo 5), FEBRABAN CNAB 240 V10.9, seção 2.2.
/// Diferente do trailer de pagamentos, este discrimina quantidades e valores
/// por carteira (simples, vinculada, caucionada e descontada) nas posições
/// 24-115 e traz o número do aviso de lançamento em 116-123.
/// </summary>
[RegistroCnab(Codigos240.TamanhoRegistro)]
public sealed record TrailerLoteCobranca240
{
    /// <summary>Código do banco na compensação (G001), posições 1-3.</summary>
    [CampoCnab("Código do Banco", 1, 3, TipoCampo.Numerico)]
    public int Banco { get; init; }

    /// <summary>Número do lote (G002), posições 4-7.</summary>
    [CampoCnab("Lote de Serviço", 4, 4, TipoCampo.Numerico)]
    public int Lote { get; init; }

    /// <summary>Tipo de registro (G003), posição 8; "5".</summary>
    [CampoCnab("Tipo de Registro", 8, 1, TipoCampo.Numerico)]
    public int TipoRegistro { get; init; }

    /// <summary>Uso exclusivo FEBRABAN/CNAB (G004), posições 9-17, brancos.</summary>
    [CampoCnab("Filler CNAB", 9, 9, TipoCampo.Filler)]
    public string FillerInicial { get; init; } = string.Empty;

    /// <summary>Quantidade de títulos em cobrança simples, posições 24-29.</summary>
    [CampoCnab("Qtde. Títulos Cobrança Simples", 24, 6, TipoCampo.Numerico)]
    public int QuantidadeSimples { get; init; }

    /// <summary>Valor total dos títulos em carteiras simples, posições 30-46, 2 decimais.</summary>
    [CampoCnab("Valor Total Títulos Simples", 30, 17, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorSimples { get; init; }

    /// <summary>Quantidade de títulos em cobrança vinculada, posições 47-52.</summary>
    [CampoCnab("Qtde. Títulos Cobrança Vinculada", 47, 6, TipoCampo.Numerico)]
    public int QuantidadeVinculada { get; init; }

    /// <summary>Valor total dos títulos em carteiras vinculadas, posições 53-69, 2 decimais.</summary>
    [CampoCnab("Valor Total Títulos Vinculada", 53, 17, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorVinculada { get; init; }

    /// <summary>Quantidade de títulos em cobrança caucionada, posições 70-75.</summary>
    [CampoCnab("Qtde. Títulos Cobrança Caucionada", 70, 6, TipoCampo.Numerico)]
    public int QuantidadeCaucionada { get; init; }

    /// <summary>Valor total dos títulos em carteiras caucionadas, posições 76-92, 2 decimais.</summary>
    [CampoCnab("Valor Total Títulos Caucionada", 76, 17, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorCaucionada { get; init; }

    /// <summary>Quantidade de títulos em cobrança descontada, posições 93-98.</summary>
    [CampoCnab("Qtde. Títulos Cobrança Descontada", 93, 6, TipoCampo.Numerico)]
    public int QuantidadeDescontada { get; init; }

    /// <summary>Valor total dos títulos em carteiras descontadas, posições 99-115, 2 decimais.</summary>
    [CampoCnab("Valor Total Títulos Descontada", 99, 17, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal ValorDescontada { get; init; }

    /// <summary>Número do aviso de lançamento (C067), posições 116-123.</summary>
    [CampoCnab("Número do Aviso de Lançamento", 116, 8, TipoCampo.Alfanumerico)]
    public string NumeroAvisoLancamento { get; init; } = string.Empty;

    /// <summary>Uso exclusivo FEBRABAN/CNAB (G004), posições 124-240, brancos.</summary>
    [CampoCnab("Filler CNAB 124-240", 124, 117, TipoCampo.Filler)]
    public string FillerFinal { get; init; } = string.Empty;
}

/// <summary>
/// Trailer de arquivo (tipo 9), FEBRABAN CNAB 240 V10.9, seção 2.2.
/// </summary>
[RegistroCnab(Codigos240.TamanhoRegistro)]
public sealed record TrailerArquivo240
{
    /// <summary>Código do banco na compensação (G001), posições 1-3.</summary>
    [CampoCnab("Código do Banco", 1, 3, TipoCampo.Numerico)]
    public int Banco { get; init; }

    /// <summary>Lote de serviço (G002), posições 4-7; "9999" no trailer de arquivo.</summary>
    [CampoCnab("Lote de Serviço", 4, 4, TipoCampo.Numerico)]
    public int Lote { get; init; }

    /// <summary>Tipo de registro (G003), posição 8; "9".</summary>
    [CampoCnab("Tipo de Registro", 8, 1, TipoCampo.Numerico)]
    public int TipoRegistro { get; init; }

    /// <summary>Uso exclusivo FEBRABAN/CNAB (G004), posições 9-17, brancos.</summary>
    [CampoCnab("Filler CNAB", 9, 9, TipoCampo.Filler)]
    public string FillerInicial { get; init; } = string.Empty;

    /// <summary>Quantidade de lotes do arquivo (G049), posições 18-23.</summary>
    [CampoCnab("Quantidade de Lotes do Arquivo", 18, 6, TipoCampo.Numerico)]
    public int QuantidadeLotes { get; init; }

    /// <summary>Quantidade de registros do arquivo (G056), posições 24-29.</summary>
    [CampoCnab("Quantidade de Registros do Arquivo", 24, 6, TipoCampo.Numerico)]
    public int QuantidadeRegistros { get; init; }

    /// <summary>Quantidade de contas para conciliação (G037), posições 30-35.</summary>
    [CampoCnab("Quantidade de Contas para Conciliação", 30, 6, TipoCampo.Numerico)]
    public int QuantidadeContasConciliacao { get; init; }

    /// <summary>Uso exclusivo FEBRABAN/CNAB (G004), posições 36-240, brancos.</summary>
    [CampoCnab("Filler CNAB 36-240", 36, 205, TipoCampo.Filler)]
    public string FillerFinal { get; init; } = string.Empty;
}
