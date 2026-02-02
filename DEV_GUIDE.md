# Guia de Desenvolvimento Local - BankMore

> ⚠️ **ATENÇÃO:** Antes de executar pela primeira vez, você DEVE:
> 1. Iniciar o Kafka: `docker-compose -f docker-compose-dev.yml up -d`
> 2. Criar o banco do ContaCorrente: `cd ContaCorrente/src/BankMore.ContaCorrente && dotnet ef database update`

## 📋 Pré-requisitos

- .NET 8 SDK
- Docker e Docker Compose
- Visual Studio 2022 ou VS Code (opcional)
- Git

## 🚀 Setup Inicial

### 1. Clonar o Repositório

```bash
git clone <repository-url>
cd BankMore
```

### 2. Iniciar Kafka (Obrigatório)

O Kafka é necessário para comunicação entre os microserviços.

```bash
# Subir apenas Kafka e Zookeeper
docker-compose -f docker-compose-dev.yml up -d

# Verificar se está rodando
docker ps
```

**Aguarde ~15 segundos** para o Kafka inicializar completamente.

### 3. Criar Tópicos Kafka (Opcional)

Os tópicos são criados automaticamente, mas você pode criá-los manualmente:

```bash
docker exec -it $(docker ps -qf "ancestor=confluentinc/cp-kafka:7.5.0") kafka-topics --bootstrap-server localhost:9092 --create --if-not-exists --topic transferencias-realizadas --replication-factor 1 --partitions 1

docker exec -it $(docker ps -qf "ancestor=confluentinc/cp-kafka:7.5.0") kafka-topics --bootstrap-server localhost:9092 --create --if-not-exists --topic tarifacoes-realizadas --replication-factor 1 --partitions 1
```

### 4. Restaurar Dependências

```bash
dotnet restore
```

### 5. Criar Bancos de Dados

#### ContaCorrente (EF Core) - OBRIGATÓRIO

```bash
cd ContaCorrente/src/BankMore.ContaCorrente
dotnet ef database update
cd ../../..
```

**⚠️ IMPORTANTE:** O ContaCorrente usa EF Core e **NÃO cria o banco automaticamente**. Você DEVE executar o comando acima antes de rodar a aplicação.

#### Transferencia (Dapper)

O banco é criado automaticamente na primeira execução usando DbUp.

**Se houver erro "no such table: idempotencia":**
```bash
cd Transferencia/src/BankMore.Transferencia
dotnet build  # Recompilar para incluir migrations
dotnet run    # Reiniciar serviço
```

#### Tarifa (Dapper)

O banco é criado automaticamente na primeira execução usando DbUp.

## 🏃 Executar os Serviços

### Opção 1: Visual Studio (Recomendado)

1. Abra `BankMore.sln`
2. Configure múltiplos projetos de inicialização:
   - Botão direito na Solution → "Configure Startup Projects"
   - Selecione "Multiple startup projects"
   - Marque como "Start":
     - `BankMore.ContaCorrente`
     - `BankMore.Transferencia`
     - `BankMore.Tarifa`
3. Pressione **F5**

### Opção 2: Linha de Comando (3 Terminais)

**Terminal 1 - ContaCorrente:**
```bash
cd ContaCorrente/src/BankMore.ContaCorrente
dotnet run
```

**Terminal 2 - Transferencia:**
```bash
cd Transferencia/src/BankMore.Transferencia
dotnet run
```

**Terminal 3 - Tarifa:**
```bash
cd Tarifa/src/BankMore.Tarifa
dotnet run
```

### Opção 3: Script de Inicialização

```bash
# Linux/Mac
./start-dev.sh

# Windows
start-dev.bat
```

## 🌐 URLs dos Serviços

Após iniciar, acesse:

- **ContaCorrente**: http://localhost:8081/swagger
- **Transferencia**: http://localhost:8082/swagger
- **Tarifa**: http://localhost:8083/swagger
- **Kafka**: localhost:9092

## 🧪 Executar Testes

```bash
# Todos os testes
dotnet test

# Apenas testes unitários (rápido)
dotnet test --filter "FullyQualifiedName~Unit"

# Apenas testes de integração
dotnet test --filter "FullyQualifiedName~Integration"

# Teste específico de um projeto
dotnet test ContaCorrente/tests/BankMore.ContaCorrente.Tests/
```

## 🔧 Problemas Comuns

### Erro: "no such table: contacorrente"

**Causa:** Banco de dados do ContaCorrente não foi criado.

**Solução (OBRIGATÓRIA na primeira vez):**
```bash
cd ContaCorrente/src/BankMore.ContaCorrente
dotnet ef database update
```

**Se o erro persistir:**
```bash
cd ContaCorrente/src/BankMore.ContaCorrente
rm BankMore.db  # Deletar banco corrompido
dotnet ef database update  # Recriar do zero
```

