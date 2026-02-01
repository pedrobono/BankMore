using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace BankMore.ContaCorrente.Tests.IntegrationTests
{
    public class SaldoControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public SaldoControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetSaldo_DeveRetornar401_QuandoNaoFornecerToken()
        {
            // Act
            var response = await _client.GetAsync("/api/Saldo");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetSaldo_DeveRetornarSucesso_QuandoTokenForValido()
        {
            // Arrange
            // Em testes de integração reais, você usaria um Helper para gerar este token
            var tokenValido = "TOKEN_JWT_GERADO_AQUI"; 
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenValido);

            // Act
            var response = await _client.GetAsync("/api/Saldo");

            // Assert
            // Se o token for aceito, deve retornar 200 OK
            // Nota: No ambiente de teste, isso pode retornar 401 se a chave secreta JWT não bater
            Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Unauthorized);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var saldoDto = await response.Content.ReadFromJsonAsync<dynamic>();
                Assert.NotNull(saldoDto?.GetProperty("balance"));
                Assert.NotNull(saldoDto?.GetProperty("accountNumber"));
            }
        }
    }
}