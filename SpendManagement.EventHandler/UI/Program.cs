using Crosscutting.Models;
using CrossCutting.Extensions.HealthChecks;
using CrossCutting.Extensions.Kafka;
using CrossCutting.Extensions.Logging;
using CrossCutting.Extensions.Mongo;
using CrossCutting.Extensions.Repositories;
using CrossCutting.Extensions.Tracing;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddFilter("Microsoft", LogLevel.Critical);
});

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
var applicationSettings = builder.Configuration.GetSection("Settings").Get<Settings>();

builder.Services.AddSingleton<ISettings>(applicationSettings ?? throw new Exception("Error while reading app settings."));

builder.Services
    .AddTracing()
    .AddHealthCheckers(applicationSettings)
    .AddKafka(applicationSettings.KafkaSettings)
    .AddMongo(applicationSettings.MongoSettings)
    .AddRepositories()
    .AddLoggingDependency();

var app = builder.Build();

app.ShowKafkaDashboard();
app.UseHealthCheckers();
app.MapGet("/", () => "Hello!");
app.Run();