### Erro: "Unknown topic or partition"

**Causa:** Tópicos Kafka ainda não existem.

**Solução:** Aguarde ~10 segundos ou faça uma transferência (cria automaticamente).

### Erro: "Connection refused" (Kafka)

**Solução:**
```bash
# Verificar se Kafka está rodando
docker ps | grep kafka

# Se não estiver, iniciar
docker-compose -f docker-compose-dev.yml up -d
```

### Erro: inotify limit (Linux)

**Solução:**
```bash
./fix-inotify-limit.sh
```

### Porta já em uso

**Solução:**
```bash
# Verificar o que está usando a porta
lsof -i :8081  # ou 8082, 8083

# Matar o processo
kill -9 <PID>
```

## 🗄️ Gerenciar Bancos de Dados

### Localização dos Bancos

- ContaCorrente: `ContaCorrente/src/BankMore.ContaCorrente/BankMore.db`
- Transferencia: `Transferencia/src/BankMore.Transferencia/transfers.db`
- Tarifa: `Tarifa/src/BankMore.Tarifa/tarifa.db`

### Resetar Bancos

```bash
# Deletar todos os bancos
find . -name "*.db" -type f -delete

# Recriar ContaCorrente
cd ContaCorrente/src/BankMore.ContaCorrente
dotnet ef database update
```

### Visualizar Dados (SQLite)

```bash
# Instalar sqlite3
sudo apt install sqlite3  # Linux
brew install sqlite3      # Mac

# Abrir banco
sqlite3 ContaCorrente/src/BankMore.ContaCorrente/BankMore.db

# Comandos úteis
.tables                    # Listar tabelas
SELECT * FROM contacorrente;  # Ver contas
.quit                      # Sair
```

## 📊 Monitorar Kafka

### Ver Tópicos

```bash
docker exec -it $(docker ps -qf "ancestor=confluentinc/cp-kafka:7.5.0") kafka-topics --bootstrap-server localhost:9092 --list
```

### Consumir Mensagens

```bash
# Transferências
docker exec -it $(docker ps -qf "ancestor=confluentinc/cp-kafka:7.5.0") kafka-console-consumer --bootstrap-server localhost:9092 --topic transferencias-realizadas --from-beginning

# Tarifações
docker exec -it $(docker ps -qf "ancestor=confluentinc/cp-kafka:7.5.0") kafka-console-consumer --bootstrap-server localhost:9092 --topic tarifacoes-realizadas --from-beginning
```

## 🔄 Workflow de Desenvolvimento

1. **Iniciar Kafka** (uma vez)
   ```bash
   docker-compose -f docker-compose-dev.yml up -d
   ```

2. **Iniciar serviços** (Visual Studio ou terminais)

3. **Fazer alterações no código**

4. **Testar**
   ```bash
   dotnet test --filter "FullyQualifiedName~Unit"
   ```

5. **Commit**
   ```bash
   git add .
   git commit -m "feat: sua alteração"
   ```

## 🛑 Parar Serviços

### Parar Aplicações .NET

- Visual Studio: Shift+F5
- Terminal: Ctrl+C

### Parar Kafka

```bash
docker-compose -f docker-compose-dev.yml down

# Remover volumes (limpar dados)
docker-compose -f docker-compose-dev.yml down -v
```

## 📝 Variáveis de Ambiente

Crie um arquivo `.env` na raiz (opcional):

```bash
JWT_SECRET=@VtDDEiunPJkT4yfmg3t1QeGyIRgq3R8
KAFKA_BOOTSTRAP_SERVERS=localhost:9092
```

## 🔍 Debug

### Visual Studio

1. Coloque breakpoints no código
2. Pressione F5
3. Faça requisições via Swagger

### VS Code

1. Instale extensão C# Dev Kit
2. Use configuração de launch (F5)
3. Selecione o projeto para debugar

### Logs

Os logs aparecem no console. Para mais detalhes, ajuste em `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

## 📚 Documentação Adicional

- [README Principal](README.md)
- [Guia Kafka](GUIA_KAFKA.md)
- [Visual Studio Guide](VISUAL_STUDIO.md)
- [Tarifa README](Tarifa/README.md)

## 💡 Dicas

- Use **Swagger** para testar APIs rapidamente
- Mantenha **Kafka rodando** sempre que desenvolver
- Execute **testes unitários** antes de commitar
- Use **Hot Reload** do .NET 8 (alterações sem restart)
- Configure **múltiplos projetos** no Visual Studio

## 🆘 Suporte

Se encontrar problemas:

1. Verifique os logs no console
2. Confirme que Kafka está rodando
3. Verifique se as portas estão livres
4. Consulte a seção "Problemas Comuns"
5. Veja os READMEs específicos de cada serviço
