using BankMore.ContaCorrente.Application.Commands;
using BankMore.ContaCorrente.Infrastructure.Persistence;
using BankMore.ContaCorrente.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using BankMore.ContaCorrente.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace BankMore.ContaCorrente.Application.Handlers {
    public class RegistrarMovimentoHandler : IRequestHandler<RegistrarMovimentoCommand> {
        private readonly DataBaseContext _context;
        private readonly ILogger<RegistrarMovimentoHandler> _logger;

        public RegistrarMovimentoHandler(DataBaseContext context, ILogger<RegistrarMovimentoHandler> logger) {
            _context = context;
            _logger = logger;
        }

        public async Task Handle(RegistrarMovimentoCommand request, CancellationToken cancellationToken) {
            _logger.LogInformation("[MOVIMENTO] Iniciando processamento | RequestId: {RequestId} | Tipo: {Tipo} | Valor: {Valor}", 
                request.RequestId, request.Tipo, request.Valor);
            
            Conta conta;
            
            if (!request.ContaId.HasValue) {
                _logger.LogDebug("[MOVIMENTO] Usando conta do token | ContaId: {ContaId}", request.ContaIdLogada);
                
                if (!request.ContaIdLogada.HasValue)
                    throw new BusinessException("Conta não identificada", "INVALID_ACCOUNT");
                    
                conta = await _context.ContaCorrente
                    .FirstOrDefaultAsync(c => c.IdContaCorrente == request.ContaIdLogada.Value, cancellationToken);
            } else {
                _logger.LogDebug("[MOVIMENTO] Buscando conta por ID | ContaId: {ContaId}", request.ContaId);
                
                conta = await _context.ContaCorrente
                    .FirstOrDefaultAsync(c => c.IdContaCorrente == request.ContaId.Value, cancellationToken);
            }

            if (conta == null) {
                _logger.LogWarning("[MOVIMENTO] Conta não encontrada | RequestId: {RequestId}", request.RequestId);
                throw new BusinessException("Conta não encontrada", "INVALID_ACCOUNT");
            }

            _logger.LogInformation("[MOVIMENTO] Conta encontrada | ContaId: {ContaId} | Numero: {Numero} | Ativo: {Ativo}", 
                conta.IdContaCorrente, conta.Numero, conta.Ativo);

            if (conta.Ativo == 0)
                throw new BusinessException("Esta conta está encerrada ou inativa", "INACTIVE_ACCOUNT");

            if (request.Valor <= 0)
                throw new BusinessException("O valor da movimentação deve ser positivo", "INVALID_VALUE");

            if (request.Tipo != "C" && request.Tipo != "D")
                throw new BusinessException("Tipo de movimento inválido", "INVALID_TYPE");

            if (request.Tipo == "D" && request.ContaIdLogada.HasValue && conta.IdContaCorrente != request.ContaIdLogada.Value)
                throw new BusinessException("Débito só pode ser realizado na própria conta", "INVALID_TYPE");

            _logger.LogDebug("[MOVIMENTO] Verificando idempotência | RequestId: {RequestId} | ContaId: {ContaId}", 
                request.RequestId, conta.IdContaCorrente);
            
            var movimentoExistente = await _context.Idempotencia
                .AnyAsync(i => i.Requisicao.Contains($"\"requestId\":\"{request.RequestId}\"") && 
                              i.Requisicao.Contains($"\"contaId\":\"{conta.IdContaCorrente}\""), cancellationToken);

            if (movimentoExistente) {
                _logger.LogInformation("[MOVIMENTO] Requisição duplicada detectada (idempotência) | RequestId: {RequestId} | ContaId: {ContaId}", 
                    request.RequestId, conta.IdContaCorrente);
                return;
            }

            if (request.Tipo == "D") {
                _logger.LogDebug("[MOVIMENTO] Verificando saldo para débito | ContaId: {ContaId}", conta.IdContaCorrente);
                
                var movimentos = await _context.Movimento
                    .Where(m => m.IdContaCorrente == conta.IdContaCorrente)
                    .ToListAsync(cancellationToken);
                    
                var saldoAtual = movimentos.Sum(m => m.TipoMovimento == "C" ? m.Valor : -m.Valor);
                
                _logger.LogInformation("[MOVIMENTO] Saldo atual: {Saldo} | Valor débito: {Valor}", saldoAtual, request.Valor);

                if (saldoAtual < request.Valor)
                    throw new BusinessException("Saldo insuficiente", "INSUFFICIENT_BALANCE");
            }

            var movimento = new Movimento {
                IdMovimento = Guid.NewGuid(),
                IdContaCorrente = conta.IdContaCorrente,
                TipoMovimento = request.Tipo,
                Valor = request.Valor,
                DataMovimento = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss")
            };

            var idempotencia = new Idempotencia {
                ChaveIdempotencia = Guid.NewGuid(),
                Requisicao = $"{{\"requestId\":\"{request.RequestId}\",\"contaId\":\"{conta.IdContaCorrente}\",\"tipo\":\"{request.Tipo}\",\"valor\":{request.Valor}}}",
                Resultado = "SUCCESS"
            };

            _logger.LogInformation("[MOVIMENTO] Salvando movimento | MovimentoId: {MovimentoId} | Tipo: {Tipo} | Valor: {Valor}", 
                movimento.IdMovimento, movimento.TipoMovimento, movimento.Valor);

            _context.Movimento.Add(movimento);
            _context.Idempotencia.Add(idempotencia);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("[MOVIMENTO] Movimento registrado com sucesso | RequestId: {RequestId} | ContaId: {ContaId} | Tipo: {Tipo} | Valor: {Valor}", 
                request.RequestId, conta.IdContaCorrente, request.Tipo, request.Valor);
        }
    }
}
