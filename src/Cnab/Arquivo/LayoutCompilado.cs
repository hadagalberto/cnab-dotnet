namespace Cnab.Arquivo;

using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Cnab.Texto;

/// <summary>
/// Metadados + delegates compilados de um layout de registro. Criado uma vez
/// por tipo (via reflexão, no primeiro uso) e cacheado em
/// <see cref="RegistroCnab{T}"/>: nenhuma reflexão acontece por registro lido
/// ou escrito.
/// </summary>
internal sealed class LayoutCompilado
{
    private readonly AcaoEscreverCampo[] _escritores;
    private readonly AcaoLerCampo[] _leitores;

    private LayoutCompilado(int tamanho, AcaoEscreverCampo[] escritores, AcaoLerCampo[] leitores)
    {
        Tamanho = tamanho;
        _escritores = escritores;
        _leitores = leitores;
    }

    /// <summary>Tamanho do registro em posições.</summary>
    internal int Tamanho { get; }

    /// <summary>Delegado compilado que escreve um campo no buffer do registro.</summary>
    /// <param name="registro">Instância de origem.</param>
    /// <param name="linha">Buffer de destino (tamanho exato do registro).</param>
    private delegate void AcaoEscreverCampo(object registro, char[] linha);

    /// <summary>Delegado compilado que lê um campo da linha para o registro.</summary>
    /// <param name="registro">Instância de destino.</param>
    /// <param name="linha">Linha do arquivo (tamanho exato do registro).</param>
    /// <param name="numeroLinha">Número 1-based da linha no arquivo, para erros.</param>
    private delegate void AcaoLerCampo(object registro, string linha, int? numeroLinha);

    /// <summary>Cria o layout compilado para um tipo de registro.</summary>
    /// <param name="tipo">Tipo anotado com <see cref="RegistroCnabAttribute"/>.</param>
    /// <returns>Layout com escritores/leitores compilados.</returns>
    /// <exception cref="InvalidOperationException">Tipo sem atributo, campos sobrepostos ou tipos incompatíveis.</exception>
    public static LayoutCompilado Criar(Type tipo)
    {
        var registroAttr = tipo.GetCustomAttribute<RegistroCnabAttribute>()
            ?? throw new InvalidOperationException(
                $"O tipo {tipo.Name} precisa de [RegistroCnab(tamanho)] para ser usado como registro CNAB.");

        var campos = new List<(PropertyInfo Prop, CampoCnabAttribute Attr)>();
        foreach (var prop in tipo.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var attr = prop.GetCustomAttribute<CampoCnabAttribute>();
            if (attr is not null)
            {
                campos.Add((prop, attr));
            }
        }

        campos.Sort(static (a, b) => a.Attr.Posicao.CompareTo(b.Attr.Posicao));

        var escritores = new List<AcaoEscreverCampo>();
        var leitores = new List<AcaoLerCampo>();
        var fimAnterior = 0;
        foreach (var (prop, attr) in campos)
        {
            var inicio = attr.Posicao - 1;
            var fim = attr.Posicao - 1 + attr.Tamanho;
            if (inicio < fimAnterior)
            {
                throw new InvalidOperationException(
                    $"Campo '{attr.Nome}' (posição {attr.Posicao}, tamanho {attr.Tamanho}) sobrepõe um campo anterior em {tipo.Name}.");
            }

            if (fim > registroAttr.Tamanho)
            {
                throw new InvalidOperationException(
                    $"Campo '{attr.Nome}' termina na posição {fim}, além do tamanho do registro ({registroAttr.Tamanho}) em {tipo.Name}.");
            }

            // Lacuna não declarada: filler com brancos (FEBRABAN V10.9:
            // campos alfanuméricos não utilizados são preenchidos com brancos).
            if (inicio > fimAnterior)
            {
                escritores.Add(CriarEscritorIntervalo(fimAnterior, new string(' ', inicio - fimAnterior)));
            }

            if (attr.Tipo == TipoCampo.Filler)
            {
                var conteudo = RepetirAte(attr.ConteudoFiller.Length == 0 ? " " : attr.ConteudoFiller, attr.Tamanho);
                escritores.Add(CriarEscritorIntervalo(inicio, conteudo));
            }
            else
            {
                ValidarTipoPropriedade(prop, attr);
                escritores.Add(CriarEscritor(prop, attr));
                leitores.Add(CriarLeitor(prop, attr));
            }

            fimAnterior = fim;
        }

        if (fimAnterior < registroAttr.Tamanho)
        {
            escritores.Add(CriarEscritorIntervalo(
                fimAnterior,
                new string(' ', registroAttr.Tamanho - fimAnterior)));
        }

        return new LayoutCompilado(registroAttr.Tamanho, [.. escritores], [.. leitores]);
    }

