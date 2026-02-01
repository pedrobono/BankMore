using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BankMore.ContaCorrente.Application.Commands;
using BankMore.ContaCorrente.Application.DTOs;
using BankMore.ContaCorrente.Tests.IntegrationTests;
using Xunit;

namespace BankMore.ContaCorrente.Tests.Integration.Controllers
{
    public class MovimentoIdempotenciaTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public MovimentoIdempotenciaTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Movimento_MesmoRequestIdEmContasDiferentes_DevePermitir()
        {
            // Arrange - Criar duas contas
            var conta1Command = new CriarContaCommand
            {
                Cpf = "52998224725",
                NomeTitular = "Usuario 1",
                Senha = "senha123"
            };

            var conta2Command = new CriarContaCommand
            {
                Cpf = "40643990054",
                NomeTitular = "Usuario 2",
                Senha = "senha123"
            };

            var criarConta1Response = await _client.PostAsJsonAsync("/api/Conta", conta1Command);
            var criarConta1Result = await criarConta1Response.Content.ReadFromJsonAsync<CriarContaResponse>();
            var numeroConta1 = criarConta1Result!.NumeroConta;

            var criarConta2Response = await _client.PostAsJsonAsync("/api/Conta", conta2Command);
            var criarConta2Result = await criarConta2Response.Content.ReadFromJsonAsync<CriarContaResponse>();
            var numeroConta2 = criarConta2Result!.NumeroConta;

            // Login conta 1
            var login1 = await _client.PostAsJsonAsync("/api/Auth/login", new LoginCommand
            {
                CpfOrNumeroConta = "52998224725",
                Senha = "senha123"
            });
            var token1Result = await login1.Content.ReadFromJsonAsync<LoginResponse>();
            var token1 = token1Result!.Token;

            // Login conta 2
            var login2 = await _client.PostAsJsonAsync("/api/Auth/login", new LoginCommand
            {
                CpfOrNumeroConta = "40643990054",
                Senha = "senha123"
            });
            var token2Result = await login2.Content.ReadFromJsonAsync<LoginResponse>();
            var token2 = token2Result!.Token;

            var requestIdCompartilhado = Guid.NewGuid().ToString();

            // Act - Movimento na conta 1 (sem especificar contaId, usa do token)
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
            var movimento1 = new RegistrarMovimentoCommand
            {
                RequestId = requestIdCompartilhado,
                Tipo = "C",
                Valor = 100
            };
            var response1 = await _client.PostAsJsonAsync("/api/Movimento", movimento1);

            // Act - Movimento na conta 2 com mesmo RequestId (usa conta do token2)
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);
            var movimento2 = new RegistrarMovimentoCommand
            {
                RequestId = requestIdCompartilhado,
                Tipo = "C",
                Valor = 200
            };
            var response2 = await _client.PostAsJsonAsync("/api/Movimento", movimento2);

            // Assert - Ambos devem ter sucesso (204)
            Assert.Equal(HttpStatusCode.NoContent, response1.StatusCode);
            Assert.Equal(HttpStatusCode.NoContent, response2.StatusCode);
        }

        [Fact]
        public async Task Movimento_MesmoRequestIdNaMesmaConta_DeveSerIdempotente()
        {
            // Arrange
            var contaCommand = new CriarContaCommand
            {
                Cpf = "90816518033",
                NomeTitular = "Usuario Teste",
                Senha = "senha123"
            };

            var criarContaResponse = await _client.PostAsJsonAsync("/api/Conta", contaCommand);
            var criarContaResult = await criarContaResponse.Content.ReadFromJsonAsync<CriarContaResponse>();
            var numeroConta = criarContaResult!.NumeroConta;

            var login = await _client.PostAsJsonAsync("/api/Auth/login", new LoginCommand
            {
                CpfOrNumeroConta = "90816518033",
                Senha = "senha123"
            });
            var tokenResult = await login.Content.ReadFromJsonAsync<LoginResponse>();
            var token = tokenResult!.Token;

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var requestId = Guid.NewGuid().ToString();
            var movimento = new RegistrarMovimentoCommand
            {
                RequestId = requestId,
                Tipo = "C",
                Valor = 100
            };

            // Act - Primeira requisição
            var response1 = await _client.PostAsJsonAsync("/api/Movimento", movimento);

            // Act - Segunda requisição com mesmo RequestId
            var response2 = await _client.PostAsJsonAsync("/api/Movimento", movimento);

            // Assert - Ambas devem retornar 204 (idempotente)
            Assert.Equal(HttpStatusCode.NoContent, response1.StatusCode);
            Assert.Equal(HttpStatusCode.NoContent, response2.StatusCode);

            // Verificar saldo - deve ter apenas 100 (não 200)
            var saldoResponse = await _client.GetAsync("/api/Saldo");
            var saldo = await saldoResponse.Content.ReadFromJsonAsync<SaldoDto>();
            Assert.Equal(100m, saldo!.Saldo);
        }
    }
}
