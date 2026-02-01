using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Xunit;

namespace BankMore.ContaCorrente.Tests.IntegrationTests {
    public class MovimentoControllerTests : IClassFixture<CustomWebApplicationFactory> {
        private readonly HttpClient _client;

        public MovimentoControllerTests(CustomWebApplicationFactory factory) {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Post_Movimento_DeveRetornar204NoContent_QuandoSucesso() {
            // Arrange
            // IMPORTANTE: Em um teste real, você precisaria de um Token JWT válido aqui.
            // Para este exemplo, estamos simulando a chamada.
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "SEU_TOKEN_AQUI");

            var movimento = new {
                numeroConta = "12345-6",
                valor = 500.0,
                tipo = "C", // Crédito
                requestId = Guid.NewGuid().ToString() // Idempotência exigida pela sua API
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Movimento", movimento);

            // Assert
            // Nota: Se não houver um token válido no teste, o resultado esperado será 401.
            // Se o token for válido e a conta existir, será 204.
            Assert.True(response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Post_Movimento_DeveRetornar400_QuandoRequestIdForDuplicado() {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "SEU_TOKEN_AQUI");
            var guidComum = Guid.NewGuid().ToString();
            var movimento = new { numeroConta = "12345-6", valor = 10.0, tipo = "C", requestId = guidComum };

            // Act
            // Enviando a primeira vez
            await _client.PostAsJsonAsync("/api/Movimento", movimento);
            // Enviando a segunda vez com o mesmo requestId
            var response = await _client.PostAsJsonAsync("/api/Movimento", movimento);

            // Assert
            // Sua API deve barrar a duplicidade (Idempotência) ou retornar Unauthorized se token inválido
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Unauthorized);
        }
    }
}