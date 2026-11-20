namespace Cnab.Modelo;

/// <summary>
/// Título de cobrança (boleto) na ótica do domínio CNAB: identificação do
/// título no banco (nosso número, com DV por banco), documento, vencimento,
/// valor e encargos, pagador, sacador avalista e instruções.
/// </summary>
public sealed record Titulo
{
    /// <summary>
    /// Nosso número: identificação do título no banco. A composição e o DV
    /// seguem o algoritmo de cada banco (<see cref="Bancos"/>); os validadores
    /// por banco verificam o formato e o DV quando o banco os publica.
    /// </summary>
    public required string NossoNumero { get; init; }

    /// <summary>Número do documento atribuído pelo beneficiário ("seu número").</summary>
    public required string NumeroDocumento { get; init; }

    /// <summary>Espécie do documento de origem (duplicata, nota promissória etc.).</summary>
    public required EspecieDocumento Especie { get; init; }

    /// <summary>Aceite do título ("N" na grande maioria das carteiras).</summary>
    public bool Aceite { get; init; }

    /// <summary>Data de emissão do documento.</summary>
    public required DateOnly DataEmissao { get; init; }

    /// <summary>
    /// Data de vencimento; <see langword="null"/> para títulos contra
    /// apresentação (fator de vencimento 0000).
    /// </summary>
    public DateOnly? DataVencimento { get; init; }

    /// <summary>Valor nominal do título (moeda corrente, 2 decimais).</summary>
    public required decimal Valor { get; init; }

    /// <summary>Carteira de cobrança (código por banco).</summary>
    public string Carteira { get; init; } = string.Empty;

    /// <summary>Juros de mora cobrados após o vencimento; <see langword="null"/> quando isento.</summary>
    public Juros? Juros { get; init; }

    /// <summary>Multa por atraso; <see langword="null"/> quando isenta.</summary>
    public Multa? Multa { get; init; }

    /// <summary>Desconto concedido até uma data limite; <see langword="null"/> quando não houver.</summary>
    public Desconto? Desconto { get; init; }

    /// <summary>Abatimento concedido sobre o valor; <see langword="null"/> quando não houver.</summary>
    public decimal? Abatimento { get; init; }

    /// <summary>Instrução de protesto/negativação; <see langword="null"/> quando não houver.</summary>
    public Protesto? Protesto { get; init; }

    /// <summary>Pagador (sacado).</summary>
    public required Pagador Pagador { get; init; }

    /// <summary>Sacador avalista / beneficiário final, quando houver.</summary>
    public SacadorAvalista? SacadorAvalista { get; init; }

    /// <summary>Mensagens/instruções impressas no boleto (por exemplo "Não receber após vencimento").</summary>
    public IReadOnlyList<string> Instrucoes { get; init; } = [];

    /// <summary>Valida o título: valores não negativos, vencimento >= emissão, encargos coerentes.</summary>
    /// <exception cref="InvalidOperationException">Alguma regra de negócio violada.</exception>
    public void Validar()
    {
        if (Valor <= 0)
        {
            throw new InvalidOperationException("Valor do título deve ser maior que zero.");
        }

        if (DataVencimento is not null && DataVencimento < DataEmissao)
        {
            throw new InvalidOperationException("Vencimento não pode ser anterior à emissão.");
        }

        if (Abatimento is < 0)
        {
            throw new InvalidOperationException("Abatimento não pode ser negativo.");
        }

        if (Abatimento is not null && Abatimento >= Valor)
        {
            throw new InvalidOperationException("Abatimento deve ser menor que o valor do título.");
        }

        Juros?.Validar();
        Multa?.Validar();
        Desconto?.Validar();
        Protesto?.Validar();
    }
}

/// <summary>Espécie do documento que originou o título. Tabela FEBRABAN/Sicredi (CNAB 240 Cobrança, item 3.0).</summary>
public enum EspecieDocumento
{
    /// <summary>Duplicata Mercantil por Indicação (A).</summary>
    DuplicataMercantilIndicacao,

    /// <summary>Duplicata Rural (B).</summary>
    DuplicataRural,

    /// <summary>Nota Promissória (C).</summary>
    NotaPromissoria,

    /// <summary>Nota Promissória Rural (D).</summary>
    NotaPromissoriaRural,

    /// <summary>Nota de Seguros (E).</summary>
    NotaSeguros,

    /// <summary>Recibo (G).</summary>
    Recibo,

    /// <summary>Letra de Câmbio (H).</summary>
    LetraCambio,

    /// <summary>Nota de Débito (I).</summary>
    NotaDebito,

    /// <summary>Duplicata de Serviço por Indicação (J).</summary>
    DuplicataServicoIndicacao,

