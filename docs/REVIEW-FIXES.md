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

## Re-verificação independente (2026-11-20, sequência de eebdc92)

A re-verificação encontrou oito itens ainda incorretos. Cada um começou com
um teste de regressão escrito à mão (valor esperado literal, derivado do manual
FEBRABAN/versões dos bancos ou de cálculo documentado em comentário), executado
e visto falhar antes da alteração de código. Nenhum teste foi removido ou
afrouxado; as duas asserções que codificavam o round-trip circular (item 5)
foram corrigidas para o valor certo, com comentário citando a origem. Snapshots
foram regenerados e lidos posição a posição.

| Item | Teste de regressão | Correção | Status |
| --- | --- | --- | --- |
| 1 — DV de bloco da arrecadação sempre módulo 10 | `FollowUpReviewFindingsTests.Finding1_LinhaDeArrecadacao_UsaModulo10Ou11ConformeTerceiroDigito` (vetores 6/7/8/9 com somas nos comentários) e correção de `ReviewFindingsTests.Finding3_LinhaDeArrecadacao48Digitos_GeraSegmentoO` | `Arrecadacao.DeLinhaParaBarras` escolhe módulo 10 (6/7) ou 11 (8/9) também para os quatro DVs de bloco; o teste do vetor "828" passou a montar os blocos com módulo 11. | Corrigido |
| 2 — `LerValor` sem centavos | `FollowUpReviewFindingsTests.Finding2_ArrecadacaoLerValor_ConverteCentavosEmReais` | `00005000000` (5.000.000 centavos) agora lê R$ 50.000,00 (`/ 100m`); doc do método atualizada. | Corrigido |
| 3 — Segmento O não conferia valor efetivo | `FollowUpReviewFindingsTests.Finding3_SegmentoO_RejeitaPagamentoComValorDiferenteDoCodigoDeBarras` e `Finding3_SegmentoO_AceitaValorCorretoENaoValoraReferencia` | `MontarSegmentoO` compara `Arrecadacao.LerValor` com `Pagamento.Valor` quando o 3º dígito é 6 ou 8; com 7/9 (referência) não há conferência. Diferença lança `ArgumentException`. | Corrigido |
| 4 — DV Sicredi/Sicoob nunca verificado | `FollowUpReviewFindingsTests.Finding4_Sicredi_SegmentoP_RejeitaDvIncorretoDoNossoNumero`, `Finding4_Sicoob_SegmentoP_RejeitaDvIncorretoDoNossoNumero`, `Finding4_Cooperativas_ComputamDvQuandoNossoNumeroVemSemEle` | Novo `Cedente.DadosCooperativa` (cooperativa/posto/beneficiário/cliente) plumbado até `NossoNumeroNoSegmentoP`; DV é calculado na base sem DV e conferido no número completo, como BB/Santander. DV errado lança. | Corrigido |
| 5 — Round-trip de retorno reconstruía a linha | `FollowUpReviewFindingsTests.Finding5_Retorno240_EscreverLinhas_DevolveLinhaOriginalExatamenteComoLida`, `Finding5_Retorno400_EscreverLinhas_DevolveLinhaOriginalExatamenteComoLida`, asserções corrigidas em `ReviewFindingsTests.Finding9_Retorno400_EscreverLinhas_PreservaLinhaOriginal` e `Finding9_Retorno240_EscreverLinhas_PreservaLinhaOriginal` | Leitores guardam a `linha` exata; `EscreverLinhas` a devolve (fallback tipado só para objetos montados à mão). Fixtures/goldens 400 restaurados (brancos e "Bradesco" misto); header de retorno Bradesco passou a X(20) para aceitar "00623" alinhado à esquerda. | Corrigido |
| 6 — Golden 240 segmento T com dados deslocados/zerados | `FollowUpReviewFindingsTests.Finding6_GoldenRetorno240_AfirmaTodosOsCamposDeTEU` e `Cnab240RetornoBancosAdicionaisTests.Retorno_ParseiaOcorrenciasValoresEDataDeTodosOsBancos` | `001_cobranca_240.ret` reescrito campo a campo (58, 59-73, 106-130, 134-148, 149-188 e líquido do U) e lido posição a posição; teste afirma todos os campos de T e U; teste dos bancos adicionais usa linhas literais de 240 posições, sem escritor da biblioteca. | Corrigido |
| 7 — Código de liquidação Itaú 400 numérico | `FollowUpReviewFindingsTests.Finding7_RetornoItau400_CodigoDeLiquidacaoAceitaLetras` | Campo 393-394 de `DetalheRetornoItau400` passou a `TipoCampo.Alfanumerico`/`string`; vetor literal "AA" é lido e preservado. | Corrigido |
| 8 — Código da empresa Bradesco 400 com letras era silenciosamente limpo | `FollowUpReviewFindingsTests.Finding8_Bradesco400_CodigoEmpresaComLetras_Rejeitado` | Gerador usa `DigitosSemDescarte`: qualquer não dígito em "ABC12" lança `ArgumentException` com nome do campo. | Corrigido |

## Verificação final

- `dotnet build` com `TreatWarningsAsErrors=true` (analisadores ativos): 0 avisos, 0 erros.
- `dotnet test` completo em `net8.0` e `net10.0`: 352 testes, 0 falhas, 0 ignorados
  (337 após a primeira revisão; 15 regressões/asserções acrescentadas ou
  corrigidas na re-verificação).
- Primeira revisão — snapshots regenerados e lidos: `001_cobranca_240.rem`
  (G057 = 000005), `104_cobranca_240.rem` (modalidade 14),
  `341_cobranca_400.rem` (31-32 = 00), `237_cobranca_400.ret` (código da
  empresa numérico na remessa).
- Re-verificação — `001_cobranca_240.ret` (T/U reconstruídos e conferidos
  campo a campo) e `077/237/341/748_cobranca_400.ret` (restaurados com
  brancos e caixa originais, também lidos após a regeneração). Nenhum valor
  foi aceito sem conferência das posições.
