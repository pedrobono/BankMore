using BankMore.Tarifa.Application.Commands;
using BankMore.Tarifa.Application.DTOs;
using Confluent.Kafka;
using MediatR;
using System.Text.Json;

namespace BankMore.Tarifa.Infrastructure.Kafka;

public class TransferenciaConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TransferenciaConsumer> _logger;
    private IConsumer<string, string>? _consumer;

    public TransferenciaConsumer(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<TransferenciaConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Aguardar Kafka estar pronto
        await Task.Delay(10000, stoppingToken);
        
        var config = new ConsumerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            GroupId = "tarifa-service",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            AllowAutoCreateTopics = true
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
        
        try
        {
            _consumer.Subscribe("transferencias-realizadas");
            _logger.LogInformation("[KAFKA] Consumer iniciado | Topic: transferencias-realizadas");
        }
        catch (Exception ex)
        {
            _logger.LogWarning("[KAFKA] Erro ao subscrever: {Error}. Aguardando tópico ser criado...", ex.Message);
            await Task.Delay(10000, stoppingToken);
            return;
        }

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(stoppingToken);
                    
                    _logger.LogInformation("[KAFKA] Mensagem recebida | Partition: {Partition} | Offset: {Offset}",
                        result.Partition.Value, result.Offset.Value);

                    var message = JsonSerializer.Deserialize<TransferenciaRealizadaMessage>(result.Message.Value);
                    
                    if (message != null)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                        
                        var command = new ProcessarTarifacaoCommand(message.RequestId, message.IdContaOrigem);
                        await mediator.Send(command, stoppingToken);
                        
                        _consumer.Commit(result);
                        _logger.LogInformation("[KAFKA] Mensagem processada e commitada");
                    }
                }
                catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
                {
                    _logger.LogDebug("[KAFKA] Tópico ainda não existe, aguardando...");
                    await Task.Delay(10000, stoppingToken);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogWarning("[KAFKA] Erro ao consumir: {Error}", ex.Message);
                    await Task.Delay(5000, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[KAFKA] Erro ao processar mensagem");
                }
            }
        }
        finally
        {
            _consumer?.Close();
            _logger.LogInformation("[KAFKA] Consumer encerrado");
        }
    }

    public override void Dispose()
    {
        _consumer?.Dispose();
        base.Dispose();
    }
}
