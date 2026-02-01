using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using BankMore.ContaCorrente.Application.Handlers;
using BankMore.ContaCorrente.Application.Queries;
using BankMore.ContaCorrente.Infrastructure.Persistence;
using BankMore.ContaCorrente.Domain.Entities;
using BankMore.ContaCorrente.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BankMore.ContaCorrente.Tests.Unit.Handlers
{
    public class ObterSaldoHandlerTests
    {
        private readonly DataBaseContext _context;
        private readonly ObterSaldoHandler _handler;

        public ObterSaldoHandlerTests()
        {
            var options = new DbContextOptionsBuilder<DataBaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new DataBaseContext(options);
            _handler = new ObterSaldoHandler(_context);
        }

        [Fact]
        public async Task Handle_ShouldReturnSaldo_WithDataHoraConsulta()
        {
            // Arrange
            var contaId = Guid.NewGuid();
            _context.ContaCorrente.Add(new Conta
            {
                IdContaCorrente = contaId,
                Salt = "12345678901",
                Numero = 12345,
                Nome = "Test User",
                Senha = "hash",
                Ativo = 1
            });
            await _context.SaveChangesAsync();

            var query = new ObterSaldoQuery { ContaId = contaId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("12345-5", result.NumeroConta);
            Assert.Equal("Test User", result.NomeTitular);
            Assert.True(result.DataHoraConsulta <= DateTime.UtcNow);
            Assert.Equal(0, result.Saldo);
        }

        [Fact]
        public async Task Handle_ShouldThrowInvalidAccount_WhenAccountNotFound()
        {
            // Arrange
            var query = new ObterSaldoQuery { ContaId = Guid.NewGuid() };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessException>(() =>
                _handler.Handle(query, CancellationToken.None));
            Assert.Equal("INVALID_ACCOUNT", exception.FailureType);
        }

        [Fact]
        public async Task Handle_ShouldThrowInactiveAccount_WhenAccountIsInactive()
        {
            // Arrange
            var contaId = Guid.NewGuid();
            _context.ContaCorrente.Add(new Conta
            {
                IdContaCorrente = contaId,
                Salt = "12345678901",
                Numero = 12345,
                Nome = "Test User",
                Senha = "hash",
                Ativo = 0
            });
            await _context.SaveChangesAsync();

            var query = new ObterSaldoQuery { ContaId = contaId };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessException>(() =>
                _handler.Handle(query, CancellationToken.None));
            Assert.Equal("INACTIVE_ACCOUNT", exception.FailureType);
        }

        [Fact]
        public async Task Handle_ShouldCalculateSaldo_WithCreditsAndDebits()
        {
            // Arrange
            var contaId = Guid.NewGuid();
            _context.ContaCorrente.Add(new Conta
            {
                IdContaCorrente = contaId,
                Salt = "12345678901",
                Numero = 12345,
                Nome = "Test User",
                Senha = "hash",
                Ativo = 1
            });

            _context.Movimento.AddRange(
                new Movimento { IdMovimento = Guid.NewGuid(), IdContaCorrente = contaId, TipoMovimento = "C", Valor = 100m, DataMovimento = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss") },
                new Movimento { IdMovimento = Guid.NewGuid(), IdContaCorrente = contaId, TipoMovimento = "C", Valor = 50m, DataMovimento = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss") },
                new Movimento { IdMovimento = Guid.NewGuid(), IdContaCorrente = contaId, TipoMovimento = "D", Valor = 30m, DataMovimento = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss") }
            );
            await _context.SaveChangesAsync();

            var query = new ObterSaldoQuery { ContaId = contaId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(120m, result.Saldo); // 100 + 50 - 30
        }
    }
}
