# BankMore - Guia Rápido

## ✅ Estrutura Criada

```
BankMore/
├── ContaCorrente/              # Microserviço de Contas
│   ├── src/BankMore.ContaCorrente/
│   ├── tests/BankMore.ContaCorrente.Tests/
│   ├── Dockerfile
│   └── BankMore.ContaCorrente.sln
├── Transferencia/              # Microserviço de Transferências
│   ├── src/BankMore.Transferencia/
│   ├── tests/BankMore.TransferService.Tests/
│   ├── Dockerfile
│   └── BankMore.Transferencia.sln
├── docker-compose.yml          # Orquestração dos 2 serviços
├── .env.example
├── .gitignore
└── README.md
```

## 🚀 Como Executar

### Opção 1: Docker Compose (Recomendado)

```bash
cd /home/bono/Projetos\ Pessoais/BankMore
docker-compose up --build
```

Acesse:
- **ContaCorrente**: http://localhost:8081/swagger
- **Transferencia**: http://localhost:8082/swagger

### Opção 2: Executar Localmente

**Terminal 1 - ContaCorrente:**
```bash
cd /home/bono/Projetos\ Pessoais/BankMore/ContaCorrente/src/BankMore.ContaCorrente
dotnet run
```

**Terminal 2 - Transferencia:**
```bash
cd /home/bono/Projetos\ Pessoais/BankMore/Transferencia/src/BankMore.Transferencia
dotnet run
```

## 🧪 Executar Testes

**ContaCorrente:**
```bash
cd /home/bono/Projetos\ Pessoais/BankMore/ContaCorrente
dotnet test
```

**Transferencia:**
```bash
cd /home/bono/Projetos\ Pessoais/BankMore/Transferencia
dotnet test
```

## 📦 Build dos Projetos

**ContaCorrente:**
```bash
cd /home/bono/Projetos\ Pessoais/BankMore/ContaCorrente/src/BankMore.ContaCorrente
dotnet build
```

**Transferencia:**
```bash
cd /home/bono/Projetos\ Pessoais/BankMore/Transferencia/src/BankMore.Transferencia
dotnet build
```

## ✨ Melhorias Aplicadas (DDD)

### ContaCorrente:
- ✅ Interfaces movidas para `Domain/Repositories/`
- ✅ DbContext movido para `Infrastructure/Persistence/`
- ✅ Migrations organizadas em `Infrastructure/Persistence/Migrations/`
- ✅ Removida entidade `Transferencia` (pertence ao outro microserviço)
- ✅ Namespaces atualizados e consistentes

### Transferencia:
- ✅ Interfaces movidas para `Domain/Repositories/`
- ✅ IContaCorrenteServiceClient movido para `Application/Services/`
- ✅ DatabaseInitializer em `Infrastructure/Persistence/`
- ✅ Middleware organizado em `Api/Middleware/`
- ✅ Alias para resolver conflito de nomes (TransferenciaEntity)
- ✅ Namespaces atualizados e consistentes

## 🔑 Variáveis de Ambiente

Crie um arquivo `.env` na raiz (use `.env.example` como base):
```bash
JWT_SECRET=sua-chave-secreta-super-segura-com-pelo-menos-32-caracteres
```

## 📝 Notas Importantes

1. **Projetos Originais Preservados**: Os projetos originais em `/home/bono/Projetos Pessoais/BankMore.ContaCorrente` e `/home/bono/Projetos Pessoais/BankMore.Transferencia` foram mantidos intactos

2. **Compilação Verificada**: Ambos os projetos compilam com sucesso (apenas warnings de nullable, não afetam funcionalidade)

3. **Docker Compose**: Configurado para rodar os 2 serviços juntos com network compartilhada e volumes persistentes

4. **Testes**: Estrutura de testes copiada e referências atualizadas

## 🎯 Próximos Passos

1. Testar os serviços com docker-compose
2. Executar os testes para garantir que tudo funciona
3. Ajustar configurações conforme necessário
4. Remover projetos originais se tudo estiver OK
