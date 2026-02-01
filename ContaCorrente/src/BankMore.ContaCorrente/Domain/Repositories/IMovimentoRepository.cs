using BankMore.ContaCorrente.Domain.Entities;

namespace BankMore.ContaCorrente.Domain.Repositories;

public interface IMovimentoRepository
{
    Task Adicionar(Movimento movimento);
    Task<bool> ExisteRequestId(string requestId);
}