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
- Algoritmos por banco com vetores oficiais: BB (Anexo XI do Doc5175: módulo 11
  com pesos 9 a 2 da direita para a esquerda, resto 10 → "X", vetor
  05009401448 → 1; campos livres dos anexos VII/IX), Bradesco (módulo 11 base 7
  com dígito "P"), Itaú (DAC módulo 10 com variantes por carteira), Santander
  (módulo 11 com regra de resto 0/1/10), Caixa SIGCB (DV do nosso número, do
  código do beneficiário, do campo livre, da conta e de agência/conta), Inter
  (DV do NN = módulo 10 sobre agência+carteira+NN, vetor 0001/110/0004309540
  → 1; campo livre da seção 7.1.3), Sicredi (DV sobre
  cooperativa/posto/beneficiário), Sicoob (constante "3197") e BTG (campo livre
  publicado no portal do desenvolvedor) (`Cnab.Bancos`).
- Correção do motor: campos `int` não anuláveis agora usam o leitor com
  verificação de faixa, gerando `CampoCnabException` com linha/coluna/campo em
  vez de `OverflowException`.
- Fontes oficiais buscadas e citadas (FEBRABAN FB-009/2023 via Banpara Anexos
  02/03, manuais Doc5175/BB, CNAB400/Inter e portal BTG) em
  `docs/layouts.md`.
- Domínio: cedente, pagador, sacador avalista, conta bancária, título (juros,
  multa, desconto, protesto, instruções), pagamento (TED/DOC/Pix/boleto/
  tributo) e lotes com totais de trailer calculados (`Cnab.Modelo`).
- Registro das fontes oficiais (nome, versão, URL, regras capturadas e
  pendências) em `docs/layouts.md`.