    /// <summary>Escreve todos os campos de um registro no buffer destino.</summary>
    /// <param name="registro">Instância de origem.</param>
    /// <param name="linha">Buffer de destino, já preenchido com brancos.</param>
    internal void Escrever(object registro, char[] linha)
    {
        foreach (var escritor in _escritores)
        {
            escritor(registro, linha);
        }
    }

    /// <summary>Lê todos os campos de uma linha para o registro.</summary>
    /// <param name="registro">Instância de destino.</param>
    /// <param name="linha">Linha do arquivo, com o tamanho exato do registro.</param>
    /// <param name="numeroLinha">Número 1-based da linha, para erros.</param>
    internal void Ler(object registro, string linha, int? numeroLinha)
    {
        foreach (var leitor in _leitores)
        {
            leitor(registro, linha, numeroLinha);
        }
    }

    private static string RepetirAte(string texto, int tamanho)
    {
        if (texto.Length == tamanho)
        {
            return texto;
        }

        var resultado = new StringBuilder(tamanho);
        while (resultado.Length < tamanho)
        {
            var faltando = tamanho - resultado.Length;
            resultado.Append(texto.Length <= faltando ? texto : texto[..faltando]);
        }

        return resultado.ToString();
    }

    private static void ValidarTipoPropriedade(PropertyInfo prop, CampoCnabAttribute attr)
    {
        var tipo = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
        var ok = attr.Tipo switch
        {
            TipoCampo.Alfanumerico => tipo == typeof(string),
            TipoCampo.Numerico => tipo == typeof(int) || tipo == typeof(long),
            TipoCampo.NumericoDecimal => tipo == typeof(decimal),
            TipoCampo.DataDdMmAa or TipoCampo.DataDdMmAaaa => tipo == typeof(DateOnly),
            _ => true,
        };
        if (!ok)
        {
            throw new InvalidOperationException(
                $"Propriedade '{prop.Name}' (campo '{attr.Nome}') tem tipo {prop.PropertyType.Name} incompatível com {attr.Tipo}.");
        }
    }

    // ---------------------------------------------------------------- escrita

    private static AcaoEscreverCampo CriarEscritor(PropertyInfo prop, CampoCnabAttribute attr)
    {
        var registroParam = Expression.Parameter(typeof(object), "registro");
        var linhaParam = Expression.Parameter(typeof(char[]), "linha");
        var registroConvertido = Expression.Convert(registroParam, prop.DeclaringType!);
        Expression valor = Expression.Property(registroConvertido, prop);
        var inicio = Expression.Constant(attr.Posicao - 1);
        var tamanho = Expression.Constant(attr.Tamanho);
        var nome = Expression.Constant(attr.Nome);

        MethodInfo metodo = attr.Tipo switch
        {
            TipoCampo.Alfanumerico => EscreverHelpers.MetodoTexto,
            TipoCampo.Numerico => EscreverHelpers.MetodoInteiro(prop.PropertyType),
            TipoCampo.NumericoDecimal => EscreverHelpers.MetodoDecimal,
            TipoCampo.DataDdMmAa => EscreverHelpers.MetodoData6,
            TipoCampo.DataDdMmAaaa => EscreverHelpers.MetodoData8,
            _ => throw new InvalidOperationException($"Tipo de campo não suportado na escrita: {attr.Tipo}."),
        };

        // Os helpers recebem anuláveis; propriedades não anuláveis são
        // convertidas explicitamente (árvores de expressão não fazem lifting).
        var tipoParametro = metodo.GetParameters()[0].ParameterType;
        if (valor.Type != tipoParametro)
        {
            valor = Expression.Convert(valor, tipoParametro);
        }

        Expression[] argumentos = attr.Tipo switch
        {
            TipoCampo.Alfanumerico or TipoCampo.Numerico =>
            [
                valor, linhaParam, inicio, tamanho, nome,
            ],
            TipoCampo.NumericoDecimal =>
            [
                valor, linhaParam, inicio, tamanho, Expression.Constant(attr.Decimais), nome,
            ],
            TipoCampo.DataDdMmAa or TipoCampo.DataDdMmAaaa =>
            [
                valor, linhaParam, inicio,
            ],
            _ => throw new InvalidOperationException($"Tipo de campo não suportado na escrita: {attr.Tipo}."),
        };

        var corpo = Expression.Call(metodo, [.. argumentos]);
        return Expression.Lambda<AcaoEscreverCampo>(corpo, registroParam, linhaParam).Compile();
    }

