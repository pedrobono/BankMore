using MediatR;

namespace BankMore.Tarifa.Application.Commands;

public record ProcessarTarifacaoCommand(
    string RequestId,
    Guid IdContaOrigem
) : IRequest<Unit>;
