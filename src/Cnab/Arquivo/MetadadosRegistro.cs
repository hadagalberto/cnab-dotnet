namespace Cnab.Arquivo;

using System.Collections.Concurrent;
using System.Reflection;

/// <summary>
/// Metadados declarativos de um registro CNAB, usados por ferramentas de
/// diagnóstico (por exemplo o comando <c>cnab inspect</c>) para descrever
/// posições, tamanhos e tipos de cada campo sem duplicar as declarações.
/// A descoberta acontece uma vez por tipo e é cacheada.
/// </summary>
public static class MetadadosRegistro
{
    private static readonly ConcurrentDictionary<Type, RegistroInfo> Cache = new();

    /// <summary>Informações declarativas de um tipo de registro.</summary>
    /// <param name="tipo">Tipo anotado com <see cref="RegistroCnabAttribute"/>.</param>
    /// <returns>Metadados cacheados.</returns>
    /// <exception cref="InvalidOperationException">Tipo sem <see cref="RegistroCnabAttribute"/>.</exception>
    public static RegistroInfo Obter(Type tipo) => Cache.GetOrAdd(tipo, Criar);

    /// <summary>Campos declarados, ordenados por posição (1-based).</summary>
    /// <param name="tipo">Tipo do registro.</param>
    /// <returns>Lista imutável de campos.</returns>
    public static IReadOnlyList<CampoCnabAttribute> ObterCampos(Type tipo) => Obter(tipo).Campos;

    /// <summary>Comprimento do registro em posições.</summary>
    /// <param name="tipo">Tipo do registro.</param>
    /// <returns>Comprimento declarado.</returns>
    public static int ObterTamanho(Type tipo) => Obter(tipo).Tamanho;

    /// <summary>Campos declarados de uma instância, com os valores atuais.</summary>
    /// <param name="registro">Registro preenchido.</param>
    /// <returns>Pares campo/valor, ordenados por posição.</returns>
    public static IReadOnlyList<CampoInfo> ObterCamposComValores(object registro)
    {
        ArgumentNullException.ThrowIfNull(registro);
        var campos = new List<(PropertyInfo Prop, CampoCnabAttribute Attr)>();
        foreach (var propriedade in registro.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var atributo = propriedade.GetCustomAttribute<CampoCnabAttribute>();
            if (atributo is not null)
            {
                campos.Add((propriedade, atributo));
            }
        }

        campos.Sort(static (a, b) => a.Attr.Posicao.CompareTo(b.Attr.Posicao));
        return campos.Select(par => new CampoInfo(par.Attr, par.Prop.Name, par.Prop.GetValue(registro))).ToArray();
    }

    private static RegistroInfo Criar(Type tipo)
    {
        var registro = tipo.GetCustomAttribute<RegistroCnabAttribute>()
            ?? throw new InvalidOperationException(
                $"O tipo {tipo.Name} não é um registro CNAB (falta [RegistroCnab]).");

        var campos = new List<CampoCnabAttribute>();
        foreach (var propriedade in tipo.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var campo = propriedade.GetCustomAttribute<CampoCnabAttribute>();
            if (campo is not null)
            {
                campos.Add(campo);
            }
        }

        campos.Sort(static (a, b) => a.Posicao.CompareTo(b.Posicao));
        return new RegistroInfo(registro.Tamanho, campos);
    }
}

/// <summary>Metadados imutáveis de um registro CNAB.</summary>
public sealed record RegistroInfo
{
    internal RegistroInfo(int tamanho, IReadOnlyList<CampoCnabAttribute> campos)
    {
        Tamanho = tamanho;
        Campos = campos;
    }

    /// <summary>Comprimento do registro em posições.</summary>
    public int Tamanho { get; }

    /// <summary>Campos declarados, ordenados por posição.</summary>
    public IReadOnlyList<CampoCnabAttribute> Campos { get; }
}

/// <summary>Um campo declarado de um registro e o valor lido/escrito nele.</summary>
public sealed record CampoInfo
{
    internal CampoInfo(CampoCnabAttribute atributo, string nomePropriedade, object? valor)
    {
        Atributo = atributo;
        NomePropriedade = nomePropriedade;
        Valor = valor;
    }

    /// <summary>Nome, posição, tamanho e tipo do campo.</summary>
    public CampoCnabAttribute Atributo { get; }

    /// <summary>Nome da propriedade que representa o campo no domínio.</summary>
    public string NomePropriedade { get; }

    /// <summary>Valor atual do campo.</summary>
    public object? Valor { get; }
}
