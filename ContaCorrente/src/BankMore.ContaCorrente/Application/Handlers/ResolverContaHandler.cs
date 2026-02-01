using BankMore.ContaCorrente.Application.Commands;
using BankMore.ContaCorrente.Domain.Exceptions;
using BankMore.ContaCorrente.Domain.Repositories;
using MediatR;

namespace BankMore.ContaCorrente.Application.Handlers {
    public class ResolverContaHandler : IRequestHandler<ResolverContaCommand, ResolverContaResponse> {
        private readonly IContaRepository _contaRepository;

        public ResolverContaHandler(IContaRepository contaRepository) {
            _contaRepository = contaRepository;
        }

        public async Task<ResolverContaResponse> Handle(ResolverContaCommand request, CancellationToken cancellationToken) {
            var conta = await _contaRepository.ObterPorNumero(request.NumeroConta);

            if (conta == null)
                throw new BusinessException("Conta não encontrada", "INVALID_ACCOUNT");

            return new ResolverContaResponse {
                ContaId = conta.IdContaCorrente,
                NumeroConta = $"{conta.Numero}-{conta.Numero % 10}"
            };
        }
    }
}
