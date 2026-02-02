using BankMore.Tarifa.Application.Commands;
using BankMore.Tarifa.Application.DTOs;
using BankMore.Tarifa.Domain.Entities;
using BankMore.Tarifa.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BankMore.Tarifa.Application.Handlers;

public class ProcessarTarifacaoHandler : IRequestHandler<ProcessarTarifacaoCommand, Unit>
{
    private readonly ITarifacaoRepository _repository;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ProcessarTarifacaoHandler> _logger;

    public ProcessarTarifacaoHandler(
        ITarifacaoRepository repository,
        IKafkaProducer kafkaProducer,
        IConfiguration configuration,
        ILogger<ProcessarTarifacaoHandler> logger)
    {
        _repository = repository;
        _kafkaProducer = kafkaProducer;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Unit> Handle(ProcessarTarifacaoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[TARIFA] Processando tarifação | RequestId: {RequestId} | Conta: {ContaId}", 
            request.RequestId, request.IdContaOrigem);

        // Verificar idempotência
        var tarifacaoExistente = await _repository.GetByRequestIdAsync(request.RequestId);
        if (tarifacaoExistente != null)
        {
            _logger.LogInformation("[TARIFA] Tarifação já processada (idempotência) | RequestId: {RequestId}", 
                request.RequestId);
            return Unit.Value;
        }

        // Obter valor da tarifa do appsettings
        var valorTarifa = _configuration.GetValue<decimal>("TarifaSettings:ValorTarifa", 2.0m);

        // Criar registro de tarifação
        var tarifacao = new Tarifacao(request.IdContaOrigem, valorTarifa, request.RequestId);
        await _repository.CreateAsync(tarifacao);

        _logger.LogInformation("[TARIFA] Tarifação registrada | Id: {TarifacaoId} | Valor: {Valor}", 
            tarifacao.IdTarifacao, valorTarifa);

        // Publicar mensagem de tarifação realizada
        var message = new TarifacaoRealizadaMessage(request.IdContaOrigem, valorTarifa);
        await _kafkaProducer.ProduceAsync("tarifacoes-realizadas", message);

        _logger.LogInformation("[TARIFA] Mensagem publicada no Kafka | Conta: {ContaId} | Valor: {Valor}", 
            request.IdContaOrigem, valorTarifa);

        return Unit.Value;
    }
}

public interface IKafkaProducer
{
    Task ProduceAsync<T>(string topic, T message);
}
