using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BankMore.ContaCorrente.Infrastructure.Persistence;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System;
using System.Linq;

namespace BankMore.ContaCorrente.Tests.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IDisposable
    {
        private const string TestJwtKey = "SuaChaveSuperSecretaComMaisDe32CaracteresAqui123!";
        private readonly string _databaseName = Guid.NewGuid().ToString();

        public void ResetDatabase()
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DataBaseContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.Sources.Clear();
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["JwtSettings:SecretKey"] = TestJwtKey,
                    ["ConnectionStrings:DefaultConnection"] = "Data Source=:memory:",
                    ["Kafka:Enabled"] = "false"
                });
            });

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<DataBaseContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Remover Kafka Consumer em testes
                var kafkaDescriptor = services.FirstOrDefault(
                    d => d.ImplementationType?.Name == "TarifacaoConsumer");
                if (kafkaDescriptor != null)
                {
                    services.Remove(kafkaDescriptor);
                }

                // Remove a configuração anterior do JWT Bearer
                var jwtDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(JwtBearerOptions));
                if (jwtDescriptor != null)
                {
                    services.Remove(jwtDescriptor);
                }

                services.AddDbContext<DataBaseContext>(options =>
                {
                    options.UseInMemoryDatabase(_databaseName);
                });

                // Reconfigura o JWT Bearer com a chave de teste
                services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtKey))
                    };
                });

                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<DataBaseContext>();
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();
                }
            });
        }
    }
}
