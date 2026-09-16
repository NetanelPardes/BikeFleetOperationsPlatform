namespace Producer.Services;

public interface IKafkaProducerService
{
    Task SendAsync<T>(string topic, string key, T data, CancellationToken cancellationToken = default);
}