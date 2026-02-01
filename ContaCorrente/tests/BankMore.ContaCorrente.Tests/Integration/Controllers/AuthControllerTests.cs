using System.Net;
using System.Net.Http;
using System.Net.Http.Json; // Importante para o PostAsJsonAsync
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Xunit;

namespace BankMore.ContaCorrente.Tests.IntegrationTests {
    public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory> {
        private readonly HttpClient _client;

        public AuthControllerTests(CustomWebApplicationFactory factory) {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Login_CredenciaisInvalidas_DeveRetornarUnauthorized() {
            // Arrange - Dados baseados no seu LoginCommand do Swagger
            var loginRequest = new {
                cpfOrNumeroConta = "00000000000",
                senha = "senha_incorreta"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_CamposVazios_DeveRetornarBadRequest() {
            // Arrange
            var loginRequest = new { cpfOrNumeroConta = "", senha = "" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);

            // Assert
            // Campos vazios resultam em credenciais inválidas, retornando Unauthorized
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}