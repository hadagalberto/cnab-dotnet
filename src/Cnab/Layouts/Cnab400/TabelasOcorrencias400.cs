namespace Cnab.Layouts.Cnab400;

using Cnab.Layouts.Cnab240;
using Cnab.Modelo;

/// <summary>
/// Tabelas de ocorrências e de motivos de rejeição do CNAB 400. Cada tabela
/// foi extraída do manual oficial correspondente (Itaú janeiro/2017,
/// Bradesco 4008.524.0121 v17, Sicredi v2.1 e Inter v9), citado em
/// <c>docs/layouts.md</c>. Códigos ausentes das fontes públicas não são
/// inventados: uma ocorrência não catalogada gera exceção explícita.
/// </summary>
public static class TabelasOcorrencias400
{
    private static readonly IReadOnlyDictionary<string, OcorrenciaRetorno> OcorrenciasBase = CriarBase();
    private static readonly IReadOnlyDictionary<string, string> MotivosBase = CriarMotivosBase();

    private static readonly Dictionary<int, IReadOnlyDictionary<string, OcorrenciaRetorno>> PorBanco = new()
    {
        [237] = BradescoOcorrencias(),
        [341] = ItauOcorrencias(),
        [748] = SicrediOcorrencias(),
        [77] = InterOcorrencias(),
    };

    private static readonly Dictionary<int, IReadOnlyDictionary<string, string>> MotivosPorBanco = new()
    {
        [237] = BradescoMotivos(),
        [341] = ItauMotivos(),
        [748] = SicrediMotivos(),
        [77] = InterMotivos(),
    };

    /// <summary>Busca uma ocorrência na tabela do banco (com fallback para a tabela comum).</summary>
    /// <param name="banco">Banco emissor.</param>
    /// <param name="codigo">Código publicado (por exemplo "06").</param>
    /// <returns>Ocorrência com código, descrição e família.</returns>
    public static OcorrenciaRetorno Obter(Banco banco, string codigo)
    {
        ArgumentNullException.ThrowIfNull(banco);
        var chave = Normalizar(codigo);
        if (PorBanco.TryGetValue(banco.Codigo, out var tabela) && tabela.TryGetValue(chave, out var especifica))
        {
            return especifica;
        }

        if (OcorrenciasBase.TryGetValue(chave, out var comum))
        {
            return comum;
        }

        throw new ArgumentException($"Ocorrência CNAB 400 '{codigo}' não catalogada para {banco}.", nameof(codigo));
    }

    /// <summary>Busca a descrição de um motivo de rejeição na tabela do banco.</summary>
    /// <param name="banco">Banco emissor.</param>
    /// <param name="codigo">Código do motivo.</param>
    /// <returns>Descrição publicada no manual.</returns>
    public static string ObterMotivo(Banco banco, string codigo)
    {
        ArgumentNullException.ThrowIfNull(banco);
        var chave = Normalizar(codigo);
        if (MotivosPorBanco.TryGetValue(banco.Codigo, out var tabela) && tabela.TryGetValue(chave, out var especifica))
        {
            return especifica;
        }

        if (MotivosBase.TryGetValue(chave, out var comum))
        {
            return comum;
        }

        throw new ArgumentException($"Motivo CNAB 400 '{codigo}' não catalogado para {banco}.", nameof(codigo));
    }

    /// <summary>Todas as ocorrências catalogadas para um banco (incluindo a tabela comum).</summary>
    /// <param name="banco">Banco.</param>
    /// <returns>Coleção somente leitura.</returns>
    public static IReadOnlyDictionary<string, OcorrenciaRetorno> Todas(Banco banco)
    {
        ArgumentNullException.ThrowIfNull(banco);
        return PorBanco.TryGetValue(banco.Codigo, out var tabela) ? tabela : OcorrenciasBase;
    }

    private static string Normalizar(string codigo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        return codigo.Trim().ToUpperInvariant();
    }

    private static Dictionary<string, OcorrenciaRetorno> Novo() => new(StringComparer.Ordinal);

    private static void Add(
        Dictionary<string, OcorrenciaRetorno> tabela,
        string codigo,
        string descricao,
        FamiliaOcorrencia familia) => tabela[codigo] = new OcorrenciaRetorno(codigo, descricao, familia);

    private static Dictionary<string, string> NovosMotivos() => new(StringComparer.Ordinal);

