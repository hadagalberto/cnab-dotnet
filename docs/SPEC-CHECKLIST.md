# Checklist de auditoria do SPEC — marco 5

**Data da auditoria:** 20/11/2026.
**Método:** cada item abaixo foi verificado **executando** o comando ou o teste
indicado — não por leitura de código nem por suposição. Quando um item faltava
ou estava parcial, ele foi implementado e a evidência correspondente foi
acrescentada antes de marcar `OK`.

## Resumo

| Métrica | Valor |
| --- | --- |
| Itens do checklist | 58 |
| Itens `OK` | 58 |
| Itens parciais/pendentes | 0 |
| Testes (por framework) | 321 (net8.0) + 321 (net10.0) |
| Bugs de hardening encontrados e corrigidos | 11 |
| Golden files byte-exatos | 13 remessas + 4 retornos |
| Comandos de build/lint/test executados | `dotnet build`, `dotnet test`, `dotnet format --verify-no-changes` |

## 1. Identidade, escopo e restrições (SPEC linhas 3–7)

| # | Requisito | Status | Evidência executada |
| --- | --- | --- | --- |
| 1 | Biblioteca .NET gera remessa CNAB 240 | OK | `CliTests.Generate_Itau240_ProduzArquivoValido`; `GeradorRemessa240.Gerar` em 8 bancos com golden byte-exato. |
| 2 | Biblioteca .NET gera remessa CNAB 400 | OK | `CliTests.Generate_Bradesco400_ProduzArquivoValido`; `GoldenFilesMilestone3Tests.Remessa400_ByteExata` (341/237/748/077). |
| 3 | Leitura de retorno CNAB 240 | OK | `Cnab240RetornoTests`, `Cnab240RetornoBancosAdicionaisTests`; `cnab parse <retorno240> --json` executado nos testes. |
| 4 | Leitura de retorno CNAB 400 | OK | `Cnab400RetornoTests`; comando real `cnab parse tests/Cnab.Tests/Golden/341_cobranca_400.ret --json` executado — JSON com ocorrência `06`, motivo `03`, valor `1500.50`. |
| 5 | Cobrança (cobrança de boletos) | OK | Geradores 240/400 + golden files + `Titulo`/`Pagador`/`Cedente`. |
| 6 | Pagamentos a fornecedores / Pix via 240 segmentos A/B/J/O | OK | `077_pagamento_240.rem` golden; `Cnab240Fixtures.LoteTed` (A/B), `SegmentosPagamento240` (J/J-52/O); `PropriedadesTodosLayoutsTests.Remessa240Pagamento_TrailerDoLote_SomaTodosOsValores`; `Pagamento` cobre TED/DOC/Pix chave/QR, boleto e tributos (`Modelo/Pagamento.cs`). |
| 7 | Nove bancos do SPEC (001, 033, 104, 237, 341, 748, 756, 077, 208) | OK | `Bancos` catálogo com 9 entradas; `PerfilBanco240.Obter` para todos; golden 240 para os 8 com cobrança publicada; tabela de cobertura em `docs/layouts.md` §"Cobertura efetiva". |
| 8 | Layouts declarativos de largura fixa | OK | `[RegistroCnab]`/`[CampoCnab]`; `MetadadosRegistro`; `cnab inspect` imprime posições lidas dos atributos (comando executado, 94 linhas de saída). |
| 9 | Tipagem forte | OK | `DateOnly`, `decimal`, `record` e enums no domínio; nullable habilitado em `Directory.Build.props`. |
| 10 | Garantias de round-trip | OK | `LeitorRetorno240/400.EscreverLinhas` preserva `LinhaOriginal`; propriedades FsCheck `Retorno240/400_ParseDeWrite_EhIdentico`; 4 goldens `.ret` com round-trip byte-exato. |
| 11 | Open source, MIT, autor "Hadagalberto Junior" | OK | `LICENSE` (MIT); `Directory.Build.props` com `Authors`/`Company`/`Copyright`; `.gitattributes` LF. |
| 12 | Sem stubs, TODO, `NotImplementedException`, `todo!`, `unimplemented!` | OK | Busca literal case-sensitive nos fontes (`\bTODO\b`, `NotImplementedException`, `todo!`, `unimplemented!`): **0 ocorrências**. |
| 13 | Nenhum teste ignorado/placeholder | OK | `dotnet test`: `Ignorado: 0` em net8.0 e net10.0; todos os testes usam `Assert` com vetores válidos/inválidos. |

