namespace Cnab.Layouts.Cnab400;

using System.Collections.ObjectModel;
using Cnab.Modelo;

/// <summary>
/// Constantes do padrão CNAB 400 (cobrança). Os identificadores de registro e
/// a largura de 400 posições são comuns aos manuais citados em
/// <c>docs/layouts.md</c>: Itaú (janeiro/2017), Bradesco (4008.524.0121 v17),
/// Sicredi (manual CNAB 400 v2.1) e Inter (manual CNAB 400 v9).
/// </summary>
public static class Codigos400
{
    /// <summary>Comprimento de todo registro CNAB 400, em posições.</summary>
    public const int TamanhoRegistro = 400;

    /// <summary>Header de arquivo (todos os bancos).</summary>
    public const int HeaderArquivo = 0;

    /// <summary>Registro de título / transação (todos os bancos).</summary>
    public const int Detalhe = 1;

    /// <summary>Registro de mensagem (Itaú/Bradesco/Sicredi).</summary>
    public const int Mensagem = 2;

    /// <summary>Registro de multa (Itaú).</summary>
    public const int Multa = 2;

    /// <summary>Registro de rateio (Itaú = 4, Bradesco = 3).</summary>
    public const int RateioItau = 4;

    /// <summary>Registro de rateio do Bradesco.</summary>
    public const int RateioBradesco = 3;

    /// <summary>Registro complementar de e-mail/sacador (Itaú = 5).</summary>
    public const int ComplementoItau = 5;

    /// <summary>Trailer de arquivo (todos os bancos).</summary>
    public const int TrailerArquivo = 9;

    /// <summary>Código "remessa" (empresa → banco).</summary>
    public const int Remessa = 1;

    /// <summary>Código "retorno" (banco → empresa).</summary>
    public const int Retorno = 2;

    /// <summary>Moeda real no código de barras (posição 4).</summary>
    public const int MoedaReal = 9;

    /// <summary>Literal do serviço de cobrança nos headers (Bradesco/Itaú/Sicredi).</summary>
    public const string LiteralCobranca = "COBRANCA";

    /// <summary>Literal de remessa nos headers.</summary>
    public const string LiteralRemessa = "REMESSA";

    /// <summary>Literal de retorno nos headers.</summary>
    public const string LiteralRetorno = "RETORNO";

    /// <summary>Código do serviço de cobrança nos headers ("01").</summary>
    public const string CodigoServicoCobranca = "01";

    /// <summary>
    /// Códigos de espécie documental (FEBRABAN C007, domínio de dois dígitos
    /// adotado também nos manuais CNAB 400 de Itaú e Bradesco), por tipo do
    /// domínio. O mesmo mapa é usado pelo gerador CNAB 240 neste repositório.
    /// </summary>
    public static IReadOnlyDictionary<EspecieDocumento, int> Especies { get; } =
        new ReadOnlyDictionary<EspecieDocumento, int>(new Dictionary<EspecieDocumento, int>
        {
            [EspecieDocumento.DuplicataMercantilIndicacao] = 1,
            [EspecieDocumento.DuplicataRural] = 2,
            [EspecieDocumento.NotaPromissoria] = 3,
            [EspecieDocumento.NotaPromissoriaRural] = 4,
            [EspecieDocumento.NotaSeguros] = 5,
            [EspecieDocumento.Recibo] = 6,
            [EspecieDocumento.LetraCambio] = 7,
            [EspecieDocumento.NotaDebito] = 8,
            [EspecieDocumento.DuplicataServicoIndicacao] = 9,
            [EspecieDocumento.Outros] = 13,
        });
}
