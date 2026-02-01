# Solução para Erro de inotify em Testes de Integração

## 🐛 Problema

Ao executar testes de integração no Linux, você pode encontrar este erro:

```
System.IO.IOException: The configured user limit (128) on the number of inotify 
instances has been reached, or the per-process limit on the number of open file 
descriptors has been reached.
```

## 🔍 Causa

O Linux usa `inotify` para monitorar mudanças em arquivos. Os testes de integração do ASP.NET Core criam múltiplas instâncias do `WebApplicationFactory`, e cada uma monitora arquivos de configuração (appsettings.json, etc.), esgotando o limite padrão de 128 instâncias.

## ✅ Soluções Implementadas

### 1. Otimização do Código (Já Aplicada)

O arquivo `CustomWebApplicationFactory.cs` foi otimizado para:
- Usar ambiente "Testing" que desabilita alguns watchers
- Limpar fontes de configuração desnecessárias
- Usar apenas configuração em memória

```csharp
builder.UseEnvironment("Testing");
builder.ConfigureAppConfiguration((context, config) =>
{
    config.Sources.Clear(); // Remove watchers de arquivos
    config.AddInMemoryCollection(new Dictionary<string, string?>
    {
        [\"JwtSettings:SecretKey\"] = TestJwtKey,
        [\"ConnectionStrings:DefaultConnection\"] = \"Data Source=:memory:\"
    });
});
```

### 2. Aumentar Limite do Sistema (Recomendado)

Execute o script fornecido:

```bash
./fix-inotify-limit.sh
```

Ou manualmente:

**Temporário (até reiniciar):**
```bash
echo 512 | sudo tee /proc/sys/fs/inotify/max_user_instances
echo 524288 | sudo tee /proc/sys/fs/inotify/max_user_watches
```

**Permanente:**
```bash
echo "fs.inotify.max_user_instances=512" | sudo tee -a /etc/sysctl.conf
echo "fs.inotify.max_user_watches=524288" | sudo tee -a /etc/sysctl.conf
sudo sysctl -p
```

### 3. Executar Testes em Paralelo Limitado

Se ainda tiver problemas, limite o paralelismo:

```bash
dotnet test --logger "console;verbosity=normal" -- xUnit.MaxParallelThreads=1
```

## 📊 Verificar Limites Atuais

```bash
cat /proc/sys/fs/inotify/max_user_instances
cat /proc/sys/fs/inotify/max_user_watches
```

## 🎯 Valores Recomendados

- `max_user_instances`: 512 (padrão: 128)
- `max_user_watches`: 524288 (padrão: 8192)

## 🔧 Alternativas

### Executar Testes Específicos

Em vez de executar todos os testes de uma vez:

```bash
# Apenas testes unitários (não usam WebApplicationFactory)
dotnet test --filter "FullyQualifiedName~Unit"

# Apenas testes de integração
dotnet test --filter "FullyQualifiedName~Integration"

# Teste específico
dotnet test --filter "FullyQualifiedName~MovimentoControllerIntegrationTests"
```

### Usar Docker para Testes

Os testes dentro de containers Docker não sofrem deste problema:

```bash
docker-compose up --build
```

## 📝 Notas

- Este é um problema específico do Linux
- Windows e macOS não têm este limite
- A otimização do código já reduz significativamente o uso de inotify
- Em CI/CD, configure os limites no ambiente de build