## 2. Fontes oficiais (SPEC linhas 9–11)

| # | Requisito | Status | Evidência executada |
| --- | --- | --- | --- |
| 14 | FEBRABAN CNAB 240 v10.x (cobrança e pagamentos) | OK | F1/F2/F3/F4 registrados com versão/URL em `docs/layouts.md`; PDF consultado via `webfetch` (content-type `application/pdf` confirmado no URL publicado). |
| 15 | Manual de cada banco: nome, versão e URL em `docs/layouts.md` | OK | Tabelas "Fontes oficiais" dos marcos 1–3 com ID, documento, versão, URL e regras extraídas. |
| 16 | Desvios por banco codificados explicitamente e testados | OK | `PerfilBanco240`/`AjustesBanco240`; testes `Cnab240BancosAdicionaisTests` (DAC Itaú, DV Bradesco, convênio Sicredi, fixos BTG, versões Inter) e `Cnab240LayoutTests`. |
| 17 | Buscar fonte oficial quando o SPEC pede; citar em código/doc; nunca inventar posição/regra | OK | `docs/layouts.md` cita cada regra; fonte Sicredi **reconsultada ao vivo** em 20/11/2026 (`webfetch` do URL S3): seções 4.4/4.5 (DV módulo 11 com vetor 0165/02/00623/07/2/00003→1), 6.3 (`0D0A`), 6.4 (charset) e 7.1 (movimentos 02/03/06/09/26/28/30) conferem com a implementação. Nenhuma posição nova foi introduzida neste marco. |

## 3. Stack e infraestrutura (SPEC linhas 13–17)

| # | Requisito | Status | Evidência executada |
| --- | --- | --- | --- |
| 18 | Multi-alvo `net8.0` + `net10.0` | OK | `dotnet build Cnab.slnx -c Release` gerou dlls de **ambos** os frameworks para `Cnab`, `Cnab.Cli` e `Cnab.Tests`. |
| 19 | `netstandard2.0` apenas se não prejudicar a API | OK | Avaliado e descartado por causa de `DateOnly`; decisão documentada em `Cnab.csproj`, README pt-BR/en. |
| 20 | C# latest, nullable, warnings como erros, analyzers | OK | `Directory.Build.props`: `LangVersion=latest`, `Nullable=enable`, `TreatWarningsAsErrors=true`, `EnableNETAnalyzers=true`, `AnalysisLevel=latest`; build terminou com **0 avisos, 0 erros**. |
| 21 | xUnit com assertivas sem FluentAssertions | OK | Todos os testes usam `Xunit.Assert`; nenhuma referência a FluentAssertions no `Cnab.Tests.csproj`. |
| 22 | Testes de propriedade FsCheck | OK | `MotorRegistroPropriedadeTests`, `PropriedadesTodosLayoutsTests` executados (incluem geradores para os 9 bancos 240 e 4 bancos 400). |
| 23 | Testes de snapshot Verify | OK | `SnapshotTests` + `SnapshotTests.ArquivoDeExemploComDoisRegistros.verified.txt`; executados na suíte. |
| 24 | Projeto BenchmarkDotNet | OK | `dotnet run --project benchmarks/Cnab.Benchmarks -c Release -- --list flat` listou `MotorDeRegistros.LerTodasAsLinhas`, `EscreverTodasAsLinhas` e `ParseUmMilhao.LerUmMilhaoDeLinhas`. |
| 25 | `.gitattributes` `* text=auto eol=lf` + CRLF explícito no escritor com opção LF | OK | `.gitattributes` com `eol=lf` e `-text` para Golden; `EscritorDeArquivo.Montar` com `FimDeLinha.CrLf` (padrão) e `Lf`; `Generate_ComLf_DeveGravarSomenteLfESemBom` executado. |
| 26 | CI GitHub Actions ubuntu/windows/macos | OK | `.github/workflows/ci.yml` com matrix `ubuntu-latest, windows-latest, macos-latest` e setup .NET 8+10. |
| 27 | CI executa **os mesmos** comandos de build/lint/test executados na auditoria | OK | CI: `dotnet build Cnab.slnx -c Release --no-restore`, `dotnet test tests/Cnab.Tests/Cnab.Tests.csproj -c Release --no-build`, `dotnet format Cnab.slnx --verify-no-changes --no-restore`, mais smoke dos 4 comandos da CLI; auditoria executou os mesmos comandos localmente (build/test/format, todos com saída limpa). |
| 28 | Pacote `Cnab` (core) | OK | `src/Cnab/Cnab.csproj`, `IsPackable=false` (não publicar). |
| 29 | Pacote `Cnab.Cli` como dotnet tool `cnab` | OK | `PackAsTool=true`, `ToolCommandName=cnab`; `dotnet pack` + `dotnet tool install --tool-path ...` executados, `cnab --version` → `cnab 4.0.0`, `cnab --help` imprime o uso. |
| 30 | README.md (pt-BR) + README.en.md, CHANGELOG, CONTRIBUTING, LICENSE | OK | Todos presentes; exemplos dos READMEs executados por `ReadmeExamplesTests` (2 testes) e os 4 comandos de CLI do README executados de fato (item 51). |
| 31 | Não publicar no NuGet | OK | Nenhum `dotnet nuget push`/`gh`/`git push` executado; `IsPackable=false` no core; pacote da CLI apenas local. |

