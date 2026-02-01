using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Xunit;
using BankMore.ContaCorrente.Application.Handlers;
using BankMore.ContaCorrente.Application.Commands;
using BankMore.ContaCorrente.Infrastructure.Persistence;
using BankMore.ContaCorrente.Domain.Entities;
using BankMore.ContaCorrente.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace BankMore.ContaCorrente.Tests.Unit.Handlers
{
    public class RegistrarMovimentoHandlerTests
    {
        private DataBaseContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<DataBaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new DataBaseContext(options);
        }

        [Fact]
        public async Task Handle_ShouldRegisterMovement_WhenValidMovement()
        {
            using var context = CreateContext();
            var logger = new Mock<ILogger<RegistrarMovimentoHandler>>();
            var handler = new RegistrarMovimentoHandler(context, logger.Object);
            
            // Arrange
            var contaId = Guid.NewGuid();
            context.ContaCorrente.Add(new Conta
            {
                IdContaCorrente = contaId,
                Salt = "12345678901",
                Numero = 12345,
                Nome = "Test User",
                Senha = "hash",
                Ativo = 1
            });
            await context.SaveChangesAsync();

            var command = new RegistrarMovimentoCommand
            {
                RequestId = Guid.NewGuid().ToString(),
                ContaId = contaId,
                Valor = 100m,
                Tipo = "C"
            };

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            var movimento = await context.Movimento.FirstOrDefaultAsync(m => m.IdContaCorrente == contaId);
            Assert.NotNull(movimento);
        }

        [Fact]
        public async Task Handle_ShouldNotRegisterMovement_WhenInvalidAccount()
        {
            using var context = CreateContext();
            var logger = new Mock<ILogger<RegistrarMovimentoHandler>>();
            var handler = new RegistrarMovimentoHandler(context, logger.Object);
            
            // Arrange
            var command = new RegistrarMovimentoCommand
            {
                RequestId = Guid.NewGuid().ToString(),
                ContaId = Guid.NewGuid(),
                Valor = 100m,
                Tipo = "C"
            };

            // Act & Assert
            await Assert.ThrowsAsync<BusinessException>(() =>
                handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_IdempotentRequest_ShouldNotDuplicate()
        {
            using var context = CreateContext();
            var logger = new Mock<ILogger<RegistrarMovimentoHandler>>();
            var handler = new RegistrarMovimentoHandler(context, logger.Object);
            
            // Arrange
            var contaId = Guid.NewGuid();
            context.ContaCorrente.Add(new Conta
            {
                IdContaCorrente = contaId,
                Salt = "12345678901",
                Numero = 12345,
                Nome = "Test User",
                Senha = "hash",
                Ativo = 1
            });
            await context.SaveChangesAsync();

            var requestId = Guid.NewGuid().ToString();
            var command = new RegistrarMovimentoCommand
            {
                RequestId = requestId,
                ContaId = contaId,
                Valor = 100m,
                Tipo = "C"
            };

            // Act
            await handler.Handle(command, CancellationToken.None);
            await handler.Handle(command, CancellationToken.None);

            // Assert
            var movimentos = await context.Movimento.Where(m => m.IdContaCorrente == contaId).ToListAsync();
            Assert.Single(movimentos);
        }

        [Fact]
        public async Task Handle_InactiveAccount_ShouldThrowException()
        {
            using var context = CreateContext();
            var logger = new Mock<ILogger<RegistrarMovimentoHandler>>();
            var handler = new RegistrarMovimentoHandler(context, logger.Object);
            
            // Arrange
            var contaId = Guid.NewGuid();
            context.ContaCorrente.Add(new Conta
            {
                IdContaCorrente = contaId,
                Salt = "12345678901",
                Numero = 12345,
                Nome = "Test User",
                Senha = "hash",
                Ativo = 0
            });
            await context.SaveChangesAsync();

            var command = new RegistrarMovimentoCommand
            {
                RequestId = Guid.NewGuid().ToString(),
                ContaId = contaId,
                Valor = 100m,
                Tipo = "C"
            };

            // Act & Assert
            await Assert.ThrowsAsync<BusinessException>(() =>
                handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowInvalidValue_WhenNegativeValue()
        {
            using var context = CreateContext();
            var logger = new Mock<ILogger<RegistrarMovimentoHandler>>();
            var handler = new RegistrarMovimentoHandler(context, logger.Object);
            
            // Arrange
            var contaId = Guid.NewGuid();
            context.ContaCorrente.Add(new Conta
            {
                IdContaCorrente = contaId,
                Salt = "12345678901",
                Numero = 12345,
                Nome = "Test User",
                Senha = "hash",
                Ativo = 1
            });
            await context.SaveChangesAsync();

            var command = new RegistrarMovimentoCommand
            {
                RequestId = Guid.NewGuid().ToString(),
                ContaId = contaId,
                Valor = -100m,
                Tipo = "C"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessException>(() =>
                handler.Handle(command, CancellationToken.None));
            Assert.Equal("INVALID_VALUE", exception.FailureType);
        }

        [Fact]
        public async Task Handle_ShouldThrowInvalidType_WhenInvalidTipo()
        {
            using var context = CreateContext();
            var logger = new Mock<ILogger<RegistrarMovimentoHandler>>();
            var handler = new RegistrarMovimentoHandler(context, logger.Object);
            
            // Arrange
            var contaId = Guid.NewGuid();
            context.ContaCorrente.Add(new Conta
            {
                IdContaCorrente = contaId,
                Salt = "12345678901",
                Numero = 12345,
                Nome = "Test User",
                Senha = "hash",
                Ativo = 1
            });
            await context.SaveChangesAsync();

            var command = new RegistrarMovimentoCommand
            {
                RequestId = Guid.NewGuid().ToString(),
                ContaId = contaId,
                Valor = 100m,
                Tipo = "X"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessException>(() =>
                handler.Handle(command, CancellationToken.None));
            Assert.Equal("INVALID_TYPE", exception.FailureType);
        }
    }
}
