using Confluent.Kafka;
using Crosscutting.Models;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;

namespace CrossCutting.Extensions.HealthChecks
{
    public static class HealthChecksExtensions
    {
        private static readonly string[] tags = ["db", "data"];
        public static IServiceCollection AddHealthCheckers(this IServiceCollection services, Settings settings)
        {
            var configKafka = new ProducerConfig { BootstrapServers = settings.KafkaSettings?.Broker };

            services.AddHealthChecks()
                .AddKafka(configKafka, name: "Kafka")
                .AddSqlServer(settings.SqlSettings!.ConnectionString!, name: "SqlServer", tags: tags)
                .AddMongoDb(settings.MongoSettings?.ConnectionString!, name: "MongoDB", tags: tags);

            services
                .AddHealthChecksUI()
                .AddInMemoryStorage();

            return services;
        }

        public static void UseHealthCheckers(this IApplicationBuilder app)
        {
            app.UseHealthChecks("/health", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.UseHealthChecksUI(options => options.UIPath = "/monitor");
        }
    }
}
