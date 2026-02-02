using Confluent.Kafka;
using System.Text.Json;

namespace BankMore.Transferencia.Infrastructure.Kafka;

public interface IKafkaProducer
{
    Task ProduceAsync<T>(string topic, T message);
}

public class KafkaProducer : IKafkaProducer, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaProducer> _logger;

    public KafkaProducer(IConfiguration configuration, ILogger<KafkaProducer> logger)
    {
        _logger = logger;
        var config = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092"
        };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task ProduceAsync<T>(string topic, T message)
    {
        try
        {
            var json = JsonSerializer.Serialize(message);
            var result = await _producer.ProduceAsync(topic, new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = json
            });

            _logger.LogInformation("[KAFKA] Mensagem enviada | Topic: {Topic} | Partition: {Partition}",
                topic, result.Partition.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[KAFKA] Erro ao enviar mensagem | Topic: {Topic}", topic);
        }
    }

    public void Dispose()
    {
        _producer?.Dispose();
    }
}
