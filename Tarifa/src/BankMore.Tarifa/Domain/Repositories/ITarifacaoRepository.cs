using BankMore.Tarifa.Domain.Entities;

namespace BankMore.Tarifa.Domain.Repositories;

public interface ITarifacaoRepository
{
    Task<Guid> CreateAsync(Tarifacao tarifacao);
    Task<Tarifacao?> GetByRequestIdAsync(string requestId);
}