    private static IReadOnlyDictionary<string, OcorrenciaRetorno> CriarBase()
    {
        var t = Novo();
        Add(t, "02", "Entrada confirmada", FamiliaOcorrencia.EntradaConfirmada);
        Add(t, "03", "Entrada rejeitada", FamiliaOcorrencia.EntradaRejeitada);
        Add(t, "04", "Transferência de carteira/entrada", FamiliaOcorrencia.EntradaConfirmada);
        Add(t, "05", "Transferência de carteira/baixa", FamiliaOcorrencia.Baixa);
        Add(t, "06", "Liquidação normal", FamiliaOcorrencia.Liquidacao);
        Add(t, "07", "Confirmação do recebimento da instrução de desconto", FamiliaOcorrencia.Informativo);
        Add(t, "08", "Confirmação do recebimento do cancelamento do desconto", FamiliaOcorrencia.Cancelamento);
        Add(t, "09", "Baixa", FamiliaOcorrencia.Baixa);
        Add(t, "10", "Baixa conforme instruções da agência", FamiliaOcorrencia.Baixa);
        Add(t, "11", "Em ser — arquivo de títulos pendentes", FamiliaOcorrencia.Informativo);
        Add(t, "12", "Abatimento concedido", FamiliaOcorrencia.Informativo);
        Add(t, "13", "Abatimento cancelado", FamiliaOcorrencia.Cancelamento);
        Add(t, "14", "Vencimento alterado", FamiliaOcorrencia.Informativo);
        Add(t, "15", "Liquidação em cartório", FamiliaOcorrencia.Liquidacao);
        Add(t, "16", "Título pago em cheque vinculado", FamiliaOcorrencia.Liquidacao);
        Add(t, "17", "Liquidação após baixa ou título não registrado", FamiliaOcorrencia.Liquidacao);
        Add(t, "18", "Acerto de depositária", FamiliaOcorrencia.Informativo);
        Add(t, "19", "Confirmação do recebimento da instrução de protesto", FamiliaOcorrencia.Informativo);
        Add(t, "20", "Confirmação do recebimento da instrução de sustação de protesto", FamiliaOcorrencia.Cancelamento);
        Add(t, "21", "Acerto do controle do participante", FamiliaOcorrencia.Informativo);
        Add(t, "23", "Entrada do título em cartório", FamiliaOcorrencia.Informativo);
        Add(t, "24", "Entrada rejeitada por CEP irregular", FamiliaOcorrencia.EntradaRejeitada);
        Add(t, "25", "Alegações do pagador", FamiliaOcorrencia.Informativo);
        Add(t, "26", "Instrução rejeitada", FamiliaOcorrencia.Rejeicao);
        Add(t, "27", "Baixa rejeitada", FamiliaOcorrencia.Rejeicao);
        Add(t, "28", "Débito de tarifas/custas", FamiliaOcorrencia.Tarifa);
        Add(t, "29", "Ocorrências do pagador", FamiliaOcorrencia.Informativo);
        Add(t, "30", "Alteração de dados rejeitada", FamiliaOcorrencia.Rejeicao);
        Add(t, "31", "Confirmação da inclusão de cadastro de pagador", FamiliaOcorrencia.Informativo);
        Add(t, "32", "Instrução rejeitada", FamiliaOcorrencia.Rejeicao);
        Add(t, "33", "Confirmação do pedido de alteração de outros dados", FamiliaOcorrencia.Informativo);
        Add(t, "34", "Retirado de cartório e manutenção em carteira", FamiliaOcorrencia.Informativo);
        Add(t, "35", "Cancelamento do agendamento do débito automático", FamiliaOcorrencia.Cancelamento);
        Add(t, "40", "Estorno de pagamento", FamiliaOcorrencia.Cancelamento);
        Add(t, "55", "Sustado judicial", FamiliaOcorrencia.Informativo);
        Add(t, "66", "Título baixado por pagamento via Pix", FamiliaOcorrencia.Baixa);
        Add(t, "68", "Acerto dos dados do rateio de crédito", FamiliaOcorrencia.Informativo);
        Add(t, "69", "Cancelamento de rateio", FamiliaOcorrencia.Cancelamento);
        return t;
    }

    private static IReadOnlyDictionary<string, string> CriarMotivosBase()
    {
        var m = NovosMotivos();
        m["00"] = "Ocorrência aceita";
        m["01"] = "Código do banco inválido";
        m["02"] = "Código do registro detalhe inválido";
        m["03"] = "Código da ocorrência inválida";
        m["04"] = "Código da ocorrência não permitida para a carteira";
        m["05"] = "Código da ocorrência não numérico";
        m["07"] = "Agência/conta/dígito inválido";
        m["08"] = "Nosso número inválido";
        m["09"] = "Nosso número duplicado";
        m["10"] = "Carteira inválida";
        m["13"] = "Identificação da emissão do bloqueto inválida";
        m["16"] = "Data de vencimento inválida";
        m["18"] = "Vencimento fora do prazo de operação";
        m["20"] = "Valor do título inválido";
        m["21"] = "Espécie do título inválida";
        m["22"] = "Espécie não permitida para a carteira";
        m["24"] = "Data de emissão inválida";
        m["27"] = "Valor/taxa de juros de mora inválido";
        m["28"] = "Código do desconto inválido";
        m["29"] = "Valor do desconto maior ou igual ao valor do título";
        m["32"] = "Valor do IOF inválido";
        m["34"] = "Valor do abatimento maior ou igual ao valor do título";
        m["38"] = "Prazo para protesto/negativação inválido";
        m["39"] = "Pedido de protesto/negativação não permitido para o título";
        m["44"] = "Código da moeda inválido";
        m["45"] = "Nome do pagador não informado";
        m["46"] = "Tipo/número de inscrição do pagador inválidos";
        m["47"] = "Endereço do pagador não informado";
        m["48"] = "CEP inválido";
        m["49"] = "CEP sem praça de cobrança";
        m["50"] = "CEP irregular — banco correspondente";
        m["53"] = "Tipo/número de inscrição do beneficiário final inválidos";
        m["54"] = "Sacador/avalista (beneficiário final) não informado";
        m["59"] = "Valor/percentual da multa inválido";
        m["63"] = "Entrada para título já cadastrado";
        m["65"] = "Limite excedido";
        m["66"] = "Número de autorização inexistente";
        m["79"] = "Data de juros de mora inválida";
        m["80"] = "Data do desconto inválida";
        m["86"] = "Seu número inválido";
        m["P1"] = "Registrado com QR Code Pix";
        m["P2"] = "Registrado sem QR Code Pix";
        m["P3"] = "Chave Pix inválida";
        m["P4"] = "Chave Pix sem cadastro no DICT";
        m["P5"] = "Chave Pix não compatível com CNPJ/CPF ou agência/conta informada";
        m["P6"] = "Identificador (TXID) em duplicidade";
        m["P7"] = "Identificador (TXID) inválido ou não encontrado";
        m["P8"] = "Alteração não permitida — QR Code concluído/removido";
        return m;
    }

