using Api.Data;
using Api.Services;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using MongoDB.Driver;
using Serilog;
using Serilog.Events;
using Microsoft.AspNetCore.Diagnostics;

DotNetEnv.Env.TraversePath().Load();

Directory.CreateDirectory("Logs");

using var logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command",LogEventLevel.Warning)
    .WriteTo.Console(outputTemplate:"{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("Logs/Api-.log",rollingInterval: RollingInterval.Day,retainedFileCountLimit: 7,outputTemplate:"{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj} {NewLine}{Exception}")
    .CreateLogger();

string mysqlConnectionString =Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING")?? throw new InvalidOperationException("MYSQL_CONNECTION_STRING is missing from .env");
string redisConnectionString =Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")?? throw new InvalidOperationException("REDIS_CONNECTION_STRING is missing from .env");
string mongoConnectionString =Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING")?? throw new InvalidOperationException("MONGODB_CONNECTION_STRING is missing from .env");

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog(logger);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<BikeFleetDbContext>(options =>options.UseMySql(mysqlConnectionString,new MySqlServerVersion(new Version(8, 4, 0)),mysql => mysql.EnableRetryOnFailure()));

builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services.AddScoped<IStationStatusService,StationStatusService>();

builder.Services.AddScoped<IStationService,StationService>();

builder.Services.AddSingleton<IMongoClient>(new MongoClient(mongoConnectionString));

builder.Services.AddScoped<IStationHistoryService,StationHistoryService>();
builder.Services.AddScoped<IDashboardService,DashboardService>();

WebApplication app = builder.Build();

app.UseExceptionHandler(errorApplication =>
{
    errorApplication.Run(async context =>
    {
        IExceptionHandlerFeature? error = context.Features.Get<IExceptionHandlerFeature>();

        if (error?.Error is not null)
        {
            logger.Error(error.Error,"Unhandled exception while processing {Method} {Path}",context.Request.Method,context.Request.Path);
        }

        context.Response.StatusCode =StatusCodes.Status500InternalServerError;

        await context.Response.WriteAsJsonAsync(new
        {
            statusCode = 500,
            message = "An unexpected error occurred."
        });
    });
});

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();