    private static AcaoEscreverCampo CriarEscritorIntervalo(int inicio, string conteudo) =>
        (_, linha) => conteudo.CopyTo(0, linha, inicio, conteudo.Length);

    /// <summary>Helpers de escrita chamados pelos delegates compilados.</summary>
    private static class EscreverHelpers
    {
        private static readonly MethodInfo TextoInfo = typeof(EscreverHelpers).GetMethod(
            nameof(EscreverTexto), BindingFlags.NonPublic | BindingFlags.Static)!;
        private static readonly MethodInfo InteiroInfo = typeof(EscreverHelpers).GetMethod(
            nameof(EscreverInteiro), BindingFlags.NonPublic | BindingFlags.Static)!;
        private static readonly MethodInfo LongInfo = typeof(EscreverHelpers).GetMethod(
            nameof(EscreverLong), BindingFlags.NonPublic | BindingFlags.Static)!;
        private static readonly MethodInfo DecimalInfo = typeof(EscreverHelpers).GetMethod(
            nameof(EscreverDecimal), BindingFlags.NonPublic | BindingFlags.Static)!;
        private static readonly MethodInfo Data6Info = typeof(EscreverHelpers).GetMethod(
            nameof(EscreverData6), BindingFlags.NonPublic | BindingFlags.Static)!;
        private static readonly MethodInfo Data8Info = typeof(EscreverHelpers).GetMethod(
            nameof(EscreverData8), BindingFlags.NonPublic | BindingFlags.Static)!;

        public static MethodInfo MetodoTexto => TextoInfo;
        public static MethodInfo MetodoInteiro(Type tipoPropriedade)
        {
            // Nullable.GetUnderlyingType devolve null para tipos NÃO anuláveis:
            // sem o ?? abaixo, um int comum cairia na via long e o erro de
            // faixa (CampoCnabException) nunca seria usado.
            var tipoBase = Nullable.GetUnderlyingType(tipoPropriedade) ?? tipoPropriedade;
            return tipoBase == typeof(int) ? InteiroInfo : LongInfo;
        }
        public static MethodInfo MetodoDecimal => DecimalInfo;
        public static MethodInfo MetodoData6 => Data6Info;
        public static MethodInfo MetodoData8 => Data8Info;

        private static void EscreverTexto(
            string? valor,
            char[] linha,
            int inicio,
            int tamanho,
            string nome)
        {
            var normalizado = CharsetCnab.Normalizar(valor);
            if (normalizado.Length > tamanho)
            {
                throw new CampoCnabException(
                    $"Conteúdo com {normalizado.Length} caracteres excede o campo de {tamanho} posição(ões)",
                    nome,
                    null,
                    inicio + 1,
                    tamanho);
            }

            normalizado.CopyTo(0, linha, inicio, normalizado.Length);
        }

        private static void EscreverInteiro(
            int? valor,
            char[] linha,
            int inicio,
            int tamanho,
            string nome)
        {
            if (valor is null)
            {
                Zerar(linha, inicio, tamanho);
                return;
            }

            if (valor.Value < 0)
            {
                throw new CampoCnabException(
                    "Campos numéricos CNAB não admitem sinal; informe um valor não negativo",
                    nome,
                    null,
                    inicio + 1,
                    tamanho);
            }

            EscreverDigitos(valor.Value.ToString(CultureInfo.InvariantCulture), linha, inicio, tamanho, nome);
        }

