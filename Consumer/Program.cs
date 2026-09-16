using Consumer.Data;
using Consumer.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

DotNetEnv.Env.TraversePath().Load();

Directory.CreateDirectory("Logs");

using var logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command",LogEventLevel.Warning)
    .WriteTo.Console(outputTemplate:"{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("Logs/consumer-.log",rollingInterval: RollingInterval.Day,retainedFileCountLimit: 7,outputTemplate:"{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj} {NewLine}{Exception}")
    .CreateLogger();

string connectionString = Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING") ?? throw new InvalidOperationException("MYSQL_CONNECTION_STRING is missing from .env");
string redisConnectionString =Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")?? throw new InvalidOperationException( "REDIS_CONNECTION_STRING is missing from .env");
ServiceCollection services = new();
services.AddLogging(logging =>{logging.ClearProviders();logging.AddSerilog(logger, dispose: false);});
services.AddDbContext<BikeFleetDbContext>(options => options.UseMySql(connectionString,new MySqlServerVersion(new Version(8, 4, 0))));

services.AddScoped<IStationInformationHandler,StationInformationHandler>();
services.AddScoped<IVehicleTypesHandler,VehicleTypesHandler>();
services.AddSingleton<ICurrentStateService,RedisCurrentStateService>();
services.AddSingleton<IKafkaConsumerService,KafkaConsumerService>();
services.AddScoped<IStationStatusHandler,StationStatusHandler>();
services.AddSingleton<IConnectionMultiplexer>( ConnectionMultiplexer.Connect(redisConnectionString));
services.AddSingleton<IStationHistoryService,MongoStationHistoryService>();
using ServiceProvider serviceProvider = services.BuildServiceProvider();
using IServiceScope scope = serviceProvider.CreateScope();

BikeFleetDbContext dbContext = scope.ServiceProvider.GetRequiredService<BikeFleetDbContext>();
await dbContext.Database.EnsureCreatedAsync();
bool canConnect = await dbContext.Database.CanConnectAsync();

Console.WriteLine(canConnect? "Connected to MySQL successfully.": "Could not connect to MySQL.");

IKafkaConsumerService kafkaConsumer =serviceProvider.GetRequiredService<IKafkaConsumerService>();

using CancellationTokenSource cancellation = new();

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cancellation.Cancel();
};

Console.WriteLine("Consumer started. Press Ctrl + C to stop.");
logger.Information("Consumer started.");

await kafkaConsumer.StartConsumingAsync(cancellation.Token);

Console.WriteLine("Consumer stopped.");
logger.Information("Consumer stopped.");