    // Bradesco — manual 4008.524.0121 v17, seção "Códigos de Ocorrência"
    // e "Motivos das Rejeições" (posições 109-110 e 319-328).
    private static IReadOnlyDictionary<string, OcorrenciaRetorno> BradescoOcorrencias()
    {
        var t = Novo();
        Add(t, "02", "Entrada confirmada", FamiliaOcorrencia.EntradaConfirmada);
        Add(t, "03", "Entrada rejeitada", FamiliaOcorrencia.EntradaRejeitada);
        Add(t, "04", "Transferência de carteira/entrada", FamiliaOcorrencia.EntradaConfirmada);
        Add(t, "05", "Transferência de carteira/baixa", FamiliaOcorrencia.Baixa);
        Add(t, "06", "Liquidação normal", FamiliaOcorrencia.Liquidacao);
        Add(t, "07", "Confirmação de exclusão de cadastro de pagador", FamiliaOcorrencia.Informativo);
        Add(t, "08", "Rejeição do pedido de exclusão de cadastro de pagador", FamiliaOcorrencia.Rejeicao);
        Add(t, "09", "Baixado automaticamente via arquivo", FamiliaOcorrencia.Baixa);
        Add(t, "10", "Baixado conforme instruções da agência", FamiliaOcorrencia.Baixa);
        Add(t, "11", "Em ser — arquivo de títulos pendentes", FamiliaOcorrencia.Informativo);
        Add(t, "12", "Abatimento concedido", FamiliaOcorrencia.Informativo);
        Add(t, "13", "Abatimento cancelado", FamiliaOcorrencia.Cancelamento);
        Add(t, "14", "Vencimento alterado", FamiliaOcorrencia.Informativo);
        Add(t, "15", "Liquidação em cartório", FamiliaOcorrencia.Liquidacao);
        Add(t, "16", "Título pago em cheque — vinculado", FamiliaOcorrencia.Liquidacao);
        Add(t, "17", "Liquidação após baixa ou título não registrado", FamiliaOcorrencia.Liquidacao);
        Add(t, "18", "Acerto de depositária", FamiliaOcorrencia.Informativo);
        Add(t, "19", "Confirmação do recebimento de instrução de protesto", FamiliaOcorrencia.Informativo);
        Add(t, "20", "Confirmação do recebimento de instrução de sustação de protesto", FamiliaOcorrencia.Cancelamento);
        Add(t, "21", "Acerto do controle do participante", FamiliaOcorrencia.Informativo);
        Add(t, "22", "Título com pagamento cancelado", FamiliaOcorrencia.Cancelamento);
        Add(t, "23", "Entrada do título em cartório", FamiliaOcorrencia.Informativo);
        Add(t, "24", "Entrada rejeitada por CEP irregular", FamiliaOcorrencia.EntradaRejeitada);
        Add(t, "25", "Confirmação do recebimento de instrução de protesto falimentar", FamiliaOcorrencia.Informativo);
        Add(t, "27", "Baixa rejeitada", FamiliaOcorrencia.Rejeicao);
        Add(t, "28", "Débito de tarifas/custas", FamiliaOcorrencia.Tarifa);
        Add(t, "29", "Ocorrências do pagador", FamiliaOcorrencia.Informativo);
        Add(t, "30", "Alteração de outros dados rejeitada", FamiliaOcorrencia.Rejeicao);
        Add(t, "31", "Confirmado — inclusão de cadastro de pagador", FamiliaOcorrencia.Informativo);
        Add(t, "32", "Instrução rejeitada", FamiliaOcorrencia.Rejeicao);
        Add(t, "33", "Confirmação do pedido de alteração de outros dados", FamiliaOcorrencia.Informativo);
        Add(t, "34", "Retirado de cartório e manutenção em carteira", FamiliaOcorrencia.Informativo);
        Add(t, "35", "Cancelamento do agendamento do débito automático", FamiliaOcorrencia.Cancelamento);
        Add(t, "37", "Rejeitada inclusão de cadastro de pagador", FamiliaOcorrencia.Rejeicao);
        Add(t, "38", "Confirmado — alteração de cadastro de pagador", FamiliaOcorrencia.Informativo);
        Add(t, "39", "Rejeitada alteração de cadastro de pagador", FamiliaOcorrencia.Rejeicao);
        Add(t, "40", "Estorno de pagamento", FamiliaOcorrencia.Cancelamento);
        Add(t, "55", "Sustado judicial", FamiliaOcorrencia.Informativo);
        Add(t, "66", "Título baixado por pagamento via Pix", FamiliaOcorrencia.Baixa);
        Add(t, "68", "Acerto dos dados do rateio de crédito", FamiliaOcorrencia.Informativo);
        Add(t, "69", "Cancelamento de rateio", FamiliaOcorrencia.Cancelamento);
        Add(t, "73", "Confirmação do recebimento do pedido de negativação", FamiliaOcorrencia.Informativo);
        Add(t, "74", "Confirmação do pedido de exclusão de negativação", FamiliaOcorrencia.Informativo);
        return t;
    }