        private static void EscreverLong(
            long? valor,
            char[] linha,
            int inicio,
            int tamanho,
            string nome)
        {
            if (valor is null)
            {
                Zerar(linha, inicio, tamanho);
                return;
            }

            if (valor.Value < 0)
            {
                throw new CampoCnabException(
                    "Campos numéricos CNAB não admitem sinal; informe um valor não negativo",
                    nome,
                    null,
                    inicio + 1,
                    tamanho);
            }

            EscreverDigitos(valor.Value.ToString(CultureInfo.InvariantCulture), linha, inicio, tamanho, nome);
        }

        private static void EscreverDecimal(
            decimal? valor,
            char[] linha,
            int inicio,
            int tamanho,
            int decimais,
            string nome)
        {
            if (valor is null)
            {
                Zerar(linha, inicio, tamanho);
                return;
            }

            var arredondado = decimal.Round(valor.Value, decimais, MidpointRounding.ToEven);
            if (arredondado != valor.Value)
            {
                throw new CampoCnabException(
                    $"Valor {valor.Value.ToString("0.############################", CultureInfo.InvariantCulture)} "
                    + $"tem mais de {decimais} casa(s) decimal(is); o campo aceita {decimais}",
                    nome,
                    null,
                    inicio + 1,
                    tamanho);
            }

            if (arredondado < 0)
            {
                throw new CampoCnabException(
                    "Campos numéricos CNAB não admitem sinal; informe um valor não negativo",
                    nome,
                    null,
                    inicio + 1,
                    tamanho);
            }

            var escala = Pow10(decimais);
            var escalado = decimal.Truncate(arredondado * escala);
            var digitos = escalado.ToString("0", CultureInfo.InvariantCulture);
            EscreverDigitos(digitos, linha, inicio, tamanho, nome);
        }

        private static void EscreverData6(DateOnly? valor, char[] linha, int inicio)
        {
            if (valor is null)
            {
                Zerar(linha, inicio, 6);
                return;
            }

            valor.Value.ToString("ddMMyy", CultureInfo.InvariantCulture).CopyTo(0, linha, inicio, 6);
        }

        private static void EscreverData8(DateOnly? valor, char[] linha, int inicio)
        {
            if (valor is null)
            {
                Zerar(linha, inicio, 8);
                return;
            }

            valor.Value.ToString("ddMMyyyy", CultureInfo.InvariantCulture).CopyTo(0, linha, inicio, 8);
        }

        private static void EscreverDigitos(
            string digitos,
            char[] linha,
            int inicio,
            int tamanho,
            string nome)
        {
            if (digitos.Length > tamanho)
            {
                throw new CampoCnabException(
                    $"Valor com {digitos.Length} dígito(s) não cabe no campo de {tamanho} posição(ões)",
                    nome,
                    null,
                    inicio + 1,
                    tamanho);
            }

            digitos.CopyTo(0, linha, inicio + tamanho - digitos.Length, digitos.Length);
            for (var i = inicio; i < inicio + tamanho - digitos.Length; i++)
            {
                linha[i] = '0';
            }
        }

        private static decimal Pow10(int expoente)
        {
            var resultado = decimal.One;
            for (var i = 0; i < expoente; i++)
            {
                resultado *= 10m;
            }

            return resultado;
        }

        private static void Zerar(char[] linha, int inicio, int tamanho)
        {
            for (var i = inicio; i < inicio + tamanho; i++)
            {
                linha[i] = '0';
            }
        }
    }

    // ----------------------------------------------------------------- leitura

