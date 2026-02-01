using BankMore.ContaCorrente.Application.Commands;
using BankMore.ContaCorrente.Infrastructure.Persistence;
using BankMore.ContaCorrente.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace BankMore.ContaCorrente.Application.Handlers {
    public class LoginHandler : IRequestHandler<LoginCommand, LoginResponse> {
        private readonly DataBaseContext _context;
        private readonly IConfiguration _configuration;

        public LoginHandler(DataBaseContext context, IConfiguration configuration) {
            _context = context;
            _configuration = configuration;
        }

        public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken) {
            var jwtSecretKey = _configuration["JwtSettings:SecretKey"] ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
            if (string.IsNullOrEmpty(jwtSecretKey)) {
                throw new InvalidOperationException("A chave secreta do JWT não foi fornecida.");
            }

            var todasContas = await _context.ContaCorrente.ToListAsync(cancellationToken);
            Conta conta = null;
            
            foreach (var c in todasContas.OrderByDescending(x => x.Ativo)) {
                var numeroFormatado = $"{c.Numero}-{c.Numero % 10}";
                
                if (BCrypt.Net.BCrypt.Verify(request.CpfOrNumeroConta, c.Salt) || numeroFormatado == request.CpfOrNumeroConta) {
                    conta = c;
                    break;
                }
            }
            
            if (conta == null || !BCrypt.Net.BCrypt.Verify(request.Senha, conta.Senha)) {
                throw new UnauthorizedAccessException("Usuário ou senha inválidos.");
            }
            
            if (conta.Ativo == 0) {
                throw new UnauthorizedAccessException("Conta inativa.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, conta.IdContaCorrente.ToString()),
                new Claim(ClaimTypes.Name, $"{conta.Numero}-{conta.Numero % 10}"),
            };

            var token = new JwtSecurityToken(
                issuer: "BankMore",
                audience: "BankMore",
                claims: claims,
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return new LoginResponse {
                Token = tokenString
            };
        }
    }
}
