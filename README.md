# BankMore - Sistema Bancário em Microserviços

Sistema bancário completo implementado em .NET 8 seguindo princípios de DDD (Domain-Driven Design) e arquitetura de microserviços.

## 🏗️ Arquitetura

O projeto é composto por 3 microserviços independentes:

### 1. **ContaCorrente** (Porta 8081)
Responsável por gerenciar contas correntes, movimentações e autenticação.

**Funcionalidades:**
- Criar e gerenciar contas correntes
- Autenticação JWT
- Registrar movimentações (crédito/débito)
- Consultar saldo
- **Consumir tarifações** via Kafka e debitar automaticamente

**Estrutura DDD:**
```
ContaCorrente/
├── src/BankMore.ContaCorrente/
│   ├── Domain/
│   │   ├── Entities/          # Conta, Movimento, Idempotencia
│   │   ├── ValueObjects/      # CPF
│   │   ├── Exceptions/        # BusinessException
│   │   └── Repositories/      # Interfaces (IContaRepository, IMovimentoRepository)
│   ├── Application/
│   │   ├── Commands/          # CriarConta, RegistrarMovimento, etc
│   │   ├── Queries/           # ObterSaldo
│   │   ├── Handlers/          # Command/Query Handlers
│   │   ├── DTOs/              # Data Transfer Objects
│   │   └── Validators/        # FluentValidation
│   ├── Infrastructure/
│   │   ├── Persistence/       # EF Core DbContext, Migrations
│   │   ├── Repositories/      # Implementações dos repositórios
│   │   └── Security/          # CryptoHelper, JWT
│   └── Api/
│       ├── Controllers/       # REST API Controllers
│       ├── Middleware/        # Error Handling
│       └── Swagger/           # Configurações Swagger
└── tests/BankMore.ContaCorrente.Tests/
```

### 2. **Transferencia** (Porta 8082)
Responsável por transferências entre contas com compensação automática.

**Funcionalidades:**
- Transferências entre contas
- Compensação automática em caso de falha
- Idempotência
- **Publicar transferências realizadas** via Kafka

**Estrutura DDD:**
```
Transferencia/
├── src/BankMore.Transferencia/
│   ├── Domain/
│   │   ├── Entities/          # Transferencia, Idempotencia
│   │   ├── ValueObjects/      # ValorMonetario
│   │   ├── Exceptions/        # TransferenciaException, CompensacaoFalhaException
│   │   └── Repositories/      # ITransferenciaRepository
│   ├── Application/
│   │   ├── Commands/          # CriarTransferencia
│   │   ├── DTOs/              # Request/Response DTOs
│   │   └── Services/          # IContaCorrenteServiceClient
│   ├── Infrastructure/
│   │   ├── Persistence/       # Dapper, DatabaseInitializer, Migrations
│   │   ├── Repositories/      # TransferenciaRepository
│   │   ├── ExternalServices/  # ContaCorrenteServiceClient
│   │   └── HealthChecks/      # Database, AccountService
│   └── Api/
│       ├── Controllers/       # TransferenciaController
│       └── Middleware/        # Exception Handler
└── tests/BankMore.TransferService.Tests/
```

### 3. **Tarifa** (Porta 8083)
Responsável por processar tarifações de transferências via Kafka.

**Estrutura DDD:**
```
Tarifa/
├── src/BankMore.Tarifa/
│   ├── Domain/
│   │   ├── Entities/          # Tarifacao
│   │   └── Repositories/      # ITarifacaoRepository
│   ├── Application/
│   │   ├── Commands/          # ProcessarTarifacaoCommand
│   │   ├── Handlers/          # ProcessarTarifacaoHandler
│   │   └── DTOs/              # Mensagens Kafka
│   ├── Infrastructure/
│   │   ├── Persistence/       # SQLite + Dapper
│   │   ├── Repositories/      # TarifacaoRepository
│   │   └── Kafka/             # Producer + Consumer
│   └── Api/
│       └── Program.cs
└── tests/BankMore.Tarifa.Tests/
```

## 🔄 Fluxo de Tarifação (Kafka)

