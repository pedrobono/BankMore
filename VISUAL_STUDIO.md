# Guia Visual Studio - BankMore

## 🎯 Executando Múltiplos Projetos Simultaneamente

### Passo 1: Abrir a Solution
- Abra o arquivo `BankMore.sln` na raiz do projeto

### Passo 2: Configurar Múltiplos Projetos de Inicialização

1. **Clique com botão direito** na Solution "BankMore" no Solution Explorer
2. Selecione **"Set Startup Projects..."** ou **"Configure Startup Projects..."**
3. Escolha a opção **"Multiple startup projects"**
4. Configure os projetos:
   - `BankMore.ContaCorrente` → **Start**
   - `BankMore.Transferencia` → **Start**
   - Deixe os projetos de teste como **None**

### Passo 3: Executar
- Pressione **F5** ou clique no botão **Start**
- Ambos os serviços serão iniciados simultaneamente
- Duas janelas de navegador abrirão automaticamente com o Swagger de cada serviço

## 📁 Estrutura no Solution Explorer

```
BankMore (Solution)
├── ContaCorrente
│   ├── src
│   │   └── BankMore.ContaCorrente (API - Porta 8081)
│   └── tests
│       └── BankMore.ContaCorrente.Tests
└── Transferencia
    ├── src
    │   └── BankMore.Transferencia (API - Porta 8082)
    └── tests
        └── BankMore.TransferService.Tests
```

## 🧪 Executando Testes

### Opção 1: Test Explorer
1. Abra o **Test Explorer** (Menu: Test → Test Explorer)
2. Clique em **Run All** para executar todos os testes
3. Ou clique com botão direito em um teste específico e selecione **Run**

### Opção 2: Linha de Comando
```bash
# Na raiz do projeto
dotnet test
```

## 🔧 Build da Solution Completa

### Visual Studio
- **Build → Build Solution** (Ctrl+Shift+B)

### Linha de Comando
```bash
# Na raiz do projeto
dotnet build
```

## 🐛 Debug

### Debugar Ambos os Serviços
1. Configure breakpoints nos projetos desejados
2. Pressione F5
3. Ambos os serviços iniciarão em modo debug
4. Você pode alternar entre os processos na barra de ferramentas de debug

### Debugar Apenas Um Serviço
1. Clique com botão direito no projeto específico
2. Selecione **"Debug → Start New Instance"**

## 📝 Dicas

- **Restaurar Pacotes**: Clique com botão direito na Solution → "Restore NuGet Packages"
- **Limpar Build**: Build → Clean Solution
- **Rebuild**: Build → Rebuild Solution
- **Ver Logs**: View → Output (selecione "Debug" no dropdown)

## 🌐 URLs dos Serviços

Após iniciar os projetos:

- **ContaCorrente API**: https://localhost:8081
- **ContaCorrente Swagger**: https://localhost:8081/swagger
- **Transferencia API**: https://localhost:8082
- **Transferencia Swagger**: https://localhost:8082/swagger

## ⚙️ Configurações de Porta

As portas são configuradas em:
- `ContaCorrente/src/BankMore.ContaCorrente/Properties/launchSettings.json`
- `Transferencia/src/BankMore.Transferencia/Properties/launchSettings.json`
