using BankMore.ContaCorrente.Application.Commands;
using Confluent.Kafka;
using MediatR;
using System.Text.Json;

namespace BankMore.ContaCorrente.Infrastructure.Kafka;

public class TarifacaoConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TarifacaoConsumer> _logger;
    private IConsumer<string, string>? _consumer;

    public TarifacaoConsumer(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<TarifacaoConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[KAFKA] Aguardando Kafka inicializar...");
        await Task.Delay(10000, stoppingToken);
        
        var config = new ConsumerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            GroupId = "conta-corrente-service-v2",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            AllowAutoCreateTopics = true
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
        
        try
        {
            _consumer.Subscribe("tarifacoes-realizadas");
            _logger.LogInformation("[KAFKA] Consumer iniciado | Topic: tarifacoes-realizadas | GroupId: {GroupId}", config.GroupId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[KAFKA] Erro ao subscrever");
            await Task.Delay(10000, stoppingToken);
            return;
        }

        try
        {
            _logger.LogInformation("[KAFKA] Iniciando loop de consumo...");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogDebug("[KAFKA] Aguardando mensagem...");
                    var result = _consumer.Consume(stoppingToken);
                    
                    _logger.LogInformation("[KAFKA] Mensagem recebida | Partition: {Partition} | Offset: {Offset}",
                        result.Partition.Value, result.Offset.Value);

                    var message = JsonSerializer.Deserialize<TarifacaoMessage>(result.Message.Value);
                    
                    if (message != null)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                        
                        var command = new RegistrarMovimentoCommand
                        {
                            RequestId = $"TARIFA-{Guid.NewGuid()}",
                            ContaId = message.IdContaCorrente,
                            Valor = message.ValorTarifado,
                            Tipo = "D" // Débito da tarifa
                        };
                        
                        await mediator.Send(command, stoppingToken);
                        
                        _consumer.Commit(result);
                        _logger.LogInformation("[KAFKA] Tarifação processada | Conta: {ContaId} | Valor: {Valor}",
                            message.IdContaCorrente, message.ValorTarifado);
                    }
                }
                catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
                {
                    _logger.LogDebug("[KAFKA] Tópico ainda não existe, aguardando...");
                    await Task.Delay(10000, stoppingToken);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "[KAFKA] Erro ao consumir: {Error}", ex.Message);
                    await Task.Delay(5000, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[KAFKA] Erro ao processar tarifação");
                    await Task.Delay(5000, stoppingToken);
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

public record TarifacaoMessage(Guid IdContaCorrente, decimal ValorTarifado);
