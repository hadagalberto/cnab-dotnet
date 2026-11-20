# Cnab.NET

**Cnab.NET** is a .NET library and the `cnab` tool to generate CNAB remessa
files and parse retorno files (CNAB 240 and CNAB 400) for Brazilian boleto
collection and supplier payments:

| Bank | Code | CNAB 240 | CNAB 400 |
| --- | --- | --- | --- |
| Banco do Brasil | 001 | yes | — |
| Santander | 033 | yes | — |
| Caixa Econômica Federal | 104 | yes | — |
| Bradesco | 237 | yes | yes |
| Itaú | 341 | yes | yes |
| Sicredi | 748 | yes | yes |
| Sicoob | 756 | yes | — |
| Banco Inter | 077 | payments | yes |
| BTG Pactual | 208 | yes | — |

This is not a toy project: no stubs, no `NotImplementedException`, no skipped
tests. Every layout position and algorithm comes from an **official manual
cited in code and in [docs/layouts.md](docs/layouts.md)** — nothing is invented.

## Requirements and build

- .NET SDK **10** for development; CI validates **.NET 8 and 10** on Ubuntu,
  Windows and macOS.
- The library and CLI target `net8.0` and `net10.0`. `netstandard2.0` was
  evaluated and dropped because the public API uses `DateOnly` (the SPEC
  allows dropping it to keep a better API surface).

```bash
dotnet restore Cnab.slnx
dotnet build Cnab.slnx -c Release                 # warnings are errors
dotnet test tests/Cnab.Tests -c Release           # xUnit + FsCheck + Verify
dotnet format Cnab.slnx --verify-no-changes       # lint
```

## Library: generate a CNAB 400 remessa

Complete example (compiles with a reference to `Cnab`):

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
var conteudo = GeradorRemessa400.Gerar(remessa); // CRLF + Latin-1 by default
File.WriteAllBytes("remessa.rem", System.Text.Encoding.Latin1.GetBytes(conteudo));
```

For CNAB 240, use `Remessa240`, `LoteCobranca` and `GeradorRemessa240.Gerar`.
Trailer counts and totals are computed automatically.

## Library: parse a retorno

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

Original lines are preserved, so `EscreverLinhas(Ler(conteudo))` reproduces
the file byte for byte.

## `cnab` CLI

The tool is a local dotnet tool (`Cnab.Cli`, command name `cnab`). Because it
multi-targets, pass the framework when using `dotnet run`:

```bash
# Validate line lengths, record types, fields and trailer totals:
dotnet run --project src/Cnab.Cli --framework net10.0 -- \
  validate tests/Cnab.Tests/Golden/341_cobranca_400.rem

# Parse a retorno as JSON (or a readable report without --json):
dotnet run --project src/Cnab.Cli --framework net10.0 -- \
  parse tests/Cnab.Tests/Golden/341_cobranca_400.ret --json

# Inspect every field with positions and values:
dotnet run --project src/Cnab.Cli --framework net10.0 -- \
  inspect tests/Cnab.Tests/Golden/341_cobranca_400.rem --bank 341

# Generate a remessa from the sample JSON:
dotnet run --project src/Cnab.Cli --framework net10.0 -- \
  generate examples/titulos.json --bank 237 --layout 400 --output remessa.rem
```

Install it as a local tool (nothing is published):

```bash
dotnet pack src/Cnab.Cli -c Release -o ./.tools
dotnet tool install --tool-path ./.local --add-source ./.tools Cnab.Cli
./.local/cnab --help
```

`generate` accepts `--layout 240|400`, requires `--bank`, writes Latin-1 with
CRLF (or `--lf`) and sends bytes to stdout when `--output` is omitted. The
JSON schema is shown in [examples/titulos.json](examples/titulos.json):
`dataGeracao`, `cedente` (name, document, agency, account and DVs) and
`titulos` (nosso número, document, species, dates, amount, wallet and pagador
with address). Missing or invalid fields exit with code 1; invalid arguments
exit with code 2.

## Tests

- **Official vectors** — solved manual examples (per-bank check digits,
  barcode, digitable line and due-date factor).
- **Golden files** — byte-exact remessas and retornos per bank/layout, with
  CRLF preserved by `.gitattributes`.
- **Properties (FsCheck)** — across **all** implemented layouts: exact line
  lengths (240/400), `parse(write(x)) == x` for retornos, and trailer totals
  equal to the sums of the domain models.
- **CLI** — valid and invalid vectors for validate, JSON parsing, inspect and
  byte-exact generation.
- **Performance** — a test parses **1,000,000 CNAB 240 lines** with a generous
  60-second bound (normally under 1 second).
- **Hardening (milestone 5)** — UTF-8 BOM, lone CR, empty/accented paths,
  invalid `--output`, unsupported bank, invalid time, spaces inside numeric
  fields, missing trailer and 1,000 titles; retorno samples cover every
  occurrence family. The executed checklist is at
  [docs/SPEC-CHECKLIST.md](docs/SPEC-CHECKLIST.md).

## Benchmarks

```bash
dotnet run --project benchmarks/Cnab.Benchmarks -c Release
```

Includes `MotorDeRegistros` (1,000 and 100,000 lines) and `ParseUmMilhao`,
which measures parsing 1,000,000 240-position records directly.

## License

MIT — [LICENSE](LICENSE). The project does not publish NuGet packages.
