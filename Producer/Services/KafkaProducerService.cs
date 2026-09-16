using System.Text.Json;
using Confluent.Kafka;

namespace Producer.Services;

public class KafkaProducerService : IKafkaProducerService
{
    private readonly IProducer<string,string> _producer;
    public KafkaProducerService()
    {
        string bootstrapServers  = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS") ?? throw new InvalidOperationException("KAFKA_BOOTSTRAP_SERVERS is missing from .env");
        ProducerConfig config = new (){BootstrapServers  = bootstrapServers,EnableIdempotence = true };
        _producer = new ProducerBuilder<string,string>(config).Build();
    }
    public async Task SendAsync<T>(string topic, string key, T data, CancellationToken cancellationToken = default)
    {
        string json = JsonSerializer.Serialize(data);
        Message<string,string> message = new(){Key = key, Value = json};
        await _producer.ProduceAsync(topic, message, cancellationToken);
    }
    public void Dispose()
    {
        try
        {
            _producer.Flush(TimeSpan.FromSeconds(10));
        }
        finally
        {
            _producer.Dispose();
        }
    }
}