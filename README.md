# Cnab.NET

**Cnab.NET** é uma biblioteca .NET e a ferramenta `cnab` para gerar arquivos
CNAB de remessa e ler retornos (CNAB 240 e CNAB 400) de cobrança e pagamentos
dos principais bancos brasileiros:

| Banco | Código | CNAB 240 | CNAB 400 |
| --- | --- | --- | --- |
| Banco do Brasil | 001 | sim | — |
| Santander | 033 | sim | — |
| Caixa Econômica Federal | 104 | sim | — |
| Bradesco | 237 | sim | sim |
| Itaú | 341 | sim | sim |
| Sicredi | 748 | sim | sim |
| Sicoob | 756 | sim | — |
| Banco Inter | 077 | pagamentos | sim |
| BTG Pactual | 208 | sim | — |

Não é um protótipo: sem stubs, sem `NotImplementedException` e sem testes
desativados. Toda posição de layout e todo algoritmo vêm de **manuais oficiais
citados no código e em [docs/layouts.md](docs/layouts.md)** — nada é inventado.

## Requisitos e compilação

- SDK .NET **10** para desenvolvimento; o CI valida **.NET 8 e 10** em
  Ubuntu, Windows e macOS.
- A biblioteca e a CLI multi-alvejam `net8.0` e `net10.0`. `netstandard2.0`
  foi avaliado e descartado porque a API usa `DateOnly` (o SPEC permite
  descartá-lo quando isso preserva a superfície pública).

```bash
dotnet restore Cnab.slnx
dotnet build Cnab.slnx -c Release                 # warnings são erros
dotnet test tests/Cnab.Tests -c Release           # xUnit + FsCheck + Verify
dotnet format Cnab.slnx --verify-no-changes       # lint
```

## Biblioteca: gerar uma remessa CNAB 400

Exemplo completo (compila com `Cnab` referenciado):

```csharp
using Cnab.Layouts.Cnab400;
using Cnab.Modelo;

var banco = Bancos.Bradesco;
var conta = new ContaBancaria
{
    Banco = banco,
    Agencia = "0057",
    Conta = "12345",
    DvConta = "7",
};
var cedente = new Cedente
{
    Nome = "EMPRESA TESTE LTDA",
    Documento = new DocumentoFiscal(DocumentoFiscal.CnpjExemplo),
    Conta = conta,
};
var titulo = new Titulo
{
    NossoNumero = "98712345",
    NumeroDocumento = "NF000001",
    Especie = EspecieDocumento.DuplicataMercantilIndicacao,
    DataEmissao = new DateOnly(2026, 11, 20),
    DataVencimento = new DateOnly(2026, 12, 20),
    Valor = 1500.50m,
    Carteira = "19",
    Pagador = new Pagador
    {
        Nome = "PAGADOR TESTE LTDA",
        Documento = new DocumentoFiscal(DocumentoFiscal.CpfExemplo),
    },
};
var remessa = new Remessa400
{
    Banco = banco,
    Cedente = cedente,
    NumeroSequencialArquivo = 1,
    DataGeracao = new DateOnly(2026, 11, 20),
    CodigoBeneficiario = "00623",
    Titulos = [titulo],
};
var conteudo = GeradorRemessa400.Gerar(remessa); // CRLF + Latin-1 por padrão
File.WriteAllBytes("remessa.rem", System.Text.Encoding.Latin1.GetBytes(conteudo));
```

Para CNAB 240, use `Remessa240`, `LoteCobranca` e
`GeradorRemessa240.Gerar`. Os trailers (quantidades e somatórias) são
calculados automaticamente.

## Biblioteca: ler um retorno

