using MediatR;

namespace BankMore.ContaCorrente.Application.Commands {
    public class InativarContaCommand : IRequest {
        public string Senha { get; set; }
        public Guid ContaId { get; set; }
    }
}
