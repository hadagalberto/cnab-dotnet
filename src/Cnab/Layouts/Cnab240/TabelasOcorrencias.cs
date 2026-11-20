namespace Cnab.Layouts.Cnab240;

using System.Collections.ObjectModel;
using Cnab.Modelo;

/// <summary>
/// Tabelas de ocorrências de retorno e de motivos de rejeição. As descrições
/// são as publicadas nos manuais citados em <c>docs/layouts.md</c>:
/// FEBRABAN CNAB 240 V10.9 (campo C044/C047), "Particularidades BB — Leiaute
/// CNAB 240" (Banco do Brasil), manual CAIXA SIGCB 67.118 v031 e manuais
/// Sicredi/Sicoob de pagamentos. Nenhum código é inventado.
/// </summary>
public static class TabelasOcorrencias
{
    private static readonly ReadOnlyDictionary<string, OcorrenciaRetorno> CodigosCobranca = CriarCobranca();
    private static readonly ReadOnlyDictionary<string, OcorrenciaRetorno> CodigosPagamento = CriarPagamento();
    private static readonly ReadOnlyDictionary<string, string> MotivosRejeicao = CriarMotivos();

    /// <summary>
    /// Busca uma ocorrência de cobrança pelo código (string de dois dígitos).
    /// </summary>
    /// <param name="codigo">Código publicado (por exemplo "06").</param>
    /// <returns>Ocorrência correspondente.</returns>
    /// <exception cref="ArgumentException">Código não catalogado.</exception>
    public static OcorrenciaRetorno ObterCobranca(string codigo) =>
        CodigosCobranca.TryGetValue(Normalizar(codigo), out var ocorrencia)
            ? ocorrencia
            : throw new ArgumentException($"Código de ocorrência de cobrança '{codigo}' não catalogado.", nameof(codigo));

    /// <summary>
    /// Busca uma ocorrência de cobrança pelo código numérico (C044).
    /// </summary>
    /// <param name="codigo">Código numérico (por exemplo 6).</param>
    /// <returns>Ocorrência correspondente.</returns>
    /// <exception cref="ArgumentException">Código não catalogado.</exception>
    public static OcorrenciaRetorno ObterCobranca(int codigo) =>
        ObterCobranca(codigo.ToString("00", System.Globalization.CultureInfo.InvariantCulture));

    /// <summary>
    /// Busca uma ocorrência de cobrança considerando o código específico do banco,
    /// quando ele suplantar o código FEBRABAN (ex.: "A4" do BB, "YA" da CAIXA/Sicoob).
    /// </summary>
    /// <param name="banco">Banco emissor do retorno.</param>
    /// <param name="codigo">Código alfanumérico lido do arquivo.</param>
    /// <returns>Ocorrência encontrada (bank-specific primeiro, FEBRABAN depois).</returns>
    /// <exception cref="ArgumentException">Código não catalogado para o banco.</exception>
    public static OcorrenciaRetorno Obter(Banco banco, string codigo)
    {
        ArgumentNullException.ThrowIfNull(banco);
        var chave = Normalizar(codigo);
        if (TentarOcorrenciaBanco(banco.Codigo, chave, out var especifica))
        {
            return especifica;
        }

        return CodigosCobranca.TryGetValue(chave, out var ocorrencia)
            ? ocorrencia
            : throw new ArgumentException(
                $"Código de ocorrência '{codigo}' não catalogado para {banco}.", nameof(codigo));
    }

    /// <summary>
    /// Busca uma ocorrência de pagamentos (G059/C044 de pagamento).
    /// </summary>
    /// <param name="codigo">Código publicado (por exemplo "00").</param>
    /// <returns>Ocorrência correspondente.</returns>
    /// <exception cref="ArgumentException">Código não catalogado.</exception>
    public static OcorrenciaRetorno ObterPagamento(string codigo) =>
        CodigosPagamento.TryGetValue(Normalizar(codigo), out var ocorrencia)
            ? ocorrencia
            : throw new ArgumentException($"Código de ocorrência de pagamento '{codigo}' não catalogado.", nameof(codigo));

