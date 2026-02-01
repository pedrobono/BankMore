using MediatR;

namespace BankMore.ContaCorrente.Application.Commands {
    public class RegistrarMovimentoCommand : IRequest {
        public string RequestId { get; set; }
        public Guid? ContaId { get; set; }  // ID da conta destino
        public decimal Valor { get; set; }
        public string Tipo { get; set; }
        public Guid? ContaIdLogada { get; set; }  // ID da conta do token
    }
}
