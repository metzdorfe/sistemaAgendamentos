# Modelagem do Banco – Espaço de Festas
 
> **Modelo fechado pelo grupo em 27/09/2026**, com base no conceitual (Chen, brModelo) validado entidade por entidade, relacionamento por relacionamento.
> **Atualizado em 28/09/2026:** Firebird 5 + Entity Framework Core; nomes por extenso no padrão do Firebird; tabela CONFIGURACAO; juros/multa e parcela de diferença no RECEBER; `ATIVO` em CLIENTE e ESPECIE; data de cancelamento. As regras de negócio completas (RN01–RN27) estão no `fluxo-sistema.md`.
 
## Visão geral
 
São **12 tabelas**, divididas em quatro grupos:
 
| Grupo | Tabelas |
|---|---|
| Cadastros | CLIENTE, SERVICO, SERVICO_FAIXA, ADICIONAL, SERVICO_ADICIONAL, ESPECIE |
| Agendamento | AGENDAMENTO, SERVICO_AGENDAMENTO, ADICIONAL_AGENDAMENTO |
| Financeiro | RECEBER, FINANCEIRO |
| Configuração | CONFIGURACAO |
 
## Convenção de nomes
 
| Onde | Padrão | Exemplo |
|---|---|---|
| Banco (tabelas e campos) | MAIÚSCULO, palavras separadas por `_`, **por extenso**, sem aspas | `SERVICO_AGENDAMENTO`, `VALOR_PARCELA`, `DATA_VENCIMENTO` |
| C# (classes e propriedades) | PascalCase | `ServicoAgendamento`, `ValorParcela` |
| C# (variáveis e parâmetros) | camelCase | `servicoAgendamento`, `valorParcela` |
 
- O banco fica no padrão natural do Firebird: sem aspas, o SQL manual (IBExpert, scripts, relatórios) funciona sem cuidado extra.
- O Entity Framework Core faz a ponte entre os dois padrões. Isso **precisa ser configurado**: por padrão o EF usaria o nome da propriedade (`ValorParcela`) como nome da coluna. A saída é uma convenção única no `OnModelCreating` que converte PascalCase para MAIÚSCULO_COM_UNDERSCORE em todas as entidades (ou `ToTable`/`HasColumnName` por entidade).
- Chave primária sempre `ID`; chave estrangeira sempre `<TABELA>_ID` (ex.: `CLIENTE_ID`).
- Datas começam com `DATA_`, valores com `VALOR_`, percentuais com `PERCENTUAL_`, quantidades com `QUANTIDADE_`.
 
## Dicionário das tabelas
 
### Cadastros
 
**CLIENTE**: quem contrata o evento.
- `CPF` é opcional; se preenchido, deve ser único e válido.
- Endereço separado em `ENDERECO`, `CIDADE` e `UF`.
- `ATIVO`: cliente com agendamento não é excluído, é inativado (RN02).
**SERVICO**: o serviço principal (o espaço). Não guarda preço.
 
**SERVICO_FAIXA**: as faixas de preço de cada serviço, criadas livremente (ex.: "Seg a Qua", "Fim de semana/Feriado", "Promoção de setembro").
- Sem regra de dia da semana no banco: no agendamento, ela escolhe a faixa num radio button.
- `ORDEM_EXIBICAO` define a ordem na tela e no catálogo.
- `ATIVO`: faixa usada em agendamento não é excluída, é inativada. Serve também para promoções: cria a faixa, usa enquanto durar, depois inativa.
**ADICIONAL**: garçom, comida, decoração, etc.
- `VALOR_VENDA` é o que se cobra do cliente; `VALOR_CUSTO` é o que a empresa paga (ex.: o garçom).
- `COBRA_POR_QUANTIDADE`: se verdadeiro, total = quantidade × valor (garçom, comida por pessoa).
**SERVICO_ADICIONAL**: quais adicionais cada serviço oferece. Tabela N:N pura, sem atributo próprio, PK composta (`SERVICO_ID`, `ADICIONAL_ID`).
- Um adicional pode servir a vários serviços (garçom serve qualquer espaço); prepara o sistema pra quando houver mais de um serviço/espaço, cada um com sua lista.
**ESPECIE**: formas de pagamento (dinheiro, pix, cartão…).
- `PERMITE_PARCELAMENTO`: se essa espécie aceita mais de 1 parcela (ex.: cartão de crédito sim, dinheiro não).
- `ATIVO`: espécie usada não é excluída, é inativada (RN02).
- Taxa da maquininha e desconto automático por espécie ficaram fora por enquanto (ver "Pontos em aberto").
### Agendamento
 
