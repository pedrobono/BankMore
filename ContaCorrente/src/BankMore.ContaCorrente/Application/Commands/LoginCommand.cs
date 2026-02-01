using MediatR;

namespace BankMore.ContaCorrente.Application.Commands {
    public class LoginCommand : IRequest<LoginResponse> {
        public string CpfOrNumeroConta { get; set; }
        public string Senha { get; set; }
    }

    public class LoginResponse {
        public string Token { get; set; }
    }
}
