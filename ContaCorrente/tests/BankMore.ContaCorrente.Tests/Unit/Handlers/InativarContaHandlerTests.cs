using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using BankMore.ContaCorrente.Application.Handlers;
using BankMore.ContaCorrente.Application.Commands;
using BankMore.ContaCorrente.Infrastructure.Persistence;
using BankMore.ContaCorrente.Domain.Entities;
using BankMore.ContaCorrente.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BankMore.ContaCorrente.Tests.Unit.Handlers
{
    public class InativarContaHandlerTests
    {
        private readonly DataBaseContext _context;
        private readonly InativarContaHandler _handler;

        public InativarContaHandlerTests()
        {
            var options = new DbContextOptionsBuilder<DataBaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new DataBaseContext(options);
            _handler = new InativarContaHandler(_context);
        }

        [Fact]
        public async Task Handle_ShouldInactivateAccount_WhenValidPassword()
        {
            // Arrange
            var contaId = Guid.NewGuid();
            var senha = "senha123";
            var senhaHash = BCrypt.Net.BCrypt.HashPassword(senha);

            _context.ContaCorrente.Add(new Conta
            {
                IdContaCorrente = contaId,
                Salt = "12345678901",
                Numero = 12345,
                Nome = "Test User",
                Senha = senhaHash,
                Ativo = 1
            });
            await _context.SaveChangesAsync();

            var command = new InativarContaCommand
            {
                ContaId = contaId,
                Senha = senha
            };

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            var conta = await _context.ContaCorrente.FindAsync(contaId);
            Assert.Equal(0, conta.Ativo);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenInvalidPassword()
        {
            // Arrange
            var contaId = Guid.NewGuid();
            var senhaHash = BCrypt.Net.BCrypt.HashPassword("senha123");

            _context.ContaCorrente.Add(new Conta
            {
                IdContaCorrente = contaId,
                Salt = "12345678901",
                Numero = 12345,
                Nome = "Test User",
                Senha = senhaHash,
                Ativo = 1
            });
            await _context.SaveChangesAsync();

            var command = new InativarContaCommand
            {
                ContaId = contaId,
                Senha = "senhaErrada"
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenAccountNotFound()
        {
            // Arrange
            var command = new InativarContaCommand
            {
                ContaId = Guid.NewGuid(),
                Senha = "senha123"
            };

            // Act & Assert
            await Assert.ThrowsAsync<BusinessException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }
    }
}
