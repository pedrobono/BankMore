# BankMore - Serviço de Tarifa

Microserviço responsável por processar tarifações de transferências bancárias utilizando Apache Kafka.

## 📋 Funcionalidades

- **Consumir mensagens Kafka** de transferências realizadas
- **Registrar tarifações** no banco de dados
- **Publicar mensagens** de tarifações realizadas
- **Valor parametrizável** da tarifa via appsettings.json
- **Idempotência** para evitar tarifações duplicadas

## 🏗️ Arquitetura DDD

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

## 🔄 Fluxo de Tarifação

1. **Transferencia** realiza transferência com sucesso
2. **Transferencia** publica mensagem no tópico `transferencias-realizadas`
3. **Tarifa** consome a mensagem
4. **Tarifa** registra tarifação no banco de dados
5. **Tarifa** publica mensagem no tópico `tarifacoes-realizadas`
6. **ContaCorrente** consome a mensagem e debita o valor da tarifa

## 📊 Banco de Dados

### Tabela: tarifacao

| Campo | Tipo | Descrição |
|-------|------|-----------|
| idtarifacao | TEXT (PK) | ID único da tarifação |
| idcontacorrente | TEXT | ID da conta corrente |
| valortarifado | REAL | Valor da tarifa |
| datahoratarifacao | TEXT | Data/hora da tarifação |
| requestid | TEXT (UNIQUE) | ID da requisição (idempotência) |

## ⚙️ Configuração

### appsettings.json

```json
{
  "TarifaSettings": {
    "ValorTarifa": 2.00
  },
  "Kafka": {
    "BootstrapServers": "localhost:9092"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=tarifa.db"
  }
}
```

## 🚀 Como Executar

### Localmente

```bash
cd Tarifa/src/BankMore.Tarifa
dotnet run
```

### Docker

```bash
docker-compose up tarifa
```

## 📡 Tópicos Kafka

### Consome
- **transferencias-realizadas**
  ```json
  {
    "RequestId": "string",
    "IdContaOrigem": "guid"
  }
  ```

### Produz
- **tarifacoes-realizadas**
  ```json
  {
    "IdContaCorrente": "guid",
    "ValorTarifado": 2.00
  }
  ```

## 🧪 Testes

```bash
cd Tarifa
dotnet test
```

## 🔧 Tecnologias

- **.NET 8**
- **Dapper** (ORM leve)
- **SQLite** (Banco de dados)
- **Confluent.Kafka** (Cliente Kafka)
- **MediatR** (CQRS)
- **Serilog** (Logging)
- **xUnit, Moq, FluentAssertions** (Testes)

## 📝 Endpoints

- **Health Check**: `GET /health`
- **Swagger**: `GET /swagger` (apenas em Development)

## 🌐 Porta

- **Local**: http://localhost:8083
- **Docker**: http://localhost:8083

## 🔍 Logs

O serviço registra logs detalhados de:
- Mensagens Kafka recebidas
- Tarifações processadas
- Mensagens Kafka enviadas
- Erros e exceções

## ⚠️ Idempotência

O serviço garante idempotência através do `RequestId`. Se uma mensagem com o mesmo `RequestId` for processada novamente, a tarifação não será duplicada.

## 🐛 Troubleshooting

### Kafka não conecta

Verifique se o Kafka está rodando:
```bash
docker ps | grep kafka
```

### Tarifação não é debitada

Verifique os logs do serviço ContaCorrente para ver se está consumindo as mensagens de `tarifacoes-realizadas`.