    private static AcaoLerCampo CriarLeitor(PropertyInfo prop, CampoCnabAttribute attr)
    {
        var registroParam = Expression.Parameter(typeof(object), "registro");
        var linhaParam = Expression.Parameter(typeof(string), "linha");
        var numeroLinhaParam = Expression.Parameter(typeof(int?), "numeroLinha");
        var registroConvertido = Expression.Convert(registroParam, prop.DeclaringType!);
        var inicio = Expression.Constant(attr.Posicao - 1);
        var tamanho = Expression.Constant(attr.Tamanho);
        var nome = Expression.Constant(attr.Nome);
        var anulavel = Nullable.GetUnderlyingType(prop.PropertyType) is not null;

        MethodInfo metodo = attr.Tipo switch
        {
            TipoCampo.Alfanumerico => LerHelpers.MetodoTexto,
            TipoCampo.Numerico => LerHelpers.MetodoInteiro(prop.PropertyType, anulavel),
            TipoCampo.NumericoDecimal => LerHelpers.MetodoDecimal(anulavel),
            TipoCampo.DataDdMmAa => LerHelpers.MetodoData6(anulavel),
            TipoCampo.DataDdMmAaaa => LerHelpers.MetodoData8(anulavel),
            _ => throw new InvalidOperationException($"Tipo de campo não suportado na leitura: {attr.Tipo}."),
        };

        Expression[] argumentos = attr.Tipo switch
        {
            TipoCampo.Alfanumerico =>
            [
                linhaParam, inicio, tamanho,
            ],
            TipoCampo.Numerico =>
            [
                linhaParam, inicio, tamanho, nome, numeroLinhaParam,
            ],
            TipoCampo.NumericoDecimal =>
            [
                linhaParam, inicio, tamanho, Expression.Constant(attr.Decimais), nome, numeroLinhaParam,
            ],
            TipoCampo.DataDdMmAa or TipoCampo.DataDdMmAaaa =>
            [
                linhaParam, inicio, nome, numeroLinhaParam,
            ],
            _ => throw new InvalidOperationException($"Tipo de campo não suportado na leitura: {attr.Tipo}."),
        };

        var valorLido = Expression.Call(metodo, [.. argumentos]);
        var valorAtribuido = Expression.Convert(valorLido, prop.PropertyType);
        var corpo = Expression.Assign(Expression.Property(registroConvertido, prop), valorAtribuido);
        return Expression.Lambda<AcaoLerCampo>(corpo, registroParam, linhaParam, numeroLinhaParam).Compile();
    }

    /// <summary>
    /// Helpers de leitura chamados pelos delegates compilados. Em campos
    /// anuláveis, conteúdo todo em zeros/brancos lê como <see langword="null"/>
    /// (convenção dos manuais: "campos não utilizados: preencher com zeros",
    /// por exemplo descontos e datas opcionais). Em campos obrigatórios, o
    /// mesmo conteúdo lê como zero; datas obrigatórias zeradas geram erro
    /// preciso.
    /// </summary>
    private static class LerHelpers
    {
        private static readonly MethodInfo TextoInfo = typeof(LerHelpers).GetMethod(
            nameof(LerTexto), BindingFlags.NonPublic | BindingFlags.Static)!;
        private static readonly MethodInfo InteiroAnulavelInfo = typeof(LerHelpers).GetMethod(
            nameof(LerInteiroAnulavel), BindingFlags.NonPublic | BindingFlags.Static)!;
        private static readonly MethodInfo InteiroObrigatorioInfo = typeof(LerHelpers).GetMethod(
            nameof(LerInteiroObrigatorio), BindingFlags.NonPublic | BindingFlags.Static)!;
        private static readonly MethodInfo LongAnulavelInfo = typeof(LerHelpers).GetMethod(
            nameof(LerLongAnulavel), BindingFlags.NonPublic | BindingFlags.Static)!;
        private static readonly MethodInfo LongObrigatorioInfo = typeof(LerHelpers).GetMethod(
            nameof(LerLongObrigatorio), BindingFlags.NonPublic | BindingFlags.Static)!;
        private static readonly MethodInfo DecimalAnulavelInfo = typeof(LerHelpers).GetMethod(
            nameof(LerDecimalAnulavel), BindingFlags.NonPublic | BindingFlags.Static)!;
        private static readonly MethodInfo DecimalObrigatorioInfo = typeof(LerHelpers).GetMethod(
            nameof(LerDecimalObrigatorio), BindingFlags.NonPublic | BindingFlags.Static)!;
        private static readonly MethodInfo Data6AnulavelInfo = typeof(LerHelpers).GetMethod(
            nameof(LerData6Anulavel), BindingFlags.NonPublic | BindingFlags.Static)!;
        private static readonly MethodInfo Data6ObrigatoriaInfo = typeof(LerHelpers).GetMethod(
            nameof(LerData6Obrigatoria), BindingFlags.NonPublic | BindingFlags.Static)!;
        private static readonly MethodInfo Data8AnulavelInfo = typeof(LerHelpers).GetMethod(
            nameof(LerData8Anulavel), BindingFlags.NonPublic | BindingFlags.Static)!;
        private static readonly MethodInfo Data8ObrigatoriaInfo = typeof(LerHelpers).GetMethod(
            nameof(LerData8Obrigatoria), BindingFlags.NonPublic | BindingFlags.Static)!;