    /// <summary>
    /// Busca a descrição do motivo de rejeição (FEBRABAN C047, posições 214-223
    /// do segmento T ou 200-207 do segmento R).
    /// </summary>
    /// <param name="codigo">Código do motivo (por exemplo "48").</param>
    /// <returns>Descrição publicada.</returns>
    /// <exception cref="ArgumentException">Motivo não catalogado.</exception>
    public static string ObterMotivoRejeicao(string codigo) =>
        MotivosRejeicao.TryGetValue(Normalizar(codigo), out var descricao)
            ? descricao
            : throw new ArgumentException($"Motivo de rejeição '{codigo}' não catalogado.", nameof(codigo));

    /// <summary>
    /// Busca a descrição de um motivo considerando a tabela específica do
    /// banco (por exemplo Sicredi P1/P2 no layout de cobrança).
    /// </summary>
    /// <param name="banco">Banco emissor do retorno.</param>
    /// <param name="codigo">Código do motivo.</param>
    /// <returns>Descrição publicada.</returns>
    public static string ObterMotivo(Banco banco, string codigo)
    {
        ArgumentNullException.ThrowIfNull(banco);
        var chave = Normalizar(codigo);
        if (banco.Codigo == 748 && MotivosSicredi.TryGetValue(chave, out var sicredi))
        {
            return sicredi;
        }

        if (banco.Codigo == 341 && MotivosItau.TryGetValue(chave, out var itau))
        {
            return itau;
        }

        return ObterMotivoRejeicao(codigo);
    }

    /// <summary>Todos os códigos de ocorrência de cobrança catalogados.</summary>
    /// <returns>Coleção somente leitura (ordenação não garantida).</returns>
    public static IEnumerable<OcorrenciaRetorno> TodasCobranca() => CodigosCobranca.Values;

    /// <summary>Todos os motivos de rejeição catalogados.</summary>
    /// <returns>Pares código/descrição em coleção somente leitura.</returns>
    public static IReadOnlyDictionary<string, string> TodosMotivos() => MotivosRejeicao;

    private static bool TentarOcorrenciaBanco(int codigoBanco, string chave, out OcorrenciaRetorno ocorrencia)
    {
        var tabela = codigoBanco switch
        {
            1 => BancoDoBrasil,
            77 => Inter,
            104 => Caixa,
            208 => Btg,
            237 => Bradesco,
            341 => Itau,
            748 => Sicredi,
            33 => Santander,
            756 => Sicoob,
            _ => null,
        };
        if (tabela is not null && tabela.TryGetValue(chave, out ocorrencia!))
        {
            return true;
        }

        ocorrencia = null!;
        return false;
    }

    private static string Normalizar(string codigo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        return codigo.Trim().ToUpperInvariant();
    }

