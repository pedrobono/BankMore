using BankMore.ContaCorrente.Application.Commands;
using BankMore.ContaCorrente.Domain.Entities;
using BankMore.ContaCorrente.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using BankMore.ContaCorrente.Domain.Exceptions;

namespace BankMore.ContaCorrente.Application.Handlers {
    public class CriarContaHandler : IRequestHandler<CriarContaCommand, string> {
        private readonly DataBaseContext _contexto;

        public CriarContaHandler(DataBaseContext contexto) {
            _contexto = contexto;
        }

        public async Task<string> Handle(CriarContaCommand request, CancellationToken cancellationToken) {
            var cpf = new Domain.ValueObjects.Cpf(request.Cpf);
            var senhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha);
            var cpfHash = BCrypt.Net.BCrypt.HashPassword(cpf.Valor);

            int numeroGerado;
            bool numeroJaExiste;

            do {
                numeroGerado = new Random().Next(10000, 99999);
                numeroJaExiste = await _contexto.ContaCorrente.AnyAsync(c => c.Numero == numeroGerado, cancellationToken);
            } while (numeroJaExiste);

            var conta = new Conta {
                IdContaCorrente = Guid.NewGuid(),
                Numero = numeroGerado,
                Nome = request.NomeTitular,
                Senha = senhaHash,
                Salt = cpfHash,
                Ativo = 1
            };

            _contexto.ContaCorrente.Add(conta);
            await _contexto.SaveChangesAsync(cancellationToken);

            return $"{conta.Numero}-{conta.Numero % 10}";
        }
    }
}
