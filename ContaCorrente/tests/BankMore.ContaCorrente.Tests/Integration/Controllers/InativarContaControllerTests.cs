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
    public class InativarContaControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public InativarContaControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task InativarConta_ShouldReturn204_WhenValidPassword()
        {
            // Arrange
            var cpf = "40643990054";
            var criarContaCommand = new CriarContaCommand
            {
                Cpf = cpf,
                NomeTitular = "Test User Inativar",
                Senha = "senha123"
            };

            var criarResponse = await _client.PostAsJsonAsync("/api/Conta", criarContaCommand);
            Assert.Equal(HttpStatusCode.Created, criarResponse.StatusCode);

            var loginCommand = new LoginCommand
            {
                CpfOrNumeroConta = cpf,
                Senha = "senha123"
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/Auth/login", loginCommand);
            var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
            var token = loginResult!.Token;

            var inativarCommand = new InativarContaCommand { Senha = "senha123" };
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            var request = new HttpRequestMessage(HttpMethod.Patch, "/api/Conta/me/inactivate")
            {
                Content = JsonContent.Create(inativarCommand)
            };
            var inativarResponse = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.NoContent, inativarResponse.StatusCode);
        }

        [Fact]
        public async Task InativarConta_ShouldReturn401_WhenInvalidPassword()
        {
            // Arrange
            var cpf = "90816518033";
            var criarContaCommand = new CriarContaCommand
            {
                Cpf = cpf,
                NomeTitular = "Test User 2",
                Senha = "senha123"
            };

            await _client.PostAsJsonAsync("/api/Conta", criarContaCommand);

            var loginCommand = new LoginCommand
            {
                CpfOrNumeroConta = cpf,
                Senha = "senha123"
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/Auth/login", loginCommand);
            var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
            var token = loginResult!.Token;

            var inativarCommand = new InativarContaCommand { Senha = "senhaErrada" };
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            var request = new HttpRequestMessage(HttpMethod.Patch, "/api/Conta/me/inactivate")
            {
                Content = JsonContent.Create(inativarCommand)
            };
            var inativarResponse = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Unauthorized, inativarResponse.StatusCode);
        }

        [Fact]
        public async Task InativarConta_ShouldReturn403_WhenNoToken()
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
        public async Task InativarConta_ShouldReturn403_WhenInvalidToken()
        {
            // Arrange
            var inativarCommand = new InativarContaCommand { Senha = "senha123" };
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "token_invalido");

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
