using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;
using BankMore.ContaCorrente.Application.Handlers;
using BankMore.ContaCorrente.Application.Commands;
using BankMore.ContaCorrente.Infrastructure.Persistence;
using BankMore.ContaCorrente.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace BankMore.ContaCorrente.Tests.UnitTests
{
    public class LoginHandlerTests
    {
        private readonly DataBaseContext _context;
        private readonly Mock<IConfiguration> _configMock;
        private readonly LoginHandler _handler;

        public LoginHandlerTests()
        {
            var options = new DbContextOptionsBuilder<DataBaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new DataBaseContext(options);
            
            _configMock = new Mock<IConfiguration>();
            _configMock.Setup(c => c["JwtSettings:SecretKey"]).Returns("TestSecretKeyForJWT123456789012345");
            
            _handler = new LoginHandler(_context, _configMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCredentials_ReturnsToken()
        {
            // Arrange
            var cpfOriginal = "12345678901";
            var cpfHash = BCrypt.Net.BCrypt.HashPassword(cpfOriginal);
            
            _context.ContaCorrente.Add(new Conta
            {
                IdContaCorrente = Guid.NewGuid(),
                Salt = cpfHash,
                Numero = 12345,
                Nome = "Test User",
                Senha = BCrypt.Net.BCrypt.HashPassword("password123"),
                Ativo = 1
            });
            await _context.SaveChangesAsync();

            var command = new LoginCommand { CpfOrNumeroConta = cpfOriginal, Senha = "password123" };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result.Token));
        }

        [Fact]
        public async Task Handle_LoginPorNumeroConta_ReturnsToken()
        {
            // Arrange
            var cpfOriginal = "98765432109";
            var cpfHash = BCrypt.Net.BCrypt.HashPassword(cpfOriginal);
            var numeroConta = "54321-1";
            
            _context.ContaCorrente.Add(new Conta
            {
                IdContaCorrente = Guid.NewGuid(),
                Salt = cpfHash,
                Numero = 54321,
                Nome = "Test User 2",
                Senha = BCrypt.Net.BCrypt.HashPassword("password123"),
                Ativo = 1
            });
            await _context.SaveChangesAsync();

            var command = new LoginCommand { CpfOrNumeroConta = numeroConta, Senha = "password123" };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result.Token));
        }

        [Fact]
        public async Task Handle_InvalidCredentials_ThrowsUnauthorizedException()
        {
            // Arrange
            var cpfOriginal = "12345678901";
            var cpfHash = BCrypt.Net.BCrypt.HashPassword(cpfOriginal);
            
            _context.ContaCorrente.Add(new Conta
            {
                IdContaCorrente = Guid.NewGuid(),
                Salt = cpfHash,
                Numero = 12345,
                Nome = "Test User",
                Senha = BCrypt.Net.BCrypt.HashPassword("password123"),
                Ativo = 1
            });
            await _context.SaveChangesAsync();

            var command = new LoginCommand { CpfOrNumeroConta = cpfOriginal, Senha = "wrongpassword" };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }
    }
}
