using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using Producer.DTOs;

namespace Producer.Services;

public class VehicleTypesService
{
    private readonly ILogger<VehicleTypesService> _logger;
    private readonly IKafkaProducerService _kafkaProducerService;
    private readonly string _topic;
    private readonly string _url;
    private readonly HttpClient _httpClient;
    private readonly VehicleTypesValidationService _vehicleTypesValidationService;
    public VehicleTypesService(HttpClient httpClient,VehicleTypesValidationService vehicleTypesValidationService,IKafkaProducerService kafkaProducerService,ILogger<VehicleTypesService> logger)
    {
        _url = Environment.GetEnvironmentVariable("VEHICLE_TYPES_URL") ?? throw new InvalidOperationException("VEHICLE_TYPES_URL is missing from .env");
        _httpClient = httpClient;
        _vehicleTypesValidationService = vehicleTypesValidationService;
         _kafkaProducerService = kafkaProducerService;
        _topic = Environment.GetEnvironmentVariable("VEHICLE_TYPES_TOPIC") ?? throw new InvalidOperationException("VEHICLE_TYPES_TOPIC is missing from .env");
        _logger = logger;
    }
    public async Task GetVehicleTypesFromApi()
    {
        try
        {
            VehicleTypesResponseDto? response = await _httpClient.GetFromJsonAsync<VehicleTypesResponseDto>(_url);
            List<VehicleTypeDto>? vehicleTypes = response?.Data?.VehicleTypes;

            if (vehicleTypes is null)
            {
                Console.WriteLine("The response does not contain vehicle types.");
                return;
            }

            int validCount = 0;
            int invalidCount = 0;

            foreach (VehicleTypeDto vehicleType in vehicleTypes)
            {
                if (!_vehicleTypesValidationService.IsValid(vehicleType))
                {
                    invalidCount++;
                    continue;
                }
                await _kafkaProducerService.SendAsync(_topic,vehicleType.VehicleTypeId!, vehicleType);
                validCount++;

                Console.WriteLine(
                    $"ID: {vehicleType.VehicleTypeId}, " +
                    $"Form: {vehicleType.FormFactor}, " +
                    $"Propulsion: {vehicleType.PropulsionType}"
                );
            }
            Console.WriteLine($"Finished sending to {_topic}: {validCount} messages sent, {invalidCount} invalid records skipped.");
            _logger.LogInformation("Finished sending to {Topic}: {SentCount} sent, {InvalidCount} invalid",_topic,validCount,invalidCount);
            //Console.WriteLine($"Received: {vehicleTypes.Count},Valid: {validCount}, Invalid: {invalidCount}");
        }
        catch(HttpRequestException ex)
        {
            _logger.LogError(ex, "Station status HTTP request failed.");
            Console.WriteLine($"Station status HTTP error: {ex.Message}");
        }
        catch(System.Text.Json.JsonException ex)
        {
            _logger.LogError(ex, "Station status JSON conversion failed.");
            Console.WriteLine($"Station status JSON error: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("Station status request was canceled or timed out.");
            Console.WriteLine("Station status request was canceled or timed out.");
        }
        catch (Confluent.Kafka.ProduceException<string, string> ex)
        {
            _logger.LogError($"Kafka send error: {ex.Error.Reason}");
            Console.WriteLine($"Kafka send error: {ex.Error.Reason}");
        }
    }
}