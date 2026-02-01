using Xunit;
using System.Threading;
using System;
using System.Threading.Tasks;
using BankMore.ContaCorrente.Application.Handlers;
using BankMore.ContaCorrente.Application.Commands;
using BankMore.ContaCorrente.Infrastructure.Persistence;
using BankMore.ContaCorrente.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using BankMore.ContaCorrente.Domain.Exceptions;

namespace BankMore.ContaCorrente.Tests.UnitTests
{
    public class CriarContaHandlerTests
    {
        private readonly DataBaseContext _context;
        private readonly CriarContaHandler _handler;

        public CriarContaHandlerTests()
        {
            var options = new DbContextOptionsBuilder<DataBaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new DataBaseContext(options);
            _handler = new CriarContaHandler(_context);
        }

        [Fact]
        public async Task Handle_DadosValidos_DeveRetornarNumeroDaContaGerado()
        {
            // Arrange
            var command = new CriarContaCommand 
            { 
                Cpf = "52998224725", 
                NomeTitular = "Pedro Bono", 
                Senha = "senha123" 
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(result));
            Assert.Contains("-", result);
            
            var todasContas = await _context.ContaCorrente.ToListAsync();
            var contaSalva = todasContas.FirstOrDefault(c => $"{c.Numero}-{c.Numero % 10}" == result);
            Assert.NotNull(contaSalva);
            Assert.Equal("Pedro Bono", contaSalva.Nome);
        }

        [Fact]
        public async Task Handle_CpfDeveSerHasheado()
        {
            // Arrange
            var cpfOriginal = "52998224725";
            var command = new CriarContaCommand 
            { 
                Cpf = cpfOriginal, 
                NomeTitular = "Test Hash", 
                Senha = "senha123" 
            };

            // Act
            var numeroConta = await _handler.Handle(command, CancellationToken.None);
            var todasContas = await _context.ContaCorrente.ToListAsync();
            var contaSalva = todasContas.FirstOrDefault(c => $"{c.Numero}-{c.Numero % 10}" == numeroConta);

            // Assert
            Assert.NotNull(contaSalva);
            Assert.NotEqual(cpfOriginal, contaSalva.Salt); // CPF não deve estar em texto plano
            Assert.True(BCrypt.Net.BCrypt.Verify(cpfOriginal, contaSalva.Salt)); // Deve validar com BCrypt
        }

        [Fact]
        public async Task Handle_SenhaDeveSerHasheada()
        {
            // Arrange
            var senhaOriginal = "senha123";
            var command = new CriarContaCommand 
            { 
                Cpf = "52998224725", 
                NomeTitular = "Test Senha", 
                Senha = senhaOriginal 
            };

            // Act
            var numeroConta = await _handler.Handle(command, CancellationToken.None);
            var todasContas = await _context.ContaCorrente.ToListAsync();
            var contaSalva = todasContas.FirstOrDefault(c => $"{c.Numero}-{c.Numero % 10}" == numeroConta);

            // Assert
            Assert.NotNull(contaSalva);
            Assert.NotEqual(senhaOriginal, contaSalva.Senha);
            Assert.True(BCrypt.Net.BCrypt.Verify(senhaOriginal, contaSalva.Senha));
        }

        [Fact]
        public async Task Handle_CpfJaExistente_DeveLancarExcecao()
        {
            // Arrange
            var cpfOriginal = "11122233344";
            var cpfHash = BCrypt.Net.BCrypt.HashPassword(cpfOriginal);
            
            _context.ContaCorrente.Add(new Conta 
            { 
                IdContaCorrente = Guid.NewGuid(), 
                Salt = cpfHash, 
                Numero = 12345, 
                Nome = "Existente", 
                Senha = BCrypt.Net.BCrypt.HashPassword("senha"), 
                Ativo = 1 
            });
            await _context.SaveChangesAsync();

            var command = new CriarContaCommand { Cpf = cpfOriginal, NomeTitular = "Repetido", Senha = "123" };

            // Act & Assert
            await Assert.ThrowsAsync<BusinessException>(() => 
                _handler.Handle(command, CancellationToken.None));
        }
    }
}
