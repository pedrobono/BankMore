using BankMore.Transferencia.Application.DTOs;

namespace BankMore.Transferencia.Application.Services;

public interface IContaCorrenteServiceClient
{
    Task CreateMovementAsync(CriarMovimentoRequest request, string authorizationToken);
    Task<Guid> ResolveAccountIdAsync(string numeroConta, string authorizationToken);
}
