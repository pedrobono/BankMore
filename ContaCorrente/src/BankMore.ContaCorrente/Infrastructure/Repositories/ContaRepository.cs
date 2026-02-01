using BankMore.ContaCorrente.Domain.Entities;
using BankMore.ContaCorrente.Domain.Repositories;
using BankMore.ContaCorrente.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BankMore.ContaCorrente.Infrastructure.Repositories
{
    public class ContaRepository : IContaRepository 
    {
        private readonly DataBaseContext _context;

        public ContaRepository(DataBaseContext context)
        {
            _context = context;
        }

        public async Task<Conta?> ObterPorNumero(string numeroConta)
        {
            var numeroFormatado = numeroConta.Replace("-", "");
            var todasContas = await _context.ContaCorrente.ToListAsync();
            return todasContas.FirstOrDefault(c => $"{c.Numero}{c.Numero % 10}" == numeroFormatado);
        }

        public async Task<Conta?> ObterPorCpf(string cpf) 
            => await _context.ContaCorrente.FirstOrDefaultAsync(c => c.Salt == cpf);

        public async Task Adicionar(Conta conta)
        {
            await _context.ContaCorrente.AddAsync(conta);
            await _context.SaveChangesAsync();
        }

        public async Task Atualizar(Conta conta)
        {
            _context.ContaCorrente.Update(conta);
            await _context.SaveChangesAsync();
        }
    }
}