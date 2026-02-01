namespace BankMore.ContaCorrente.Tests.Integration.Controllers
{
    public class CriarContaResponse
    {
        public string NumeroConta { get; set; }
    }

    public class LoginResponse
    {
        public string Token { get; set; }
    }

    public class ErrorResponse
    {
        public string Message { get; set; }
        public string FailureType { get; set; }
    }

    public class ResolverContaResult
    {
        public System.Guid ContaId { get; set; }
        public string NumeroConta { get; set; }
    }
}
