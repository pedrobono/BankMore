using Xunit;
using BankMore.ContaCorrente.Domain.ValueObjects;
using BankMore.ContaCorrente.Domain.Exceptions;

namespace BankMore.ContaCorrente.Tests.Unit.ValueObjects
{
    public class CpfTests
    {
        [Theory]
        [InlineData("52998224725")]
        [InlineData("11144477735")]
        public void Cpf_ValidCpf_ShouldCreateSuccessfully(string cpfValido)
        {
            // Act
            var cpf = new Cpf(cpfValido);

            // Assert
            Assert.Equal(cpfValido, cpf.Valor);
        }

        [Theory]
        [InlineData("00000000000")]
        [InlineData("11111111111")]
        [InlineData("12345678901")]
        [InlineData("123")]
        [InlineData("")]
        public void Cpf_InvalidCpf_ShouldThrowBusinessException(string cpfInvalido)
        {
            // Act & Assert
            Assert.Throws<BusinessException>(() => new Cpf(cpfInvalido));
        }
    }
}
