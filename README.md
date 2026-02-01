# BankMore - Sistema Bancário em Microserviços

Sistema bancário completo implementado em .NET 8 seguindo princípios de DDD (Domain-Driven Design) e arquitetura de microserviços.

## 🏗️ Arquitetura

O projeto é composto por 2 microserviços independentes:

### 1. **ContaCorrente** (Porta 8081)
Responsável por gerenciar contas correntes, movimentações e autenticação.

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

## 🚀 Como Executar

### Opção 1: Visual Studio (Recomendado para Desenvolvimento)

1. Abra o arquivo `BankMore.sln` na raiz do projeto
2. No Visual Studio, você verá todos os projetos organizados:
   - **ContaCorrente** (pasta com src e tests)
   - **Transferencia** (pasta com src e tests)
3. Para executar ambos os serviços simultaneamente:
   - Clique com botão direito na Solution
   - Selecione "Configure Startup Projects"
   - Escolha "Multiple startup projects"
   - Defina ambos `BankMore.ContaCorrente` e `BankMore.Transferencia` como "Start"
4. Pressione F5 ou clique em "Start"

### Opção 2: Linha de Comando

**Compilar toda a solution:**
```bash
cd BankMore
dotnet build
```

**Executar todos os testes:**
```bash
dotnet test
```

**Executar os serviços separadamente:**

*Terminal 1 - ContaCorrente:*
```bash
cd ContaCorrente/src/BankMore.ContaCorrente
dotnet run
```

*Terminal 2 - Transferencia:*
```bash
cd Transferencia/src/BankMore.Transferencia
dotnet run
```

### Opção 3: Docker Compose

```bash
cd BankMore
docker-compose up --build
```

**Serviços disponíveis:**
- ContaCorrente API: http://localhost:8081
- ContaCorrente Swagger: http://localhost:8081/swagger
- Transferencia API: http://localhost:8082
- Transferencia Swagger: http://localhost:8082/swagger

## 🔧 Tecnologias

- **.NET 8**
- **EF Core** (ContaCorrente) / **Dapper** (Transferencia)
- **SQLite** (Banco de dados)
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

## 🔗 Comunicação entre Microserviços

O serviço de Transferencia se comunica com ContaCorrente via HTTP para:
- Resolver IDs de contas
- Registrar movimentações (débito/crédito)
- Compensação em caso de falha

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
