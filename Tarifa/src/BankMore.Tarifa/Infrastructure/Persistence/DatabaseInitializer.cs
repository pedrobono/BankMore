using Microsoft.Data.Sqlite;
using System.Reflection;

namespace BankMore.Tarifa.Infrastructure.Persistence;

public class DatabaseInitializer
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(IConfiguration configuration, ILogger<DatabaseInitializer> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Data Source=tarifa.db";
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("[DATABASE] Inicializando banco de dados...");

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var assembly = Assembly.GetExecutingAssembly();
        var migrations = assembly.GetManifestResourceNames()
            .Where(name => name.Contains(".Persistence.") && name.EndsWith(".sql"))
            .OrderBy(name => name)
            .ToList();

        foreach (var migration in migrations)
        {
            _logger.LogInformation("[DATABASE] Executando migração: {Migration}", migration);
            
            using var stream = assembly.GetManifestResourceStream(migration);
            if (stream != null)
            {
                using var reader = new StreamReader(stream);
                var sql = await reader.ReadToEndAsync();
                
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                await command.ExecuteNonQueryAsync();
            }
        }

        _logger.LogInformation("[DATABASE] Banco de dados inicializado com sucesso");
    }
}
