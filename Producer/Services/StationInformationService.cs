using System.Net.Http.Json;
using Producer.DTOs;
using Microsoft.Extensions.Logging;

namespace Producer.Services;

public class StationInformationService
{
    private readonly ILogger<StationInformationService> _logger;
    private readonly IKafkaProducerService _kafkaProducerService;
    private readonly string _topic;
    private readonly string _url;
    private readonly HttpClient _httpClient;
    private readonly StationInformationValidationService _validationService;

    public StationInformationService(HttpClient httpClient,StationInformationValidationService validationService,IKafkaProducerService kafkaProducerService,ILogger<StationInformationService> logger)
    {
        _url = Environment.GetEnvironmentVariable("STATION_INFORMATION_URL") ?? throw new InvalidOperationException("STATION_INFORMATION_URL is missing from .env");
        _httpClient = httpClient;
        _validationService = validationService;
        _kafkaProducerService = kafkaProducerService;
        _topic = Environment.GetEnvironmentVariable("STATION_INFORMATION_TOPIC") ?? throw new InvalidOperationException("STATION_INFORMATION_TOPIC is missing from .env");
        _logger = logger;
    }

    public async Task GetStationsFromApi()
    {
        try
        {
            StationInformationResponseDto? response =await _httpClient.GetFromJsonAsync<StationInformationResponseDto>(_url);

            List<StationInformationDto>? stations = response?.Data?.Stations;

            if (stations is null)
            {
                Console.WriteLine("The response does not contain a stations list.");
                return;
            }

            int validCount = 0;
            int invalidCount = 0;

            foreach (StationInformationDto station in stations)
            {
                if (!_validationService.IsValid(station))
                {
                    invalidCount++;
                    continue;
                }
                await _kafkaProducerService.SendAsync(_topic,station.StationId!, station);
                validCount++;
            }
            Console.WriteLine($"Finished sending to {_topic}: {validCount} messages sent, {invalidCount} invalid records skipped.");
            _logger.LogInformation("Finished sending to {Topic}: {SentCount} sent, {InvalidCount} invalid",_topic,validCount,invalidCount);
            //Console.WriteLine($"Received: {stations.Count}, Valid: {validCount}, Invalid: {invalidCount}");
        }
        catch(HttpRequestException ex)
        {
            System.Console.WriteLine($"Station status HTTP error: {ex.Message}");
        }
        catch(System.Text.Json.JsonException ex)
        {
            Console.WriteLine($"Station status JSON error: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("Station status request was canceled or timed out.");
        }
        catch (Confluent.Kafka.ProduceException<string, string> ex)
        {
            Console.WriteLine($"Kafka send error: {ex.Error.Reason}");
        }
    }
}