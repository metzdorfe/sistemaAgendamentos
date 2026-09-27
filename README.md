# Modelagem do Banco – Espaço de Festas

> Proposta para revisão do grupo. Comentem o que não fizer sentido.

## Visão geral

São **10 tabelas**, divididas em três grupos:

| Grupo | Tabelas |
|---|---|
| Cadastros | CLIENTE, SERVICO, SERVICO_FAIXA, ADICIONAL, ESPECIE |
| Agendamento | AGENDAMENTO, ADICIONAL_AGENDAMENTO |
| Financeiro | FORMA_PAGAMENTO_AGENDAMENTO, RECEBER, CAIXA |

## Diagrama ER

```mermaid
erDiagram
    CLIENTE ||--o{ AGENDAMENTO : "faz"
    SERVICO ||--|{ SERVICO_FAIXA : "tem faixas de preço"
    SERVICO ||--o{ AGENDAMENTO : "é contratado em"
    SERVICO_FAIXA ||--o{ AGENDAMENTO : "define o preço de"
    AGENDAMENTO ||--o{ ADICIONAL_AGENDAMENTO : "tem"
    ADICIONAL ||--o{ ADICIONAL_AGENDAMENTO : "é usado em"
    AGENDAMENTO ||--o{ FORMA_PAGAMENTO_AGENDAMENTO : "é pago por"
    ESPECIE ||--o{ FORMA_PAGAMENTO_AGENDAMENTO : "usada em"
    FORMA_PAGAMENTO_AGENDAMENTO ||--|{ RECEBER : "gera parcelas"
    RECEBER ||--o{ CAIXA : "baixa gera"
    ESPECIE ||--o{ CAIXA : "usada em"
    AGENDAMENTO ||--o{ CAIXA : "movimenta"

    CLIENTE {
        int id PK
        string nome
        string endereco
        string telefone
        string cpf "opcional"
        datetime data_cadastro
    }
    SERVICO {
        int id PK
        string nome
        string descricao
        blob imagem "aparece no catálogo"
        bool ativo
    }
    SERVICO_FAIXA {
        int id PK
        int servico_id FK
        string nome "ex: Seg a Qua"
        decimal valor
        int ordem
    }
    ADICIONAL {
        int id PK
        string nome
        string descricao
        decimal valor_venda
        decimal valor_custo
        bool cobra_por_quantidade
        bool ativo
    }
    AGENDAMENTO {
        int id PK
        int cliente_id FK
        int servico_id FK
        int servico_faixa_id FK
        date data
        time hora
        int ordem "posição na lista do dia"
        int qtd_convidados
        char status "O A F C"
        decimal valor_servico
        decimal desconto
        decimal valor_total
        string motivo_cancelamento
        string obs
        datetime data_cadastro
    }
    ADICIONAL_AGENDAMENTO {
        int id PK
        int agendamento_id FK
        int adicional_id FK
        int quantidade
        decimal valor_unitario
        decimal custo_unitario
        decimal valor_total
    }
    ESPECIE {
        int id PK
        string descricao
        decimal taxa_percentual
        decimal desconto_percentual
    }
    FORMA_PAGAMENTO_AGENDAMENTO {
        int id PK
        int agendamento_id FK
        int especie_id FK
        decimal valor
        int qtd_parcelas
    }
    RECEBER {
        int id PK
        int forma_pagamento_agendamento_id FK
        int numero_parcela
        decimal valor
        date vencimento
        decimal valor_recebido
        date data_recebimento
        char status "A P C"
    }
    CAIXA {
        int id PK
        date data
        char tipo "E ou S"
        decimal valor
        int especie_id FK
        int agendamento_id FK "opcional"
        int receber_id FK "opcional"
        string descricao
    }
```

## Dicionário das tabelas

### Cadastros

**CLIENTE**: quem contrata o evento.
- `cpf` é opcional; se preenchido, deve ser único e válido.

**SERVICO**: o serviço principal (o espaço). Não guarda preço.
- `imagem` vai para o catálogo em PDF.

**SERVICO_FAIXA**: as faixas de preço de cada serviço, criadas livremente.
- Ex.: "Seg a Qua – R$ 800", "Qui e Sex – R$ 1.000", "Fim de semana/Feriado – R$ 1.500".
- Não há regra fixa de dia da semana nem de feriado: no agendamento, a faixa é escolhida num radio button.
- `ordem` define a ordem de exibição na tela e no catálogo.

