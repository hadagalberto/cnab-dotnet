# Cnab.NET

**Cnab.NET** is a .NET library (and, in upcoming milestones, a `cnab` CLI) to
generate CNAB remessa files and parse retorno files (CNAB 240 and CNAB 400)
for Brazilian boleto collection and payments, for the main banks:

| Bank | Code |
| --- | --- |
| Banco do Brasil | 001 |
| Santander | 033 |
| Caixa Econômica Federal | 104 |
| Bradesco | 237 |
| Itaú | 341 |
| Sicredi | 748 |
| Sicoob | 756 |
| Banco Inter | 077 |
| BTG Pactual | 208 |

This is not a toy project: no stubs, no `NotImplementedException`, no skipped
tests. Every layout position and every check-digit algorithm comes from an
**official manual cited in code and in
[docs/layouts.md](docs/layouts.md)** — nothing is invented.

## Current state (milestone 1)

- **Fixed-width record engine** — declarative fields by attribute (position,
  length, numeric/alpha/decimal types, `DDMMAA`/`DDMMAAAA` dates, fillers),
  **compiled** readers and writers (cached expression trees — zero per-record
  reflection), record length validation (240/400) and precise errors with
  **line, column and field name**.
- **CNAB charset** — uppercasing, accent stripping and replacement of
  unsupported characters, per the manuals (details in
  [docs/layouts.md](docs/layouts.md)).
- **Domain model** — cedente/beneficiário, bank account, título (nosso número,
  due date, amount, interest, fine, discount, protest, pagador, sacador
  avalista, instructions), pagamento (TED/DOC/Pix key/Pix QR/boleto/tributo),
  batches and trailer totals computed automatically.
- **Boleto** — fator de vencimento (including the **22/02/2025 reset rule**),
  código de barras (44) and linha digitável (47) with per-bank check digits,
  validated against official manual vectors and real API samples.
- **Per-bank algorithms** — nosso número and agency/account check digits per
  each bank's manual (BB, Bradesco, Itaú, Santander, Caixa, Inter, Sicredi and
  Sicoob with solved vectors; BTG with its published campo livre and the
  bank-assigned DV — see [docs/layouts.md](docs/layouts.md)).

Upcoming milestones: per-bank CNAB 240/400 layouts, remessa generation,
retorno parsing, payment segments (A/B/J/O) and the `cnab` CLI.

## Requirements

- .NET SDK **10** for development (CI validates **8** and **10**).
- The library targets `net8.0` and `net10.0`. `netstandard2.0` was evaluated
  and dropped: the API uses `DateOnly` and the required shim would hurt the
  public surface (the SPEC allows skipping it in that case).

## Build and tests

```bash
dotnet build Cnab.slnx -c Release   # warnings are errors
dotnet test tests/Cnab.Tests       # xUnit + FsCheck + Verify
dotnet format Cnab.slnx --verify-no-changes  # lint
```

The test suite includes:

- **Official vectors** — worked examples from the manuals (per-bank nosso
  número check digits, per-bank barcode and digitable line, fator de
  vencimento tables).
- **Property tests (FsCheck)** — every written line has the exact layout
  length; `parse(write(x)) == x`; the output charset always belongs to the
  CNAB set; trailer totals always match.
- **Snapshots (Verify)** — the sample file is frozen byte for byte (CRLF
  included).

## Benchmarks

```bash
dotnet run --project benchmarks/Cnab.Benchmarks -c Release
```

Measures reading and writing of 240-position records; with the measured
numbers, **1 million lines** are parsed in about **1 s** on the development
hardware (well within the SPEC's generous bound).

## License

MIT — [LICENSE](LICENSE).