    private static Dictionary<string, string> BradescoMotivos()
    {
        var m = NovosMotivos();
        foreach (var (codigo, descricao) in MotivosBase)
        {
            m[codigo] = descricao;
        }

        // Ocorrência 02 — confirmação de entrada (Tabela 1 do manual).
        m["15"] = "Características da cobrança incompatíveis";
        m["17"] = "Data de vencimento anterior à data de emissão";
        m["21"] = "Espécie do título inválido";
        m["38"] = "Prazo para protesto/negativação inválido";
        m["39"] = "Pedido para protesto/negativação não permitido para o título";
        m["43"] = "Prazo para baixa e devolução inválido";
        m["67"] = "Débito automático agendado";
        m["68"] = "Débito não agendado — erro nos dados de remessa";
        m["69"] = "Débito não agendado — pagador não consta no cadastro de autorizante";
        m["70"] = "Débito não agendado — beneficiário não autorizado pelo pagador";
        m["71"] = "Débito não agendado — beneficiário não participa da modalidade de débito automático";
        m["72"] = "Débito não agendado — código de moeda diferente de R$";
        m["73"] = "Débito não agendado — data de vencimento inválida/vencida";
        m["75"] = "Débito não agendado — tipo/número de inscrição do pagador debitado inválido";
        m["76"] = "Pagador eletrônico DDA (somente empresas cadastradas)";
        m["87"] = "Título baixado por coobrigação e devolvido para carteira";
        // Ocorrência 03 — entrada rejeitada (Tabela 2 do manual).
        m["04"] = "Código de ocorrência não permitida para a carteira";
        m["05"] = "Código de ocorrência não numérico";
        m["90"] = "Linha da mensagem inválida/quantidade excedida";
        m["A3"] = "Beneficiário final/sacador/pagador devem ser iguais";
        m["A6"] = "Espécie BDP/depósito e aporte não aceita pagamento parcial";
        // Tarifas (ocorrência 28).
        m["12"] = "Tarifa de registro";
        m["03"] = "Sustação de protesto";
        m["04"] = "Protesto";
        m["08"] = "Custas de protesto";
        m["83"] = "Tarifa de rateio";
        return m;
    }

