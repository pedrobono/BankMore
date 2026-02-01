namespace BankMore.Transferencia.Controllers.DTOs;

public record CriarTransferenciaRequest(
    string RequestId,
    string NumeroContaDestino,
    decimal Valor
);
