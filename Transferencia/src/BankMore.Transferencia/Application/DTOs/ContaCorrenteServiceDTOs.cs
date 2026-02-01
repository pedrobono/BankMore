namespace BankMore.Transferencia.Application.DTOs;

public record CriarMovimentoRequest(
    string RequestId,
    Guid? ContaId,
    decimal Valor,
    string Tipo
);

public record ErroResponse(
    string Message,
    string FailureType
);
