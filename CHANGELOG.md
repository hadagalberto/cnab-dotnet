# Changelog

Todas as mudanças notáveis deste projeto serão documentadas neste arquivo.

O formato segue [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/)
e o projeto adere ao [Versionamento Semântico](https://semver.org/lang/pt-BR/).

## [Não publicado]

### Adicionado (marco 1)

- Scaffold da solução: biblioteca `Cnab` multi-alvo (`net8.0;net10.0`),
  testes (`xUnit` + `FsCheck` + `Verify`), benchmarks (`BenchmarkDotNet`),
  CI GitHub Actions (ubuntu/windows/macos, .NET 8 + 10), `LICENSE` (MIT),
  `.gitattributes` (LF), `.editorconfig` e warnings como erros.
- Motor de registros de largura fixa com campos declarativos e
  leitores/escritores compilados (sem reflexão por registro), validação de
  comprimento 240/400 e erros com linha/coluna/campo
  (`Cnab.Arquivo`).
- Normalização de texto para o charset CNAB (`Cnab.Texto.CharsetCnab`).
- Fator de vencimento com a regra do reinício de 22/02/2025, código de
  barras (44 posições) e linha digitável (47 dígitos) com composição,
  reversão e validação de DVs (`Cnab.Boleto`).
- Algoritmos por banco com vetores oficiais: Bradesco (módulo 11 base 7 com
  dígito "P"), Itaú (DAC módulo 10 com variantes por carteira), Santander
  (módulo 11 com regra de resto 0/1/10), Caixa SIGCB (DV do nosso número, do
  código do beneficiário, do campo livre, da conta e de agência/conta),
  Sicredi (DV sobre cooperativa/posto/beneficiário) e Sicoob (constante
  "3197") (`Cnab.Bancos`).
- Domínio: cedente, pagador, sacador avalista, conta bancária, título (juros,
  multa, desconto, protesto, instruções), pagamento (TED/DOC/Pix/boleto/
  tributo) e lotes com totais de trailer calculados (`Cnab.Modelo`).
- Registro das fontes oficiais (nome, versão, URL, regras capturadas e
  pendências) em `docs/layouts.md`.
