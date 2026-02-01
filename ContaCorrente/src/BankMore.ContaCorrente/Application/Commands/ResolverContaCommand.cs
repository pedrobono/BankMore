using MediatR;

namespace BankMore.ContaCorrente.Application.Commands {
    public class ResolverContaCommand : IRequest<ResolverContaResponse> {
        public string NumeroConta { get; set; }
    }

    public class ResolverContaResponse {
        public Guid ContaId { get; set; }
        public string NumeroConta { get; set; }
    }
}
