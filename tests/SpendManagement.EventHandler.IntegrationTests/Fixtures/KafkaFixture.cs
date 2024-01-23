using KafkaFlow;
using KafkaFlow.Serializer;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Formatting.Json;
using SpendManagement.EventHandler.IntegrationTests.Configuration;
using SpendManagement.EventHandler.IntegrationTests.Helpers;
using SpendManagement.Topics;

namespace SpendManagement.EventHandler.IntegrationTests.Fixtures
{
    public class KafkaFixture
    {
        private readonly IMessageProducer<Contracts.V1.Interfaces.IEvent> _eventProducer;

        public KafkaFixture()
        {
            var settings = TestSettings.KafkaSettings;

            var services = new ServiceCollection();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(new JsonFormatter())
                .CreateLogger();

            services.AddSingleton(Log.Logger);

            services.AddKafka(kafka => kafka
               .UseLogHandler<ConsoleLogHandler>()
                   .AddCluster(cluster => cluster
                       .WithBrokers(new[] { settings?.Broker })
                       .CreateTopicIfNotExists(KafkaTopics.Events.GetReceiptEvents(settings!.Environment), 2, 1)
                       .AddProducer<Contracts.V1.Interfaces.IEvent>(
                       p => p
                       .DefaultTopic(KafkaTopics.Events.GetReceiptEvents(settings!.Environment))
                       .AddMiddlewares(m => m
                       .AddSerializer<JsonCoreSerializer>())
                        .WithAcks(Acks.All)
            )));

            var provider = services.BuildServiceProvider();
            this._eventProducer = provider.GetRequiredService<IMessageProducer<Contracts.V1.Interfaces.IEvent>>();
        }

        public async Task ProduceEventAsync(Contracts.V1.Interfaces.IEvent message, IMessageHeaders? headers = null)
        {
            await this._eventProducer.ProduceAsync(message.RoutingKey, message, headers, null);
        }
    }
}
