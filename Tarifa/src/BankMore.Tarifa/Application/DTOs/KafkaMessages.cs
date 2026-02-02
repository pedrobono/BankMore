namespace BankMore.Tarifa.Application.DTOs;

public record TransferenciaRealizadaMessage(
    string RequestId,
    Guid IdContaOrigem
);

public record TarifacaoRealizadaMessage(
    Guid IdContaCorrente,
    decimal ValorTarifado
);
