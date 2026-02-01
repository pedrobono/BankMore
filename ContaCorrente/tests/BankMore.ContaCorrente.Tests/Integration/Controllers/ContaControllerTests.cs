using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Xunit;

namespace BankMore.ContaCorrente.Tests.IntegrationTests {
    // Substituímos o 'TestFixture' pelo WebApplicationFactory padrão do .NET
    public class ContaControllerTests : IClassFixture<CustomWebApplicationFactory> {
        private readonly HttpClient _client;

        public ContaControllerTests(CustomWebApplicationFactory factory) {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CriarConta_ComDadosValidos_DeveRetornar201Created() {
            // Arrange - Campos exatos do seu CriarContaCommand no Swagger
            var novaConta = new {
                cpf = "52998224725",
                senha = "senha123",
                nomeTitular = "Pedro Bono Teste"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Conta", novaConta);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            // Valida se o corpo contém o número da conta (conforme seu Swagger)
            var resultado = await response.Content.ReadFromJsonAsync<dynamic>();
            Assert.NotNull(resultado?.GetProperty("numeroConta").GetString());
        }

        [Fact]
        public async Task CriarConta_ComCpfInvalido_DeveRetornar400BadRequest() {
            // Arrange - CPF vazio para disparar o FluentValidation
            var contaInvalida = new {
                cpf = "",
                senha = "123",
                nomeTitular = "Erro"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Conta", contaInvalida);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}