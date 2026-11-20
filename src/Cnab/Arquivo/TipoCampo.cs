namespace Cnab.Arquivo;

/// <summary>
/// Tipos de campo suportados pelo motor de registros de largura fixa.
/// A imagem (<c>Picture</c>) segue a notação cobrança/pagamentos: <c>X(n)</c>
/// alfanumérico, <c>9(n)</c> numérico, <c>9(n)V9(2)</c> numérico com decimais
/// implícitas, datas em <c>DDMMAA</c> ou <c>DDMMAAAA</c> (FEBRABAN,
/// <c>Layout Padrão CNAB 240 V10.9</c>, seção "Alinhamento de Campos":
/// numérico alinhado à direita com zeros à esquerda; alfanumérico alinhado à
/// esquerda com brancos à direita).
/// </summary>
public enum TipoCampo
{
    /// <summary>Alfanumérico <c>X(n)</c>: caixa alta, charset CNAB, brancos à direita.</summary>
    Alfanumerico,

    /// <summary>Numérico <c>9(n)</c>: dígitos, zeros à esquerda.</summary>
    Numerico,

    /// <summary>Numérico com decimais implícitas <c>9(n)V9(d)</c>: valor escalado por 10^d.</summary>
    NumericoDecimal,

    /// <summary>Data <c>DDMMAA</c> (6 posições). Ao ler, o século assumido é o 2000.</summary>
    DataDdMmAa,

    /// <summary>Data <c>DDMMAAAA</c> (8 posições).</summary>
    DataDdMmAaaa,

    /// <summary>
    /// Filler (uso exclusivo FEBRABAN/banco): conteúdo fixo declarado no
    /// atributo; não é lido de volta. Lacunas não declaradas entre campos são
    /// preenchidas com brancos, conforme o padrão de campos alfanuméricos
    /// não utilizados (FEBRABAN V10.9).
    /// </summary>
    Filler,
}