    private static ReadOnlyDictionary<string, OcorrenciaRetorno> CriarCobranca()
    {
        var itens = new[]
        {
            new OcorrenciaRetorno("02", "Entrada confirmada", FamiliaOcorrencia.EntradaConfirmada),
            new OcorrenciaRetorno("03", "Entrada rejeitada", FamiliaOcorrencia.EntradaRejeitada),
            new OcorrenciaRetorno("04", "Transferência de carteira/entrada", FamiliaOcorrencia.EntradaConfirmada),
            new OcorrenciaRetorno("05", "Transferência de carteira/baixa", FamiliaOcorrencia.Baixa),
            new OcorrenciaRetorno("06", "Liquidação", FamiliaOcorrencia.Liquidacao),
            new OcorrenciaRetorno("07", "Confirmação do recebimento da instrução de desconto", FamiliaOcorrencia.Informativo),
            new OcorrenciaRetorno("08", "Confirmação do recebimento do cancelamento do desconto", FamiliaOcorrencia.Cancelamento),
            new OcorrenciaRetorno("09", "Baixa", FamiliaOcorrencia.Baixa),
            new OcorrenciaRetorno("11", "Títulos em carteira (em ser)", FamiliaOcorrencia.Informativo),
            new OcorrenciaRetorno("12", "Confirmação de recebimento de instrução de abatimento", FamiliaOcorrencia.Informativo),
            new OcorrenciaRetorno("13", "Confirmação de recebimento de instrução de cancelamento de abatimento", FamiliaOcorrencia.Cancelamento),
            new OcorrenciaRetorno("14", "Confirmação de recebimento de instrução de alteração de vencimento", FamiliaOcorrencia.Informativo),
            new OcorrenciaRetorno("15", "Franco de pagamento", FamiliaOcorrencia.Informativo),
            new OcorrenciaRetorno("17", "Liquidação após baixa ou liquidação de título não registrado", FamiliaOcorrencia.Liquidacao),
            new OcorrenciaRetorno("19", "Confirmação de recebimento de instrução de protesto", FamiliaOcorrencia.Informativo),
            new OcorrenciaRetorno("20", "Confirmação de recebimento de instrução de sustação/cancelamento de protesto", FamiliaOcorrencia.Cancelamento),
            new OcorrenciaRetorno("23", "Remessa a cartório (aponte em cartório)", FamiliaOcorrencia.Informativo),
            new OcorrenciaRetorno("24", "Retirada de cartório e manutenção em carteira", FamiliaOcorrencia.Informativo),
            new OcorrenciaRetorno("25", "Protestado e baixado (baixa por ter sido protestado)", FamiliaOcorrencia.Baixa),
            new OcorrenciaRetorno("26", "Instrução rejeitada", FamiliaOcorrencia.Rejeicao),
            new OcorrenciaRetorno("27", "Confirmação do pedido de alteração de outros dados", FamiliaOcorrencia.Informativo),
            new OcorrenciaRetorno("28", "Débito de tarifas/custas", FamiliaOcorrencia.Tarifa),
            new OcorrenciaRetorno("29", "Ocorrências do pagador", FamiliaOcorrencia.Informativo),
            new OcorrenciaRetorno("30", "Alteração de dados rejeitada", FamiliaOcorrencia.Rejeicao),
            new OcorrenciaRetorno("33", "Confirmação da alteração dos dados do rateio de crédito", FamiliaOcorrencia.Informativo),
            new OcorrenciaRetorno("34", "Confirmação do cancelamento dos dados do rateio de crédito", FamiliaOcorrencia.Cancelamento),
            new OcorrenciaRetorno("35", "Confirmação do desagendamento do débito automático", FamiliaOcorrencia.Cancelamento),
            new OcorrenciaRetorno("36", "Confirmação de envio de e-mail/SMS", FamiliaOcorrencia.Informativo),
            new OcorrenciaRetorno("37", "Envio de e-mail/SMS rejeitado", FamiliaOcorrencia.Rejeicao),
            new OcorrenciaRetorno("38", "Confirmação de alteração do prazo limite de recebimento", FamiliaOcorrencia.Informativo),
            new OcorrenciaRetorno("39", "Confirmação de dispensa de prazo limite de recebimento", FamiliaOcorrencia.Informativo),
            new OcorrenciaRetorno("40", "Confirmação da alteração do número do título dado pelo beneficiário", FamiliaOcorrencia.Informativo),
            new OcorrenciaRetorno("41", "Confirmação da alteração do número controle do participante", FamiliaOcorrencia.Informativo),
            new OcorrenciaRetorno("42", "Confirmação da alteração dos dados do pagador", FamiliaOcorrencia.Informativo),
            new OcorrenciaRetorno("44", "Título pago com cheque devolvido (BB)", FamiliaOcorrencia.Rejeicao),
            new OcorrenciaRetorno("50", "Título pago com cheque pendente de compensação (BB)", FamiliaOcorrencia.Informativo),
            new OcorrenciaRetorno("51", "Título DDA reconhecido pelo pagador", FamiliaOcorrencia.Informativo),
            new OcorrenciaRetorno("52", "Título DDA não reconhecido pelo pagador", FamiliaOcorrencia.Informativo),
            new OcorrenciaRetorno("53", "Título DDA recusado pela CIP", FamiliaOcorrencia.Rejeicao),
            new OcorrenciaRetorno("61", "Confirmação do recebimento da instrução de não protestar", FamiliaOcorrencia.Informativo),
            new OcorrenciaRetorno("85", "Inclusão de negativação (BB)", FamiliaOcorrencia.Informativo),
            new OcorrenciaRetorno("86", "Exclusão de negativação (BB)", FamiliaOcorrencia.Informativo),
            new OcorrenciaRetorno("93", "Baixa operacional (BB)", FamiliaOcorrencia.Baixa),
            new OcorrenciaRetorno("94", "Intenção de pagamento (FEBRABAN V11.0)", FamiliaOcorrencia.Informativo),
            new OcorrenciaRetorno("95", "Cancelamento de intenção de pagamento (FEBRABAN V11.0)", FamiliaOcorrencia.Cancelamento),
            new OcorrenciaRetorno("96", "Retorno do Super Inteligente — Split Payment (FEBRABAN V11.0)", FamiliaOcorrencia.Informativo),
        };
        return new ReadOnlyDictionary<string, OcorrenciaRetorno>(
            itens.ToDictionary(static o => o.Codigo, static o => o, StringComparer.Ordinal));
    }

