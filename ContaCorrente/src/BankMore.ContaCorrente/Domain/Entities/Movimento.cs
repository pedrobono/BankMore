using System.ComponentModel.DataAnnotations;

namespace BankMore.ContaCorrente.Domain.Entities {
    public class Movimento {
        [Key]
        public Guid IdMovimento { get; set; }
        public Guid IdContaCorrente { get; set; }
        public Conta Conta { get; set; }
        public string DataMovimento { get; set; }
        public string TipoMovimento { get; set; }
        public decimal Valor { get; set; }
    }
}
