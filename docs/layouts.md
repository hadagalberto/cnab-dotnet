# Registro de fontes oficiais (layouts e algoritmos)

O SPEC é categórico: **nenhuma posição de layout ou regra de dígito pode ser
inventada** — todo comando de código deste repositório cita o manual oficial
que o justifica. Este arquivo registra o nome, a versão e a URL de cada
documento consultado, e o que exatamente foi extraído dele. Cada marco estende
este registro; o marco 4 (CLI, propriedades e benchmarks) não introduziu nova
posição de layout, apenas reutilizou as fontes abaixo.

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

## Marco 2 — layouts base CNAB 240 e remessa/retorno por banco

Todas as posições abaixo foram extraídas dos PDFs/manuais oficiais consultados
durante este marco (download e extração de texto das tabelas). Nenhuma posição
de campo foi inventada: quando uma informação não pôde ser verificada na fonte
pública, ela não foi codificada e o fato está registrado ao final desta seção.

### Fontes oficiais (marco 2)

| ID | Documento | Versão | URL | Uso |
| --- | --- | --- | --- | --- |
| F1 | FEBRABAN, *Intercâmbio de Informações entre Bancos e Empresas — Padrão FEBRABAN 240 Posições* | V10.9 (14/10/2021) | https://cmsarquivos.febraban.org.br/Arquivos/documentos/PDF/Layout%20padrao%20CNAB240%20V%2010%2009%20-%2014_10_21.pdf | Header/trailer de arquivo e lote; segmentos P, Q, R, T, U (cobrança); A, B, J, J-52, O (pagamentos); tabelas C004/C044/C047/G059 |
| F2 | FEBRABAN, mesmo título | V10.10 (30/08/2022) | https://cmsarquivos.febraban.org.br/Arquivos/documentos/PDF/Layout%20padrao%20CNAB240%20V%2010%2010%20-%2030_08_2022_2.pdf | Conferência das tabelas de cobrança (idênticas à V10.9) |
| F3 | FEBRABAN, mesmo título | V10.11 (31/07/2023) | https://cmsarquivos.febraban.org.br/Arquivos/documentos/PDF/Layout%20padrao%20CNAB240%20V%2010%2011%20-%2031_07_2023.pdf | Tabelas de pagamentos (seções 3.1.2–3.1.4) e domínios G025/G028/G029/G060/G100–G102 |
| B1 | Banco do Brasil, *Particularidades BB — Leiaute CNAB 240* | junho/2026 | https://www.bb.com.br/docs/pub/emp/empl/dwn/CNAB240SegPQRSTY.pdf | Convênio composto, versões 083/042, DV ag/conta em branco, campos 20.3Q/21.3Q/22.3Q, Anexo 01, códigos de movimento |
| B2 | Banco do Brasil, *Particularidades BB — CNAB240 — Arquivo de Pagamentos* | junho/2026 | https://www.bb.com.br/docs/portal/disem/PgtVer03BB.pdf | Convênio `0126`, ocorrências 231-240 = zeros na remessa, B com `BRL`, J/O |
| C1 | CAIXA, *Manual de Leiaute de Arquivo Eletrônico CNAB 240 (Cobrança)* | 67.118 v031 | https://www.caixa.gov.br/Downloads/cobranca-caixa/Manual_de_Leiaute_de_Arquivo_Eletronico_CNAB_240.pdf | Convênio 33-52 = zeros, código do beneficiário 59-65/34-40/24-30, nosso número 43-57, C047-A |
| C2 | CAIXA, *Manual Operacional — Pagamento de Salários, Fornecedores e Autopagamento* | 37.270 v036 | https://caixa.gov.br/Downloads/pagamentos-de-salarios-fornecedores-e-auto-pagamento/Leiaute_CNAB240_Pagamentos_e_Debito_Automatico.pdf | Conta com operação (59-62) + conta (63-70), versão de lote 041 |
| S1 | Santander, *Layout Cobrança — H7815 — CNAB 240 Posições* | V6 (fevereiro/2023) | https://cms.santander.com.br/sites/WPS/documentos/arq-cobranca-portugues-out23/23-10-24_142404_h7815layoutcobrancacnab240.pdf | Header de arquivo 9-16/17/18-32/33-47, versões 040/030, P 45-57/58/63-77/225 |
| S2 | Santander, *Pagamento a Fornecedores — Layout CNAB 240 Posições* | YLEC_2403 V11.3.1 (04/2021) | https://cms.santander.com.br/sites/WPS/documentos/arq-layout-pagamento-fornecedores/22-01-07_121040_pagamento-a-fornecedores-layout-cnab-240-v11.3.1.pdf | Header de lote de pagamentos 103-222, versão 031, segmentos A/B/J/O |
| CO1 | Sicoob, *Guia de Importação de Arquivos CNAB 240* | v3.2 | https://www.sicoob.com.br/documents/20128/1635942/Guia+de+importacao+de+arquivos+CNAB+240.pdf/1c857be9-c3a0-64bd-a023-604533f9bf68?version=1.0&t=1745426919012&download=true | Versões 087 (arquivo) e 045 (lote de pagamentos), trailer de arquivo, G059 |
| IT1 | Itaú, *Layout de Arquivos — FEBRABAN 240 (Cobrança)* | janeiro/2017 | https://download.itau.com.br/bankline/cobranca_cnab240.pdf | Confirmação dos trailers de arquivo/lote e do domínio C044 |
| VE1 | Banco do Brasil, *Particularidades BB — Leiaute CNAB 240* | agosto/2017 | https://www.bb.com.br/docs/pub/inst/dwn/Cnab240Agosto2017.pdf | Confirmação das posições de T/U e da tabela de ocorrências |

