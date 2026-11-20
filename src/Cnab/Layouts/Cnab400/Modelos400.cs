namespace Cnab.Layouts.Cnab400;

using Cnab.Arquivo;
using Cnab.Layouts.Cnab240;
using Cnab.Modelo;

/// <summary>
/// Arquivo de remessa CNAB 400 (cobrança). As posições e os valores fixos por
/// banco são implementados em registros declarativos próprios, citados em
/// <c>docs/layouts.md</c>. Campos específicos de um banco ficam explícitos
/// aqui: nenhum é preenchido com valor inventado pelo gerador.
/// </summary>
public sealed record Remessa400
{
    /// <summary>Banco emissor do arquivo.</summary>
    public required Banco Banco { get; init; }

    /// <summary>Cedente/beneficiário (empresa).</summary>
    public required Cedente Cedente { get; init; }

    /// <summary>Número sequencial do arquivo (Itaú/Bradesco: 6 posições; Sicredi: 7).</summary>
    public required int NumeroSequencialArquivo { get; init; }

    /// <summary>Data de geração do arquivo.</summary>
    public required DateOnly DataGeracao { get; init; }

    /// <summary>
    /// Código do cedente/beneficiário junto ao banco: Itaú usa agência/conta,
    /// Bradesco usa o código de empresa de 20 posições, Sicredi o código do
    /// beneficiário de 5 posições.
    /// </summary>
    public string CodigoBeneficiario { get; init; } = string.Empty;

    /// <summary>Número da remessa (Bradesco e Sicredi).</summary>
    public int NumeroRemessa { get; init; } = 1;

    /// <summary>Código do produto (Bradesco, 3 posições no segmento P / 3 no registro 1).</summary>
    public string CodigoProduto { get; init; } = string.Empty;

    /// <summary>Posto da cooperativa (Sicredi); alfanumérico é convertido em "00" no campo livre.</summary>
    public string Posto { get; init; } = string.Empty;

    /// <summary>Cooperativa/agência beneficiária (Sicredi, 4 posições).</summary>
    public string Cooperativa { get; init; } = string.Empty;

    /// <summary>Número da operação (Inter, 7 posições no campo livre).</summary>
    public string NumeroOperacao { get; init; } = string.Empty;

    /// <summary>Código de ocorrência/movimento de entrada (padrão "01" nos manuais).</summary>
    public int CodigoMovimentoEntrada { get; init; } = 1;

    /// <summary>Títulos emitidos no arquivo.</summary>
    public IReadOnlyList<Titulo> Titulos { get; init; } = [];

    /// <summary>Opções de montagem do arquivo (CRLF/Latin-1 por padrão).</summary>
    public OpcoesArquivo OpcoesArquivo { get; init; } = OpcoesArquivo.Padrao;
}

/// <summary>Valores e datas extraídos de um registro de retorno CNAB 400.</summary>
public sealed record ValoresRetorno400
{
    /// <summary>Valor nominal do título (posições que variam por banco).</summary>
    public decimal ValorTitulo { get; init; }

    /// <summary>Valor efetivamente pago/creditado.</summary>
    public decimal ValorPago { get; init; }

    /// <summary>Valor líquido creditado em conta, quando publicado.</summary>
    public decimal ValorLiquido { get; init; }

    /// <summary>Tarifa/despesas de cobrança.</summary>
    public decimal Tarifa { get; init; }

    /// <summary>Juros de mora/multa.</summary>
    public decimal JurosMulta { get; init; }

    /// <summary>Desconto concedido.</summary>
    public decimal Desconto { get; init; }

    /// <summary>Abatimento concedido.</summary>
    public decimal Abatimento { get; init; }

    /// <summary>IOF recolhido, quando publicado.</summary>
    public decimal Iof { get; init; }

    /// <summary>Data da ocorrência no banco.</summary>
    public DateOnly? DataOcorrencia { get; init; }

    /// <summary>Data do crédito da liquidação.</summary>
    public DateOnly? DataCredito { get; init; }

    /// <summary>Data de vencimento do título.</summary>
    public DateOnly? DataVencimento { get; init; }
}

/// <summary>Um registro de detalhe de um retorno CNAB 400.</summary>
public sealed record ItemRetorno400
{
    /// <summary>Linha resserializada a partir do registro tipado (round-trip byte-exato).</summary>
    public required string LinhaOriginal { get; init; }

    /// <summary>Identificação do registro (normalmente 1).</summary>
    public int TipoRegistro { get; init; }

    /// <summary>Número sequencial do registro no arquivo.</summary>
    public int Sequencial { get; init; }

    /// <summary>Nosso número lido da linha.</summary>
    public string? NossoNumero { get; init; }

    /// <summary>Seu número / número do documento lido da linha.</summary>
    public string? SeuNumero { get; init; }

    /// <summary>Ocorrência mapeada para a tabela do banco.</summary>
    public OcorrenciaRetorno? Ocorrencia { get; init; }

    /// <summary>Códigos de motivo/rejeição mapeados para a tabela do banco.</summary>
    public IReadOnlyList<OcorrenciaRetorno> Motivos { get; init; } = [];

    /// <summary>Valores e datas do registro, quando é um detalhe de cobrança.</summary>
    public ValoresRetorno400? Valores { get; init; }
}

/// <summary>Dados comuns do header de arquivo de um retorno CNAB 400.</summary>
public sealed record CabecalhoRetorno400
{
    /// <summary>Código do banco lido nas posições 1-3 (quando publicadas no header).</summary>
    public int? Banco { get; init; }

    /// <summary>Código remessa/retorno ("1" remessa, "2" retorno).</summary>
    public int CodigoRemessaRetorno { get; init; }

    /// <summary>Data de geração gravada no header.</summary>
    public DateOnly? DataGeracao { get; init; }

    /// <summary>Data de crédito gravada no header (quando publicada).</summary>
    public DateOnly? DataCredito { get; init; }

    /// <summary>Número sequencial do arquivo.</summary>
    public int NumeroSequencialArquivo { get; init; }

    /// <summary>Linha resserializada a partir do registro tipado (round-trip byte-exato).</summary>
    public required string LinhaOriginal { get; init; }
}

/// <summary>Dados comuns do trailer de arquivo de um retorno CNAB 400.</summary>
public sealed record TrailerRetorno400
{
    /// <summary>Tipo de registro (9).</summary>
    public int TipoRegistro { get; init; }

    /// <summary>Código remessa/retorno ("1" remessa, "2" retorno).</summary>
    public int CodigoRemessaRetorno { get; init; }

    /// <summary>Número sequencial final.</summary>
    public int NumeroSequencialArquivo { get; init; }

    /// <summary>Linha resserializada a partir do registro tipado (round-trip byte-exato).</summary>
    public required string LinhaOriginal { get; init; }
}

/// <summary>Arquivo retorno CNAB 400 parseado.</summary>
public sealed record Retorno400
{
    /// <summary>Banco emissor.</summary>
    public required Banco Banco { get; init; }

    /// <summary>Header de arquivo.</summary>
    public required CabecalhoRetorno400 HeaderArquivo { get; init; }

    /// <summary>Registros de detalhe, na ordem do arquivo.</summary>
    public required IReadOnlyList<ItemRetorno400> Itens { get; init; }

    /// <summary>Trailer de arquivo.</summary>
    public required TrailerRetorno400 TrailerArquivo { get; init; }
}
