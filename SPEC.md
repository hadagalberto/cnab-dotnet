# Cnab.NET — SPEC

**Cnab.NET**: a .NET library + CLI to generate CNAB remessa files and parse retorno files (CNAB 240 and CNAB 400) for boleto collection
(cobrança) and payments (pagamento a fornecedores / Pix via CNAB 240 segmentos A/B/J/O) for the main Brazilian banks:
Banco do Brasil (001), Santander (033), Caixa (104), Bradesco (237), Itaú (341), Sicredi (748), Sicoob (756), Inter (077), BTG (208).
Declarative fixed-width layouts, strong typing, round-trip guarantees. Open source, MIT, "Hadagalberto Junior".
NOT a toy: no stubs, no TODO, no NotImplementedException, no skipped or placeholder tests.

## Sources of truth (fetch and cite, never guess)
- FEBRABAN CNAB 240 layout (versão 10.x, cobrança and pagamentos).
- Each bank's published CNAB 240/400 manual. Record the manual name, version and URL for every layout in `docs/layouts.md`. Where a bank deviates from FEBRABAN, encode the deviation explicitly and test it.

## Stack
.NET 8 and .NET 10 multi-target (netstandard2.0 too if feasible without hurting the API), C# latest, nullable enabled, TreatWarningsAsErrors, analyzers on.
xUnit + FluentAssertions-free assertions (use xUnit Assert or Shouldly), FsCheck property tests, Verify snapshot tests, BenchmarkDotNet project.
`.gitattributes` `* text=auto eol=lf` and explicit CRLF handling in the writer (CNAB files use CRLF; option for LF). CI GitHub Actions ubuntu/windows/macos.
Packages: `Cnab` (core), `Cnab.Cli` (dotnet tool `cnab`). README.md (pt-BR) + README.en.md, CHANGELOG, CONTRIBUTING, LICENSE. Do not publish to NuGet.

## Core
- Fixed-width record engine: declarative field definitions (position, length, type num/alfa, decimals, date formats DDMMAA/DDMMAAAA, padding rules, uppercase and accent stripping to the CNAB charset), source-generated or cached-compiled readers/writers (no reflection per record), line length validation (240/400), precise errors with line/column/field name.
- Domain model: Cedente/Beneficiário, Conta (agência, conta, DVs per bank algorithm), Título (nosso número with each bank's DV algorithm and carteira rules, vencimento, valor, juros/multa/desconto, protesto, pagador, sacador avalista, instruções), Pagamento (TED/DOC/Pix chave/Pix QR, boleto, tributos), lotes, trailers with automatic counts and sums.
- Remessa generation for cobrança (240 and 400) and pagamentos (240) per bank; sequential numbering (NSA, NSR) helpers.
- Retorno parsing: occurrence codes (liquidação, entrada confirmada, baixa, tarifa, rejeição with motivo codes) mapped to a typed enum + bank-specific description table; values (pago, tarifa, juros, desconto, abatimento, IOF), dates of crédito/ocorrência.
- Integration with boleto line: compute linha digitável / código de barras from a Título (FEBRABAN 44 positions, fator de vencimento including the 2025 reset rule), consistent with the bank's nosso número.

## CLI
`cnab validate arquivo.rem`, `cnab parse retorno.ret --json`, `cnab inspect arquivo --bank 341` (pretty field-by-field table with positions), `cnab generate titulos.json --bank 237 --layout 400`.

## Tests
- For every bank and layout: golden remessa files built from published manual examples (byte-exact), parse → write → byte-identical round trip, retorno samples covering every occurrence code family.
- Property tests: any valid model writes lines of exact length, parse(write(x)) == x, trailer totals always match.
- DV algorithms vs. known vectors from each manual.
- Benchmark: parse 1 million CNAB 240 lines under a generous bound.

## Hard constraints
- Never run `git push`, `gh`, `dotnet nuget push`, or publish anything. Commit locally after each milestone. Stay inside this repository.
