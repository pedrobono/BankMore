using System.ComponentModel.DataAnnotations;

namespace BankMore.ContaCorrente.Domain.Entities;

public class Conta {
    [Key]
    public Guid IdContaCorrente { get; set; }
    public int Numero { get; set; }
    public string Nome { get; set; }
    public int Ativo { get; set; }
    public string Senha { get; set; }
    public string Salt { get; set; }
}