**AGENDAMENTO**: o evento.
- `STATUS`: **O** = orçamento, **A** = agendado, **F** = faturado, **C** = cancelado.
- `VALOR_TOTAL` = soma dos serviços + soma dos adicionais − desconto.
- `VALOR_DESCONTO`: manual, só pode ser informado até o faturamento.
- `DATA_CANCELAMENTO` e `MOTIVO_CANCELAMENTO`: obrigatórios ao cancelar e vazios nos demais casos (CHECK). O cancelamento só pode ocorrer **até a data do evento, inclusive** (CHECK `DATA_CANCELAMENTO <= DATA_EVENTO`).
- **Faturado é imutável:** itens, valores, desconto e formas de pagamento não mudam mais; a única ação é cancelar (garantido pela aplicação).
- **Orçamento com sinal:** é o orçamento que tem entrada no FINANCEIRO ligada a ele. Não precisa de campo próprio.
**SERVICO_AGENDAMENTO**: os serviços principais de cada agendamento.
- Hoje, na prática, cada agendamento terá um serviço, mas o modelo já aceita vários (ex.: salão + área externa).
- `VALOR_UNITARIO` vem da faixa escolhida e pode ser alterado.
**ADICIONAL_AGENDAMENTO**: os adicionais contratados em cada agendamento.
- Só aparecem, na tela, os adicionais vinculados (via SERVICO_ADICIONAL) ao(s) serviço(s) escolhido(s) nesse agendamento.
- `VALOR_UNITARIO` e `VALOR_CUSTO_UNITARIO` são **copiados** do cadastro no momento do agendamento. Se o preço mudar depois, o agendamento antigo não muda.
- O total da linha não é gravado: é `QUANTIDADE × VALOR_UNITARIO`.
### Financeiro
 
**RECEBER**: as parcelas **ainda não pagas**, com vencimento no futuro.
- Liga direto no `AGENDAMENTO_ID` e no `ESPECIE_ID` (a espécie **combinada**).
- `STATUS`: **A** = aberto, **P** = pago, **C** = cancelado.
- `VALOR_JUROS`, `VALOR_MULTA`, `VALOR_RECEBIDO` e `DATA_RECEBIMENTO` são preenchidos na baixa. `VALOR_RECEBIDO` é o total que entrou (parcela + multa + juros, ou menos, se for pagamento parcial).
- `PARCELA_ORIGEM_ID`: preenchido só na parcela criada para cobrar a diferença de uma baixa parcial; aponta para a parcela original.
- **Só existe quando tem algo pendente.** Pagamento feito na hora (à vista ou sinal) não passa por aqui — vai direto pro FINANCEIRO.
**Cálculo de juros e multa (na baixa de parcela vencida):**
```
juros_dia   = VALOR_PARCELA × PERCENTUAL_JUROS_MES ÷ 100 ÷ 30
VALOR_JUROS = juros_dia × dias_de_atraso
VALOR_MULTA = VALOR_PARCELA × PERCENTUAL_MULTA ÷ 100
total       = VALOR_PARCELA + VALOR_MULTA + VALOR_JUROS
```
Exemplo: parcela R$ 1.000,00, juros 2% a.m., multa 2%, 15 dias de atraso → juros R$ 10,00, multa R$ 20,00, total R$ 1.030,00. Mês comercial de 30 dias.
 
