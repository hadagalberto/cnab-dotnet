# Registro de fontes oficiais (layouts e algoritmos)

O SPEC é categórico: **nenhuma posição de layout ou regra de dígito pode ser
inventada** — todo comando de código deste repositório cita o manual oficial
que o justifica. Este arquivo registra o nome, a versão e a URL de cada
documento consultado, e o que exatamente foi extraído dele. Marcos futuros
(CNAB 240/400 por banco, remessa/retorno, CLI) devem estender este registro.

## Convenções comuns (FEBRABAN)

| Documento | Versão | URL | Uso |
| --- | --- | --- | --- |
| FEBRABAN, *Layout Padrão CNAB 240 Posições* | V10.09 (14/10/21) | https://cmsarquivos.febraban.org.br/Arquivos/documentos/PDF/Layout%20padrao%20CNAB240%20V%2010%2009%20-%2014_10_21.pdf | Registro de 240 bytes; alinhamento (numérico à direita com zeros, alfanumérico à esquerda com brancos); campos genéricos G009 (DV agência), G010 (conta), G011 (DV conta), G012 (DV ag/conta), G015 (código remessa/retorno), G016 (DDMMAAAA), G018 (NSA). |
| Banrisul, *Leiaute Padrão Febraban CNAB 240* (cópia autorizada das seções FEBRABAN) | V10.3 (04/09/2026) | https://banrisul.com.br/bob/site/link/midias/48684_LeiauteBanrisulFebraban_pdr240_v103_04092026.pdf | Delimitadores de registro (CR/LF hex 0D 0A); observação 4: "em campos alfanuméricos, não utilizar acentuação gráfica, pontuação, parênteses, tabulações e caracteres especiais". |

## Charset CNAB

O conjunto de caracteres aceito em campos alfanuméricos segue a tabela publicada
pelo Sicredi (documento citado abaixo, seção 6.4 "Tipos de campos"), o mais
restritivo encontrado em manual oficial: `A-Z`, `0-9`, espaço e
`! * - ( ) [ ] { } , . ; : / % + =`. Diacríticos são removidos por
decomposição Unicode; `ç/Ç` vira `C`; travessões (`–—`) viram hífen; qualquer
caractere fora do conjunto é substituído por espaço (comportamento
documentado e testado em `CharsetCnabTests`).

## Cobrança — código de barras, linha digitável e fator de vencimento

Regra geral (44 posições: banco 3 + moeda 9 + DV geral + fator 4 + valor 10 +
campo livre 25; linha digitável de 47 dígitos em 5 campos) e reinício do fator
de vencimento em 22/02/2025 — confirmado em TODOS os manuais abaixo (tabelas
idênticas: 03/07/2000 → 1000 … 21/02/2025 → 9999; 22/02/2025 → 1000):
Bradesco, Itaú, Caixa, Santander, Sicredi e Sicoob.

| Banco | Documento | Versão | URL |
| --- | --- | --- | --- |
| Bradesco (237) | *Layout da Cobrança — Versão Português* | 4008-524-0121 | https://assets.bradesco/content/dam/portal-bradesco/assets/pessoajuridica/pdf/4008-524-0121-layout-cobranca-versao-portugues.pdf |
| Itaú (341) | *COBRANÇA CNAB 400 BYTES* (Anexos 2, 3, 4 e 6) | Janeiro 2017 | https://download.itau.com.br/bankline/layout_cobranca_400bytes_cnab_itau.pdf |
| Santander (033) | *Código de Barras — Cobrança* | v34, setembro 2021 | https://cms.santander.com.br/sites/WPS/documentos/arq-cobranca-barras-port-jan22/22-01-13_181337_cobran%C3%A7a+-+c%C3%B3digo+de+barras+portugu%C3%AAs.pdf |
| Caixa (104) | *Especificações técnicas para emissão de boletos de cobrança bancária — SIGCB* (Anexos I a VI) | 67.119 v014 | https://caixa.gov.br/Downloads/cobranca-caixa/ESP_COD_BARRAS_SIGCB_COBRANCA_CAIXA.pdf |
| Sicredi (748) | *CNAB 240 — Cobrança Sicredi* (itens 3.0, 4.4, 4.5, 6.4, 10.2-10.9) | portal do desenvolvedor (atualização 2026-09-29) | https://developers.sicredi.com.br/public/docs/getting-started-cnabs-240-billing.md |
| Sicredi (748) | *Geração do Nosso Número* | portal do desenvolvedor | https://developers.sicredi.com.br/public/docs/gera%C3%A7%C3%A3o-do-nosso-n%C3%BAmero.md |
| Sicredi (748) | API de Cobrança (resposta de exemplo com código de barras e linha digitável oficiais) | v3.9.1 | https://developers.sicredi.com.br/public/reference/cadastrarboleto.md |
| Sicoob (756) | *Layouts para troca de informações — Cobrança* (planilha "02.Especificações do Boleto", itens 3.13 e 3.29) | distribuição .xlsx do banco | https://www.sicoob.com.br/ (cópia de trabalho arquivada durante o desenvolvimento) |