    private static ReadOnlyDictionary<string, OcorrenciaRetorno> CriarPagamento()
    {
        var itens = new[]
        {
            new OcorrenciaRetorno("00", "Crédito ou débito efetivado", FamiliaOcorrencia.Liquidacao),
            new OcorrenciaRetorno("01", "Insuficiência de fundos — débito não efetuado", FamiliaOcorrencia.Rejeicao),
            new OcorrenciaRetorno("02", "Crédito ou débito cancelado pelo pagador/credor", FamiliaOcorrencia.Cancelamento),
            new OcorrenciaRetorno("03", "Débito autorizado pela agência — efetuado", FamiliaOcorrencia.Liquidacao),
        };
        return new ReadOnlyDictionary<string, OcorrenciaRetorno>(
            itens.ToDictionary(static o => o.Codigo, static o => o, StringComparer.Ordinal));
    }

    private static readonly IReadOnlyDictionary<string, OcorrenciaRetorno> BancoDoBrasil = CriarBb();
    private static readonly IReadOnlyDictionary<string, OcorrenciaRetorno> Caixa = CriarCaixa();
    private static readonly IReadOnlyDictionary<string, OcorrenciaRetorno> Santander = CriarSantander();
    private static readonly IReadOnlyDictionary<string, OcorrenciaRetorno> Sicoob = CriarSicoob();
    private static readonly IReadOnlyDictionary<string, OcorrenciaRetorno> Inter = CriarInter();
    private static readonly IReadOnlyDictionary<string, OcorrenciaRetorno> Btg = CriarBtg();
    private static readonly IReadOnlyDictionary<string, OcorrenciaRetorno> Bradesco = CriarBradesco();
    private static readonly IReadOnlyDictionary<string, OcorrenciaRetorno> Itau = CriarItau();
    private static readonly IReadOnlyDictionary<string, OcorrenciaRetorno> Sicredi = CriarSicredi();

    private static IReadOnlyDictionary<string, OcorrenciaRetorno> CriarBb() => new Dictionary<string, OcorrenciaRetorno>(StringComparer.Ordinal)
    {
        ["52"] = new("52", "Registro de título já liquidado — carteira 17", FamiliaOcorrencia.Rejeicao),
        ["A4"] = new("A4", "Sacado DDA", FamiliaOcorrencia.Informativo),
    };

    private static IReadOnlyDictionary<string, OcorrenciaRetorno> CriarCaixa() => new Dictionary<string, OcorrenciaRetorno>(StringComparer.Ordinal)
    {
        ["06"] = new("06", "Tarifa de outras ocorrências", FamiliaOcorrencia.Tarifa),
        ["07"] = new("07", "Tarifa de envio de duplicata ao pagador", FamiliaOcorrencia.Tarifa),
        ["08"] = new("08", "Custas de protesto", FamiliaOcorrencia.Tarifa),
        ["09"] = new("09", "Custas de sustação de protesto", FamiliaOcorrencia.Tarifa),
        ["10"] = new("10", "Custas de cartório distribuidor", FamiliaOcorrencia.Tarifa),
        ["11"] = new("11", "Custas de edital", FamiliaOcorrencia.Tarifa),
        ["12"] = new("12", "Redisponibilização de arquivo retorno eletrônico", FamiliaOcorrencia.Tarifa),
        ["13"] = new("13", "Tarifa sobre registro cobrada na baixa/liquidação", FamiliaOcorrencia.Tarifa),
        ["14"] = new("14", "Tarifa sobre reapresentação automática", FamiliaOcorrencia.Tarifa),
        ["15"] = new("15", "Banco de pagadores", FamiliaOcorrencia.Informativo),
        ["16"] = new("16", "Tarifa sobre informações via fax", FamiliaOcorrencia.Tarifa),
        ["YA"] = new("YA", "Título não encontrado — procure o beneficiário", FamiliaOcorrencia.Rejeicao),
    };

