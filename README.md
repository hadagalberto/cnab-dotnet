# Cnab.NET

**Cnab.NET** é uma biblioteca .NET (e, em marcos futuros, uma CLI `cnab`) para
gerar arquivos CNAB de remessa e ler arquivos de retorno (CNAB 240 e CNAB 400)
de cobrança e pagamentos dos principais bancos brasileiros:

| Banco | Código |
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

O projeto não é um protótipo: sem stubs, sem `NotImplementedException`, sem
testes desativados. Toda regra de layout e todo algoritmo de dígito
verificador vêm de **manuais oficiais citados no código e em
[docs/layouts.md](docs/layouts.md)** — nada é inventado.

## Estado atual (marco 1)

- **Motor de registros de largura fixa** — campos declarativos por atributo
  (posição, tamanho, tipo numérico/alfanumérico/decimal/datas `DDMMAA` e
  `DDMMAAAA`, fillers), leitura e escrita **compiladas** (árvores de expressão
  em cache — zero reflexão por registro), validação de comprimento (240/400) e
  erros precisos com **linha, coluna e nome do campo**.
- **Charset CNAB** — caixa alta, remoção de acentuação e substituição de
  caracteres não aceitos, conforme os manuais (detalhes em
  [docs/layouts.md](docs/layouts.md)).
- **Domínio** — cedente/beneficiário, conta bancária, título (nosso número,
  vencimento, valor, juros, multa, desconto, protesto, pagador, sacador
  avalista, instruções), pagamento (TED/DOC/Pix por chave/Pix QR/boleto/
  tributo), lotes e totais de trailer calculados automaticamente.
- **Boleto** — fator de vencimento (com a regra do reinício de
  **22/02/2025**), código de barras (44) e linha digitável (47) com os DVs por
  banco, contra vetores oficiais dos manuais e amostras reais das APIs.
- **Algoritmos por banco** — DV de nosso número e de agência/conta conforme o
  manual de cada banco (BB, Bradesco, Itaú, Santander, Caixa, Inter, Sicredi e
  Sicoob com vetores resolvidos; BTG com o campo livre publicado e o DV
  atribuído pelo banco — ver
  [docs/layouts.md](docs/layouts.md)).

Marcos futuros: layouts CNAB 240/400 por banco, geração de remessa, leitura de
retorno, segmentos de pagamento (A/B/J/O) e CLI `cnab`.

## Requisitos

- SDK .NET **10** para desenvolvimento (o CI valida **8** e **10**).
- A biblioteca multi-alveja `net8.0` e `net10.0`. O `netstandard2.0` foi
  avaliado e descartado: a API usa `DateOnly`, e o shim correspondente
  prejudicaria a superfície pública (o SPEC permite descartá-lo nesse caso).

## Compilação e testes

```bash
dotnet build Cnab.slnx -c Release   # warnings são erros
dotnet test tests/Cnab.Tests       # xUnit + FsCheck + Verify
dotnet format Cnab.slnx --verify-no-changes  # lint
```

Os testes incluem:

- **Vetores oficiais** — exemplos resolvidos dos manuais (DV de nosso número
  por banco, código de barras e linha digitável por banco, tabelas do fator de
  vencimento).
- **Testes de propriedade (FsCheck)** — toda linha escrita tem o comprimento
  exato do layout; `parse(write(x)) == x`; o charset de saída sempre pertence
  ao conjunto CNAB; totais de trailer sempre batem.
- **Snapshots (Verify)** — o arquivo de exemplo é congelado byte a byte
  (CRLF incluso).

## Benchmarks

```bash
dotnet run --project benchmarks/Cnab.Benchmarks -c Release
```

Mede leitura e escrita de registros de 240 posições; com os números medidos,
**1 milhão de linhas** são lidas em cerca de **1 s** no hardware de
desenvolvimento (margem generosa do SPEC).

## Licença

MIT — [LICENSE](LICENSE).
