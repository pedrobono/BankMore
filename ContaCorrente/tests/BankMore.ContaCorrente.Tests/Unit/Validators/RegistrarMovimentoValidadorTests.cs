using Xunit;
using BankMore.ContaCorrente.Application.Commands;
using BankMore.ContaCorrente.Application.Validators;
using System;
using System.Threading.Tasks;

namespace BankMore.ContaCorrente.Tests.Unit.Validators
{
    public class RegistrarMovimentoValidadorTests
    {
        private readonly RegistrarMovimentoValidator _validator;

        public RegistrarMovimentoValidadorTests()
        {
            _validator = new RegistrarMovimentoValidator();
        }

        [Fact]
        public async Task Validate_ValidCommand_ShouldPass()
        {
            // Arrange
            var command = new RegistrarMovimentoCommand
            {
                RequestId = Guid.NewGuid().ToString(),
                ContaId = Guid.NewGuid(),
                Valor = 100m,
                Tipo = "C"
            };

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task Validate_NegativeValue_ShouldFail()
        {
            // Arrange
            var command = new RegistrarMovimentoCommand
            {
                RequestId = Guid.NewGuid().ToString(),
                ContaId = Guid.NewGuid(),
                Valor = -100m,
                Tipo = "C"
            };

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task Validate_InvalidTipo_ShouldFail()
        {
            // Arrange
            var command = new RegistrarMovimentoCommand
            {
                RequestId = Guid.NewGuid().ToString(),
                ContaId = Guid.NewGuid(),
                Valor = 100m,
                Tipo = "X"
            };

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            Assert.False(result.IsValid);
        }
    }
}