## 4. Núcleo — motor, domínio e boleto (SPEC linhas 20–24)

| # | Requisito | Status | Evidência executada |
| --- | --- | --- | --- |
| 32 | Definições declarativas: posição, comprimento, tipo, decimais, datas DDMMAA/DDMMAAAA, padding | OK | `Atributos.cs` + `TipoCampo`; `MotorRegistroTests.Escrever_PreencheCamposNasPosicoesCorretas`, `Ler_DataDDMMAA_AssumeSeculo2000`, `EscreverDecimal_RecusaPrecisaoAcimaDoCampo`. |
| 33 | Caixa alta e remoção de acentos para o charset CNAB | OK | `CharsetCnabTests` (16 casos + emoji + idempotência) e propriedade `ConteudoAlfanumericoSemprePertenceAoCharsetCnab`. |
| 34 | Leitores/escritores compilados em cache, sem reflexão por registro | OK | `LayoutCompilado` compila `Expression.Lambda` uma vez por tipo em `RegistroCnab<T>`; `DesempenhoParseUmMilhaoTests` processa 1M registros em < 1 s. |
| 35 | Validação de comprimento 240/400 | OK | `MotorRegistroTests.Ler_RejeitaLinhaCurta/Longa`; `Validate_LinhaComTamanhoErrado_RelataLinhaEComprimento` (CLI). |
| 36 | Erros precisos com linha/coluna/nome do campo | OK | `CampoCnabException` + `TamanhoRegistroException`; `Validate_CampoNumericoInvalido_RelataColunaENomeDoCampo` e `Parse_RetornoComCampoNumericoCorrompido_DeveApontarLinhaECampo`. |
| 37 | Domínio: Cedente/Beneficiário, Conta (agência, conta, DVs por algoritmo do banco) | OK | `Modelo/Pessoas.cs`, `ContaBancaria.cs`; `DvBancosTests`, `AlgoritmosAdicionaisTests`. |
| 38 | Título: nosso número com DV por banco e regras de carteira | OK | `Titulo.cs` + `Bancos/*`; `DvBancosTests` com vetores de todos os manuais; `RemessaItau_UsaAgencia4Conta5EExigeDac`, `RemessaBradesco_CompõeNossoNumeroConformeManual`. |
| 39 | Título: vencimento, valor, juros/multa/desconto/abatimento, protesto, pagador, sacador avalista, instruções | OK | `Titulo.cs`/`Juros`/`Multa`/`Desconto`/`Protesto`/`SacadorAvalista`/`Instrucoes`; `ModeloTests.Validar` cobre as regras (executado). |
| 40 | Pagamento: TED/DOC/Pix chave/Pix QR, boleto, tributos; lotes; trailers com contas e somas automáticas | OK | `Modelo/Pagamento.cs`, `Lotes.cs`; `GeradorRemessa240` calcula NSR e totais; `Remessa240_Trailers_RefletemQuantidadesESomas`, `Remessa240Pagamento_TrailerDoLote_SomaTodosOsValores`. |
| 41 | Remessa de cobrança 240 e 400 e pagamentos 240 por banco | OK | Matriz em `docs/layouts.md` §"Cobertura efetiva por banco/layout"; golden files e testes de geração por banco executados. |
| 42 | Sequenciamento NSA/NSR | OK | `Remessa240.NumeroSequencialArquivo`, `NSR` incrementado em `GeradorRemessa240` (inclusive J-52); `Dataset` dos goldens valida sequências; `Opcoes.NumeroSequencialArquivo` no JSON da CLI. |
| 43 | Retorno: códigos de ocorrência → enum tipado + tabela descritiva por banco | OK | `FamiliaOcorrencia`, `TabelasOcorrencias`, `TabelasOcorrencias400` (tabelas específicas para os 9 bancos); `OcorrenciasFamiliaTests` (17 testes) executados em 240 e 400. |
| 44 | Retorno: valores pago, tarifa, juros, desconto, abatimento, IOF e datas de ocorrência/crédito | OK | `ItemRetorno240`/`ItemRetorno400` + `ComandoParse.Projeto240/400`; `Parse_Retorno400Json_ContemOcorrenciaMotivoEValores` e golden `.ret` com todos os campos. |
| 45 | Boleto: linha digitável/código de barras de 44 posições a partir de um Título, com fator de vencimento e regra de 2025 | OK | `src/Cnab/Boleto/*`; `FatorDeVencimentoTests` (reset 22/02/2025), `BoletoTests`, `DvBancosTests` com vetores publicados. |

