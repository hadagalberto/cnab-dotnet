namespace Cnab.Digitos;

/// <summary>
/// Algoritmos de dígito verificador usados na cobrança, extraídos dos manuais
/// oficiais citados em cada chamador. As variantes por banco (regras para resto
/// 0/1/10) ficam nos tipos de cada banco; aqui ficam apenas os cálculos-base
/// compartilhados.
/// </summary>
public static class Modulo
{
    /// <summary>
    /// Módulo 10 padrão da linha digitável e do DAC Itaú: pesos 2 e 1
    /// alternados da direita para a esquerda; produtos com dois dígitos são
    /// reduzidos à soma dos dígitos; o DV é <c>10 - (soma mod 10)</c>, com
    /// resto 0 resultando em DV 0.
    /// <para>Fontes: Bradesco <c>Layout da Cobrança – Versão Português
    /// (4008-524-0121)</c>, seção "Cálculo dos DACs dos Campos da Linha
    /// Digitável"; Itaú <c>Cobrança CNAB 400 (jan/2017)</c>, Anexo 3; Caixa
    /// <c>Especificações técnicas SIGCB (67.119 v014)</c>, item 5.5.1.</para>
    /// </summary>
    /// <param name="digitos">Dígitos sobre os quais calcular o DV (apenas 0-9).</param>
    /// <returns>DV de uma posição (0 a 9).</returns>
    /// <exception cref="ArgumentException">Conteúdo com caracteres não numéricos.</exception>
    public static int Modulo10(ReadOnlySpan<char> digitos)
    {
        ExigirDigitos(digitos);
        var soma = 0;
        var peso = 2;
        for (var i = digitos.Length - 1; i >= 0; i--)
        {
            var produto = (digitos[i] - '0') * peso;
            soma += produto < 10 ? produto : produto - 9;
            peso = peso == 2 ? 1 : 2;
        }

        return (10 - (soma % 10)) % 10;
    }

    /// <summary>
    /// Soma do módulo 11 com pesos 2 até <paramref name="baseDeCalculo"/>
    /// aplicados da direita para a esquerda, reciclando o peso ao atingir a
    /// base. Devolve apenas o resto da divisão por 11; cada banco define o DV a
    /// partir do resto (regras distintas para resto 0/1/10).
    /// <para>Fontes: Bradesco 4008-524-0121 ("Módulo 11, com base de cálculo
    /// igual a 9"); Itaú Anexo 2; Santander <c>Código de Barras – Cobrança
    /// v34</c>; Caixa SIGCB Anexo I; Sicoob <c>Layouts para troca de
    /// informações</c>.</para>
    /// </summary>
    /// <param name="digitos">Dígitos sobre os quais calcular (apenas 0-9).</param>
    /// <param name="baseDeCalculo">Peso máximo antes de reciclar para 2 (9 no padrão FEBRABAN; 7 no Bradesco NN).</param>
    /// <returns>Resto da divisão da soma ponderada por 11 (0 a 10).</returns>
    /// <exception cref="ArgumentException">Conteúdo com caracteres não numéricos ou base inválida.</exception>
    public static int RestoModulo11(ReadOnlySpan<char> digitos, int baseDeCalculo = 9)
    {
        if (baseDeCalculo < 2 || baseDeCalculo > 9)
        {
            throw new ArgumentOutOfRangeException(
                nameof(baseDeCalculo),
                "A base de cálculo do módulo 11 deve estar entre 2 e 9.");
        }

        ExigirDigitos(digitos);
        var soma = 0;
        var peso = 2;
        for (var i = digitos.Length - 1; i >= 0; i--)
        {
            soma += (digitos[i] - '0') * peso;
            peso = peso == baseDeCalculo ? 2 : peso + 1;
        }

        return soma % 11;
    }

    /// <summary>
    /// DV do código de barras (posição 5 do código de 44 posições): módulo 11
    /// com pesos 2 a 9 da direita para a esquerda, sobre as posições 1-4 e
    /// 6-44; DV = 11 - resto; se o resultado for 0, 1 ou maior que 9, o DV é 1.
    /// <para>Fontes: Bradesco 4008-524-0121 ("Módulo 11, com base de cálculo
    /// igual a 9... se o resultado da subtração for igual a 0, 1 ou maior que
    /// 9, deverão assumir o dígito igual a 1"); Santander v34 (variante
    /// equivalente "somatório x 10 / 11, resto 0/1/10 → dígito 1"); Caixa SIGCB
    /// Anexo I ("não admite 0": resultado 0 ou maior que 9 → DV 1); Sicoob
    /// Layouts ("se o resultado for 0, 1 ou maior que 9, assumir 1").</para>
    /// </summary>
    /// <param name="codigoDeBarrasSemDv">Os 43 dígitos do código de barras sem o DV (posições 1-4 e 6-44, na ordem).</param>
    /// <returns>DV geral (1 a 9; nunca 0 na regra Caixa).</returns>
    public static int DvCodigoDeBarras(ReadOnlySpan<char> codigoDeBarrasSemDv)
    {
        var resto = RestoModulo11(codigoDeBarrasSemDv, 9);
        var dv = 11 - resto;
        return dv is 0 or 1 or > 9 ? 1 : dv;
    }

    /// <summary>
    /// DV dos campos da linha digitável (módulo 10). Ver <see cref="Modulo10"/>.
    /// </summary>
    /// <param name="campo">Dígitos do campo (banco+moeda+parte do campo livre, ou bloco do campo livre).</param>
    /// <returns>DV do campo (0 a 9).</returns>
    public static int DvCampoLinhaDigitavel(ReadOnlySpan<char> campo) => Modulo10(campo);

    private static void ExigirDigitos(ReadOnlySpan<char> digitos)
    {
        foreach (var c in digitos)
        {
            if (c is < '0' or > '9')
            {
                throw new ArgumentException(
                    $"Conteúdo '{new string(digitos)}' tem caractere não numérico '{c}'.",
                    nameof(digitos));
            }
        }
    }
}
