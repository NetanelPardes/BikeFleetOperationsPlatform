using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Producer.Services;
using Microsoft.Extensions.Logging;

DotNetEnv.Env.TraversePath().Load();

Directory.CreateDirectory("Logs");
using var logger = new LoggerConfiguration()
.MinimumLevel.Information()
.WriteTo.Console(outputTemplate:"{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
.WriteTo.File("Logs/producer-.log",rollingInterval:RollingInterval.Day,retainedFileCountLimit:7,outputTemplate:"{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
.CreateLogger();

ServiceCollection services = new();
services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddSerilog(logger,dispose:false);
});
services.AddSingleton<StationInformationValidationService>();
services.AddHttpClient<StationInformationService>();

services.AddSingleton<StationStatusValidationService>();
services.AddHttpClient<StationStatusService>();

services.AddSingleton<VehicleTypesValidationService>();
services.AddHttpClient<VehicleTypesService>();

services.AddSingleton<IKafkaProducerService, KafkaProducerService>();

using ServiceProvider serviceProvider = services.BuildServiceProvider();

using CancellationTokenSource cancellation = new();

StationInformationService stationInformation = serviceProvider.GetRequiredService<StationInformationService>();

StationStatusService stationStatusService = serviceProvider.GetRequiredService<StationStatusService>();

VehicleTypesService vehicleTypesService  = serviceProvider.GetRequiredService<VehicleTypesService>();

Console.CancelKeyPress += (_, e) => {e.Cancel = true; cancellation.Cancel();};
try
{
    logger.Information("Producer started.");
    await Task.WhenAll(
        RunPeriodicallyAsync(stationInformation.GetStationsFromApi,TimeSpan.FromHours(1),cancellation.Token),

        RunPeriodicallyAsync(stationStatusService.GetStationsStatusFromApi,TimeSpan.FromSeconds(60),cancellation.Token),

        RunPeriodicallyAsync(vehicleTypesService.GetVehicleTypesFromApi,TimeSpan.FromHours(1),cancellation.Token)
    );
}
catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
{
    logger.Information("Producer stopped.");
    Console.WriteLine("Producer stopped.");
}
static async Task RunPeriodicallyAsync(Func<Task> action, TimeSpan interval ,CancellationToken cancellationToken)
{
    using PeriodicTimer timer = new(interval);
    cancellationToken.ThrowIfCancellationRequested();
    await action();
    while(await timer.WaitForNextTickAsync(cancellationToken))
    {
        await action();
    }
}