1. **Transferencia** realiza transferência com sucesso
2. **Transferencia** publica mensagem no tópico `transferencias-realizadas`
3. **Tarifa** consome a mensagem
4. **Tarifa** registra tarifação no banco (valor configurável em appsettings)
5. **Tarifa** publica mensagem no tópico `tarifacoes-realizadas`
6. **ContaCorrente** consome a mensagem e debita o valor da tarifa

## 🚀 Como Executar

> **📖 Para desenvolvimento local detalhado, veja [DEV_GUIDE.md](DEV_GUIDE.md)**

### Opção 1: Docker Compose (Produção)

```bash
cd BankMore
docker-compose up --build
```

**Serviços disponíveis:**
- ContaCorrente API: http://localhost:8081
- ContaCorrente Swagger: http://localhost:8081/swagger
- Transferencia API: http://localhost:8082
- Transferencia Swagger: http://localhost:8082/swagger
- Tarifa API: http://localhost:8083
- Tarifa Swagger: http://localhost:8083/swagger
- Kafka: localhost:9092

### Opção 2: Desenvolvimento Local

**1. Iniciar Kafka:**
```bash
docker-compose -f docker-compose-dev.yml up -d
```

**2. Executar serviços:**

*Visual Studio:*
- Abra `BankMore.sln`
- Configure múltiplos projetos de inicialização
- Pressione F5

*Linha de comando (3 terminais):*
```bash
# Terminal 1
cd ContaCorrente/src/BankMore.ContaCorrente && dotnet run

# Terminal 2
cd Transferencia/src/BankMore.Transferencia && dotnet run

# Terminal 3
cd Tarifa/src/BankMore.Tarifa && dotnet run
```

**3. Criar banco de dados (primeira vez):**
```bash
cd ContaCorrente/src/BankMore.ContaCorrente
dotnet ef database update
```

## 🔧 Tecnologias

- **.NET 8**
- **EF Core** (ContaCorrente) / **Dapper** (Transferencia, Tarifa)
- **SQLite** (Banco de dados)
- **Apache Kafka** (Mensageria assíncrona)
- **MediatR** (CQRS Pattern)
- **FluentValidation** (Validações)
- **JWT** (Autenticação)
- **Swagger/OpenAPI** (Documentação)
- **Serilog** (Logging)
- **xUnit, Moq, FluentAssertions** (Testes)

## 📋 Variáveis de Ambiente

```bash
JWT_SECRET=sua-chave-secreta-super-segura-com-pelo-menos-32-caracteres
```

## 🎯 Princípios DDD Aplicados

- **Bounded Contexts**: Cada microserviço representa um contexto delimitado
- **Entities**: Objetos com identidade única (Conta, Transferencia)
- **Value Objects**: Objetos imutáveis sem identidade (CPF, ValorMonetario)
- **Repositories**: Abstração de persistência no Domain
- **Domain Services**: Lógica de negócio complexa
- **Application Services**: Orquestração de casos de uso
- **Infrastructure**: Detalhes técnicos isolados

## 📦 Volumes Docker

- `conta-data`: Banco de dados do ContaCorrente
- `transfer-data`: Banco de dados do Transferencia
- `tarifa-data`: Banco de dados do Tarifa

## 🔗 Comunicação entre Microserviços

### HTTP (Síncrono)
- **Transferencia → ContaCorrente**: Resolver IDs, registrar movimentações, compensação

### Kafka (Assíncrono)
- **Transferencia → Tarifa**: Publica transferências realizadas
- **Tarifa → ContaCorrente**: Publica tarifações para débito automático

**Tópicos Kafka:**
- `transferencias-realizadas`: Transferências concluídas com sucesso
- `tarifacoes-realizadas`: Tarifações processadas

## ✅ Health Checks

Ambos os serviços expõem endpoints de health check:
- `/health` - Status geral do serviço

## ⚠️ Problemas Conhecidos

### Erro de inotify em Testes (Linux)

Se você encontrar o erro:
```
System.IO.IOException: The configured user limit (128) on the number of inotify instances has been reached
```

**Solução rápida:**
```bash
./fix-inotify-limit.sh
```

Veja [INOTIFY_FIX.md](INOTIFY_FIX.md) para mais detalhes e soluções alternativas.
