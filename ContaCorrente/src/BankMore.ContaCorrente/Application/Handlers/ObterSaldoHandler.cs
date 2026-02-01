using BankMore.ContaCorrente.Application.DTOs;
using BankMore.ContaCorrente.Application.Queries;
using BankMore.ContaCorrente.Domain.Exceptions;
using BankMore.ContaCorrente.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BankMore.ContaCorrente.Application.Handlers {
    public class ObterSaldoHandler : IRequestHandler<ObterSaldoQuery, SaldoDto> {
        private readonly DataBaseContext _context;

        public ObterSaldoHandler(DataBaseContext context) {
            _context = context;
        }

        public async Task<SaldoDto> Handle(ObterSaldoQuery request, CancellationToken cancellationToken) {
            var conta = await _context.ContaCorrente
                .FirstOrDefaultAsync(c => c.IdContaCorrente == request.ContaId, cancellationToken);

            if (conta == null)
                throw new BusinessException("Conta não encontrada", "INVALID_ACCOUNT");

            if (conta.Ativo == 0)
                throw new BusinessException("Conta inativa", "INACTIVE_ACCOUNT");

            var movimentos = await _context.Movimento
                .Where(m => m.IdContaCorrente == conta.IdContaCorrente)
                .ToListAsync(cancellationToken);

            var saldo = movimentos.Where(m => m.TipoMovimento == "C").Sum(m => m.Valor) -
                        movimentos.Where(m => m.TipoMovimento == "D").Sum(m => m.Valor);

            return new SaldoDto {
                NumeroConta = $"{conta.Numero}-{conta.Numero % 10}",
                NomeTitular = conta.Nome,
                DataHoraConsulta = DateTime.UtcNow,
                Saldo = saldo
            };
        }
    }
}
