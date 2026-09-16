using System.Text.Json;
using Confluent.Kafka;
using Consumer.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Consumer.Services;

public class KafkaConsumerService : IKafkaConsumerService
{
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly IConsumer<string, string> _consumer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly string _stationInformationTopic;
    private readonly string _vehicleTypesTopic;
    private readonly string _stationStatusTopic;

    public KafkaConsumerService(IServiceScopeFactory scopeFactory,ILogger<KafkaConsumerService> logger)
    {
        _scopeFactory = scopeFactory;

        string bootstrapServers = GetRequiredEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS");

        string groupId =GetRequiredEnvironmentVariable("KAFKA_CONSUMER_GROUP_ID");

        _stationInformationTopic =GetRequiredEnvironmentVariable("STATION_INFORMATION_TOPIC");
        _vehicleTypesTopic =GetRequiredEnvironmentVariable("VEHICLE_TYPES_TOPIC");
        _stationStatusTopic =GetRequiredEnvironmentVariable("STATION_STATUS_TOPIC"
    );

        ConsumerConfig config = new()
        {
            BootstrapServers = bootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
        _logger = logger;
    }
    public async Task StartConsumingAsync(CancellationToken cancellationToken)
    {
        _consumer.Subscribe([_stationInformationTopic,_vehicleTypesTopic,_stationStatusTopic,]);
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ConsumeResult<string, string> result;
                try
                {
                    result = _consumer.Consume(cancellationToken);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex,"Kafka consume error: {Reason}",ex.Error.Reason);
                    continue;
                }
                _logger.LogInformation("Message received | Topic: {Topic} | Key: {Key}",result.Topic,result.Message.Key);
                try
                {
                    await HandleMessageAsync(result,cancellationToken);
                    _consumer.Commit(result);
                    _logger.LogInformation("Message processed successfully | Topic: {Topic} | Key: {Key}",result.Topic,result.Message.Key);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex,"Invalid JSON skipped | Topic: {Topic} | Key: {Key}",result.Topic,result.Message.Key);
                    _consumer.Commit(result);
                }
                catch (ArgumentException ex)
                {
                    _logger.LogWarning(ex,"Invalid message skipped | Topic: {Topic} | Key: {Key}",result.Topic,result.Message.Key);
                    _consumer.Commit(result);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex,"Message processing failed | Topic: {Topic} | Key: {Key}",result.Topic,result.Message.Key);
                    throw;
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Kafka consumer canceled.");
        }
        finally
        {
            _consumer.Close();
            _consumer.Dispose();
            _logger.LogInformation("Kafka consumer closed.");
        }
    }
    private async Task HandleMessageAsync(ConsumeResult<string, string> result,CancellationToken cancellationToken)
    {
        using IServiceScope scope = _scopeFactory.CreateAsyncScope();
        if(result.Topic == _stationInformationTopic)
        {
            StationInformation? station =JsonSerializer.Deserialize<StationInformation>( result.Message.Value);
            if (station is null)
            {
                throw new JsonException("Station information message is empty.");
            }
            IStationInformationHandler handler =scope.ServiceProvider.GetRequiredService<IStationInformationHandler>();
            await handler.HandleAsync(station,cancellationToken);
            return;
        }
        if (result.Topic == _vehicleTypesTopic)
        {
            VehicleType? vehicleType =JsonSerializer.Deserialize<VehicleType>(result.Message.Value);
            if (vehicleType is null)
            {
                throw new JsonException("Vehicle type message is empty.");
            }
            IVehicleTypesHandler handler =scope.ServiceProvider.GetRequiredService<IVehicleTypesHandler>();
            await handler.HandleAsync(vehicleType,cancellationToken);
        }
        if (result.Topic == _stationStatusTopic)
        {
            StationStatus? status =JsonSerializer.Deserialize<StationStatus>(result.Message.Value);
            if (status is null)
            {
                throw new JsonException("Station status message is empty.");
            }
            IStationStatusHandler handler =scope.ServiceProvider.GetRequiredService<IStationStatusHandler>();
            await handler.HandleAsync(status,cancellationToken);
            return;
        }
    }
    private static string GetRequiredEnvironmentVariable(string name)
    {
        return Environment.GetEnvironmentVariable(name)?? throw new InvalidOperationException( $"{name} is missing from .env");
    }
}