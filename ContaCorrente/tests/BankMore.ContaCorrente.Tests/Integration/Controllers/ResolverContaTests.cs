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
    public class ResolverContaTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ResolverContaTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task ResolverConta_ComNumeroValido_DeveRetornarContaId()
        {
            // Arrange - Criar conta
            var contaCommand = new CriarContaCommand
            {
                Cpf = "90816518033",
                NomeTitular = "Usuario Resolver",
                Senha = "senha123"
            };

            var criarContaResponse = await _client.PostAsJsonAsync("/api/Conta", contaCommand);
            var criarContaResult = await criarContaResponse.Content.ReadFromJsonAsync<CriarContaResponse>();
            var numeroConta = criarContaResult!.NumeroConta;

            // Login
            var login = await _client.PostAsJsonAsync("/api/Auth/login", new LoginCommand
            {
                CpfOrNumeroConta = "90816518033",
                Senha = "senha123"
            });
            var tokenResult = await login.Content.ReadFromJsonAsync<LoginResponse>();
            var token = tokenResult!.Token;

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act - Resolver conta
            var resolverResponse = await _client.PostAsJsonAsync("/api/Conta/resolve", new ResolverContaCommand
            {
                NumeroConta = numeroConta
            });

            // Assert
            Assert.Equal(HttpStatusCode.OK, resolverResponse.StatusCode);
            var resultado = await resolverResponse.Content.ReadFromJsonAsync<ResolverContaResult>();
            Assert.NotNull(resultado);
            Assert.NotEqual(System.Guid.Empty, resultado!.ContaId);
            Assert.Equal(numeroConta, resultado.NumeroConta);
        }

        [Fact]
        public async Task ResolverConta_ComNumeroInvalido_DeveRetornar404()
        {
            // Arrange - Login com conta existente
            var contaCommand = new CriarContaCommand
            {
                Cpf = "12345678909",
                NomeTitular = "Usuario Login",
                Senha = "senha123"
            };

            await _client.PostAsJsonAsync("/api/Conta", contaCommand);

            var login = await _client.PostAsJsonAsync("/api/Auth/login", new LoginCommand
            {
                CpfOrNumeroConta = "12345678909",
                Senha = "senha123"
            });
            var tokenResult = await login.Content.ReadFromJsonAsync<LoginResponse>();
            var token = tokenResult!.Token;

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act - Tentar resolver conta inexistente
            var resolverResponse = await _client.PostAsJsonAsync("/api/Conta/resolve", new ResolverContaCommand
            {
                NumeroConta = "99999-9"
            });

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, resolverResponse.StatusCode);
        }
    }
}
