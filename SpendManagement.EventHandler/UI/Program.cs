 using Crosscutting.Extensions;
using Crosscutting.Models;
using CrossCutting.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureAppConfiguration((hosting, config) =>
{
    var currentDirectory = Directory.GetCurrentDirectory();
    config
        .SetBasePath(currentDirectory)
        .AddJsonFile($"{currentDirectory}/appsettings.json");
}).ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddFilter("Microsoft", LogLevel.Critical);
});

var applicationSettings = builder.Configuration.GetSection("Settings").Get<Settings>();


builder.Services
    .AddKafka(applicationSettings.KafkaSettings)
    .AddRepositories()
    .AddLoggingDependency();

var app = builder.Build();
app.ShowKafkaDashboard();
app.Run();