**Baixa parcial:** se `VALOR_RECEBIDO` for menor que o total, o sistema pergunta se deseja criar uma nova parcela com a diferença (novo vencimento, `PARCELA_ORIGEM_ID` = parcela original). Em qualquer caso a parcela original fica **P**; se a resposta for não, a diferença é perdoada.
 
**FINANCEIRO**: o que **entrou ou saiu de fato** ("caixa"). Duas FKs opcionais, no máximo uma preenchida:
 
| Lançamento | RECEBER_ID | AGENDAMENTO_ID |
|---|---|---|
| Baixa de parcela, estorno de baixa | preenchido | vazio |
| Pagamento à vista, sinal, despesa do evento, devolução, estorno de à vista/sinal | vazio | preenchido |
| Lançamento avulso (compra de material) | vazio | vazio |
 
- `ESPECIE_ID` é sempre preenchido (a espécie **real**, o que aconteceu de fato — pode ser diferente da combinada em RECEBER).
### Configuração
 
**CONFIGURACAO**: parâmetros de negócio que ela ajusta pela tela de configurações. Tabela de **uma linha só** (`ID = 1`, garantido por CHECK), com um campo por parâmetro, para cada configuração ter nome e tipo próprios.
- `BLOQUEIO_UM_POR_DIA`: se verdadeiro, cada serviço só pode estar em um agendamento por dia (ver regra 4).
- `PERCENTUAL_JUROS_MES`: juros simples ao mês por atraso.
- `PERCENTUAL_MULTA`: multa por atraso sobre o valor da parcela. Sugestão de padrão: 2% (limite usual do CDC para consumidor; confirmar com quem entende da parte jurídica).
- `PRAZO_CANCELAMENTO_DIAS`: cancelamento feito até esse número de dias antes do evento tem devolução.
## Restrições (CHECK)
 
O banco guarda só as restrições **estruturais e fixas**. Regras que dependem de configuração (disponibilidade, prazo de devolução) e a imutabilidade do faturado são validadas na aplicação.
 
```sql
-- FINANCEIRO: no máximo uma origem
ALTER TABLE FINANCEIRO ADD CONSTRAINT CK_FINANCEIRO_ORIGEM
  CHECK (RECEBER_ID IS NULL OR AGENDAMENTO_ID IS NULL);
 
-- AGENDAMENTO: data e motivo obrigatórios só no cancelamento
ALTER TABLE AGENDAMENTO ADD CONSTRAINT CK_AGENDAMENTO_CANCELAMENTO
  CHECK ((STATUS = 'C' AND DATA_CANCELAMENTO IS NOT NULL AND MOTIVO_CANCELAMENTO IS NOT NULL)
      OR (STATUS <> 'C' AND DATA_CANCELAMENTO IS NULL AND MOTIVO_CANCELAMENTO IS NULL));
 
-- AGENDAMENTO: cancelamento só até a data do evento, inclusive (RN27)
ALTER TABLE AGENDAMENTO ADD CONSTRAINT CK_AGENDAMENTO_DATA_CANCELAMENTO
  CHECK (DATA_CANCELAMENTO IS NULL OR DATA_CANCELAMENTO <= DATA_EVENTO);
 
-- CONFIGURACAO: uma linha só
ALTER TABLE CONFIGURACAO ADD CONSTRAINT CK_CONFIGURACAO_UNICA CHECK (ID = 1);
```
 
Status e tipos ficam garantidos pelos domínios (ver "Tipos e domínios").
 
## Regras de negócio que afetam o banco
 
A lista completa e numerada está no `fluxo-sistema.md`. As que mexem na estrutura ou nos dados:
 