    // Itaú — manual "Cobrança CNAB 400" janeiro/2017, Nota 17 (ocorrências)
    // e Tabela 1 (erros de entrada, posições 378-385).
    private static IReadOnlyDictionary<string, OcorrenciaRetorno> ItauOcorrencias()
    {
        var t = Novo();
        void A(string c, string d, FamiliaOcorrencia f) => Add(t, c, d, f);
        A("02", "Entrada confirmada com possibilidade de mensagem", FamiliaOcorrencia.EntradaConfirmada);
        A("03", "Entrada rejeitada", FamiliaOcorrencia.EntradaRejeitada);
        A("04", "Alteração de dados — nova entrada ou alteração/exclusão acatada", FamiliaOcorrencia.Informativo);
        A("05", "Alteração de dados — baixa", FamiliaOcorrencia.Baixa);
        A("06", "Liquidação normal", FamiliaOcorrencia.Liquidacao);
        A("07", "Liquidação parcial — cobrança inteligente (B2B)", FamiliaOcorrencia.Liquidacao);
        A("08", "Liquidação em cartório", FamiliaOcorrencia.Liquidacao);
        A("09", "Baixa simples", FamiliaOcorrencia.Baixa);
        A("10", "Baixa por ter sido liquidado", FamiliaOcorrencia.Baixa);
        A("11", "Em ser (somente no retorno mensal)", FamiliaOcorrencia.Informativo);
        A("12", "Abatimento concedido", FamiliaOcorrencia.Informativo);
        A("13", "Abatimento cancelado", FamiliaOcorrencia.Cancelamento);
        A("14", "Vencimento alterado", FamiliaOcorrencia.Informativo);
        A("15", "Baixas rejeitadas", FamiliaOcorrencia.Rejeicao);
        A("16", "Instruções rejeitadas", FamiliaOcorrencia.Rejeicao);
        A("17", "Alteração/exclusão de dados rejeitados", FamiliaOcorrencia.Rejeicao);
        A("18", "Cobrança contratual — instruções/alterações rejeitadas ou pendentes", FamiliaOcorrencia.Rejeicao);
        A("19", "Confirmação do recebimento da instrução de protesto", FamiliaOcorrencia.Informativo);
        A("20", "Confirmação do recebimento da instrução de sustação de protesto/tarifa", FamiliaOcorrencia.Cancelamento);
        A("21", "Confirmação do recebimento da instrução de não protestar", FamiliaOcorrencia.Informativo);
        A("23", "Título enviado a cartório/tarifa", FamiliaOcorrencia.Informativo);
        A("24", "Instrução de protesto rejeitada/sustada/pendente", FamiliaOcorrencia.Rejeicao);
        A("25", "Alegações do pagador", FamiliaOcorrencia.Informativo);
        A("26", "Tarifa de aviso de cobrança", FamiliaOcorrencia.Tarifa);
        A("27", "Tarifa de extrato de posição (B40X)", FamiliaOcorrencia.Tarifa);
        A("28", "Tarifa de relação das liquidações", FamiliaOcorrencia.Tarifa);
        A("29", "Tarifa de manutenção de títulos vencidos", FamiliaOcorrencia.Tarifa);
        A("30", "Débito mensal de tarifas (entradas e baixas)", FamiliaOcorrencia.Tarifa);
        A("32", "Baixa por ter sido protestado", FamiliaOcorrencia.Baixa);
        A("33", "Custas de protesto", FamiliaOcorrencia.Tarifa);
        A("34", "Custas de sustação", FamiliaOcorrencia.Tarifa);
        A("35", "Custas de cartório distribuidor", FamiliaOcorrencia.Tarifa);
        A("36", "Custas de edital", FamiliaOcorrencia.Tarifa);
        A("37", "Tarifa de emissão de boleto/envio de duplicata", FamiliaOcorrencia.Tarifa);
        A("38", "Tarifa de instrução", FamiliaOcorrencia.Tarifa);
        A("39", "Tarifa de ocorrências", FamiliaOcorrencia.Tarifa);
        A("40", "Tarifa mensal de emissão de boleto/envio de duplicata", FamiliaOcorrencia.Tarifa);
        A("41", "Débito mensal de tarifas — extrato de posição B4EP/B4OX", FamiliaOcorrencia.Tarifa);
        A("42", "Débito mensal de tarifas — outras instruções", FamiliaOcorrencia.Tarifa);
        A("43", "Débito mensal de tarifas — manutenção de títulos vencidos", FamiliaOcorrencia.Tarifa);
        A("44", "Débito mensal de tarifas — outras ocorrências", FamiliaOcorrencia.Tarifa);
        A("45", "Débito mensal de tarifas — protesto", FamiliaOcorrencia.Tarifa);
        A("46", "Débito mensal de tarifas — sustação de protesto", FamiliaOcorrencia.Tarifa);
        A("47", "Baixa com transferência para desconto", FamiliaOcorrencia.Baixa);
        A("48", "Custas de sustação judicial", FamiliaOcorrencia.Tarifa);
        A("51", "Tarifa mensal de entradas de bancos correspondentes na carteira", FamiliaOcorrencia.Tarifa);
        A("52", "Tarifa mensal de baixas na carteira", FamiliaOcorrencia.Tarifa);
        A("53", "Tarifa mensal de baixas em bancos correspondentes", FamiliaOcorrencia.Tarifa);
        A("54", "Tarifa mensal de liquidações na carteira", FamiliaOcorrencia.Tarifa);
        A("55", "Tarifa mensal de liquidações em bancos correspondentes", FamiliaOcorrencia.Tarifa);
        A("56", "Custas de irregularidade", FamiliaOcorrencia.Tarifa);
        A("57", "Instrução cancelada", FamiliaOcorrencia.Cancelamento);
        A("59", "Baixa por crédito em C/C através do SISPAG", FamiliaOcorrencia.Baixa);
        A("60", "Entrada rejeitada carnê", FamiliaOcorrencia.EntradaRejeitada);
        A("61", "Tarifa de emissão de aviso de movimentação de títulos (2154)", FamiliaOcorrencia.Tarifa);
        A("62", "Débito mensal de tarifa de aviso de movimentação de títulos (2154)", FamiliaOcorrencia.Tarifa);
        A("63", "Título sustado judicialmente", FamiliaOcorrencia.Informativo);
        A("64", "Entrada confirmada com rateio de crédito", FamiliaOcorrencia.EntradaConfirmada);
        A("65", "Pagamento com cheque — aguardando compensação", FamiliaOcorrencia.Informativo);
        A("69", "Cheque devolvido", FamiliaOcorrencia.Rejeicao);
        A("71", "Entrada registrada, aguardando avaliação", FamiliaOcorrencia.Informativo);
        A("72", "Baixa por crédito em C/C via SISPAG sem título correspondente", FamiliaOcorrencia.Baixa);
        A("73", "Entrada confirmada na cobrança simples — não aceita na contratual", FamiliaOcorrencia.EntradaConfirmada);
        A("74", "Instrução de negativação expressa rejeitada", FamiliaOcorrencia.Rejeicao);
        A("75", "Confirmação do recebimento da instrução de entrada em negativação expressa", FamiliaOcorrencia.Informativo);
        A("76", "Cheque compensado", FamiliaOcorrencia.Liquidacao);
        A("77", "Confirmação do recebimento da instrução de exclusão de entrada em negativação expressa", FamiliaOcorrencia.Informativo);
        A("78", "Confirmação do recebimento da instrução de cancelamento da negativação expressa", FamiliaOcorrencia.Informativo);
        A("79", "Negativação expressa informacional", FamiliaOcorrencia.Informativo);
        A("80", "Confirmação de entrada em negativação expressa — tarifa", FamiliaOcorrencia.Tarifa);
        A("82", "Confirmação do cancelamento de negativação expressa — tarifa", FamiliaOcorrencia.Tarifa);
        A("83", "Confirmação de exclusão/cancelamento de negativação expressa por liquidação — tarifa", FamiliaOcorrencia.Tarifa);
        A("85", "Tarifa por boleto (até 3 envios) cobrança ativa eletrônica", FamiliaOcorrencia.Tarifa);
        A("86", "Tarifa de e-mail cobrança ativa eletrônica", FamiliaOcorrencia.Tarifa);
        A("87", "Tarifa de SMS cobrança ativa eletrônica", FamiliaOcorrencia.Tarifa);
        A("88", "Tarifa mensal por boleto cobrança ativa eletrônica", FamiliaOcorrencia.Tarifa);
        A("89", "Tarifa mensal de e-mail cobrança ativa eletrônica", FamiliaOcorrencia.Tarifa);
        A("90", "Tarifa mensal de SMS cobrança ativa eletrônica", FamiliaOcorrencia.Tarifa);
        A("91", "Tarifa mensal de exclusão de entrada em negativação expressa", FamiliaOcorrencia.Tarifa);
        A("92", "Tarifa mensal de cancelamento de negativação expressa", FamiliaOcorrencia.Tarifa);
        A("93", "Tarifa mensal de exclusão/cancelamento por liquidação", FamiliaOcorrencia.Tarifa);
        return t;
    }