### Regras capturadas por banco (marco 1)

| Banco | Nosso número | DV nosso número | Campo livre (44 pos. 20-44) | DVs agência/conta |
| --- | --- | --- | --- | --- |
| Bradesco | 11 dígitos + DV | Módulo 11 base 7 sobre carteira(2)+NN(11); resto 1 → "P", resto 0 → 0 (vetores do manual: 19/00000000002→8; 19/00000000001→P; 19/00000000006→0) | agência(4)+carteira(2)+NN(11)+conta(7)+"0" (vetor: ag 31, carteira 04, NN 00317720028, conta 95279 → linha digitável "23790.03102 40031.772003 28009.527905 7 10010000000000") | Algoritmo não publicado no manual de cobrança: o "DC" é fornecido pelo banco na contratação; o campo livre usa agência/conta **sem** DV |
| Itaú | 8 dígitos + DAC | Módulo 10 sobre agência(4)+conta(5)+carteira(3)+NN(8); carteiras 126/131/146/150/168 usam apenas carteira+NN (Anexo 4; vetor: 0057/12345/110/12345678 → DAC 8) | carteira(3)+NN(8)+DAC(1)+agência(4)+conta(5)+DAC ag/conta(1)+"000" (vetor do Anexo 2: barcode "34196166700000123451101234567880057123457000", linha "34191.10121 34567.880058 71234.570001 6 16670000012345") | DAC ag/conta = módulo 10 sobre agência+conta (vetor: 0057/12345 → 7); carteira 198 (NN 15 posições) especificada no Anexo 5 |
| Santander | 12 dígitos + DV | Módulo 11 pesos 2-9; resto 0/1 → 0, resto 10 → 1, senão 11-resto (vetor: 566612457800 → DV 2) | CNAB 240: "9"+PSK(7)+NN(13, com DV)+"0"+modalidade(3: 101/104); CNAB 400 legado: "9"+PSK+"00000"+NN(8, com DV)+"0"+modalidade (vetor: PSK 0282033, NN 5666124578002, carteira 101, fator 2046, valor 273,71 → DV geral 8) | Não publicado: o Santander usa o "código do beneficiário padrão" (PSK), fornecido pelo banco |
| Caixa (SIGCB) | 17 posições (2 de carteira/emissão + 15 livres) + DV | Módulo 11 pesos 2-9 sobre 17 posições; resultado > 9 → 0 (Anexo IV; vetor: "1400000000000019" → DV 7) | código beneficiário(6)+DV(1)+NN[2..4](3)+NN[0](1)+NN[5..7](3)+NN[1](1)+NN[8..16](9)+DV campo livre(1) (Anexo III; vetor: 005507 DV 7 + NN "14/222333777777777" → DV campo livre 1; DV geral 4, "não admite 0") | DV conta (NE051, CNAB 400 67.126 v029): módulo 11 pesos 2-9 sobre conta(12), >9 → 0 (vetor: 000000109990 → 6); DV ag/conta (NE052): módulo 11 sobre agência(4)+conta(12) (vetor: 0161+000000109990 → 5); DV do código do beneficiário (Anexo VI): módulo 11 (vetor: 005507 → 7); DV agência: "fornecido pela CAIXA, calculado pelo módulo 11" (NE049) |
| Sicredi | AA/B/NNNNN (8) + DV | Módulo 11 pesos 2-9 sobre cooperativa(4)+posto(2)+beneficiário(5)+AA(2)+B(1)+NNNNN(5); resultado 10/11 → 0 (vetor da documentação: 0165/02/00623/07/2/00003 → DV 1) | tipo cobrança "1"+tipo carteira "1"+NN(9, com DV)+cooperativa(4)+posto(2)+beneficiário(5)+indicador de valor(1: "1" com valor, "0" zerado)+"0"+DV (vetor do item 10.4: "11"+"072000031"+"0165"+"02"+"00623"+"1"+"0" → DV 8; amostra real da API: "112510061420512031533510"+DV 3, barcode "74891886400000099901125100614205120315335103", linha "74891125110061420512803153351030188640000009990") | Header CNAB 240 Sicredi traz "Conta DV" como dado do banco; algoritmo não publicado na documentação pública |
| Sicoob | 7 dígitos (valor iniciado em 1) + DV | Módulo 11 com constante "3197" alinhada à esquerda sobre cooperativa(4)+cliente(10)+NN(7); resto 0/1 → 0, senão 11-resto (vetor do documento: 0001+0000000019+0000021 → somatório 36, DV 8) | carteira(1)+agência(4)+modalidade(2)+código do cliente(7)+NN(8, com DV)+parcela(3, "001" se única); linha digitável campo 1 "AAABC.DDDDE" | Não publicado na planilha de especificações do boleto |
| BB (001) | convênio 4 → convênio(4)+seq(7)+DV; convênio 6 → convênio(6)+seq(5)+DV; convênio 7 → convênio(7)+seq(10), sem DV | Anexo XI: módulo 11 com pesos 9→2 da direita para a esquerda; resto 1-9 → DV = resto, 10 → "X", 0 → 0 (vetor: 05009401448 → 1) | conv4: convênio(4)+seq(7)+agência(4)+conta(8)+carteira(2); conv6: convênio(6)+seq(5)+agência(4)+conta(8)+carteira(2); conv7: zeros(6)+convênio(7)+seq(10)+carteira(2) | DV do prefixo da agência: mesmo Anexo XI (vetor derivado: 1234 → 3); G012 (DV ag/conta) não é tratado pelo BB |
| Inter (077) | 10 dígitos + DV (carteira 110; 112/121 atribuídas pelo banco) | Módulo 10 sobre agência(4)+carteira(3)+NN(10) (vetor: 0001/110/0004309540 → DV 1) | agência(4)+carteira(3)+nº operação(7)+NN(11, com DV) | Agência única 0001; DV de conta não publicado |
| BTG (208) | 11 dígitos atribuídos pelo banco | Não publicado | agência(4)+carteira(2)+NN(11, sem DV)+conta(7)+"0" | Não publicado |