### Posições implementadas (resumo)

- Header de arquivo FEBRABAN: convênio 33-52 (20), nome da empresa 73-102,
  nome do banco 103-132, código remessa/retorno 143, data 144-151, hora
  152-157, NSA 158-163, versão do layout 164-166 (padrão **103**), densidade
  167-171 (`01600`). F1/F2.
- Trailer de lote de **cobrança** é diferente do de **pagamentos**: o primeiro
  discrimina quantidades/valores por carteira (simples 24-46, vinculada
  47-69, caucionada 70-92, descontada 93-115) e aviso de lançamento 116-123;
  o segundo usa quantidade de registros 18-23, somatória 24-41 (16 inteiras +
  2 decimais), somatória de moedas 42-59, aviso de débito 60-65 e ocorrências
  231-240. F1/F3, IT1.
- Segmentos de cobrança: P (nosso número 38-57, carteira 58, vencimento
  78-85, valor 86-100, protesto 221-224), Q (pagador 18-153, sacador/avalista
  154-209), R (descontos 18-65, multa 66-89, mensagens 100-179), T (movimento
  C044 16-17, tarifa 199-213, motivo C047 214-223), U (valores 18-153,
  ocorrência do pagador A001 154-157). F1, B1.
- Segmentos de pagamentos: A (câmara 18-20, favorito 21-43, valor 120-134,
  ocorrências 231-240), B genérico V10.x (informações 10/11/12 em 33-226) e
  variante de endereço (33-127) exigida por BB/CAIXA/Sicoob, J (código de
  barras numérico 18-61), J-52 (identificador `52` em 18-19), O (código de
  barras alfanumérico 18-61). F3, B2, CO1.
- O campo "ocorrência" tem quatro tabelas distintas, que não devem ser
  confundidas: C044 (movimento de retorno, T/U 16-17), C047 (motivo/rejeição,
  T 214-223), A001 (ocorrência do pagador, U 154-157) e G059 (pagamentos,
  231-240). F1/F3.

### Peculiaridades por banco codificadas em `PerfilBanco240`

- **BB (001)**: versão do arquivo 083, do lote de cobrança 042; DV ag/conta
  não tratado (branco); 20.3Q/21.3Q em branco/zeros; 21.3R/22.3R/23.3R/25.3R
  zerados; ocorrências de remessa de pagamentos 231-240 = zeros; código C044
  52 do BB ("registro de título já liquidado — carteira 17") sobrescreve o
  código FEBRABAN; A4 = "Sacado DDA". B1/B2/VE1.