1. **Valores congelados (RN08):** o agendamento guarda os valores praticados; mudar o cadastro não altera eventos já lançados.
2. **Nada é excluído se já foi usado (RN02, RN25):** cadastros usados são inativados; histórico financeiro nunca é apagado.
3. **Todo agendamento tem pelo menos um serviço (RN05).**
4. **Disponibilidade configurável (RN10, RN11):** com `BLOQUEIO_UM_POR_DIA` ligado, um serviço só pode estar em um agendamento por dia com status A, F, ou O **com sinal**. Orçamento sem sinal não bloqueia. Validado na aplicação.
5. **Adicionais oferecidos no agendamento** são só os vinculados, via SERVICO_ADICIONAL, ao(s) serviço(s) escolhidos (RN07).
6. **RECEBER só existe pra dívida futura (RN12, RN16).** Pagamento feito na hora (à vista, sinal) grava direto em FINANCEIRO, ligado ao agendamento.
7. **Espécie é só referenciada**, nunca pelo agendamento direto: o mesmo agendamento pode usar várias espécies ao mesmo tempo.
8. **Faturado é imutável (RN17):** só pode ser cancelado.
9. **Juros, multa e baixa parcial (RN18, RN19):** ver cálculo em RECEBER.
10. **Estorno de baixa (RN21):** reabre a parcela (limpa juros, multa, valor e data recebidos), lança saída no FINANCEIRO ligada à parcela e cancela a parcela de diferença, se houver.
11. **Estorno de à vista ou sinal (RN26):** saída no FINANCEIRO ligada ao agendamento.
12. **Cancelamento (RN22–RN24, RN27):** só até a data do evento; exige data e motivo; parcelas em aberto viram canceladas e não são cobradas; se feito até `PRAZO_CANCELAMENTO_DIAS` antes do evento, tudo que foi pago (sinal e parcelas) é devolvido como saída no FINANCEIRO ligada ao agendamento; depois do prazo, não há devolução.
## Decisões de modelagem (justificativas)
 
- **Sem FORMA_PAGAMENTO_AGENDAMENTO.** Existia pra guardar "o que foi combinado", mas tudo que ela tinha (agendamento, espécie, valor, parcelas) já está no RECEBER. Manter as duas era redundância.
- **RECEBER só guarda dívida futura, não histórico de tudo que já foi pago.** Uma parcela que nasce e já é baixada no mesmo instante (pagamento à vista) não precisa existir — vai direto pro FINANCEIRO.
- **SERVICO_ADICIONAL existe mesmo com um serviço só hoje**, porque prepara o sistema pra quando houver um segundo espaço com adicionais próprios, sem precisar redesenhar nada.
- **FINANCEIRO com duas FKs opcionais em vez de um campo genérico de origem.** Um campo "módulo + id" apontando para tabelas diferentes (*polymorphic association*) não permite FK, e a integridade teria de ser simulada com triggers. Duas FKs mantêm a integridade no próprio banco — o preço é o CHECK garantindo que nunca as duas vêm preenchidas juntas, algo que a cardinalidade do ER sozinha não expressa.
- **`VALOR_TOTAL` no AGENDAMENTO é uma desnormalização consciente.** Poderia ser calculado, mas o calendário e os relatórios leem esse valor o tempo todo. A aplicação recalcula e grava sempre que os itens mudam (só até faturar).
- **`STATUS` no RECEBER**, pelo mesmo motivo: filtrar as contas em aberto sem calcular.
- **Total da linha em ADICIONAL_AGENDAMENTO não é gravado**, porque é uma conta simples feita na hora.
- **`DATA_CANCELAMENTO` gravada** para o histórico e para permitir o CHECK de "só até a data do evento" direto no banco — é uma regra fixa, não configurável.
- **Juros e multa gravados separados em RECEBER** (e não só somados no `VALOR_RECEBIDO`), para relatórios e para o banco ser autoexplicativo.
- **`PARCELA_ORIGEM_ID` em vez de reaproveitar a parcela original** na baixa parcial: a parcela paga fica fechada com o que realmente entrou, e a diferença tem vencimento e status próprios, mantendo o rastro.
- **CONFIGURACAO com uma coluna por parâmetro, em vez de chave/valor.** Cada parâmetro tem nome e tipo definidos no banco, o que facilita leitura e manutenção.
- **Regras configuráveis ficam fora do banco.** Bloqueio por dia e prazo de devolução podem ser ligados/alterados pelo usuário; por isso são validados na aplicação, não por trigger ou índice.
- **Nomes do banco no padrão do Firebird (MAIÚSCULO_COM_UNDERSCORE, sem aspas)** e PascalCase no C#, com o EF fazendo o mapeamento. camelCase no banco exigiria aspas em todo SQL manual.
## Tipos e domínios
 