    private static IReadOnlyDictionary<string, OcorrenciaRetorno> CriarSantander() => new Dictionary<string, OcorrenciaRetorno>(StringComparer.Ordinal)
    {
        ["YE"] = new("YE", "Código de ocorrência inválido", FamiliaOcorrencia.Rejeicao),
    };

    private static IReadOnlyDictionary<string, OcorrenciaRetorno> CriarSicoob() => new Dictionary<string, OcorrenciaRetorno>(StringComparer.Ordinal)
    {
        ["TA"] = new("TA", "Lote não aceito — totais do lote com diferença", FamiliaOcorrencia.Rejeicao),
        ["YA"] = new("YA", "Título não encontrado", FamiliaOcorrencia.Rejeicao),
        ["YB"] = new("YB", "Identificador de registro opcional inválido", FamiliaOcorrencia.Rejeicao),
    };

    // Inter — manual CNAB240 Pagamentos V1.21 (10/02/2026), Nota 01,
    // ocorrências de retorno nas posições 231-240.
    private static IReadOnlyDictionary<string, OcorrenciaRetorno> CriarInter() => new Dictionary<string, OcorrenciaRetorno>(StringComparer.Ordinal)
    {
        ["00"] = new("00", "Crédito ou débito efetivado", FamiliaOcorrencia.Liquidacao),
        ["AR"] = new("AR", "Valor do lançamento inválido", FamiliaOcorrencia.Rejeicao),
        ["AG"] = new("AG", "Agência/conta corrente/DV inválido", FamiliaOcorrencia.Rejeicao),
        ["ZI"] = new("ZI", "Beneficiário divergente", FamiliaOcorrencia.Rejeicao),
        ["AP"] = new("AP", "Data de lançamento inválida", FamiliaOcorrencia.Rejeicao),
        ["HF"] = new("HF", "Conta corrente da empresa com saldo insuficiente", FamiliaOcorrencia.Rejeicao),
        ["AB"] = new("AB", "Tipo de operação inválido", FamiliaOcorrencia.Rejeicao),
        ["AC"] = new("AC", "Tipo de serviço inválido", FamiliaOcorrencia.Rejeicao),
        ["HA"] = new("HA", "Lote não aceito", FamiliaOcorrencia.Rejeicao),
        ["BD"] = new("BD", "Inclusão efetuada com sucesso", FamiliaOcorrencia.Informativo),
        ["PA"] = new("PA", "Pix não efetivado", FamiliaOcorrencia.Rejeicao),
        ["PJ"] = new("PJ", "Chave não cadastrada no DICT", FamiliaOcorrencia.Rejeicao),
        ["PM"] = new("PM", "Chave de pagamento inválida", FamiliaOcorrencia.Rejeicao),
        ["PN"] = new("PN", "Chave de pagamento não informada", FamiliaOcorrencia.Rejeicao),
        ["PK"] = new("PK", "QR Code inválido/vencido", FamiliaOcorrencia.Rejeicao),
        ["PB"] = new("PB", "Transação interrompida — erro no PSP do recebedor", FamiliaOcorrencia.Rejeicao),
        ["PD"] = new("PD", "Tipo incorreto para conta transacional", FamiliaOcorrencia.Rejeicao),
        ["PE"] = new("PE", "Tipo de transação não suportado/autorizado", FamiliaOcorrencia.Rejeicao),
        ["PH"] = new("PH", "Ordem rejeitada pelo PSP do recebedor", FamiliaOcorrencia.Rejeicao),
        ["AL"] = new("AL", "Código do banco favorecido/instituição/depositário inválido", FamiliaOcorrencia.Rejeicao),
        ["PG"] = new("PG", "CPF/CNPJ do usuário recebedor incorreto", FamiliaOcorrencia.Rejeicao),
        ["AS"] = new("AS", "Aviso ao favorecido — identificação inválida", FamiliaOcorrencia.Rejeicao),
        ["ZK"] = new("ZK", "Boleto já liquidado", FamiliaOcorrencia.Rejeicao),
        ["HC"] = new("HC", "Convênio com a empresa inexistente/inválido", FamiliaOcorrencia.Rejeicao),
        ["CD"] = new("CD", "Código de barras — valor do título inválido", FamiliaOcorrencia.Rejeicao),
        ["ZH"] = new("ZH", "Sistema em contingência — título indexado", FamiliaOcorrencia.Informativo),
        ["PI"] = new("PI", "ISPB do PSP do pagador inválido", FamiliaOcorrencia.Rejeicao),
        ["HE"] = new("HE", "Tipo de serviço inválido para o contrato", FamiliaOcorrencia.Rejeicao),
        ["YA"] = new("YA", "Título não encontrado", FamiliaOcorrencia.Rejeicao),
        ["PF"] = new("PF", "CPF/CNPJ do recebedor inconsistente com titular/chave", FamiliaOcorrencia.Rejeicao),
        ["ZJ"] = new("ZJ", "Limite de pagamentos parciais excedido", FamiliaOcorrencia.Rejeicao),
    };

