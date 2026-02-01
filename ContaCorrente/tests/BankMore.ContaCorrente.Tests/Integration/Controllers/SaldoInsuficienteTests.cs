using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BankMore.ContaCorrente.Application.Commands;
using BankMore.ContaCorrente.Tests.IntegrationTests;
using Xunit;

namespace BankMore.ContaCorrente.Tests.Integration.Controllers
{
    public class SaldoInsuficienteTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public SaldoInsuficienteTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Movimento_DebitoComSaldoInsuficiente_DeveRetornar400()
        {
            // Arrange - Criar conta
            var contaCommand = new CriarContaCommand
            {
                Cpf = "52998224725",
                NomeTitular = "Usuario Teste",
                Senha = "senha123"
            };

            var criarContaResponse = await _client.PostAsJsonAsync("/api/Conta", contaCommand);
            var criarContaResult = await criarContaResponse.Content.ReadFromJsonAsync<CriarContaResponse>();
            var numeroConta = criarContaResult!.NumeroConta;

            // Login
            var login = await _client.PostAsJsonAsync("/api/Auth/login", new LoginCommand
            {
                CpfOrNumeroConta = "52998224725",
                Senha = "senha123"
            });
            var tokenResult = await login.Content.ReadFromJsonAsync<LoginResponse>();
            var token = tokenResult!.Token;

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Creditar 100 (sem especificar contaId, usa do token)
            await _client.PostAsJsonAsync("/api/Movimento", new RegistrarMovimentoCommand
            {
                RequestId = Guid.NewGuid().ToString(),
                Tipo = "C",
                Valor = 100
            });

            // Act - Tentar debitar 150 (mais que o saldo)
            var debitoResponse = await _client.PostAsJsonAsync("/api/Movimento", new RegistrarMovimentoCommand
            {
                RequestId = Guid.NewGuid().ToString(),
                Tipo = "D",
                Valor = 150
            });

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, debitoResponse.StatusCode);
            var erro = await debitoResponse.Content.ReadFromJsonAsync<ErrorResponse>();
            Assert.Equal("INSUFFICIENT_BALANCE", erro!.FailureType);
        }

        [Fact]
        public async Task Movimento_DebitoComSaldoSuficiente_DevePermitir()
        {
            // Arrange - Criar conta
            var contaCommand = new CriarContaCommand
            {
                Cpf = "40643990054",
                NomeTitular = "Usuario Teste 2",
                Senha = "senha123"
            };

            var criarContaResponse = await _client.PostAsJsonAsync("/api/Conta", contaCommand);
            var criarContaResult = await criarContaResponse.Content.ReadFromJsonAsync<CriarContaResponse>();
            var numeroConta = criarContaResult!.NumeroConta;

            // Login
            var login = await _client.PostAsJsonAsync("/api/Auth/login", new LoginCommand
            {
                CpfOrNumeroConta = "40643990054",
                Senha = "senha123"
            });
            var tokenResult = await login.Content.ReadFromJsonAsync<LoginResponse>();
            var token = tokenResult!.Token;

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Creditar 100 (sem especificar contaId, usa do token)
            await _client.PostAsJsonAsync("/api/Movimento", new RegistrarMovimentoCommand
            {
                RequestId = Guid.NewGuid().ToString(),
                Tipo = "C",
                Valor = 100
            });

            // Act - Debitar 50 (menos que o saldo)
            var debitoResponse = await _client.PostAsJsonAsync("/api/Movimento", new RegistrarMovimentoCommand
            {
                RequestId = Guid.NewGuid().ToString(),
                Tipo = "D",
                Valor = 50
            });

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, debitoResponse.StatusCode);
        }
    }
}
