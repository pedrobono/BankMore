namespace BankMore.ContaCorrente.Domain.Exceptions
{
    public class BusinessException : Exception
    {
        public string FailureType { get; private set; }

        public BusinessException(string message, string failureType)
            : base(message)
        {
            FailureType = failureType;
        }
    }
}