**ADICIONAL**: garçom, comida, decoração, etc.
- `valor_venda` é o que se cobra do cliente; `valor_custo` é o que a empresa paga (ex.: o garçom).
- `cobra_por_quantidade`: se sim, total = quantidade × valor (garçom, comida por pessoa).

**ESPECIE**: formas de pagamento (dinheiro, pix, cartão…).
- `taxa_percentual`: taxa da maquininha, para saber o valor líquido.
- `desconto_percentual`: desconto sugerido (ex.: "à vista no dinheiro, 5%").

### Agendamento

**AGENDAMENTO**: o evento.
- `status`: **O** = orçamento, **A** = agendado, **F** = faturado, **C** = cancelado.
- `valor_servico`: vem da faixa escolhida, mas pode ser alterado.
- `valor_total` = valor_servico + soma dos adicionais − desconto.
- `ordem`: permite reordenar os agendamentos na lista do dia.

**ADICIONAL_AGENDAMENTO**: os adicionais contratados em cada agendamento.
- `valor_unitario` e `custo_unitario` são **copiados** do cadastro no momento do agendamento. Se o preço mudar depois, o agendamento antigo não muda.

### Financeiro

**FORMA_PAGAMENTO_AGENDAMENTO**: o que foi **combinado**.
- Ex.: R$ 500 no pix (1x) + R$ 1.200 no dinheiro (3x).
- A soma das formas tem de fechar com o `valor_total` do agendamento.

**RECEBER**: as **parcelas** geradas a partir de cada forma.
- `status`: **A** = aberto, **P** = pago, **C** = cancelado.

**CAIXA**: o que **entrou ou saiu de fato**.
- Entrada da baixa de parcela: `receber_id` e `agendamento_id` preenchidos.
- Saída do evento (pagar garçom, devolver sinal): só `agendamento_id`.
- Saída avulsa: nenhum dos dois.

## Regras de negócio que afetam o banco

1. **Valores congelados:** o agendamento guarda os valores praticados; mudar o cadastro não altera eventos já lançados.
2. **Nada é excluído se já foi usado:** clientes, serviços e adicionais usados em agendamento são inativados.
3. **Disponibilidade:** agendamentos não cancelados e não orçamento no mesmo dia não podem passar do máximo configurado.
4. **Orçamento não bloqueia a data** no calendário.
5. **Cancelamento:** parcelas em aberto viram canceladas; o que já entrou continua no caixa. Devolução de sinal = saída no CAIXA.
6. **Estorno de baixa:** reabre a parcela no RECEBER e lança uma saída no CAIXA. Nada é apagado.
7. **Valor pago e saldo não são gravados:** são calculados a partir do CAIXA e do RECEBER.

## Fora do banco (arquivo `appsettings.json`)

- Máximo de agendamentos por dia.
- Dados da empresa para o PDF: nome, telefone, endereço e caminho da logo.



# Fluxo do Sistema – Espaço de Festas

> Como o sistema vai funcionar, tela por tela. Proposta para revisão do grupo.

## Mapa das telas

```mermaid
flowchart TD
    MENU[Menu principal]
    MENU --> CAL[Calendário de agendamentos]
    MENU --> CAD[Cadastros]
    MENU --> FIN[Financeiro]
    MENU --> REL[Relatórios]
    MENU --> CAT[Gerar catálogo PDF]

    CAD --> C1[Clientes]
    CAD --> C2[Serviços + faixas de preço]
    CAD --> C3[Adicionais]
    CAD --> C4[Espécies de pagamento]

    CAL --> DIA[Lista do dia]
    DIA --> AG[Tela de agendamento]
    AG --> FAT[Tela de faturamento]

    FIN --> RC[Contas a receber / baixa de parcelas]
    FIN --> CX[Caixa: entradas e saídas]

    REL --> R1[Relatório de agendamentos]
    REL --> R2[Relatório de caixa por cliente]
```

## 1. Calendário (tela principal)

- Visão do mês, com cada dia colorido:
  - **livre**;
  - **com agendamento**;
  - **lotado** (atingiu o máximo do dia);
  - **orçamento** com outra cor, sem bloquear a data.
- Clique no dia → abre a **lista do dia**.

## 2. Lista do dia

- Agendamentos daquele dia: hora, cliente, status e valor.
- Setas para **subir e descer** a ordem dos agendamentos.
- Botões: **Novo**, **Abrir**, **Cancelar**.
- **Novo** fica desabilitado se o dia estiver lotado.