- **CAIXA (104)**: convênio do header de arquivo 33-52 = zeros; código do
  beneficiário 59-65 (header), 34-40 + 13 zeros (header de lote) e 24-30 + 7
  zeros (segmento P); nosso número nas posições 40-57; 41.3P zerado; tabela
  de tarifas 06–16. C1/C2.
- **Santander (033)**: layout próprio de cobrança — 9-16 reservado (8 brancos),
  inscrição 17/18-32 (15), código de transmissão 33-47 (15), versões 040 e
  030; P com nosso número 45-57 (13), carteira 58, seu número 63-77 (15),
  zero fixo 225. S1/S2.
- **Sicoob (756)**: versão do arquivo 087, do lote de pagamentos 045; para
  cobrança o espelho público do XLS de 03/09/2018 (fonte oficial protegida por
  senha) indica versão 081, DV ag/conta 72 = 0, 22.1 = zeros, 15.3P = 0,
  16.3P = branco, 38.3P = 0, 40.3P = 09 e 41.3P = zeros — esses valores são
  marcados como **menor confiança** no código; os de pagamentos vêm do PDF
  oficial CO1.

### Itens não verificados (não inventados)

- A versão de layout de **remessa** da CAIXA não foi extraível do texto do PDF
  (o manual só explicita as versões de retorno 040/047). O gerador usa o
  padrão FEBRABAN e permite substituição explícita pelo chamador.
- O domínio completo da carteira C006 da CAIXA e do BB (além de `1` = simples)
  e a tabela completa C044 da CAIXA não foram extraíveis; o código cobre os
  códigos publicados nas tabelas citadas e expõe mapas injetáveis.
- As posições de **cobrança** do Sicoob vêm de espelho não oficial do XLS
  (a fonte oficial é uma planilha protegida). Nenhuma regra adicional foi
  inferida além das ali publicadas.
- A descrição do código A001 na faixa 0301–0306 foi reconstruída de um manual
  com colunas entrelaçadas na extração; os demais códigos seguem F1.

## Marco 3 — Itaú, Bradesco, Sicredi, Inter e BTG (240/400)

Nenhuma posição ou regra desta seção foi inventada: todos os valores abaixo
foram extraídos dos manuais oficiais (ou, onde indicado, de transcrição
terceirizada/dev portal do próprio banco) e confrontados com as tabelas do
marco 2. Os PDFs oficiais foram baixados e o texto extraído com PyMuPDF; as
páginas HTML oficiais e as transcrições citadas foram lidas diretamente.

### Fontes oficiais (marco 3)

| ID | Documento | Versão / data | URL |
| --- | --- | --- | --- |
| I1 | Itaú, *Cobrança Bancária — Layout de Arquivos FEBRABAN 240* | janeiro/2017 | https://download.itau.com.br/bankline/cobranca_cnab240.pdf |
| I2 | Itaú, *Cobrança Bancária — Layout de Arquivos CNAB 400* | janeiro/2017 | https://download.itau.com.br/bankline/layout_cobranca_400bytes_cnab_itau.pdf |
| B3 | Bradesco, *MPO — Troca de Arquivos 240 Posições* | v04, dezembro/2024 | https://assets.bradesco.com.br/content/dam/portal-bradesco/assets/pessoajuridica/pdf/MPO-Troca-Arquivos-Layout-240P.pdf |
| B4 | Bradesco, *MPO 240 posições* 4008.524.0339 | v09, 09/2020 | https://assets.bradesco.com.br/content/dam/portal-bradesco/assets/pessoajuridica/pdf/4008-524-0339-mp-operacionais-troca-arquivos-240-posicoes.pdf |
| B1' | Bradesco, *Cobrança — Versão Português* 4008.524.0121 | v17, 28/09/2020 | https://assets.bradesco.com.br/content/dam/portal-bradesco/assets/pessoajuridica/pdf/4008-524-0121-layout-cobranca-versao-portugues.pdf |
| S3 | Sicredi, *CNAB 240 — Cobrança Sicredi* (manual + tabelas no portal) | atualizado 29/09/2026 | https://developers.sicredi.com.br/public/docs/getting-started-cnabs-240-billing |
| S4 | Sicredi, *CNAB 400 — Cobrança Sicredi* | v2.1, novembro/2021 | https://www.sicredi.com.br/media/produtos/manual_cnab_400_2_1.pdf |
| IN1 | Inter, *Manual CNAB400 — Emissão de boletos de cobrança* | v2.2, 26/08/2024 | https://s3.sa-east-1.amazonaws.com/static.bancointer.com.br/openbanking/cnab/manual/manual.pdf |
| IN2 | Inter, *Manual CNAB240 Pagamentos* | v1.21, 10/02/2026 | https://static.bancointer.com.br/enterprise-solution/cnab240/manual/Manual_CNAB240_Pagamentos.pdf |
| T1 | BTG Pactual, portal do desenvolvedor — *CNAB Febraban 240 posições* (FEBRABAN 10.9) | atualizado 15/03/2025 | https://developers.empresas.btgpactual.com/docs/cnab-febraban-240-posi%C3%A7%C3%B5es |
| T2 | VeHub/Vertrau (transcrição do manual BTG, **terceirizada**) — *Remessa 240* / *Retorno 240* | © 2024/2025 | https://docs.vehub.com.br/Layouts/%F0%9F%8F%A6%20BTG%20Pactual/Manual/Remessa%20240/ · https://docs.vehub.com.br/Layouts/%F0%9F%8F%A6%20BTG%20Pactual/Manual/Retorno%20240/ |
| F4 | FEBRABAN, *Padrão 240 posições* | V11.0, 11/09/2026 | https://cmsarquivos.febraban.org.br/Arquivos/documentos/PDF/Layout%20padrao%20CNAB240%20V%2011_0%20-%202026_09_11.pdf |

