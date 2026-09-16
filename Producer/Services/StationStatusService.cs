using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using Producer.DTOs;

namespace Producer.Services;

public class StationStatusService
{
    private readonly ILogger<StationStatusService> _logger;
    private readonly IKafkaProducerService _kafkaProducerService;
    private readonly string _topic;
    private readonly string _url;
    private readonly HttpClient _httpClient;
    private readonly StationStatusValidationService _stationStatusValidationService;

    public StationStatusService(HttpClient httpClient,StationStatusValidationService stationStatusValidationService,IKafkaProducerService kafkaProducerService,ILogger<StationStatusService> logger)
    {
        _url = Environment.GetEnvironmentVariable("STATION_STATUS_URL") ?? throw new InvalidOperationException("STATION_STATUS_URL is missing from .env");
        _httpClient = httpClient;
        _stationStatusValidationService = stationStatusValidationService;
         _kafkaProducerService = kafkaProducerService;
        _topic = Environment.GetEnvironmentVariable("STATION_STATUS_TOPIC") ?? throw new InvalidOperationException("STATION_STATUS_TOPIC is missing from .env");
        _logger = logger;
    }

    public async Task GetStationsStatusFromApi()
    {
        try
        {
            StationStatusResponseDto? response =await _httpClient.GetFromJsonAsync<StationStatusResponseDto>(_url);

            List<StationStatusDto>? stationsStatus = response?.Data?.Stations;

            if (stationsStatus is null)
            {
                Console.WriteLine("The response does not contain a stations list.");
                return;
            }

            int validCount = 0;
            int invalidCount = 0;

            foreach (StationStatusDto station in stationsStatus)
            {
                if (!_stationStatusValidationService.IsValid(station))
                {
                    invalidCount++;
                    continue;
                }
                await _kafkaProducerService.SendAsync(_topic,station.StationId!, station);
                validCount++;

                if (validCount <= 5)
                {
                    Console.WriteLine(
                        $"ID: {station.StationId}, " +
                        $"Vehicles: {station.AvailableVehicles}, " +
                        $"Docks: {station.AvailableDocks}, " +
                        $"Renting: {station.IsRenting}, " +
                        $"Returning: {station.IsReturning}"
                    );
                }
            }
            Console.WriteLine($"Finished sending to {_topic}: {validCount} messages sent, {invalidCount} invalid records skipped.");
            _logger.LogInformation("Finished sending to {Topic}: {SentCount} sent, {InvalidCount} invalid",_topic,validCount,invalidCount);
            //Console.WriteLine($"Received: {stationsStatus.Count}, Valid: {validCount}, Invalid: {invalidCount}");
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