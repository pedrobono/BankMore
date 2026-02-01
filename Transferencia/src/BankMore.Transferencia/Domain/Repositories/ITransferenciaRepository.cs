using BankMore.Transferencia.Domain.Entities;
using TransferenciaEntity = BankMore.Transferencia.Domain.Entities.Transferencia;

namespace BankMore.Transferencia.Domain.Repositories;

public interface ITransferenciaRepository
{
    Task<TransferenciaEntity?> GetByOriginAndRequestIdAsync(Guid originAccountId, string requestId);
    Task<Guid> CreateAsync(TransferenciaEntity transfer);
    Task SaveIdempotenciaAsync(Guid originAccountId, string requestId, string requisicao, string resultado);
}