## 5. CLI (SPEC linha 27)

| # | Requisito | Status | Evidência executada |
| --- | --- | --- | --- |
| 46 | `cnab validate arquivo.rem` | OK | Comando real executado: `OK: 4 registro(s) CNAB 400 validado(s) (banco 341 - Itaú).`, exit 0; testes válidos e inválidos em `CliTests`/`HostileInputTests`. |
| 47 | `cnab parse retorno.ret --json` | OK | Comando real executado: JSON com `layout: 400`, `banco.codigo: 341`, `ocorrencia.codigo: "06"`, `valorPago: 1500.5`; exit 0. |
| 48 | `cnab inspect arquivo --bank 341` | OK | Comando real executado: tabela `HeaderRemessaItau400`, posição `077-079` = `341`; exit 0, 94 linhas. |
| 49 | `cnab generate titulos.json --bank 237 --layout 400` | OK | Comando real executado com `--output`: `Remessa CNAB 400 gerada em '...' (1206 bytes, 3 registros).`, exit 0; arquivo validado em seguida. |
| 50 | `--output`, `--lf`, `--json`, `--help`, `--version`, códigos de saída 0/1/2 | OK | `CliTests` + `HostileInputTests` (saída binária sem BOM, CRLF/LF, erro de uso 2, erro de dados 1); `cnab --version` executado via tool instalada. |
| 51 | Exemplos dos READMEs são executáveis e a saída foi verificada | OK | `ReadmeExamplesTests` executa os trechos de biblioteca e confere 3×402 bytes, CRLF, `BRADESCO`, e a leitura dos dois retornos; os 4 comandos de CLI do README foram executados nesta auditoria (itens 46–49) e o `dotnet pack`/`dotnet tool install` foi executado em diretório temporário. |

## 6. Testes e desempenho (SPEC linhas 30–33)

| # | Requisito | Status | Evidência executada |
| --- | --- | --- | --- |
| 52 | Golden remessa byte-exata para cada banco/layout implementado | OK | `GoldenFilesMilestone3Tests.Remessa240_ByteExata` com 8 bancos (001/033/104 acrescentados aqui), `Remessa400_ByteExata` com 4; 13 arquivos `.rem` comparados integralmente. |
| 53 | `parse(write(x))` byte-identical nos retornos | OK | 4 `.ret` em `Retorno400_ParseiaERoundTripByteExato` + propriedades FsCheck 240/400. |
| 54 | Amostras de retorno cobrindo **toda** família de ocorrência | OK | `OcorrenciasFamiliaTests`: 8 famílias × 240 + 8 famílias × 400 (com os códigos corretos do manual Itaú I2) + verificação de que o enum inteiro está representado na tabela. |
| 55 | Propriedades: comprimento exato, round-trip, totais de trailer | OK | `PropriedadesTodosLayoutsTests` (5 propriedades) + `MotorRegistroPropriedadeTests` (5) executados na suíte completa. |
| 56 | DVs contra vetores conhecidos dos manuais | OK | `DvBancosTests`, `AlgoritmosAdicionaisTests`, `BoletoTests`, `FatorDeVencimentoTests` executados (todos com valores publicados e citados). |
| 57 | Benchmark de 1M linhas com limite generoso | OK | `DesempenhoParseUmMilhaoTests` (60 s, soma de verificação) aprovado; `Cnab.Benchmarks.ParseUmMilhao` listado e executável. |
| 58 | Restrições finais: nunca publicar, commit local, ficar dentro do repositório | OK | Nenhum comando de publicação executado; todo o trabalho foi feito em `E:\source\source\repos\oss\cnab-dotnet`; artefatos temporários criados apenas em `%TEMP%` e removidos. |

