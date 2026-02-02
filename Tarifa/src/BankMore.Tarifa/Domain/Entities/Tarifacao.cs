namespace BankMore.Tarifa.Domain.Entities;

public class Tarifacao
{
    public Guid IdTarifacao { get; set; }
    public Guid IdContaCorrente { get; set; }
    public decimal ValorTarifado { get; set; }
    public DateTime DataHoraTarifacao { get; set; }
    public string RequestId { get; set; } = string.Empty;

    public Tarifacao() { }

    public Tarifacao(Guid idContaCorrente, decimal valorTarifado, string requestId)
    {
        IdTarifacao = Guid.NewGuid();
        IdContaCorrente = idContaCorrente;
        ValorTarifado = valorTarifado;
        DataHoraTarifacao = DateTime.UtcNow;
        RequestId = requestId;
    }
}