### Posições e desvios implementados

- **Itaú 240 (I1)**: versão do arquivo `040` (164-166) e do lote `030`
  (014-016); agência de 4 posições e conta de 5, com DAC na posição 72 do
  header e 73 do header de lote (Anexo 4); segmento P com carteira em 038-040,
  nosso número em 041-048 e DAC em 049; segmento Q com brancos em 064-073.
  O DAC usa módulo 10 sobre agência+conta+carteira+nosso número (carteiras
  126/131/146/150/168 usam só carteira+nosso número). No retorno, os motivos
  ficam em T 214-221 (8 posições, 4 códigos) e as tarifas em T 199-213;
  ocorrências proprietárias 59, 64, 65, 74-79 e 94 mapeadas.
- **Itaú 400 (I2)**: header com agência 027-030, conta 033-037 e DAC 038;
  registro 1 com uso da empresa 038-062, nosso número 063-070, carteira
  numérica 084-086 e código de carteira 108; juros em 161-173, desconto em
  180-192, abatimento em 206-218; **motivos de rejeição em 378-385**, não
  319-328; data de crédito em 296-301 (no header, 114-119); registro 2 de
  multa (posições 002-023); trailer de remessa sem totalizadores. Tabela
  completa de ocorrências (Nota 17) e motivos de entrada (Tabela 1) e de
  devolução de cheque (Tabela 9) codificada em `TabelasOcorrencias400`.
- **Bradesco 240 (B3/B4)**: versões `084`/`042`; identificação do título no
  banco decomposta em produto(3)+zeros(5)+nosso número(11)+DV(1) nas posições
  038-057 (o DV é o da carteira+NN, módulo 11, resto 1 → "P"); carteira real
  vai no produto, não em C006. Ocorrência 66 ("Pix") sobrescreve a tabela
  FEBRABAN.