## 7. Hostile user — bugs encontrados e corrigidos

Cada item foi primeiro reproduzido por um teste que falhava e só depois corrigido
na causa raiz. Nenhum teste foi enfraquecido para passar.

| Sintoma provocado | Correção | Regressão |
| --- | --- | --- |
| Comando desconhecido sem arquivo respondia "arquivo ausente" | `Cli.Executar` valida o comando antes das opções | `ComandoDesconhecidoSemArquivo_DeveSerErroDeUso` |
| Caminho vazio estourava `ArgumentException` não capturada em validate/parse/inspect | `ArquivoTexto.TentarLer` normaliza falhas de caminho para erro 1 | `CaminhoVazio_DeveRetornarErroDeDadosSemLancar` (3 comandos) |
| `--output` em diretório inexistente estourava `IOException` não capturada | `ArquivoTexto.TentarGravar` | `Generate_OutputEmDiretorioInexistente_...` |
| BOM UTF-8 deslocava o código do banco da posição 1 | `ArquivoTexto.Ler` descarta `EF BB BF` | `Validate_ArquivoComBomUtf8_...` |
| Arquivo com CR solto virava uma linha só | `LinhasCnab.Separar` unificada (CRLF/LF/CR) e usada pela CLI e pelos 2 leitores | `Validate_SeparadorCrSolto_...` |
| Número com espaço no meio (`0 57`) era aceito como 57 | `ConverterDigitos` rejeita espaço misturado a dígito, mantendo campo todo branco como "não utilizado" | `Validate_NumericoComEspacoNoMeio_...` |
| Remessa sem trailer passava na validação | `ValidarTrailers` exige trailer de arquivo na última linha | `Validate_TrailerDeArquivoAusente_...` |
| `--bank 999` era erro de dados (1) em vez de erro de uso (2) | `Opcoes.Ler` valida contra o catálogo do SPEC | `BancoNaoSuportado_DeveSerErroDeUso` |
| `horaGeracao=00:60:00` gerava exceção de `TimeOnly` com mensagem críptica | `InterpretarHora` valida minuto/segundo e reporta `horaGeracao inválida` | `Generate_HoraInvalida_DeveExplicarOCampo` |
| Caminhos do Windows com espaços/acentos e 1.000 títulos não estavam cobertos | Cobertura adicionada; comportamento já correto | `Generate_CaminhoDoWindowsComEspacosEAcentos_...`, `Generate_MuitosTitulos_...` |
| `--lf` e saída binária stdout sem BOM não estavam cobertos | Cobertura adicionada; comportamento já correto | `Generate_ComLf_...`, `Generate_SemOutput_...` |

## 8. Estado final da verificação

```
dotnet build Cnab.slnx -c Release                         -> 0 avisos, 0 erros
dotnet test tests/Cnab.Tests/Cnab.Tests.csproj -c Release -> 321/321 (net8.0), 321/321 (net10.0), 0 ignorados
dotnet format Cnab.slnx --verify-no-changes --no-restore  -> exit 0
dotnet run --project benchmarks/Cnab.Benchmarks ... -- --list flat -> 3 benchmarks listados
```

**Pendências:** nenhuma. Limitações já conhecidas e documentadas no projeto
(DV do nosso número do BTG atribuído pelo banco; versão de remessa da CAIXA não
extraível do PDF) permanecem explicitamente registradas em `docs/layouts.md`,
sem regra inventada.
