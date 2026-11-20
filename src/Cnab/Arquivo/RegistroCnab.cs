namespace Cnab.Arquivo;

/// <summary>
/// Motor de registros CNAB de largura fixa: escreve e lê registros declarados
/// com <see cref="RegistroCnabAttribute"/> e <see cref="CampoCnabAttribute"/>.
/// <para>
/// Os leitores/escritores são compilados uma única vez por tipo de registro
/// (via árvores de expressão) e cacheados em campos estáticos: nenhum uso de
/// reflexão acontece por registro lido ou escrito — requisito do SPEC
/// ("compiled readers/writers with no per-record reflection").
/// </para>
/// <para>
/// Convenções (FEBRABAN, <c>Layout Padrão CNAB 240 V10.9</c>, "Alinhamento de
/// Campos"): numérico à direita com zeros à esquerda; alfanumérico à esquerda
/// com brancos à direita; campos não declarados entre campos ficam em brancos;
/// o comprimento do registro é validado na leitura e na escrita.
/// </para>
/// </summary>
public static class RegistroCnab<T>
    where T : class, new()
{
    private static readonly LayoutCompilado Layout = LayoutCompilado.Criar(typeof(T));

    /// <summary>Tamanho do registro em posições (240/400 nos layouts CNAB).</summary>
    public static int Tamanho => Layout.Tamanho;

    /// <summary>
    /// Escreve o registro como linha de largura fixa.
    /// </summary>
    /// <param name="registro">Instância com os valores dos campos.</param>
    /// <returns>Linha com exatamente <see cref="Tamanho"/> caracteres.</returns>
    /// <exception cref="CampoCnabException">Valor fora do campo (comprimento/valor/precisão).</exception>
    public static string Escrever(T registro)
    {
        ArgumentNullException.ThrowIfNull(registro);

        var linha = new char[Layout.Tamanho];
        Array.Fill(linha, ' ');
        Layout.Escrever(registro, linha);
        return new string(linha);
    }

    /// <summary>
    /// Lê uma linha para um novo registro, validando o comprimento e o
    /// conteúdo de cada campo.
    /// </summary>
    /// <param name="linha">Linha do arquivo (sem delimitadores CR/LF).</param>
    /// <param name="numeroLinha">Número 1-based da linha no arquivo, para mensagens de erro; opcional.</param>
    /// <returns>Instância populada.</returns>
    /// <exception cref="TamanhoRegistroException">Comprimento diferente do layout.</exception>
    /// <exception cref="CampoCnabException">Conteúdo inválido para o campo.</exception>
    public static T Ler(string linha, int? numeroLinha = null)
    {
        ArgumentNullException.ThrowIfNull(linha);

        if (linha.Length != Layout.Tamanho)
        {
            throw new TamanhoRegistroException(
                "Linha com tamanho incorreto para o registro",
                Layout.Tamanho,
                linha.Length,
                numeroLinha);
        }

        var registro = new T();
        Layout.Ler(registro, linha, numeroLinha);
        return registro;
    }
}