```csharp
using Cnab.Layouts.Cnab240;
using Cnab.Layouts.Cnab400;

var retorno400 = LeitorRetorno400.Ler(File.ReadAllText("retorno.ret"));
foreach (var item in retorno400.Itens)
{
    Console.WriteLine($"{item.NossoNumero}: {item.Ocorrencia?.Descricao} — {item.Valores?.ValorPago}");
}

var retorno240 = LeitorRetorno240.Ler(File.ReadAllText("retorno240.ret"));
foreach (var lote in retorno240.Lotes)
{
    foreach (var item in lote.Itens)
    {
        Console.WriteLine($"{item.Titulo?.NossoNumero}: {item.Ocorrencia?.Codigo}");
    }
}
```

As linhas originais são preservadas, garantindo round-trip byte-exato
(`EscreverLinhas(Ler(conteudo))` reproduz o arquivo).

## CLI `cnab`

A ferramenta é um dotnet tool local (`Cnab.Cli`, comando `cnab`). Como o
projeto é multi-alvo, ao usar `dotnet run` informe o framework:

```bash
# Validar line lengths, tipos de registro, campos e totais de trailer:
dotnet run --project src/Cnab.Cli --framework net10.0 -- \
  validate tests/Cnab.Tests/Golden/341_cobranca_400.rem

# Parsear retorno em JSON (ou texto legível, sem --json):
dotnet run --project src/Cnab.Cli --framework net10.0 -- \
  parse tests/Cnab.Tests/Golden/341_cobranca_400.ret --json

# Inspecionar campo a campo, com posições e valores:
dotnet run --project src/Cnab.Cli --framework net10.0 -- \
  inspect tests/Cnab.Tests/Golden/341_cobranca_400.rem --bank 341

# Gerar remessa a partir do JSON de exemplo:
dotnet run --project src/Cnab.Cli --framework net10.0 -- \
  generate examples/titulos.json --bank 237 --layout 400 --output remessa.rem
```

Para instalar como ferramenta local (sem publicar nada):

```bash
dotnet pack src/Cnab.Cli -c Release -o ./.tools
dotnet tool install --tool-path ./.local --add-source ./.tools Cnab.Cli
./.local/cnab --help
```

`generate` aceita `--layout 240|400`, exige `--bank`, grava em Latin-1 com
CRLF (ou `--lf`) e escreve no stdout quando `--output` não é informado. O
esquema do JSON está em [examples/titulos.json](examples/titulos.json):
`dataGeracao`, `cedente` (nome, documento, agência, conta e DVs) e `titulos`
(nosso número, documento, espécie, datas, valor, carteira e pagador com
endereço). Campos ausentes ou inválidos produzem erro com código de saída 1;
argumentos inválidos, código 2.

## Testes

- **Vetores oficiais** — exemplos resolvidos dos manuais (DV por banco,
  código de barras, linha digitável e fator de vencimento).
- **Golden files** — remessas e retornos byte-exatos por banco/layout, com
  CRLF preservado (`.gitattributes`).
- **Propriedades (FsCheck)** — em **todos** os layouts implementados: linhas
  com comprimento exato (240/400), `parse(write(x)) == x` para retornos e
  totais de trailer iguais às somas dos modelos.
- **CLI** — validação com vetores válidos e inválidos, JSON de retorno,
  inspeção e geração com saída byte-exata.
- **Desempenho** — um teste analisa **1.000.000 de linhas CNAB 240** com
  limite generoso de 60 s (geralmente < 1 s).
- **Hardening (marco 5)** — BOM UTF-8, CR solto, caminhos vazios/com acentos,
  `--output` inválido, banco não suportado, hora inválida, numérico com espaço,
  trailer ausente e 1.000 títulos; amostras de retorno cobrem todas as famílias
  de ocorrência. O checklist executado está em
  [docs/SPEC-CHECKLIST.md](docs/SPEC-CHECKLIST.md).

## Benchmarks

```bash
dotnet run --project benchmarks/Cnab.Benchmarks -c Release
```

Inclui `MotorDeRegistros` (1.000 e 100.000 linhas) e `ParseUmMilhao`, que
mede diretamente o parse de 1.000.000 de registros de 240 posições.

## Licença

MIT — [LICENSE](LICENSE). O projeto não publica pacotes em NuGet.