**Tipos (Firebird 5):**
- IDs como INTEGER; no FINANCEIRO, BIGINT.
- Dinheiro como NUMERIC(18,2) (gravado como inteiro de 64 bits); percentuais como NUMERIC(5,2).
- Flags (ativo, permite parcelamento, etc.) como **BOOLEAN** nativo: o EF mapeia direto para `bool`, sem conversor.
- Status e tipo de lançamento como CHAR(1).
- DATE quando não precisa de hora, não TIMESTAMP.
- VARCHAR com tamanho justo.
**Domínios propostos** (definidos uma vez, usados em todas as tabelas):
```sql
CREATE DOMAIN D_VALOR              AS NUMERIC(18,2) DEFAULT 0 NOT NULL;
CREATE DOMAIN D_PERCENTUAL         AS NUMERIC(5,2)  DEFAULT 0 NOT NULL;
CREATE DOMAIN D_ATIVO              AS BOOLEAN DEFAULT TRUE NOT NULL;
CREATE DOMAIN D_STATUS_AGENDAMENTO AS CHAR(1) NOT NULL CHECK (VALUE IN ('O','A','F','C'));
CREATE DOMAIN D_STATUS_RECEBER     AS CHAR(1) NOT NULL CHECK (VALUE IN ('A','P','C'));
CREATE DOMAIN D_TIPO_LANCAMENTO    AS CHAR(1) NOT NULL CHECK (VALUE IN ('E','S'));
```
 
**Índices:**
- Em todas as FKs (o Firebird cria sozinho).
- AGENDAMENTO(DATA_EVENTO), para o calendário.
- RECEBER(STATUS, DATA_VENCIMENTO), para o contas a receber.
- FINANCEIRO(DATA_LANCAMENTO), para o relatório de caixa.
- CLIENTE(NOME), para busca.
**CPF único mas opcional:** no Firebird, índice único comum já aceita vários nulos.
 
**Autodocumentação:** toda tabela e todo campo recebem `COMMENT ON`, para o significado ficar no próprio banco.
 
## Fora do banco (arquivo `config.ini`)
 
Só configurações **técnicas**:
- Caminho / conexão do banco de dados.
- Dados da empresa para o PDF: nome, telefone, endereço e caminho da logo.
No .NET, o `.ini` é lido pelo pacote gratuito `Microsoft.Extensions.Configuration.Ini`. Parâmetros de negócio (juros, multa, prazo, bloqueio) ficam na tabela CONFIGURACAO.
 
## Pontos em aberto
 
- [x] Banco: **Firebird 5**, acesso via **Entity Framework Core** (`FirebirdSql.EntityFrameworkCore.Firebird`).
- [x] Nomes por extenso, padrão do Firebird no banco e PascalCase no C#.
- [x] Flags como BOOLEAN nativo.
- [ ] Tamanhos exatos dos VARCHAR (definir no DDL).
- [ ] Taxa da maquininha e desconto automático por espécie ficaram fora da v1 (decisão consciente); hoje o desconto é sempre manual, no campo `VALOR_DESCONTO` do agendamento.