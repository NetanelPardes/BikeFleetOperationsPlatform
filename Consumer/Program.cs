using Consumer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

DotNetEnv.Env.TraversePath().Load();

string connectionString = Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING") ?? throw new InvalidOperationException("MYSQL_CONNECTION_STRING is missing from .env");

ServiceCollection services = new();

services.AddDbContext<BikeFleetDbContext>(options => options.UseMySql(connectionString,new MySqlServerVersion(new Version(8, 4, 0))));

using ServiceProvider serviceProvider = services.BuildServiceProvider();
using IServiceScope scope = serviceProvider.CreateScope();

BikeFleetDbContext dbContext = scope.ServiceProvider.GetRequiredService<BikeFleetDbContext>();
await dbContext.Database.EnsureCreatedAsync();
bool canConnect = await dbContext.Database.CanConnectAsync();

Console.WriteLine(canConnect? "Connected to MySQL successfully.": "Could not connect to MySQL.");