### Banco com algoritmo pendente de fonte pública

| Banco | Situação no marco 1 | Fonte consultada |
| --- | --- | --- |
| BTG (208) | O campo livre do código de barras está publicado no portal do desenvolvedor (ver tabela acima) e foi codificado. O **DV do nosso número** não tem algoritmo publicado: o número (com DV) é atribuído pelo banco e devolvido no retorno/API. A biblioteca valida o formato de 11 posições e não inventa regra de cálculo. | developers.empresas.btgpactual.com/docs/cobrança (documentação oficial consultada) |

BB e Inter tiveram seus algoritmos localizados em manuais oficiais durante
este marco (ver tabela de fontes adicionais) e estão implementados com vetores
resolvidos. Para o BTG, a biblioteca codifica **exatamente o que está
publicado** e deixa explícito, em XML docs e testes, o que depende de valor
fornecido pelo banco. Nenhuma regra foi inventada.

## Fonte adicionais consultadas neste marco (busca efetiva)

| Documento | Versão | URL | Regras confirmadas |
| --- | --- | --- | --- |
| FEBRABAN, Comunicado **FB-009/2023** (reproduzido em manuais oficiais de banco) | 2023 | https://www.banpara.b.br/PortalImagens/2tdgh54u/manual-de-procedimento-de-impressao-de-boletos-por-terceiros-cobranca-banpara-v1-2.pdf (Banpara, *Manual de Procedimento de Impressão de Boletos por Terceiros – Cobrança Registrada v1.2*, Anexos 02 e 03) | Fator 9999 em 21/02/2025; 1000 em 22/02/2025; 1667 em 21/12/2026; 4789 em 09/07/2035; 9999 em 13/10/2049. O Anexo 03 expressa a nova data-base como 29/05/2022 (29/05/2022 + 1000 dias = 22/02/2025). |
| BB, *Especificações Técnicas para Confecção de Boleto de Pagamento do Banco do Brasil* (Doc5175Bloqueto) | maio/2019 | https://www.bb.com.br/docs/pub/emp/empl/dwn/Doc5175Bloqueto.pdf (espelho consultado: https://boletobancario-codigodebarras.com/docs/Doc5175Bloqueto.pdf) | Anexos VII/IX (campo livre por convênio 4/6/7) e **Anexo XI**: DV do nosso número/prefixo da agência/código do beneficiário por módulo 11 com pesos **9 a 2 da direita para a esquerda**; resto 1-9 → DV = resto, resto 10 → "X", resto 0 → 0. Vetor resolvido: 05009401448 → DV 1. |
| Inter, *Manual CNAB400 – Emissão de boletos de cobrança* | V9 (06/07/2026) | https://s3.sa-east-1.amazonaws.com/static.bancointer.com.br/openbanking/cnab/manual/manual.pdf | Seção 7.1.3: campo livre = agência(4)+carteira(3)+nº operação(7)+NN(11). Seção 7.3: DV do NN = módulo 10 sobre agência(4)+carteira(3)+NN(10); vetor 0001/110/0004309540 → DV 1. Carteiras 110 (cliente numera), 112/121 (banco numera). |
| BTG Pactual, portal do desenvolvedor — *Cobrança* | atual | https://developers.empresas.btgpactual.com/docs/cobran%C3%A7a | Campo livre = agência(4)+carteira(2)+NN(11, sem DV)+conta(7)+zero fixo; tabela do fator com reinício em 22/02/2025. |

