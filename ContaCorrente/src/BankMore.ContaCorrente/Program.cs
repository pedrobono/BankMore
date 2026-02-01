using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MediatR;
using BankMore.ContaCorrente.Infrastructure.Persistence;
using BankMore.ContaCorrente.Application.Commands;
using Microsoft.OpenApi.Any;
using BankMore.ContaCorrente.Api.Middleware;
using BankMore.ContaCorrente.Api.Swagger;
using BankMore.ContaCorrente.Domain.Repositories;
using BankMore.ContaCorrente.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. ADICIONE ESTA LINHA: Registra os serviços necessários para Controllers
builder.Services.AddControllers();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<DataBaseContext>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BankMore Conta Corrente API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        Description = "JWT Authorization header usando o esquema Bearer. Exemplo: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });

    c.OperationFilter<SwaggerRequestExampleFilter>();
    c.OperationFilter<SwaggerResponseOperationFilter>();
});

builder.Services.AddDbContext<DataBaseContext>(options => {
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlite(connectionString);
});
builder.Services.AddScoped<IContaRepository, ContaRepository>();
builder.Services.AddScoped<IMovimentoRepository, MovimentoRepository>();
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(CriarContaCommand).Assembly);
});

var jwtSecretKey = builder.Configuration["JwtSettings:SecretKey"] ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY");

if (string.IsNullOrEmpty(jwtSecretKey)) {
    throw new InvalidOperationException("A chave secreta do JWT não foi fornecida.");
}

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey))
    };
});

builder.Services.AddAuthorization();
//builder.Services.AddSingleton<ErrorHandlingMiddleware>(); 

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllers();

app.Run();

// Torna a classe Program acessível para testes de integração
public partial class Program { }