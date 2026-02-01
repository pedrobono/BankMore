namespace BankMore.ContaCorrente.Application.DTOs {
    public class SaldoDto {
        public string NumeroConta { get; set; }
        public string NomeTitular { get; set; }
        public DateTime DataHoraConsulta { get; set; }
        public decimal Saldo { get; set; }
    }
}
