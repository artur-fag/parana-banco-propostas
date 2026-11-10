# Paraná Banco | Microsserviço de Propostas

Microsserviço responsável pela análise de crédito de clientes, desenvolvido em **.NET 10**, com **Clean Architecture**, **SQL Server**, **Entity Framework Core**, **RabbitMQ** e **xUnit**.

Este projeto faz parte do desafio técnico Backend do Paraná Banco.

## 1. Responsabilidades

- Consumir o evento `ClienteCadastrado` publicado pelo microsserviço Clientes.
- Calcular um score de crédito determinístico.
- Gerar e armazenar a proposta de crédito.
- Publicar o evento `PropostaGerada` utilizando o padrão Transactional Outbox.
- Disponibilizar uma API REST para consulta de propostas.
- Implementar mecanismos de resiliência com retry e dead-letter queue (DLQ).

## 2. Tecnologias

- .NET 10 / ASP.NET Core Web API
- C#
- SQL Server
- Entity Framework Core
- RabbitMQ
- Docker
- xUnit
- Clean Architecture
- Transactional Outbox

## 3. Arquitetura

A solução está organizada em quatro camadas:

| Projeto | Responsabilidade |
|---|---|
| Propostas.Domain | Entidades e regras de negócio |
| Propostas.Application | Casos de uso, handlers, interfaces e cálculo de score |
| Propostas.Infrastructure | EF Core, SQL Server, RabbitMQ, repositórios e workers |
| Propostas.API | Endpoints REST e configuração da aplicação |

Os testes estão organizados nos projetos `Propostas.UnitTests` e `Propostas.IntegrationTests`.

## 4. Regras de crédito

| Score | Status | Cartões previstos | Limite por cartão |
|---|---|---|---|
| 0 a 100 | Negada | 0 | R$ 0 |
| 101 a 500 | Aprovada | 1 | R$ 1.000 |
| 501 a 1000 | Aprovada | 2 | R$ 5.000 |

O score é calculado de maneira determinística a partir do CPF, utilizando SHA-256 e produzindo um valor entre 0 e 1000.

Esse cálculo é uma simulação para o desafio técnico, não um modelo real de avaliação de risco de crédito.

## 5. Fluxo de processamento

1. O microsserviço Clientes publica `ClienteCadastrado`.
2. O RabbitMQ encaminha o evento para a fila de Propostas.
3. O consumer `ClienteCadastradoConsumer` recebe a mensagem.
4. O handler verifica se já existe proposta para o cliente.
5. O `ScoreService` calcula o score.
6. A proposta e o evento de saída são persistidos na mesma transação do banco.
7. O `PropostasOutboxWorker` publica `PropostaGerada` no RabbitMQ.
8. O microsserviço Cartões consome o evento e realiza a emissão, quando aplicável.

## 6. Resiliência e consistência

A solução utiliza:

- **Transactional Outbox:** evita perder o evento de saída quando a proposta é salva.
- **Idempotência:** evita criar propostas duplicadas para o mesmo cliente.
- **Retry:** permite novas tentativas de processamento.
- **Dead-letter queue:** recebe mensagens que não puderam ser processadas.
- **Publisher confirms:** confirma a publicação das mensagens no RabbitMQ.

O processamento é assíncrono e utiliza consistência eventual.

## 7. Pré-requisitos

- .NET SDK 10
- Docker Desktop
- SQL Server 2022
- RabbitMQ
- Git

O ambiente de infraestrutura pode ser iniciado utilizando o `compose.yaml` disponível no repositório Clientes.

## 8. Configuração local

Na pasta do microsserviço Clientes, configure um arquivo `.env` com as variáveis exigidas pelo Docker Compose:

```dotenv
MSSQL_SA_PASSWORD=defina_uma_senha_forte
RABBITMQ_USER=defina_um_usuario
RABBITMQ_PASSWORD=defina_uma_senha_forte
```

Não publique esse arquivo no GitHub.

Inicie os containers:

```powershell
docker compose up -d
```

No projeto Propostas, configure as credenciais locais utilizando User Secrets:

```powershell
dotnet user-secrets init --project src/Propostas.API
```

```powershell
dotnet user-secrets set "ConnectionStrings:PropostasDb" "Server=localhost,1434;Database=ParanaBancoPropostas;User Id=sa;Password=SUA_SENHA_SQL;TrustServerCertificate=True" --project src/Propostas.API
```

```powershell
dotnet user-secrets set "RabbitMQ:HostName" "localhost" --project src/Propostas.API
dotnet user-secrets set "RabbitMQ:Port" "5672" --project src/Propostas.API
dotnet user-secrets set "RabbitMQ:UserName" "SEU_USUARIO" --project src/Propostas.API
dotnet user-secrets set "RabbitMQ:Password" "SUA_SENHA" --project src/Propostas.API
```

Substitua os valores de exemplo pelas credenciais configuradas localmente.

## 9. Banco de dados

A aplicação utiliza o banco `ParanaBancoPropostas`, com migrations do Entity Framework Core.

Para aplicar as migrations:

```powershell
dotnet ef database update --project src/Propostas.Infrastructure --startup-project src/Propostas.API
```

É necessário ter a ferramenta `dotnet-ef` instalada e a configuração do banco disponível para o processo de design-time.

## 10. Executar a API

Na raiz do projeto:

```powershell
dotnet restore Propostas.slnx
dotnet build Propostas.slnx
dotnet run --project src/Propostas.API --urls http://localhost:5150
```

A API ficará disponível em:

`http://localhost:5150`

O consumer RabbitMQ e o Outbox Worker são iniciados junto com a aplicação.

## 11. Endpoint REST

### Consultar proposta por cliente

```http
GET /api/v1/propostas/{clienteId}
```

Exemplo:

```powershell
curl.exe -i http://localhost:5150/api/v1/propostas/0773264e-528c-496c-aa9a-6508d5ac86b0
```

Respostas:

- `200 OK`: proposta encontrada.
- `400 Bad Request`: identificador inválido.
- `404 Not Found`: proposta não encontrada.

A criação da proposta ocorre pelo consumo do evento RabbitMQ, não por um POST nesta API.

## 12. Testes automatizados

```powershell
dotnet test Propostas.slnx
```

Os testes unitários verificam as regras de negócio e os componentes de aplicação.

## 13. Repositórios relacionados

- [Clientes](https://github.com/artur-fag/parana-banco-clientes)
- [Propostas](https://github.com/artur-fag/parana-banco-propostas)
- [Cartões](https://github.com/artur-fag/parana-banco-cartoes)

## 14. Decisões técnicas

A arquitetura orientada a eventos permite desacoplar o cadastro, a análise de crédito e a emissão de cartões.

O Transactional Outbox reduz o risco de inconsistência entre a persistência da proposta e a publicação do evento. A idempotência protege contra reprocessamentos, enquanto retry e DLQ permitem lidar com falhas na mensageria.

O uso de repositórios independentes por microsserviço mantém responsabilidades separadas e facilita manutenção, testes e evolução.