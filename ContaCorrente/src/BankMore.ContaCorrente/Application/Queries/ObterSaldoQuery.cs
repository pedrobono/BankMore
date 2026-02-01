using BankMore.ContaCorrente.Application.DTOs;
using MediatR;

namespace BankMore.ContaCorrente.Application.Queries {
    public class ObterSaldoQuery : IRequest<SaldoDto> {
        public Guid ContaId { get; set; }
    }
}
