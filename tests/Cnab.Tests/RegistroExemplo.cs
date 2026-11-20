namespace Cnab.Tests;

using Cnab.Arquivo;

/// <summary>
/// Registro de exemplo com 240 posições usado para exercitar o motor de
/// registros em todos os tipos de campo (numérico, decimal, datas, alfanumérico,
/// fillers, lacunas e anuláveis). Os layouts reais por banco entram em marcos
/// posteriores; este registro existe apenas para testar o MOTOR.
/// </summary>
[RegistroCnab(240)]
public sealed record RegistroExemplo
{
    [CampoCnab("Código do Banco", 1, 3, TipoCampo.Numerico)]
    public int Banco { get; init; }

    [CampoCnab("Lote de Serviço", 4, 4, TipoCampo.Numerico)]
    public int Lote { get; init; }

    [CampoCnab("Tipo de Registro", 8, 1, TipoCampo.Numerico)]
    public int TipoRegistro { get; init; }

    // 9-17: lacuna não declarada (filler com brancos).

    [CampoCnab("Inscrição da Empresa", 18, 14, TipoCampo.Numerico)]
    public long Inscricao { get; init; }

    [CampoCnab("Nome da Empresa", 73, 30, TipoCampo.Alfanumerico)]
    public string NomeEmpresa { get; init; } = string.Empty;

    [CampoCnab("Data de Geração", 144, 8, TipoCampo.DataDdMmAaaa)]
    public DateOnly DataGeracao { get; init; }

    [CampoCnab("Hora de Geração", 152, 6, TipoCampo.Numerico)]
    public int HoraGeracao { get; init; }

    [CampoCnab("Sequencial do Arquivo", 158, 6, TipoCampo.Numerico)]
    public int SequencialArquivo { get; init; }

    [CampoCnab("Filler CNAB", 164, 3, TipoCampo.Filler, ConteudoFiller = "081")]
    public string FillerLayout { get; init; } = string.Empty;

    [CampoCnab("Densidade", 167, 5, TipoCampo.Numerico)]
    public int Densidade { get; init; }

    // Registro de detalhe dentro do mesmo tipo, para as posições finais.
    [CampoCnab("Valor do Título", 221, 13, TipoCampo.NumericoDecimal, decimais: 2)]
    public decimal? ValorTitulo { get; init; }

    [CampoCnab("Data de Vencimento (DDMMAA)", 180, 6, TipoCampo.DataDdMmAa)]
    public DateOnly? VencimentoCurto { get; init; }

    [CampoCnab("Desconto Concedido", 240, 1, TipoCampo.NumericoDecimal, decimais: 0)]
    public decimal? DescontoFlag { get; init; }
}