## Fator de vencimento — tabelas publicadas usadas nos testes

- Bradesco 4008-524-0121 (p. 34): 03/07/2000 → 1000; 05/07/2000 → 1002;
  01/05/2002 → 1667; 17/11/2010 → 4789; 21/02/2025 → 9999; **22/02/2025 →
  1000** ("nova data-base").
- Itaú CNAB 400, Anexo 6: mesma tabela (1000 → 03/07/2000 … 9999 → 21/02/2025,
  1000 → 22/02/2025), além da janela de validade (3.000 fatores anteriores e
  5.500 futuros).
- Caixa SIGCB Anexo II: acrescenta o ciclo novo completo — 22/02/2025 → 1000,
  21/12/2026 → 1667, 09/07/2035 → 4789, 13/10/2049 → 9999.
- Santander v34: data-base 07/10/1997; exemplos 06/12/2000 → 1156,
  15/12/2000 → 1165, 30/12/2000 → 1180; nova data-base a partir de 22/02/2025.
- Sicredi item 10.7: data-base 07.10.1997, exemplo 20/12/2007 → 3726;
  Comunicados FB-082 e FB-122 da FEBRABAN citados para o reinício.
- Sicoob (Especificações do Boleto): fator = (vencimento - 03/07/2000) + 1000;
  "em 22/02/2025 o fator retornará automaticamente para 1000", nova
  data-base adotada a partir de 22/02/2025.

## DNIs deste marco

- `Cnab.Arquivo`: motor de registros de largura fixa (leitura/escrita
  compiladas por árvores de expressão, cacheadas por tipo; sem reflexão por
  registro; erros com linha/coluna/campo).
- `Cnab.Texto.CharsetCnab`: normalização para o charset CNAB.
- `Cnab.Boleto`: fator de vencimento, código de barras (44) e linha digitável
  (47), composição e reversão, com os DVs por banco.
- `Cnab.Bancos`: regras por banco citadas acima (vetores de teste em
  `DvBancosTests` e `BoletoTests`).
- `Cnab.Modelo`: domínio (cedente, pagador, título, pagamento, lotes e totais).
