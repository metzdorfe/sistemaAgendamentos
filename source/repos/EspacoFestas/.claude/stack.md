# Stack e Configuração do Projeto – Espaço de Festas
 
> **Criado em 28/09/2026.** Referência técnica do projeto: tecnologias, pacotes, criação da solução, organização de pastas, convenções e configuração. Regras de negócio no `fluxo-sistema.md`; banco no `modelagem-banco.md`.
 
## Stack
 
| Item | Decisão | Observação |
|---|---|---|
| Plataforma | **.NET 10** | Obrigatório. |
| Interface | **WPF** | Detalhes visuais a definir. |
| Banco | **Firebird 5** | |
| Acesso ao banco | **Entity Framework Core 10** | Provider oficial do Firebird para EF Core 10. |
| PDF | **QuestPDF** | Conferir as condições da licença Community antes da entrega. |
| Configuração técnica | **config.ini** | Lido pelo `Microsoft.Extensions.Configuration.Ini`. |
| Versionamento | **Git** | |
| IDE | **Visual Studio** | |
 
## Pacotes NuGet
 
| Pacote | Para quê |
|---|---|
| `FirebirdSql.EntityFrameworkCore.Firebird` | Provider do EF Core para Firebird (versão compatível com EF Core 10). Já traz o `FirebirdSql.Data.FirebirdClient`. |
| `Microsoft.EntityFrameworkCore.Design` | Ferramentas do EF (migrations/scaffold), se forem usadas. |
| `Microsoft.Extensions.Configuration.Ini` | Leitura do `config.ini`. |
| `QuestPDF` | Geração do catálogo em PDF. |
 
Instalar sempre a versão estável mais recente compatível com .NET 10.
 
## Criação da solução no Visual Studio
 
1. Novo projeto → **Aplicativo WPF** (o que **não** tem "(.NET Framework)" no nome).
2. Informar o nome do projeto.
3. Em **Framework**, escolher **.NET 10.0**. Se não aparecer, instalar o SDK do .NET 10 ou atualizar o Visual Studio.
> Os templates com "(.NET Framework)" usam a plataforma antiga (4.x) e não rodam .NET 10 nem o EF Core atual.
 
## Organização de pastas
 
Projeto único, separado em pastas:
 
```
EspacoFestas/
├── Models/        entidades (Cliente, Agendamento, Receber...)
├── Data/          DbContext, mapeamentos do EF, convenção de nomes
├── Services/      regras de negócio (disponibilidade, faturamento, baixa, cancelamento)
├── Views/         telas WPF (XAML)
├── Reports/       geração de PDF (catálogo)
├── config.ini     conexão e dados da empresa (não versionado)
└── App.xaml
```
 
- Se o grupo preferir separar em camadas mais tarde, entidades e acesso ao banco podem ir para uma **Biblioteca de Classes** comum (não a WPF).
- Uso de MVVM (pasta `ViewModels/`): a definir junto com os detalhes visuais.
## Convenções de nomes
 
| Onde | Padrão | Exemplo |
|---|---|---|
| Banco (tabelas e campos) | MAIÚSCULO_COM_UNDERSCORE, por extenso, sem aspas | `SERVICO_AGENDAMENTO`, `VALOR_PARCELA` |
| C# (classes e propriedades) | PascalCase | `ServicoAgendamento`, `ValorParcela` |
| C# (variáveis e parâmetros) | camelCase | `servicoAgendamento`, `valorParcela` |
 
- **Mapeamento no EF:** por padrão o EF usaria o nome da propriedade como nome da coluna. É preciso configurar no `OnModelCreating` uma convenção que converta PascalCase para MAIÚSCULO_COM_UNDERSCORE em todas as entidades (ou `ToTable`/`HasColumnName` por entidade).
- Detalhes do banco (PK `ID`, FK `<TABELA>_ID`, prefixos `DATA_`, `VALOR_`, `PERCENTUAL_`, `QUANTIDADE_`) no `modelagem-banco.md`.
## Configuração
 
**Técnica → `config.ini`** (fora do banco):
- Caminho / conexão do banco de dados.
- Dados da empresa para o PDF: nome, telefone, endereço e caminho da logo.
**De negócio → tabela CONFIGURACAO** (no banco, alterada pela tela de Configurações):
- Um agendamento por dia (bloqueio por serviço).
- Juros ao mês (%).
- Multa (%) — padrão sugerido 2%.
- Prazo de cancelamento (dias).
## Git
 
- `.gitignore` padrão do Visual Studio (ignora `bin/`, `obj/`, `.vs/`).
- **Não versionar** o arquivo do banco (`.fdb`) nem o `config.ini` real, que tem o caminho da máquina de cada um. Versionar um `config.exemplo.ini` com os campos preenchidos de exemplo.
- Versionar os scripts SQL do banco (DDL, domínios, `COMMENT ON`) numa pasta própria, ex.: `Database/`.
- Fluxo de branches e divisão de tarefas: a definir pelo grupo.
## Pontos em aberto
 
- [ ] Detalhes visuais da interface WPF (e uso de MVVM).
- [ ] Conferir a licença Community do QuestPDF.
- [ ] Fluxo de branches e divisão de tarefas no Git.
- [ ] Backup: fora do escopo por enquanto.