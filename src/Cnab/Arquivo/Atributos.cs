namespace Cnab.Arquivo;

/// <summary>
/// Marca uma classe como registro CNAB de largura fixa.
/// O comprimento declarado é validado na leitura (a linha deve ter
/// exatamente esse tamanho) e garantido na escrita.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class RegistroCnabAttribute : Attribute
{
    /// <summary>Inicializa o registro com o comprimento exigido.</summary>
    /// <param name="tamanho">Tamanho do registro em posições (por exemplo 240 ou 400).</param>
    public RegistroCnabAttribute(int tamanho)
    {
        Tamanho = tamanho;
    }

    /// <summary>Tamanho do registro em posições.</summary>
    public int Tamanho { get; }
}

/// <summary>
/// Declara um campo de um registro CNAB: nome, posição inicial (1-based),
/// tamanho, tipo e, para numéricos decimais, a quantidade de casas.
/// <para>
/// Posição/tamanho seguem a notação dos manuais (por exemplo FEBRABAN V10.9:
/// "Campo 08.0 Conta-Corrente Agência, De 53, Até 57").
/// </para>
/// <para>Tipos suportados nas propriedades: <see cref="int"/>, <see cref="long"/>,
/// <see cref="decimal"/>, <see cref="string"/>, <see cref="DateOnly"/> e os
/// correspondentes anuláveis. Propriedades anuláveis em branco/zeros são lidas
/// como <see langword="null"/>.</para>
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class CampoCnabAttribute : Attribute
{
    /// <summary>Inicializa um campo com posição, tamanho e tipo.</summary>
    /// <param name="nome">Nome do campo (usado em erros e diagnóstico).</param>
    /// <param name="posicao">Posição inicial, 1-based.</param>
    /// <param name="tamanho">Tamanho em posições.</param>
    /// <param name="tipo">Tipo do campo.</param>
    /// <param name="decimais">Casas decimais implícitas, quando <paramref name="tipo"/> é <see cref="TipoCampo.NumericoDecimal"/>.</param>
    public CampoCnabAttribute(string nome, int posicao, int tamanho, TipoCampo tipo, int decimais = 0)
    {
        if (posicao < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(posicao), "Posição inicial deve ser maior ou igual a 1.");
        }

        if (tamanho < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(tamanho), "Tamanho deve ser maior ou igual a 1.");
        }

        if (tipo == TipoCampo.NumericoDecimal && (decimais < 0 || decimais > 28))
        {
            throw new ArgumentOutOfRangeException(nameof(decimais), "Decimais devem estar entre 0 e 28.");
        }

        Nome = nome;
        Posicao = posicao;
        Tamanho = tamanho;
        Tipo = tipo;
        Decimais = decimais;
    }

    /// <summary>Nome do campo.</summary>
    public string Nome { get; }

    /// <summary>Posição inicial, 1-based.</summary>
    public int Posicao { get; }

    /// <summary>Tamanho em posições.</summary>
    public int Tamanho { get; }

    /// <summary>Tipo do campo.</summary>
    public TipoCampo Tipo { get; }

    /// <summary>Casas decimais implícitas (apenas <see cref="TipoCampo.NumericoDecimal"/>).</summary>
    public int Decimais { get; }

    /// <summary>
    /// Texto fixo escrito quando <see cref="Tipo"/> é <see cref="TipoCampo.Filler"/>
    /// (repetido/truncado para o tamanho; brancos por omissão).
    /// </summary>
    public string ConteudoFiller { get; set; } = " ";
}