## 3. Tela de agendamento

```mermaid
flowchart TD
    A[Novo agendamento] --> B[Escolhe cliente]
    B --> C[Informa hora e nº de convidados]
    C --> D[Escolhe o serviço principal]
    D --> E[Aparecem as faixas do serviço<br/>em radio button]
    E --> F[Marca a faixa → valor preenchido<br/>e editável]
    F --> G[Lista de adicionais ativos:<br/>marca, informa quantidade,<br/>ajusta valor]
    G --> H[Total calculado na hora]
    H --> I{Tem sinal?}
    I -- Sim --> J[Informa valor e espécie do sinal]
    I -- Não --> K{O que fazer?}
    J --> K
    K -- Salvar orçamento --> L[Status: Orçamento<br/>não bloqueia a data]
    K -- Agendar --> M[Status: Agendado]
    K -- Agendar e faturar --> N[Abre tela de faturamento]
```

**O que acontece com o sinal:** vira uma forma de pagamento de 1 parcela já paga e gera uma **entrada no caixa** na hora.

**Adicionais:**
- Garçom e comida (por pessoa) pedem quantidade.
- Decoração é valor fixo.
- O sistema também mostra o **custo** dos adicionais, para ver o lucro do evento.

## 4. Tela de faturamento

Pode ser aberta na hora do agendamento ou depois, a partir dele.

```mermaid
flowchart TD
    A[Faturamento] --> B[Mostra total dos serviços<br/>e sinal já pago]
    B --> C[Escolhe espécie e nº de parcelas]
    C --> D{Espécie tem desconto?}
    D -- Sim --> E[Sistema sugere o desconto<br/>editável]
    D -- Não --> F[Sem desconto automático]
    E --> G[Pode adicionar outra forma<br/>ex: parte pix, parte dinheiro]
    F --> G
    G --> H{Soma das formas = valor a faturar?}
    H -- Não --> C
    H -- Sim --> I[Gera parcelas no Receber<br/>vencimentos editáveis]
    I --> J[Status: Faturado]
```

## 5. Recebimento (baixa de parcelas)

```mermaid
flowchart LR
    A[Parcela em aberto] -->|Cliente paga| B[Baixa]
    B --> C[Parcela: Paga]
    B --> D[Entrada no Caixa]
    C -->|Baixa por engano| E[Estorno]
    E --> F[Parcela volta a Aberta]
    E --> G[Saída no Caixa]
```

## 6. Status do agendamento

```mermaid
stateDiagram-v2
    [*] --> Orcamento: Salvar orçamento
    [*] --> Agendado: Agendar
    Orcamento --> Agendado: Cliente confirmou
    Agendado --> Faturado: Faturar
    Orcamento --> Cancelado
    Agendado --> Cancelado
    Faturado --> Cancelado
    Cancelado --> [*]
```

**No cancelamento:**
- Informa o motivo.
- Parcelas em aberto são canceladas.
- O que já foi pago continua no caixa. Se devolver o sinal, lança uma saída no caixa.

## 7. Cadastros

| Tela | O que tem |
|---|---|
| Clientes | nome, endereço, telefone, CPF (opcional) |
| Serviços | nome, descrição, imagem e uma grade de **faixas** (nome livre + valor + ordem) |
| Adicionais | nome, descrição, valor de venda, valor de custo, cobra por quantidade |
| Espécies | descrição, taxa (%), desconto (%) |

Nenhum cadastro já usado é excluído: ele é **inativado**.

## 8. Catálogo em PDF

- Puxa direto dos cadastros, então sai sempre com o preço atual.
- Cabeçalho com logo e dados da empresa.
- Formato:

```
SALÃO DE FESTAS                    [imagem]
  Seg a Qua ................ R$   800,00
  Qui e Sex ................ R$ 1.000,00
  Fim de semana/Feriado .... R$ 1.500,00

ADICIONAIS
  • Decoração simples ...... R$   400,00
  • Buffet (por pessoa) .... R$    45,00
  • Garçom (por unidade) ... R$   150,00
```

## 9. Relatórios

- **Agendamentos:** filtro por período e status; mostra cliente, serviço, total, pago e saldo.
- **Caixa por cliente:** entradas agrupadas por cliente, com total recebido e pendente.

## 10. Configurações (`appsettings.json`)

- Máximo de agendamentos por dia.
- Dados da empresa e logo para o PDF.