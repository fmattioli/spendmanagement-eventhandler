﻿using Crosscutting.Models;
using CrossCutting.Extensions.Configuration;
using CrossCutting.Extensions.HealthChecks;
using CrossCutting.Extensions.Kafka;
using CrossCutting.Extensions.Logging;
using CrossCutting.Extensions.Mongo;
using CrossCutting.Extensions.Repositories;
using CrossCutting.Extensions.Tracing;
using CrossCutting.Extensions.UnitOfWork;
using KafkaFlow;

var builder = WebApplication.CreateBuilder(args);

var enviroment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

builder.Configuration
    .AddJsonFile("appsettings.json", true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{enviroment}.json", true, reloadOnChange: true)
    .AddEnvironmentVariables();

var applicationSettings = builder.Configuration.GetApplicationSettings(builder.Environment);

builder.Logging
    .ClearProviders()
    .AddFilter("Microsoft", LogLevel.Warning)
    .AddFilter("Microsoft", LogLevel.Error)
    .AddFilter("Microsoft", LogLevel.Critical);

builder.Services.AddSingleton<ISettings>(applicationSettings);

builder.Services
    .AddTracing(applicationSettings.TracingSettings)
    .AddHealthCheckers(applicationSettings)
    .AddUnitOfWork(applicationSettings.SqlSettings!.ConnectionString!)
    .AddKafka(applicationSettings.KafkaSettings!)
    .AddMongo(applicationSettings.MongoSettings!)
    .AddRepositories()
    .AddLoggingDependency();

var app = builder.Build();

app.ShowKafkaDashboard();
app.UseHealthCheckers();
app.MapGet("/", () => "Hello! I'm working. My work is only reading events from a kafka topic, process it, make some logic and save it on a NoSQL database. \n" +
                      "Check my health to understand if everything is okay." + applicationSettings.KafkaSettings!.Sasl_Username);

var kafkaBus = app.Services.CreateKafkaBus();
await kafkaBus.StartAsync();
app.Run();
