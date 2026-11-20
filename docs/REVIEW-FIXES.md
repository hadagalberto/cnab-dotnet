# Correções da revisão independente (2026-11-20)

Toda correção começou com um teste de regressão escrito à mão (valor esperado
derivado de manual oficial, SPEC ou cálculo documentado no comentário), executado
e visto falhar antes da alteração do código. Nenhum teste ou asserção foi
removido ou afrouxado; os que codificavam bugs foram corrigidos com comentário
citing a origem. Snapshots que fixavam bugs foram regenerados e lidos.

| Item | Teste de regressão | Correção | Status |
| --- | --- | --- | --- |
| 1 — CRÍTICO: trailer de lote 240 cobrança sem G057 (18-23) | `ReviewFindingsTests.Finding1_TrailerLoteCobranca_QuantidadeDeRegistrosDoLote` | Declarado `QuantidadeRegistros` (18-23, numérico) em `TrailerLoteCobranca240`; gerador preenche `sequencial + 2` (header + detalhes + trailer). Comentário incorreto removido. Goldens 240 regenerados e conferidos. | Corrigido |
| 2 — ALTO: `ParaVencimento(1667)` preso no ciclo 2002 | `ReviewFindingsTests.Finding2_ParaVencimento_Fator1667NoCicloRecente`, `Finding2_SegmentJ_VencimentoDoFator1667NoCicloRecente`, `FatorDeVencimentoTests.ParaVencimento_SemReferenciaEscolheOCicloMaisProximo` | `ParaVencimento` recebe referência opcional (padrão: hoje) e escolhe o ciclo mais próximo; `CodigoDeBarras.LerVencimento` repassa a referência. Segmento J 92-99 passa a gravar 21122026. `BoletoTests` e `FatorDeVencimentoTests` corrigidos com comentário citando a revisão. | Corrigido |
| 3 — ALTO: arrecadação tratada como boleto | `ReviewFindingsTests.Finding3_LinhaDeArrecadacao48Digitos_GeraSegmentoO` | Novo `Cnab.Boleto.Arrecadacao`: converte linha de 48 dígitos em barras de 44, confere DVs de bloco (módulo 10) e DV geral (módulo 10/11 pela 3ª posição), lê valor de 11 posições. `TipoEfetivo` detecta prefixo 8 e roteia para segmento O; boleto com prefixo 8 é rejeitado com mensagem clara. `DataVencimento` zerada — arrecadação não tem fator. | Corrigido |
| 4 — MÉDIO: lote mistura formas de lançamento | `ReviewFindingsTests.Finding4_LoteComFormasDeLancamentoDiferentes_Rejeitado` | `Validar` calcula a forma efetiva de cada pagamento e rejeita lotes com mais de uma forma (G029 é do lote, não do detalhe). | Corrigido |
| 5 — MÉDIO: campos "Zeros" escritos como brancos | `ReviewFindingsTests.Finding5_CamposDocumentadosComoZeros_SaoEscritosComZeros` | `ConteudoFiller = "0"` declarado nos atributos dos 12 campos Itaú 400 (31-32, 22-23), Bradesco 400 (21, 63-70), Inter 400 (18-20, 63-70) e Sicredi 400 (197-205, 220, 320-325, 202-227). Causa raiz: o motor usa `CampoCnabAttribute.ConteudoFiller`, não o valor inicial da propriedade. | Corrigido |
| 6 — MÉDIO: Bradesco 400 código da empresa alfanumérico | `ReviewFindingsTests.Finding6_Bradesco400_CodigoEmpresaNumericoZeroPadded` | Campo 27-46 passou a `TipoCampo.Numerico`/`long` no header de remessa e de retorno; gerador faz parse e escrita com zeros à esquerda. | Corrigido |
| 7 — MÉDIO: nosso número cru no segmento P | `ReviewFindingsTests.Finding7_BB_SegmentoP_UsaConvênioEFromDoNossoNumero`, `Finding7_Santander_SegmentoP_UsaDvDoNossoNumero` | BB compõe convênio + sequencial + DV (Anexo XI, vetor `05009401448-1`); Santander aplica o DV do manual v34 (vetor `566612457800-2`); BTG/Sicredi/Sicoob validam formato (e DV quando há contexto). Fixtures passaram a usar números válidos por banco. Asserção Santander corrigida com comentário. | Corrigido |
| 8 — BAIXO: fator 10000 gera barras de 45 dígitos | `ReviewFindingsTests.Finding8_CalcularFatorAlemUltimoDiaDoCiclo_Rejeita`, `FatorDeVencimentoTests.Calcular_VencimentoAlemDe13102049EhRejeitado` | `Calcular` lança `ArgumentOutOfRangeException` quando o fator ultrapassa 9999 (vencimento após 13/10/2049). | Corrigido |
| 9 — QUALIDADE: round-trip circular e sem golden 240 `.ret` | `ReviewFindingsTests.Finding9_Retorno400_EscreverLinhas_ReserializaRegistrosTipados`, `Finding9_Retorno240_GoldenIndependenteExisteEParseia`, `Finding9_Retorno240_EscreverLinhasReserializaRegistrosTipados` | `LeitorRetorno400`/`LeitorRetorno240` resserializam os registros tipados em vez de devolver a linha original; fixtures manuais corrigidas para zeros numéricos; novo golden `001_cobranca_240.ret` escrito por posições, sem uso do escritor da biblioteca. | Corrigido |
| Não verificado — CAIXA P modalidade `00` | `ReviewFindingsTests.VerificacaoAdicional_CaixaSegmentoPModalidadeSIGCB` | Manual SIGCB 67.118 v031 G069/G069A: posição 40 fixa `0` (SINCO), 41-42 = `14` (registrado/emissão beneficiário) configurável via `OpcoesGeracao240.ModalidadeCarteiraCaixa`. Golden 104 regenerado. | Corrigido (confirmado no manual) |
| Não verificado — convênio CAIXA em campo numérico | coberto por `Cnab240RemessaTests.Caixa_CodigoBeneficiario_OcupaPosicoesDoManual` | Já era sobrescrito por `PerfilBanco240.AjustesHeaderArquivo` com zeros; nenhuma alteração necessária. | Sem alteração |
| Não verificado — Santander header posição 17 | coberto por `Cnab240RemessaTests.Santander_UsaLayoutProprioNosHeadersEPeriodoP` | Campo é `TipoInscricaoEmpresa` (G005) e já recebe o tipo do cedente; nenhuma alteração necessária. | Sem alteração |
| Não verificado — Itaú 400 detalhe 4-17 alfanumérico | coberto por `Cnab400RemessaTests.RemessaItau_UsaAgencia4Conta5Carteira3ENossoNumero8` | Inscrição de 14 dígitos preenche integralmente o campo; alinhamento não altera o conteúdo. Nenhuma alteração necessária. | Sem alteração |

## Verificação final

- `dotnet build` com `TreatWarningsAsErrors=true` (analisadores ativos).
- `dotnet test` completo em `net8.0` e `net10.0`: 337 testes, 0 falhas, 0 ignorados.
- Snapshots regenerados e lidos: `001_cobranca_240.rem` (G057 = 000005),
  `104_cobranca_240.rem` (modalidade 14), `341_cobranca_400.rem` (31-32 = 00),
  `237_cobranca_400.ret` (código da empresa numérico). Nenhum valor foi aceito
  sem conferência das posições.
