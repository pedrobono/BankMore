using Xunit;
using BankMore.ContaCorrente.Application.Commands;
using BankMore.ContaCorrente.Application.Validators;
using System.Threading.Tasks;

namespace BankMore.ContaCorrente.Tests.Unit.Validators
{
    public class CriarContaValidadorTests
    {
        private readonly CriarContaValidador _validator;

        public CriarContaValidadorTests()
        {
            _validator = new CriarContaValidador();
        }

        [Fact]
        public async Task Validate_ValidCommand_ShouldPass()
        {
            // Arrange
            var command = new CriarContaCommand
            {
                Cpf = "52998224725",
                Senha = "senha123",
                NomeTitular = "Pedro Bono"
            };

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task Validate_EmptyCpf_ShouldFail()
        {
            // Arrange
            var command = new CriarContaCommand
            {
                Cpf = "",
                Senha = "senha123",
                NomeTitular = "Pedro Bono"
            };

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task Validate_EmptySenha_ShouldFail()
        {
            // Arrange
            var command = new CriarContaCommand
            {
                Cpf = "52998224725",
                Senha = "",
                NomeTitular = "Pedro Bono"
            };

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            Assert.False(result.IsValid);
        }
    }
}
