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
    public class SaldoControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public SaldoControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task ObterSaldo_ShouldReturn200_WithCorrectData()
        {
            _factory.ResetDatabase();
            var client = _factory.CreateClient();
            var cpf = "61864317035";
            var criarContaCommand = new CriarContaCommand
            {
                Cpf = cpf,
                NomeTitular = "Test User Saldo",
                Senha = "senha123"
            };

            await client.PostAsJsonAsync("/api/Conta", criarContaCommand);

            var loginCommand = new LoginCommand
            {
                CpfOrNumeroConta = cpf,
                Senha = "senha123"
            };

            var loginResponse = await client.PostAsJsonAsync("/api/Auth/login", loginCommand);
            var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
            var token = loginResult!.Token;

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync("/api/Saldo");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var saldo = await response.Content.ReadFromJsonAsync<SaldoDto>();
            Assert.NotNull(saldo);
            Assert.Equal("Test User Saldo", saldo.NomeTitular);
            Assert.Equal(0m, saldo.Saldo);
            Assert.NotEqual(default(DateTime), saldo.DataHoraConsulta);
        }

        [Fact]
        public async Task ObterSaldo_ShouldCalculateCorrectly_WithMultipleMovements()
        {
            _factory.ResetDatabase();
            var client = _factory.CreateClient();
            var cpf = "79320874069";
            var criarContaCommand = new CriarContaCommand
            {
                Cpf = cpf,
                NomeTitular = "Test User Calc",
                Senha = "senha123"
            };

            var criarResponse = await client.PostAsJsonAsync("/api/Conta", criarContaCommand);
            var criarResult = await criarResponse.Content.ReadFromJsonAsync<CriarContaResponse>();
            var numeroConta = criarResult!.NumeroConta;

            var loginCommand = new LoginCommand
            {
                CpfOrNumeroConta = cpf,
                Senha = "senha123"
            };

            var loginResponse = await client.PostAsJsonAsync("/api/Auth/login", loginCommand);
            var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
            var token = loginResult!.Token;

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Movimentos sem ContaId usam a conta do token
            await client.PostAsJsonAsync("/api/Movimento", new RegistrarMovimentoCommand
            {
                RequestId = Guid.NewGuid().ToString(),
                Valor = 100m,
                Tipo = "C"
            });

            await client.PostAsJsonAsync("/api/Movimento", new RegistrarMovimentoCommand
            {
                RequestId = Guid.NewGuid().ToString(),
                Valor = 50m,
                Tipo = "C"
            });

            await client.PostAsJsonAsync("/api/Movimento", new RegistrarMovimentoCommand
            {
                RequestId = Guid.NewGuid().ToString(),
                Valor = 30m,
                Tipo = "D"
            });

            var response = await client.GetAsync("/api/Saldo");
            var saldo = await response.Content.ReadFromJsonAsync<SaldoDto>();

            Assert.Equal(120m, saldo!.Saldo);
        }

        [Fact]
        public async Task ObterSaldo_ShouldReturn401_WhenNoToken()
        {
            _factory.ResetDatabase();
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/Saldo");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
