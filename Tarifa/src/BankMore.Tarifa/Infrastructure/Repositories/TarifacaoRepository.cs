using BankMore.Tarifa.Domain.Entities;
using BankMore.Tarifa.Domain.Repositories;
using Dapper;
using Microsoft.Data.Sqlite;

namespace BankMore.Tarifa.Infrastructure.Repositories;

public class TarifacaoRepository : ITarifacaoRepository
{
    private readonly string _connectionString;

    public TarifacaoRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Data Source=tarifa.db";
    }

    public async Task<Guid> CreateAsync(Tarifacao tarifacao)
    {
        using var connection = new SqliteConnection(_connectionString);
        
        var sql = @"
            INSERT INTO tarifacao (idtarifacao, idcontacorrente, valortarifado, datahoratarifacao, requestid)
            VALUES (@IdTarifacao, @IdContaCorrente, @ValorTarifado, @DataHoraTarifacao, @RequestId)";

        await connection.ExecuteAsync(sql, new
        {
            IdTarifacao = tarifacao.IdTarifacao.ToString(),
            IdContaCorrente = tarifacao.IdContaCorrente.ToString(),
            tarifacao.ValorTarifado,
            DataHoraTarifacao = tarifacao.DataHoraTarifacao.ToString("yyyy-MM-dd HH:mm:ss"),
            tarifacao.RequestId
        });

        return tarifacao.IdTarifacao;
    }

    public async Task<Tarifacao?> GetByRequestIdAsync(string requestId)
    {
        using var connection = new SqliteConnection(_connectionString);
        
        var sql = "SELECT * FROM tarifacao WHERE requestid = @RequestId";
        
        var result = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { RequestId = requestId });
        
        if (result == null) return null;

        return new Tarifacao
        {
            IdTarifacao = Guid.Parse((string)result.idtarifacao),
            IdContaCorrente = Guid.Parse((string)result.idcontacorrente),
            ValorTarifado = (decimal)(double)result.valortarifado,
            DataHoraTarifacao = DateTime.Parse((string)result.datahoratarifacao),
            RequestId = (string)result.requestid
        };
    }
}
