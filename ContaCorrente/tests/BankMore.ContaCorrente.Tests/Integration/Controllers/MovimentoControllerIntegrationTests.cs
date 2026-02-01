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
    public class MovimentoControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public MovimentoControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task RegistrarMovimento_ShouldReturn204_WhenValidCredit()
        {
            _factory.ResetDatabase();
            var client = _factory.CreateClient();
            var cpf = "96915209077";
            var criarContaCommand = new CriarContaCommand
            {
                Cpf = cpf,
                NomeTitular = "Test User Movimento",
                Senha = "senha123"
            };

            var criarResponse = await client.PostAsJsonAsync("/api/Conta", criarContaCommand);
            Assert.True(criarResponse.IsSuccessStatusCode, $"Falha ao criar conta: {await criarResponse.Content.ReadAsStringAsync()}");
            
            var criarResult = await criarResponse.Content.ReadFromJsonAsync<CriarContaResponse>();
            Assert.NotNull(criarResult);
            var numeroConta = criarResult.NumeroConta;

            var loginCommand = new LoginCommand
            {
                CpfOrNumeroConta = cpf,
                Senha = "senha123"
            };

            var loginResponse = await client.PostAsJsonAsync("/api/Auth/login", loginCommand);
            Assert.True(loginResponse.IsSuccessStatusCode, $"Falha ao fazer login: {await loginResponse.Content.ReadAsStringAsync()}");
            
            var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
            Assert.NotNull(loginResult);
            Assert.NotNull(loginResult.Token);
            var token = loginResult.Token;

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            // Resolver conta para obter o ID
            var resolverResponse = await client.PostAsJsonAsync("/api/Conta/resolve", new { numeroConta });
            Assert.True(resolverResponse.IsSuccessStatusCode, $"Falha ao resolver conta: {await resolverResponse.Content.ReadAsStringAsync()}");
            
            var resolverResult = await resolverResponse.Content.ReadFromJsonAsync<ResolverContaResponse>();
            Assert.NotNull(resolverResult);
            var contaId = resolverResult.ContaId;

            var movimentoCommand = new RegistrarMovimentoCommand
            {
                RequestId = Guid.NewGuid().ToString(),
                ContaId = contaId,
                Valor = 100m,
                Tipo = "C"
            };

            var response = await client.PostAsJsonAsync("/api/Movimento", movimentoCommand);

            Assert.True(response.IsSuccessStatusCode, $"Falha ao registrar movimento: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task RegistrarMovimento_ShouldUseTokenAccount_WhenNumeroContaNotProvided()
        {
            _factory.ResetDatabase();
            var client = _factory.CreateClient();
            var cpf = "44009776099";
            var criarContaCommand = new CriarContaCommand
            {
                Cpf = cpf,
                NomeTitular = "Test User Token",
                Senha = "senha123"
            };

            var criarResponse = await client.PostAsJsonAsync("/api/Conta", criarContaCommand);
            var criarContent = await criarResponse.Content.ReadAsStringAsync();
            Assert.True(criarResponse.IsSuccessStatusCode, $"Falha ao criar conta: {criarContent}");

            var criarResult = await criarResponse.Content.ReadFromJsonAsync<CriarContaResponse>();
            Assert.NotNull(criarResult);
            Assert.False(string.IsNullOrWhiteSpace(criarResult.NumeroConta), "Número da conta está vazio");
            var numeroConta = criarResult.NumeroConta;

            var loginCommand = new LoginCommand
            {
                CpfOrNumeroConta = cpf,
                Senha = "senha123"
            };

            var loginResponse = await client.PostAsJsonAsync("/api/Auth/login", loginCommand);
            Assert.True(loginResponse.IsSuccessStatusCode, $"Falha ao fazer login: {await loginResponse.Content.ReadAsStringAsync()}");
            
            var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
            Assert.NotNull(loginResult);
            Assert.NotNull(loginResult.Token);
            var token = loginResult.Token;

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Movimento sem ContaId usa a conta do token
            var movimentoCommand = new RegistrarMovimentoCommand
            {
                RequestId = Guid.NewGuid().ToString(),
                Valor = 50m,
                Tipo = "C"
            };

            var response = await client.PostAsJsonAsync("/api/Movimento", movimentoCommand);

            Assert.True(response.IsSuccessStatusCode, $"Falha ao registrar movimento: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }

}
