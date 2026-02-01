using BankMore.ContaCorrente.Domain.Entities;

namespace BankMore.ContaCorrente.Domain.Repositories {
    public interface IContaRepository {
        Task<Conta?> ObterPorNumero(string numeroConta);
        Task<Conta?> ObterPorCpf(string cpf);
        Task Adicionar(Conta conta);
        Task Atualizar(Conta conta);
    }
}