    private static Dictionary<string, string> ItauMotivos()
    {
        var m = NovosMotivos();
        // Tabela 1 — entradas rejeitadas (posições 378-385).
        m["03"] = "Agência cobradora: CEP sem atendimento de protesto no momento";
        m["04"] = "Sigla do estado inválida";
        m["05"] = "Prazo da operação menor que o mínimo ou maior que o máximo";
        m["07"] = "Valor do título maior que 10.000.000,00";
        m["08"] = "Nome do pagador não informado ou deslocado";
        m["09"] = "Agência encerrada";
        m["10"] = "Logradouro não informado ou deslocado";
        m["11"] = "CEP não numérico ou inválido";
        m["12"] = "Nome do sacador/avalista não informado ou deslocado";
        m["13"] = "CEP incompatível com a sigla do estado";
        m["14"] = "Nosso número já registrado no cadastro do banco ou fora da faixa";
        m["15"] = "Nosso número em duplicidade no mesmo movimento";
        m["18"] = "Data de entrada inválida para operar com esta carteira";
        m["19"] = "Ocorrência inválida";
        m["21"] = "Agência cobradora não cadastrada/encerrada ou estado diferente do pagador";
        m["22"] = "Carteira não permitida (necessário cadastrar faixa livre)";
        m["26"] = "Agência/conta não liberada para operar com cobrança";
        m["27"] = "CNPJ do beneficiário inapto/devolução de título em garantia";
        m["29"] = "Categoria da conta inválida";
        m["30"] = "Entrada bloqueada — conta suspensa em cobrança";
        m["31"] = "Conta não tem permissão para protestar";
        m["35"] = "IOF maior que 5%";
        m["36"] = "Quantidade de moeda incompatível com o valor do título";
        m["37"] = "CNPJ/CPF do pagador não numérico ou igual a zeros";
        m["42"] = "Nosso número fora de faixa";
        m["52"] = "Empresa não aceita banco correspondente";
        m["53"] = "Empresa não aceita banco correspondente — cobrança mensagem";
        m["54"] = "Banco correspondente — título com vencimento inferior a 15 dias";
        m["55"] = "CEP não pertence à depositária informada";
        m["56"] = "Vencimento superior a 180 dias da data de entrada";
        m["57"] = "CEP só depositária Banco do Brasil com vencimento inferior a 8 dias";
        m["60"] = "Valor do abatimento inválido";
        m["61"] = "Juros de mora maior que o permitido";
        m["62"] = "Desconto maior que o valor do título";
        m["63"] = "Valor da importância por dia de desconto (IDD) não permitido";
        m["64"] = "Data de emissão inválida";
        m["65"] = "Taxa de financiamento inválida";
        m["66"] = "Data de vencimento inválida/fora de prazo de operação";
        m["67"] = "Valor do título/quantidade de moeda inválido";
        m["68"] = "Carteira inválida ou não cadastrada no intercâmbio da cobrança";
        m["90"] = "Número da linha de mensagem inválido/quantidade excedida";
        m["97"] = "Cobrança mensagem sem mensagem, porém com registro complementar";
        m["98"] = "Flash inválido — registro de mensagem sem flash cadastrado";
        m["99"] = "Flash inválido — conta com flash cadastrado e sem registro de mensagem";
        // Tabela 8 — mensagem informativa (ocorrência 02).
        m["01"] = "CEP sem atendimento de protesto no momento";
        m["02"] = "Estado com determinação legal que impede inscrição de inadimplentes no prazo solicitado";
        // Tabela 9 — motivo de devolução de cheque (ocorrência 69).
        m["11"] = "Sem fundos — 1ª apresentação";
        m["12"] = "Sem fundos — 2ª apresentação";
        m["13"] = "Conta encerrada";
        m["14"] = "Prática espúria";
        m["20"] = "Folha cancelada pelo correntista";
        m["21"] = "Contra-ordem/oposição pelo emitente ou portador";
        m["22"] = "Divergência/insuficiência de assinatura";
        m["23"] = "Cheques de órgãos públicos em desacordo com o Decreto-Lei 200/67";
        m["24"] = "Bloqueio judicial/BACEN";
        m["25"] = "Cancelamento de talonário pelo banco";
        m["28"] = "Contra-ordem por furto/roubo";
        m["29"] = "Bloqueado por falta de confirmação de talonário";
        m["30"] = "Furto/roubo de malotes";
        m["31"] = "Erro formal no cheque";
        m["32"] = "Ausência/irregularidade do carimbo de compensação";
        m["33"] = "Divergência de endosso";
        m["34"] = "Apresentado por banco não indicado no cruzamento sem endosso-mandato";
        m["35"] = "Cheque fraudado/universal/adulteração da praça";
        m["36"] = "Mais de um endosso";
        m["40"] = "Moeda inválida";
        m["41"] = "Apresentado a banco não pagador";
        m["42"] = "Não compensável na sessão/sistema";
        m["43"] = "Devolvido anteriormente pelos motivos 21, 22, 23, 24, 31, 34 e motivo persiste";
        m["44"] = "Prescrito";
        m["45"] = "Entidade obrigada a movimentar recursos do Tesouro via ordem bancária";
        m["48"] = "Valor superior ao estabelecido sem identificação do beneficiário";
        m["49"] = "Remessa nula (reapresentação)";
        return m;
    }