        public static MethodInfo MetodoTexto => TextoInfo;

        public static MethodInfo MetodoInteiro(Type tipoPropriedade, bool anulavel)
        {
            var tipoBase = Nullable.GetUnderlyingType(tipoPropriedade) ?? tipoPropriedade;
            var ehInt = tipoBase == typeof(int);
            return (ehInt, anulavel) switch
            {
                (true, true) => InteiroAnulavelInfo,
                (true, false) => InteiroObrigatorioInfo,
                (false, true) => LongAnulavelInfo,
                (false, false) => LongObrigatorioInfo,
            };
        }

        public static MethodInfo MetodoDecimal(bool anulavel) =>
            anulavel ? DecimalAnulavelInfo : DecimalObrigatorioInfo;

        public static MethodInfo MetodoData6(bool anulavel) =>
            anulavel ? Data6AnulavelInfo : Data6ObrigatoriaInfo;

        public static MethodInfo MetodoData8(bool anulavel) =>
            anulavel ? Data8AnulavelInfo : Data8ObrigatoriaInfo;

        private static string LerTexto(string linha, int inicio, int tamanho) =>
            linha.Substring(inicio, tamanho).TrimEnd(' ');

        private static int? LerInteiroAnulavel(
            string linha,
            int inicio,
            int tamanho,
            string nome,
            int? numeroLinha)
        {
            if (TodoZeroOuBranco(linha, inicio, tamanho))
            {
                return null;
            }

            return LerInteiroObrigatorio(linha, inicio, tamanho, nome, numeroLinha);
        }

        private static int? LerInteiroObrigatorio(
            string linha,
            int inicio,
            int tamanho,
            string nome,
            int? numeroLinha)
        {
            var valor = ConverterDigitos(linha, inicio, tamanho, nome, numeroLinha);
            if (valor < int.MinValue || valor > int.MaxValue)
            {
                throw new CampoCnabException(
                    $"Valor {valor} fora da faixa do tipo int",
                    nome,
                    numeroLinha,
                    inicio + 1,
                    tamanho);
            }

            return (int)valor;
        }

        private static long? LerLongAnulavel(
            string linha,
            int inicio,
            int tamanho,
            string nome,
            int? numeroLinha)
        {
            if (TodoZeroOuBranco(linha, inicio, tamanho))
            {
                return null;
            }

            return LerLongObrigatorio(linha, inicio, tamanho, nome, numeroLinha);
        }

        private static long? LerLongObrigatorio(
            string linha,
            int inicio,
            int tamanho,
            string nome,
            int? numeroLinha) =>
            ConverterDigitos(linha, inicio, tamanho, nome, numeroLinha);

        private static decimal? LerDecimalAnulavel(
            string linha,
            int inicio,
            int tamanho,
            int decimais,
            string nome,
            int? numeroLinha)
        {
            if (TodoZeroOuBranco(linha, inicio, tamanho))
            {
                return null;
            }

            return LerDecimalObrigatorio(linha, inicio, tamanho, decimais, nome, numeroLinha);
        }

