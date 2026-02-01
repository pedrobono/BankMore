using System.ComponentModel.DataAnnotations;

namespace BankMore.ContaCorrente.Domain.Entities {
    public class Idempotencia {
        [Key]
        public Guid ChaveIdempotencia { get; set; }
        public string Requisicao { get; set; }
        public string Resultado { get; set; }
    }
}