    // Sicredi — manual CNAB 400 v2.1, tabela de ocorrências (109-110) e motivos (319-328).
    private static IReadOnlyDictionary<string, OcorrenciaRetorno> SicrediOcorrencias()
    {
        var t = Novo();
        Add(t, "02", "Entrada confirmada", FamiliaOcorrencia.EntradaConfirmada);
        Add(t, "03", "Entrada rejeitada", FamiliaOcorrencia.EntradaRejeitada);
        Add(t, "06", "Liquidação normal", FamiliaOcorrencia.Liquidacao);
        Add(t, "07", "Intenção de pagamento", FamiliaOcorrencia.Informativo);
        Add(t, "09", "Baixado automaticamente via arquivo", FamiliaOcorrencia.Baixa);
        Add(t, "10", "Baixado conforme instruções da cooperativa", FamiliaOcorrencia.Baixa);
        Add(t, "12", "Abatimento concedido", FamiliaOcorrencia.Informativo);
        Add(t, "13", "Abatimento cancelado", FamiliaOcorrencia.Cancelamento);
        Add(t, "14", "Vencimento alterado", FamiliaOcorrencia.Informativo);
        Add(t, "15", "Liquidação em cartório", FamiliaOcorrencia.Liquidacao);
        Add(t, "17", "Liquidação após baixa", FamiliaOcorrencia.Liquidacao);
        Add(t, "19", "Confirmação do protesto/aceite do pagador", FamiliaOcorrencia.Informativo);
        Add(t, "20", "Confirmação da sustação do protesto", FamiliaOcorrencia.Cancelamento);
        Add(t, "23", "Entrada de título em cartório", FamiliaOcorrencia.Informativo);
        Add(t, "24", "Entrada rejeitada por CEP irregular", FamiliaOcorrencia.EntradaRejeitada);
        Add(t, "27", "Baixa rejeitada", FamiliaOcorrencia.Rejeicao);
        Add(t, "28", "Tarifa", FamiliaOcorrencia.Tarifa);
        Add(t, "29", "Rejeição do pagador", FamiliaOcorrencia.Rejeicao);
        Add(t, "30", "Alteração rejeitada", FamiliaOcorrencia.Rejeicao);
        Add(t, "32", "Instrução rejeitada", FamiliaOcorrencia.Rejeicao);
        Add(t, "33", "Confirmação da alteração de outros dados", FamiliaOcorrencia.Informativo);
        Add(t, "34", "Retirado de cartório", FamiliaOcorrencia.Informativo);
        Add(t, "35", "Aceite do pagador", FamiliaOcorrencia.Informativo);
        Add(t, "78", "Confirmação do pedido de negativação", FamiliaOcorrencia.Informativo);
        Add(t, "79", "Confirmação da exclusão de negativação", FamiliaOcorrencia.Informativo);
        Add(t, "80", "Entrada de negativação", FamiliaOcorrencia.Informativo);
        Add(t, "81", "Entrada de negativação rejeitada", FamiliaOcorrencia.Rejeicao);
        Add(t, "82", "Exclusão de negativação", FamiliaOcorrencia.Informativo);
        Add(t, "83", "Exclusão de negativação rejeitada", FamiliaOcorrencia.Rejeicao);
        Add(t, "84", "Exclusão de negativação por outros motivos", FamiliaOcorrencia.Informativo);
        Add(t, "85", "Ocorrência informacional", FamiliaOcorrencia.Informativo);
        return t;
    }