        private static decimal? LerDecimalObrigatorio(
            string linha,
            int inicio,
            int tamanho,
            int decimais,
            string nome,
            int? numeroLinha)
        {
            var escalado = ConverterDigitos(linha, inicio, tamanho, nome, numeroLinha);
            var escala = decimal.One;
            for (var i = 0; i < decimais; i++)
            {
                escala /= 10m;
            }

            return escalado * escala;
        }

        private static DateOnly? LerData6Anulavel(string linha, int inicio, string nome, int? numeroLinha)
        {
            if (TodoZeroOuBranco(linha, inicio, 6))
            {
                return null;
            }

            return LerData6Obrigatoria(linha, inicio, nome, numeroLinha);
        }

        private static DateOnly? LerData6Obrigatoria(string linha, int inicio, string nome, int? numeroLinha)
        {
            var dia = LerDoisDigitos(linha, inicio, nome, numeroLinha);
            var mes = LerDoisDigitos(linha, inicio + 2, nome, numeroLinha);
            var ano = LerDoisDigitos(linha, inicio + 4, nome, numeroLinha) + 2000;
            return MontarData(dia, mes, ano, inicio, 6, nome, numeroLinha);
        }

        private static DateOnly? LerData8Anulavel(string linha, int inicio, string nome, int? numeroLinha)
        {
            if (TodoZeroOuBranco(linha, inicio, 8))
            {
                return null;
            }

            return LerData8Obrigatoria(linha, inicio, nome, numeroLinha);
        }

        private static DateOnly? LerData8Obrigatoria(string linha, int inicio, string nome, int? numeroLinha)
        {
            var dia = LerDoisDigitos(linha, inicio, nome, numeroLinha);
            var mes = LerDoisDigitos(linha, inicio + 2, nome, numeroLinha);
            var ano = (int)ConverterDigitos(linha, inicio + 4, 4, nome, numeroLinha);
            return MontarData(dia, mes, ano, inicio, 8, nome, numeroLinha);
        }

        private static DateOnly? MontarData(
            int dia,
            int mes,
            int ano,
            int inicio,
            int tamanho,
            string nome,
            int? numeroLinha)
        {
            if (!DateOnly.TryParse(
                    string.Create(CultureInfo.InvariantCulture, $"{ano:0000}-{mes:00}-{dia:00}"),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var data))
            {
                throw new CampoCnabException(
                    "Data inválida (dia, mês e/ou ano inexistente)",
                    nome,
                    numeroLinha,
                    inicio + 1,
                    tamanho);
            }

            return data;
        }

        private static long ConverterDigitos(
            string linha,
            int inicio,
            int tamanho,
            string nome,
            int? numeroLinha)
        {
            // Um campo numérico inteiramente em branco/zeros é "não utilizado"
            // e lê como zero (ou null nos anuláveis). Espaços MISTURADOS com
            // dígitos, porém, indicam alinhamento inválido e são erro: o
            // padrão FEBRABAN preenche numéricos com zeros à esquerda.
            var fieldTodoZerosOuBrancos = TodoZeroOuBranco(linha, inicio, tamanho);
            long valor = 0;
            for (var i = inicio; i < inicio + tamanho; i++)
            {
                var c = linha[i];
                if (c == ' ')
                {
                    if (fieldTodoZerosOuBrancos)
                    {
                        continue;
                    }

                    throw new CampoCnabException(
                        "Espaço em campo numérico preenchido (use zeros à esquerda)",
                        nome,
                        numeroLinha,
                        inicio + 1,
                        tamanho);
                }

                if (c is < '0' or > '9')
                {
                    throw new CampoCnabException(
                        $"Caractere '{c}' inválido em campo numérico",
                        nome,
                        numeroLinha,
                        inicio + 1,
                        tamanho);
                }

                valor = (valor * 10) + (c - '0');
            }

            return valor;
        }

        private static int LerDoisDigitos(string linha, int inicio, string nome, int? numeroLinha) =>
            (int)ConverterDigitos(linha, inicio, 2, nome, numeroLinha);

        private static bool TodoZeroOuBranco(string linha, int inicio, int tamanho)
        {
            for (var i = inicio; i < inicio + tamanho; i++)
            {
                if (linha[i] is not ('0' or ' '))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
