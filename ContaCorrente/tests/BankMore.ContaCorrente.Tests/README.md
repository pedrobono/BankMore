# BankMore.ContaCorrente.Tests

Este projeto contém testes unitários e de integração para a API BankMore.ContaCorrente, garantindo qualidade e confiabilidade do código.

## 📊 Estatísticas

- **Total de Testes:** 65
- **Taxa de Sucesso:** 100% ✅
- **Cobertura:** Unitários + Integração

## 📁 Estrutura do Projeto

### 🔗 Integration Tests
Testes de integração que validam o comportamento end-to-end dos controllers.

#### Controllers
- **`AuthControllerTests.cs`**
  - Testes de autenticação (login)
  - Validação de credenciais
  - Geração de tokens JWT

- **`ContaControllerTests.cs`**
  - Criação de contas
  - Validação de CPF
  - Inativação de contas

- **`ResolverContaTests.cs`** ✨ *Novo*
  - Resolução de número de conta para ID
  - Validação de contas inexistentes
  - Autenticação JWT

- **`MovimentoControllerTests.cs`**
  - Registro de movimentações (crédito/débito)
  - Validações de negócio
  - Autorização por token

- **`MovimentoIdempotenciaTests.cs`**
  - Idempotência de transações
  - Validação de `requestId` único por conta
  - Comportamento com múltiplas contas

- **`SaldoInsuficienteTests.cs`** ✨ *Novo*
  - Validação de saldo antes de débitos
  - Erro `INSUFFICIENT_BALANCE`
  - Débitos com saldo suficiente

- **`SaldoControllerTests.cs`**
  - Consulta de saldo
  - Cálculo correto (créditos - débitos)
  - Validação de contas ativas

- **`TokenValidationTests.cs`**
  - Validação de tokens JWT
  - Tokens expirados/inválidos
  - Acesso não autorizado

- **`InativarContaControllerTests.cs`**
  - Inativação de contas
  - Validação de senha
  - Autorização

#### Outros
- **`CompleteFlowTests.cs`**
  - Fluxos completos de uso
  - Integração entre múltiplos endpoints

### 🧪 Unit Tests
Testes unitários que validam a lógica de negócio isoladamente.

#### Handlers
- **`CriarContaHandlerTests.cs`**
  - Lógica de criação de conta
  - Geração de número de conta
  - Hash de senha

- **`LoginHandlerTests.cs`**
  - Validação de credenciais
  - Geração de JWT
  - Cenários de falha

- **`ObterSaldoHandlerTests.cs`**
  - Cálculo de saldo
  - Validações de conta
  - Retorno de dados

- **`RegistrarMovimentoHandlerTests.cs`**
  - Registro de movimentações
  - Validações de negócio
  - Idempotência
  - Validação de saldo insuficiente ✨ *Atualizado*

- **`InativarContaHandlerTests.cs`**
  - Lógica de inativação
  - Validação de senha

#### Validators
- **`CriarContaValidadorTests.cs`**
  - Validação de CPF
  - Validação de campos obrigatórios

- **`RegistrarMovimentoValidadorTests.cs`**
  - Validação de valores
  - Validação de tipos
  - Validação de `requestId`

#### Value Objects
- **`CpfTests.cs`**
  - Validação de CPF
  - Algoritmo de dígitos verificadores
  - Formatação

### 🛠️ Helpers
- **`CustomWebApplicationFactory.cs`**
  - Factory para testes de integração
  - Configuração de ambiente de teste
  - Banco de dados em memória

- **`SharedTestDtos.cs`** ✨ *Novo*
  - DTOs compartilhados entre testes
  - Evita duplicação de código

## 🚀 Executando os Testes

### Todos os testes
```bash
dotnet test
```

### Apenas testes de integração
```bash
dotnet test --filter "FullyQualifiedName~Integration"
```

### Apenas testes unitários
```bash
dotnet test --filter "FullyQualifiedName~Unit"
```

### Testes específicos
```bash
# Testes de saldo insuficiente
dotnet test --filter "FullyQualifiedName~SaldoInsuficienteTests"

# Testes de idempotência
dotnet test --filter "FullyQualifiedName~MovimentoIdempotenciaTests"

# Testes de autenticação
dotnet test --filter "FullyQualifiedName~AuthControllerTests"
```

### Com verbosidade detalhada
```bash
dotnet test --verbosity detailed
```

### Com cobertura de código
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## ✨ Novidades (FASE 1)

### Testes Adicionados
1. **`SaldoInsuficienteTests.cs`**
   - Validação de saldo antes de débitos
   - Testa erro `INSUFFICIENT_BALANCE`
   - Valida débitos com saldo suficiente

2. **`ResolverContaTests.cs`**
   - Testa endpoint `/api/Conta/resolve`
   - Valida resolução de número para ID
   - Testa contas inexistentes

### Testes Atualizados
- **`RegistrarMovimentoHandlerTests.cs`**
  - Adicionado crédito antes de débito nos testes
  - Ajustado para nova validação de saldo

## 📊 Cobertura de Testes

### Por Funcionalidade
- ✅ Autenticação e Autorização
- ✅ Criação e Gerenciamento de Contas
- ✅ Movimentações (Crédito/Débito)
- ✅ Consulta de Saldo
- ✅ Idempotência de Transações
- ✅ Validação de Saldo Insuficiente
- ✅ Resolução de Conta (Número → ID)
- ✅ Validações de Negócio
- ✅ Tratamento de Erros

### Por Tipo
- ✅ Testes Unitários: ~40 testes
- ✅ Testes de Integração: ~25 testes
- ✅ Taxa de Sucesso: 100%

## 📝 Boas Práticas

### Padrões Utilizados
- **AAA Pattern**: Arrange, Act, Assert
- **Isolation**: Cada teste é independente
- **In-Memory Database**: Testes rápidos sem dependências externas
- **Factory Pattern**: `CustomWebApplicationFactory` para testes de integração
- **Shared DTOs**: Evita duplicação de código

### Nomenclatura
- `MethodName_Scenario_ExpectedBehavior`
- Exemplo: `Handle_ShouldThrowException_WhenInvalidAccount`

## 🔧 Manutenção

Para adicionar novos testes:

1. **Testes Unitários**: Adicione em `Unit/Handlers/` ou `Unit/Validators/`
2. **Testes de Integração**: Adicione em `Integration/Controllers/`
3. **DTOs Compartilhados**: Use `SharedTestDtos.cs`
4. **Siga o padrão AAA**: Arrange, Act, Assert
5. **Mantenha testes isolados**: Cada teste deve ser independente

## 📚 Referências

- [xUnit Documentation](https://xunit.net/)
- [ASP.NET Core Testing](https://docs.microsoft.com/en-us/aspnet/core/test/)
- [Entity Framework Core In-Memory](https://docs.microsoft.com/en-us/ef/core/testing/)