    // BTG — de-para de retorno publicado no portal do desenvolvedor (208).
    private static IReadOnlyDictionary<string, OcorrenciaRetorno> CriarBtg() => new Dictionary<string, OcorrenciaRetorno>(StringComparer.Ordinal)
    {
        ["74"] = new("74", "Recompra com substituição", FamiliaOcorrencia.Informativo),
        ["85"] = new("85", "Recompra com adiantamento", FamiliaOcorrencia.Informativo),
        ["93"] = new("93", "Título rejeitado", FamiliaOcorrencia.Rejeicao),
    };

    // Bradesco — MPO 240 v04, ocorrências proprietárias complementares.
    private static IReadOnlyDictionary<string, OcorrenciaRetorno> CriarBradesco() => new Dictionary<string, OcorrenciaRetorno>(StringComparer.Ordinal)
    {
        ["66"] = new("66", "Título baixado por pagamento via Pix", FamiliaOcorrencia.Baixa),
    };

    // Itaú — manual janeiro/2017, Nota 22 (subset proprietário complementar).
    private static IReadOnlyDictionary<string, OcorrenciaRetorno> CriarItau() => new Dictionary<string, OcorrenciaRetorno>(StringComparer.Ordinal)
    {
        ["59"] = new("59", "Baixa por crédito em C/C através do SISPAG", FamiliaOcorrencia.Baixa),
        ["64"] = new("64", "Entrada confirmada com rateio de crédito", FamiliaOcorrencia.EntradaConfirmada),
        ["65"] = new("65", "Pagamento com cheque — aguardando compensação", FamiliaOcorrencia.Informativo),
        ["74"] = new("74", "Instrução de negativação expressa rejeitada", FamiliaOcorrencia.Rejeicao),
        ["75"] = new("75", "Confirmação do recebimento da instrução de entrada em negativação expressa", FamiliaOcorrencia.Informativo),
        ["77"] = new("77", "Confirmação do recebimento da instrução de exclusão de entrada em negativação expressa", FamiliaOcorrencia.Informativo),
        ["78"] = new("78", "Confirmação do recebimento da instrução de cancelamento da negativação expressa", FamiliaOcorrencia.Informativo),
        ["79"] = new("79", "Negativação expressa informacional", FamiliaOcorrencia.Informativo),
        ["94"] = new("94", "Confirmação do recebimento da instrução de não negativar", FamiliaOcorrencia.Informativo),
    };

    // Sicredi — manual CNAB 240 cobrança, movimentos de retorno complementares.
    private static IReadOnlyDictionary<string, OcorrenciaRetorno> CriarSicredi() => new Dictionary<string, OcorrenciaRetorno>(StringComparer.Ordinal)
    {
        ["61"] = new("61", "Liquidação PIX", FamiliaOcorrencia.Liquidacao),
        ["78"] = new("78", "Confirmação do pedido de negativação", FamiliaOcorrencia.Informativo),
        ["79"] = new("79", "Confirmação da exclusão de negativação", FamiliaOcorrencia.Informativo),
        ["80"] = new("80", "Confirmação da entrada de negativação", FamiliaOcorrencia.Informativo),
        ["81"] = new("81", "Entrada de negativação rejeitada", FamiliaOcorrencia.Rejeicao),
        ["82"] = new("82", "Confirmação da exclusão de negativação", FamiliaOcorrencia.Informativo),
        ["83"] = new("83", "Exclusão de negativação rejeitada", FamiliaOcorrencia.Rejeicao),
        ["84"] = new("84", "Exclusão de negativação por outros motivos", FamiliaOcorrencia.Informativo),
        ["85"] = new("85", "Ocorrência informacional por outros motivos", FamiliaOcorrencia.Informativo),
        ["P1"] = new("P1", "Confirmado com QrCode", FamiliaOcorrencia.Informativo),
        ["P2"] = new("P2", "Confirmado sem QrCode", FamiliaOcorrencia.Informativo),
    };

