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
    public class TokenValidationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public TokenValidationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Movimento_SemToken_DeveRetornar401()
        {
            // Arrange
            var movimento = new RegistrarMovimentoCommand
            {
                RequestId = Guid.NewGuid().ToString(),
                Tipo = "C",
                Valor = 100
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Movimento", movimento);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Movimento_TokenInvalido_DeveRetornar401()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "token_completamente_invalido");
            
            var movimento = new RegistrarMovimentoCommand
            {
                RequestId = Guid.NewGuid().ToString(),
                Tipo = "C",
                Valor = 100
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Movimento", movimento);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Saldo_SemToken_DeveRetornar401()
        {
            // Act
            var response = await _client.GetAsync("/api/Saldo");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Saldo_TokenInvalido_DeveRetornar401()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "token_invalido");

            // Act
            var response = await _client.GetAsync("/api/Saldo");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task InativarConta_SemToken_DeveRetornar401()
        {
            // Arrange
            var inativarCommand = new InativarContaCommand { Senha = "senha123" };

            // Act
            var request = new HttpRequestMessage(HttpMethod.Patch, "/api/Conta/me/inactivate")
            {
                Content = JsonContent.Create(inativarCommand)
            };
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task InativarConta_TokenInvalido_DeveRetornar401()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "token_invalido");
            var inativarCommand = new InativarContaCommand { Senha = "senha123" };

            // Act
            var request = new HttpRequestMessage(HttpMethod.Patch, "/api/Conta/me/inactivate")
            {
                Content = JsonContent.Create(inativarCommand)
            };
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
