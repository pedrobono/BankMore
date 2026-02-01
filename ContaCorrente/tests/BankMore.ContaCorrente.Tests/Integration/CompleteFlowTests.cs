using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace BankMore.ContaCorrente.Tests.IntegrationTests
{
    public class CompleteFlowTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public CompleteFlowTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CompleteFlow_CreateAccount_Login_RegisterMovement_CheckBalance()
        {
            // 1. Criar conta
            var novaConta = new
            {
                cpf = "52998224725",
                senha = "senha123",
                nomeTitular = "Teste Completo"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/Conta", novaConta);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var createResult = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
            var numeroConta = createResult.GetProperty("numeroConta").GetString();
            Assert.NotNull(numeroConta);

            // 2. Login
            var loginRequest = new
            {
                cpfOrNumeroConta = "52998224725",
                senha = "senha123"
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);
            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

            var loginResult = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
            var token = loginResult.GetProperty("token").GetString();
            Assert.NotNull(token);

            // 3. Registrar movimento
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var movimento = new
            {
                requestId = Guid.NewGuid().ToString(),
                numeroConta = numeroConta,
                valor = 500.0,
                tipo = "C"
            };

            var movimentoResponse = await _client.PostAsJsonAsync("/api/Movimento", movimento);
            Assert.Equal(HttpStatusCode.NoContent, movimentoResponse.StatusCode);

            // 4. Consultar saldo
            var saldoResponse = await _client.GetAsync("/api/Saldo");
            Assert.Equal(HttpStatusCode.OK, saldoResponse.StatusCode);

            var saldoResult = await saldoResponse.Content.ReadFromJsonAsync<JsonElement>();
            var saldo = saldoResult.GetProperty("saldo").GetDecimal();
            Assert.True(saldo >= 0); // Verifica que o saldo foi retornado
        }
    }
}