    private static Dictionary<string, string> SicrediMotivos()
    {
        var m = NovosMotivos();
        foreach (var (codigo, descricao) in MotivosBase)
        {
            m[codigo] = descricao;
        }

        m["01"] = "Código do banco inválido";
        m["02"] = "Código do registro detalhe inválido";
        m["03"] = "Código da ocorrência inválida";
        m["04"] = "Código da ocorrência não permitida para a carteira";
        m["05"] = "Código da ocorrência não numérico";
        m["07"] = "Cooperativa/conta/dígito inválidos";
        m["08"] = "Nosso número inválido";
        m["09"] = "Nosso número duplicado";
        m["10"] = "Carteira inválida";
        m["14"] = "Título protestado";
        m["15"] = "Cooperativa/carteira/agência/conta/nosso número inválidos";
        m["16"] = "Vencimento inválido";
        m["17"] = "Vencimento anterior à emissão";
        m["18"] = "Vencimento fora do prazo de operação";
        m["20"] = "Valor inválido";
        m["21"] = "Espécie inválida";
        m["22"] = "Espécie não permitida para a carteira";
        m["24"] = "Emissão inválida";
        m["29"] = "Desconto maior ou igual ao valor do título";
        m["31"] = "Desconto anterior existe";
        m["33"] = "Abatimento inválido";
        m["34"] = "Abatimento maior ou igual ao valor do título";
        m["36"] = "Abatimento anterior existe";
        m["38"] = "Prazo para protesto/negativação inválido";
        m["39"] = "Protesto não permitido para o título";
        m["40"] = "Ordem de protesto/negativação emitida";
        m["41"] = "Cancelamento sem ordem de protesto";
        m["44"] = "Cooperativa/agência beneficiária não prevista";
        m["45"] = "Nome do pagador inválido";
        m["46"] = "Inscrição do pagador inválida";
        m["47"] = "Endereço do pagador não informado";
        m["48"] = "CEP irregular";
        m["49"] = "Tipo de pessoa do beneficiário final inválido";
        m["50"] = "CEP irregular de banco correspondente";
        m["53"] = "Inscrição do beneficiário final inválida";
        m["54"] = "Beneficiário final não informado";
        m["60"] = "Título não cadastrado";
        m["63"] = "Título já cadastrado";
        m["A"] = "Aceito";
        m["D"] = "Desprezado";
        m["A1"] = "Praça do pagador não cadastrada";
        m["A2"] = "Tipo de cobrança divergente da praça";
        m["A3"] = "Cooperativa/agência depositária divergente";
        m["A4"] = "Beneficiário não cadastrado/CNPJ inválido";
        m["A5"] = "Pagador não cadastrado";
        m["A6"] = "Data da instrução inválida";
        m["A7"] = "Ocorrência não comandável";
        m["A8"] = "Liquidação fora da rede Sicredi — compensação eletrônica";
        m["B4"] = "Moeda inválida";
        m["B5"] = "Desconto/juros inválido";
        m["B6"] = "Mensagem padrão não cadastrada";
        m["B7"] = "Seu número inválido";
        m["B8"] = "Percentual de multa inválido";
        m["B9"] = "Valor/percentual de juros inválido";
        m["C1"] = "Data limite de desconto inválida";
        m["C2"] = "Aceite inválido";
        m["C3"] = "Campo alterado inválido";
        m["C4"] = "Não confirmado pela centralizadora";
        m["C5"] = "Rejeitado pela centralizadora";
        m["C6"] = "Título já liquidado";
        m["C7"] = "Título já baixado";
        m["C8"] = "Instrução pendente";
        m["C9"] = "Abatimento prévio inexistente";
        m["D1"] = "Título em dia";
        m["D2"] = "Espécie não permite protesto";
        m["D3"] = "Baixa pendente";
        m["D4"] = "Mensagens excedem o limite";
        m["D5"] = "Quantidade inválida de boletos pré-impressos";
        m["D6"] = "Tipo de impressão inválido sem registro";
        m["D7"] = "Cidade/UF do pagador não informada";
        m["D8"] = "Sequência do nosso número esgotada";
        m["D9"] = "Registro de mensagem de título não cadastrado";
        m["E2"] = "Registro complementar não cadastrado";
        m["E3"] = "Tipo de postagem inválido";
        m["E4"] = "Pedido de boletos pré-impressos";
        m["E5"] = "Confirmação/rejeição de pedido não cadastrado";
        m["E6"] = "Pagador/beneficiário final não cadastrado";
        m["E7"] = "Valor para protesto inválido";
        m["E8"] = "Tipo de impressão inválido";
        m["E9"] = "Código do pagador divergente da cooperativa";
        m["F1"] = "Liquidado no cliente";
        m["F2"] = "Baixado no cliente";
        m["F3"] = "Título caucionado/descontado";
        m["F4"] = "Instrução fixa com caracteres inválidos";
        m["F5"] = "Nosso número/parcela fora da sequência";
        m["F6"] = "Nosso número/parcela fora da sequência";
        m["F7"] = "Falta comprovante de prestação de serviço";
        m["F8"] = "Nome do beneficiário incompleto";
        m["F9"] = "CNPJ/CPF incompatível com o nome";
        m["G1"] = "CNPJ/CPF do pagador incompatível com a espécie";
        m["H1"] = "Nome do pagador incompleto";
        m["H2"] = "Sustação judicial";
        m["H3"] = "Pagador não encontrado";
        m["H4"] = "Alteração de carteira";
        m["H5"] = "Liquidação fora da rede Sicredi — valor inferior";
        m["H6"] = "Liquidação fora da rede Sicredi — valor superior";
        m["H7"] = "Espécie exige pessoa jurídica";
        m["H8"] = "Contingência Compe";
        m["H9"] = "Dados não conferem com o arquivo enviado";
        m["I1"] = "Pagador/beneficiário final/vencimento/comprovantes inconsistentes";
        m["J1"] = "Endereço/endosso/assinatura/preenchimento/direito de regresso inconsistentes";
        m["K1"] = "Duplicidade/protestado/letra vencida/falta declaração/câmbio/documento físico/falecido/quitação/jurisdição";
        m["L1"] = "Concordata/falência/edital/dados do pagador/código duplicado/comando PF/protesto com negativação simultâneos";
        m["M1"] = "Reconhecimento de dívida";
        m["M2"] = "Não reconhecimento de dívida";
        m["M3"] = "Desconto 2/3 inválido";
        m["N1"] = "Decurso de prazo";
        m["N2"] = "Decisão judicial";
        m["N3"] = "Solicitação do conveniado";
        m["N4"] = "Devolução pelos correios";
        m["N5"] = "Outros motivos";
        m["P1"] = "Confirmado com QrCode";
        m["P2"] = "Confirmado sem QrCode";
        m["P6"] = "TXID duplicado/inválido";
        m["PX"] = "Liquidação via QrCode";
        m["S1"] = "Rejeitado pela empresa de negativação";
        m["X0"] = "Pago com cheque";
        m["X1"] = "Regularização pela centralizadora";
        m["X6"] = "Cheque bloqueado entre 24 e 144 horas";
        m["S4"] = "Inclusão de negativação";
        m["S5"] = "Exclusão de negativação";
        return m;
    }

    // Inter — manual CNAB400 V2.2 (26/08/2024), posições 90-91 do retorno.
    private static IReadOnlyDictionary<string, OcorrenciaRetorno> InterOcorrencias()
    {
        var t = Novo();
        Add(t, "02", "Em aberto", FamiliaOcorrencia.Informativo);
        Add(t, "03", "Erro — motivo em texto livre nas posições 241-280", FamiliaOcorrencia.Rejeicao);
        Add(t, "06", "Pago", FamiliaOcorrencia.Liquidacao);
        Add(t, "07", "Cancelado", FamiliaOcorrencia.Cancelamento);
        return t;
    }

    // O Inter publica o motivo de rejeição em texto livre (241-380), sem
    // tabela codificada: não há códigos para catalogar neste marco.
    private static Dictionary<string, string> InterMotivos() => NovosMotivos();
}
