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

### Adicionado (marco 3)

- Remessa e retorno CNAB 240 para Bradesco (237), Itaú (341), Sicredi (748) e
  BTG (208), com perfis em `PerfilBanco240` e ajustes dinâmicos documentados
  em `AjustesBanco240` (DAC do Itaú, decomposição do nosso número do
  Bradesco, agência sem DV do Sicredi e valores fixos do BTG).
- Remessa de pagamentos CNAB 240 do Inter (077) com versão 107/046 e agência
  única 00001/DV 9; tabela própria de ocorrências de retorno (231-240).
- Motor CNAB 400 (`Cnab.Layouts.Cnab400`): registros declarativos de 400
  posições, gerador de remessa, leitor de retorno, modelos tipados e tabelas
  de ocorrências/motivos por banco.
- Remessa e retorno CNAB 400 para Itaú, Bradesco, Sicredi e Inter, incluindo
  valores, tarifas, datas de ocorrência/crédito e tabelas completas de
  ocorrências e motivos extraídas dos manuais oficiais.
- Round-trip byte-exato de retornos 240/400 preservando as linhas originais
  dos cabeçalhos, detalhes e trailers.
- Golden files por banco/layout em `tests/Cnab.Tests/Golden/` e testes
  `GoldenFilesMilestone3Tests` comparando byte a byte gerador/leitor.
- Registro integral das fontes do marco 3 (manuais I1/I2, B3/B4/B1', S3/S4,
  IN1/IN2, T1/T2 e F4) com versões, URLs, posições e desvios em
  `docs/layouts.md`.

### Adicionado (marco 4)

- Ferramenta `dotnet cnab` (`src/Cnab.Cli`, comando `cnab`, pacote local
  `Cnab.Cli`) com os quatro comandos do SPEC: `validate` (comprimento, tipo de
  registro, campos e totais de trailer, com linha/coluna/campo), `parse`
  (resumo ou JSON de ocorrências, motivos e valores), `inspect` (tabela
  campo a campo com posições e valores) e `generate` (remessa 240/400 a
  partir de JSON, saída Latin-1/CRLF com opção `--lf`).
- `Cnab.Arquivo.MetadadosRegistro`: metadados declarativos cacheados
  (comprimento, campos e valores) usados pela inspeção sem duplicar regras de
  layout.
- Testes da CLI (`CliTests`) com vetores válidos e inválidos para os quatro
  comandos, incluindo erros de uso (código 2) e de dados (código 1).
- Testes de propriedade FsCheck (`PropriedadesTodosLayoutsTests`) cobrindo
  **todos** os layouts: comprimento exato das linhas, `parse(write(x)) == x`
  nos retornos 240/400 e totais de trailer iguais às somas dos modelos.
- Benchmark `ParseUmMilhao` (BenchmarkDotNet) e teste
  `DesempenhoParseUmMilhaoTests`: 1.000.000 de linhas CNAB 240 com limite
  generoso de 60 s e soma de verificação.
- `examples/titulos.json` e READMEs (pt-BR/en) com exemplos executáveis da
  biblioteca e da CLI; `docs/layouts.md` com a matriz de cobertura por banco,
  as regras de leitura da CLI e o registro do marco 4; `CONTRIBUTING.md`
  atualizado.