    /// <summary>Outros (K).</summary>
    Outros,
}

/// <summary>Tipo de cobrança de juros de mora (FEBRABAN: valor diário, taxa diária ou taxa mensal).</summary>
public enum TipoJuros
{
    /// <summary>Valor fixo por dia de atraso (código A nos manuais).</summary>
    ValorDiario,

    /// <summary>Percentual ao dia (código B).</summary>
    PercentualDiario,

    /// <summary>Percentual ao mês (código B, base mensal).</summary>
    PercentualMensal,
}

/// <summary>Juros de mora cobrados após o vencimento.</summary>
public sealed record Juros
{
    /// <summary>Como o juro é cobrado (valor por dia ou percentual).</summary>
    public required TipoJuros Tipo { get; init; }

    /// <summary>
    /// Valor por dia (quando <see cref="Tipo"/> é <see cref="TipoJuros.ValorDiario"/>)
    /// ou percentual com até 2 decimais (quando percentual).
    /// </summary>
    public required decimal Valor { get; init; }

    /// <summary>Data a partir da qual o juro é cobrado.</summary>
    public required DateOnly DataInicio { get; init; }

    /// <summary>Valida o juro (valor positivo).</summary>
    /// <exception cref="InvalidOperationException">Valor não positivo.</exception>
    public void Validar()
    {
        if (Valor <= 0)
        {
            throw new InvalidOperationException("Juros deve ser maior que zero.");
        }
    }
}

/// <summary>Tipo de multa por atraso no pagamento.</summary>
public enum TipoMulta
{
    /// <summary>Valor fixo (código A nos manuais).</summary>
    Valor,

    /// <summary>Percentual sobre o título (código B).</summary>
    Percentual,
}

/// <summary>Multa por atraso.</summary>
public sealed record Multa
{
    /// <summary>Como a multa é cobrada (valor ou percentual).</summary>
    public required TipoMulta Tipo { get; init; }

    /// <summary>Valor fixo ou percentual com até 2 decimais.</summary>
    public required decimal Valor { get; init; }

    /// <summary>Data a partir da qual a multa é cobrada.</summary>
    public required DateOnly DataInicio { get; init; }

    /// <summary>Valida a multa (valor positivo).</summary>
    /// <exception cref="InvalidOperationException">Valor não positivo.</exception>
    public void Validar()
    {
        if (Valor <= 0)
        {
            throw new InvalidOperationException("Multa deve ser maior que zero.");
        }
    }
}

/// <summary>Desconto condicional: concedido quando o título é pago até a data limite.</summary>
public sealed record Desconto
{
    /// <summary>Valor do desconto (2 decimais; manuais também admitem percentual em alguns bancos).</summary>
    public required decimal Valor { get; init; }

    /// <summary>Data limite para concessão do desconto.</summary>
    public required DateOnly DataLimite { get; init; }

    /// <summary>Valida o desconto (valor positivo).</summary>
    /// <exception cref="InvalidOperationException">Valor não positivo.</exception>
    public void Validar()
    {
        if (Valor <= 0)
        {
            throw new InvalidOperationException("Desconto deve ser maior que zero.");
        }
    }
}

/// <summary>Modo de instrução de protesto/negativação.</summary>
public enum TipoProtesto
{
    /// <summary>Sem instrução de protesto (código 0/3 dos manuais).</summary>
    Nenhum,

    /// <summary>Protestar após N dias corridos do vencimento (código 1).</summary>
    ProtestarDiasCorridos,

    /// <summary>Negativar (Serasa) após N dias (código 9, conforme manuais recentes).</summary>
    NegativarDiasCorridos,

    /// <summary>Cancelar instrução de protesto/negativação pendente (código 2).</summary>
    CancelarInstrucao,
}

/// <summary>Instrução de protesto ou negativação do título vencido.</summary>
public sealed record Protesto
{
    /// <summary>Tipo da instrução.</summary>
    public required TipoProtesto Tipo { get; init; }

    /// <summary>
    /// Dias após o vencimento para a ação. Sicredi exige entre 3 e 99 dias
    /// (API de Cobrança v3.x, campo diasProtestoAuto/diasNegativacaoAuto);
    /// demais manuais usam faixas semelhantes.
    /// </summary>
    public required int Dias { get; init; }

    /// <summary>Valida a instrução de protesto.</summary>
    /// <exception cref="InvalidOperationException">Dias fora da faixa quando o tipo exige.</exception>
    public void Validar()
    {
        if (Tipo is TipoProtesto.ProtestarDiasCorridos or TipoProtesto.NegativarDiasCorridos
            && Dias is < 3 or > 99)
        {
            throw new InvalidOperationException("Prazo de protesto/negativação deve estar entre 3 e 99 dias.");
        }
    }
}