- **Bradesco 400 (B1')**: header com código de empresa 027-046 e sistema "MX"
  em 109-110; registro 1 com carteira 022-024, agência/conta beneficiária
  025-037, nosso número 071-082 (11+DV), ocorrência 109-110, tarifa 176-188,
  custas de protesto 189-201, valor pago 254-266, data de crédito 296-301 e
  motivos 319-328 (até 5 pares); mensagens no registro 2; trailer com blocos
  por carteira. Tabelas de ocorrências e motivos (Tabelas 1 e 2 do manual,
  incluindo P1-P8) codificadas.
- **Sicredi 240 (S3)**: versões `081`/`040`; convênio em branco; agência sem
  DV (58/59/23 em branco); nosso número `AA/B/XXXXX-D` com DV módulo 11;
  código de baixa/devolução 224 = 1 e prazo 225-227 = 000; segmentos S e Y
  proprietários documentados; movimentos de retorno 61 (Pix) e 78-85 e P1/P2
  catalogados; motivos P1/P2/P6/PX têm descrições próprias.
- **Sicredi 400 (S4)**: datas de geração/gravação em `AAAAMMDD` (gravadas
  como texto); literal `SICREDI` na remessa e `BANSICREDI` no retorno;
  registro 1 com nosso número 048-056, instrução 109-110, seu número 111-120,
  vencimento 121-126, valor 127-139; no retorno, nosso número 048-062,
  ocorrência 109-110, valor pago 254-266, motivos 319-328 e data prevista de
  crédito 329-336 (`AAAAMMDD`). Tabela de motivos do manual (códigos A1-B9,
  C1-C9, D1-D9, E1-E9, F1-F9, G1-G9, H1-H9, I1-I9, J1-J9, K1-K9, L1-L7,
  M1-M3, N1-N5, P1/P2/P6/PX, S1/S4/S5, X0-XB) codificada.
- **Inter 240 (IN2)**: o Inter **não possui layout CNAB 240 de cobrança** — o
  manual V1.21 cobre apenas pagamentos (segmentos A/B/J/J-52/O). Perfil com
  versão `107`/`046`, agência única `00001` com DV `9` e tabela própria de
  ocorrências G059 em 231-240 (00, AR, AG, ZI, PA, PJ, PK, HA, ...).
- **Inter 400 (IN1)**: literal `INTER`, sequencial em 111-117; registro 1 com
  carteira 021-023 (`110`/`112`), conta 028-036, nosso número 090-100,
  ocorrência 109-110, valor 127-139; **no retorno a ocorrência está em
  090-91, a data em 092-097, o valor em 160-172 e a data de crédito em
  173-178** — diferente da convenção 109-110/111-116/254-266; motivo de
  rejeição é texto livre em 241-380, sem tabela codificada (não inventada).
  Trailer de remessa informa quantidade de boletos em 002-007.
- **BTG 240 (T1/T2)**: FEBRABAN 10.9, versões `103`/`060`, carteira fixa `1`,
  cadastramento `1`, tipo de documento `2`, emissão/distribuição `1`.
  O portal oficial publica o de-para de retorno 74/85/93; o campo "Chave NFe"
  241-284 da transcrição terceirizada fica além dos 240 bytes e **não é
  gravado**.

### Golden files e round-trip

`tests/Cnab.Tests/Golden/` contém `NNN_cobranca_240.rem` (237, 341, 748, 208),
`077_pagamento_240.rem`, `NNN_cobranca_400.rem` e `NNN_cobranca_400.ret`
(341, 237, 748, 077), todos com CRLF preservado por `.gitattributes`.
`GoldenFilesMilestone3Tests` compara byte a byte a saída dos geradores e o
round-trip `parse(write(x)) == x` dos retornos. LINHA de retorno é preservada
integralmente nos modelos (`LinhaOriginal`, `LinhaHeader`, `LinhaTrailer`) —
isso cobre também bancos cujos cabeçalhos divergem do registro FEBRABAN
genérico (Itaú/Inter), sem reescrever campos não publicados.

## Marco 4 — ferramenta `cnab`, propriedades e desempenho

Nenhuma posição nova foi acrescentada neste marco: a ferramenta de linha de
comando, os testes de propriedade e os benchmarks leem/escrevem **exatamente**
os registros declarativos já citados nas seções anteriores (F1/F3, I1/I2,
B1'/B3/B4, S3/S4, IN1/IN2, T1/T2). Onde a CLI precisa interpretar um campo
para diagnóstico, ela usa os mesmos atributos `CampoCnab` do motor —
`Cnab.Arquivo.MetadadosRegistro` —, sem tabelas paralelas.

### Cobertura efetiva por banco/layout

| Banco | Código | CNAB 240 cobrança | CNAB 240 pagamento | CNAB 400 cobrança |
| --- | --- | --- | --- | --- |
| Banco do Brasil | 001 | remessa + retorno (B1/B2/VE1) | remessa (B2) | — |
| Santander | 033 | remessa (registros próprios, S1) | remessa (S2) | — |
| Caixa | 104 | remessa + retorno (C1/C2) | remessa (C2) | — |
| Bradesco | 237 | remessa + retorno (B3/B4) | remessa (B3) | remessa + retorno (B1') |
| Itaú | 341 | remessa + retorno (I1) | remessa (I1) | remessa + retorno (I2) |
| Sicredi | 748 | remessa + retorno (S3) | remessa (S3) | remessa + retorno (S4) |
| Sicoob | 756 | remessa (CO1 + espelho do XLS) | remessa (CO1) | — |
| Inter | 077 | — (o manual IN2 cobre só pagamentos) | remessa (IN2) | remessa + retorno (IN1) |
| BTG Pactual | 208 | remessa + retorno (T1/T2) | — | — |

### Regras de leitura usadas pela CLI (todas das fontes acima)

- **Detecção do layout**: comprimento do registro — 240 (F1, "Tamanho do
  Registro") ou 400 (manuais I2/B1'/S4/IN1).
- **Detecção do banco**: CNAB 240, posições 1-3 (G001, F1); CNAB 400, posições
  77-79 dos headers implementados (I2/B1'/S4/IN1) — mesma regra já usada por
  `LeitorRetorno400`.
- **Tipo de registro**: CNAB 240, posição 8 (G003, F1); CNAB 400, posição 1
  (I2/B1'/S4/IN1). O segmento CNAB 240 vem da posição 14 (G039, F1).
- **Sentido do arquivo**: CNAB 240, posição 143 (G015, F1); CNAB 400, posição
  2 ("1" remessa, "2" retorno, I2/B1'/S4/IN1). Essa distinção é necessária
  porque o detalhe CNAB 400 usa a posição 2 para o tipo de inscrição.
- **Validação de totais**: trailer de arquivo CNAB 240 — quantidade de lotes
  (G049, 18-23) e registros (G056, 24-29), F1; trailer CNAB 400 Itaú/
  Bradesco/Sicredi — sequencial final (395-400). O trailer do Inter informa
  quantidade de boletos (002-007), não total de registros (IN1), por isso a
  CLI não compara esse campo com a contagem de linhas.
- **Retorno 240 sintético dos testes**: C044 "06" = liquidação (tabela
  FEBRABAN C044 em F1), C047 preenchido com zeros (campo não utilizado) e
  A001 "0000" (sem ocorrência do pagador). Nenhum desses valores é inventado.
- **Valor pago nos retornos 400 sintéticos**: Inter 160-172 (IN1); Itaú,
  Bradesco e Sicredi 254-266 (I2/B1'/S4). Os golden files preservam esses
  números e as propriedades FsCheck apenas trocam o valor por outro vetor
  numérico válido de 13 posições com 2 decimais implícitas.

### Comandos

- `cnab validate <arquivo>` — comprimento, tipo de registro, leitura de todos
  os campos compilados e consistência dos trailers. Erros trazem linha, coluna
  e nome do campo (requisito do SPEC).
- `cnab parse <retorno> [--json]` — usa `LeitorRetorno240`/`LeitorRetorno400` e
  projeta ocorrência, motivos e valores em JSON estável (camelCase); sem
  `--json`, imprime resumo legível.
- `cnab inspect <arquivo> [--bank <código>]` — para cada tipo de registro
  presente, imprime uma tabela com posição 1-based, tipo (numérico,
  alfanumérico, decimal com casas, data), nome e valor, vinda dos atributos
  declarativos.
- `cnab generate <titulos.json> --bank <código> --layout <240|400>` — converte
  um JSON de domínio nos modelos `Remessa240`/`Remessa400` e usa os geradores
  já validados pelos golden files; saída Latin-1, CRLF por padrão.

### Propriedades e desempenho

`PropriedadesTodosLayoutsTests` cobre os nove bancos CNAB 240 e os quatro
CNAB 400 com geradores FsCheck: toda linha escrita tem o comprimento exato,
o retorno parseado é reescrito byte a byte e os trailers refletem contagens e
somatórias dos modelos. `DesempenhoParseUmMilhaoTests` analisa 1.000.000 de
linhas de 240 posições com limite generoso de 60 s, e o projeto
`Cnab.Benchmarks` inclui o benchmark equivalente (`ParseUmMilhao`).