    private static readonly IReadOnlyDictionary<string, string> MotivosSicredi = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["P1"] = "Confirmado com QrCode",
        ["P2"] = "Confirmado sem QrCode",
        ["P6"] = "TXID duplicado/inválido",
        ["PX"] = "Liquidação via QrCode",
    };

    private static readonly IReadOnlyDictionary<string, string> MotivosItau = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["01"] = "CEP sem atendimento de protesto no momento",
        ["02"] = "Estado com determinação legal que impede inscrição de inadimplentes no prazo solicitado",
        ["03"] = "Boleto não liquidado no desconto de duplicatas e transferido para cobrança simples",
    };

    private static ReadOnlyDictionary<string, string> CriarMotivos()
    {
        var itens = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["01"] = "Código do banco inválido",
            ["02"] = "Código do registro inválido",
            ["03"] = "Código do segmento inválido",
            ["04"] = "Código de movimento não permitido para a carteira",
            ["05"] = "Código de movimento inválido",
            ["06"] = "Tipo/número de inscrição do beneficiário inválidos",
            ["07"] = "Agência/conta/DV inválidos",
            ["08"] = "Nosso número inválido",
            ["09"] = "Nosso número duplicado",
            ["10"] = "Carteira inválida",
            ["11"] = "Forma de cadastramento do título inválido",
            ["12"] = "Tipo de documento inválido",
            ["13"] = "Identificação da emissão do boleto de pagamento inválida",
            ["14"] = "Identificação da distribuição do boleto de pagamento inválida",
            ["15"] = "Características da cobrança incompatíveis",
            ["16"] = "Data de vencimento inválida",
            ["17"] = "Data de vencimento anterior à data de emissão",
            ["18"] = "Vencimento fora do prazo de operação",
            ["19"] = "Título a cargo de bancos correspondentes com vencimento inferior a XX dias",
            ["20"] = "Valor do título inválido",
            ["21"] = "Espécie do título inválida",
            ["22"] = "Espécie do título não permitida para a carteira",
            ["23"] = "Aceite inválido",
            ["24"] = "Data da emissão inválida",
            ["25"] = "Data da emissão posterior à data de entrada",
            ["26"] = "Código de juros de mora inválido",
            ["27"] = "Valor/taxa de juros de mora inválido",
            ["28"] = "Código do desconto inválido",
            ["29"] = "Valor do desconto maior ou igual ao valor do título",
            ["30"] = "Desconto a conceder não confere",
            ["31"] = "Concessão de desconto — já existe desconto anterior",
            ["32"] = "Valor do IOF inválido",
            ["33"] = "Valor do abatimento inválido",
            ["34"] = "Valor do abatimento maior ou igual ao valor do título",
            ["35"] = "Valor a conceder não confere",
            ["36"] = "Concessão de abatimento — já existe abatimento anterior",
            ["37"] = "Código para protesto inválido",
            ["38"] = "Prazo para protesto inválido",
            ["39"] = "Pedido de protesto não permitido para o título",
            ["40"] = "Título com ordem de protesto emitida",
            ["41"] = "Pedido de cancelamento/sustação para títulos sem instrução de protesto",
            ["42"] = "Código para baixa/devolução inválido",
            ["43"] = "Prazo para baixa/devolução inválido",
            ["44"] = "Código da moeda inválido",
            ["45"] = "Nome do pagador não informado",
            ["46"] = "Tipo/número de inscrição do pagador inválidos",
            ["47"] = "Endereço do pagador não informado",
            ["48"] = "CEP inválido",
            ["49"] = "CEP sem praça de cobrança (não localizado)",
            ["50"] = "CEP referente a um banco correspondente",
            ["51"] = "CEP incompatível com a unidade da federação",
            ["52"] = "Unidade da federação inválida",
            ["53"] = "Tipo/número de inscrição do sacador/avalista inválidos",
            ["54"] = "Sacador/avalista não informado",
            ["55"] = "Nosso número no banco correspondente não informado",
            ["56"] = "Código do banco correspondente inválido",
            ["57"] = "Código da multa inválido",
            ["58"] = "Data da multa inválida",
            ["59"] = "Valor/percentual da multa inválido",
            ["60"] = "Movimento para título não cadastrado",
            ["61"] = "Alteração da agência cobradora/DV inválida",
            ["62"] = "Tipo de impressão inválido",
            ["63"] = "Entrada para título já cadastrado",
            ["64"] = "Número da linha inválido",
            ["65"] = "Código do banco para débito inválido",
            ["66"] = "Agência/conta/DV para débito inválido",
            ["67"] = "Dados para débito incompatível com a identificação da emissão do boleto de pagamento",
            ["68"] = "Débito automático agendado",
            ["69"] = "Débito não agendado — erro nos dados da remessa",
            ["70"] = "Débito não agendado — pagador não consta do cadastro de autorizante",
            ["71"] = "Débito não agendado — beneficiário não autorizado pelo pagador",
            ["72"] = "Débito não agendado — beneficiário não participa da modalidade débito automático",
            ["73"] = "Débito não agendado — código de moeda diferente de real (R$)",
            ["74"] = "Débito não agendado — data de vencimento inválida",
            ["75"] = "Débito não agendado, conforme seu pedido, título não registrado",
            ["76"] = "Débito não agendado, tipo/número de inscrição do debitado inválido",
            ["77"] = "Transferência para desconto não permitida para a carteira do título",
            ["78"] = "Data inferior ou igual ao vencimento para débito automático",
            ["79"] = "Data juros de mora inválido",
            ["80"] = "Data do desconto inválida",
            ["81"] = "Tentativas de débito esgotadas — baixado",
            ["82"] = "Tentativas de débito esgotadas — pendente",
            ["83"] = "Limite excedido",
            ["84"] = "Número de autorização inexistente",
            ["85"] = "Título com pagamento vinculado",
            ["86"] = "Seu número inválido",
            ["87"] = "E-mail/SMS enviado",
            ["88"] = "E-mail lido",
            ["89"] = "E-mail/SMS devolvido — endereço de e-mail ou número do celular incorreto",
            ["90"] = "E-mail devolvido — caixa postal cheia",
            ["91"] = "E-mail/número do celular do pagador não informado",
            ["92"] = "Pagador optante por boleto de pagamento eletrônico — e-mail não enviado",
            ["93"] = "Código para emissão de boleto de pagamento não permite envio de e-mail",
            ["94"] = "Código da carteira inválido para envio de e-mail",
            ["95"] = "Contrato não permite o envio de e-mail",
            ["96"] = "Número de contrato inválido",
            ["97"] = "Rejeição da alteração do prazo limite de recebimento",
            ["98"] = "Rejeição de dispensa de prazo limite de recebimento",
            ["99"] = "Rejeição da alteração do número do título dado pelo beneficiário",
            ["A1"] = "Rejeição da alteração do número controle do participante",
            ["A2"] = "Rejeição da alteração dos dados do pagador",
            ["A3"] = "Rejeição da alteração dos dados do sacador/avalista",
            ["A4"] = "Pagador DDA",
            ["A5"] = "Registro rejeitado — título já liquidado",
            ["A6"] = "Código do convenente inválido ou encerrado",
            ["A7"] = "Título já se encontra na situação pretendida",
            ["A8"] = "Valor do abatimento inválido para cancelamento",
            ["A9"] = "Não autoriza pagamento parcial",
            ["B1"] = "Autoriza recebimento parcial",
            ["B2"] = "Valor nominal do título conflitante",
            ["B3"] = "Tipo de pagamento inválido",
            ["B4"] = "Valor máximo/percentual inválido",
            ["B5"] = "Valor mínimo/percentual inválido",
            ["P1"] = "Registrado com QR Code Pix",
            ["P2"] = "Registrado sem QR Code Pix",
            ["P3"] = "Chave Pix inválida",
            ["P4"] = "Chave Pix sem cadastro na DICT",
            ["P5"] = "Chave Pix não compatível com o CNPJ",
            ["P6"] = "Identificador (TXID) em duplicidade",
            ["P7"] = "Identificador (TXID) inválido ou não encontrado",
        };
        return new ReadOnlyDictionary<string, string>(itens);
    }
}
