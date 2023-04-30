using Application.Kafka.Handlers;
using Crosscutting.HostedService;
using Crosscutting.Middlewares;
using Crosscutting.Models;
using KafkaFlow;
using KafkaFlow.Admin.Dashboard;
using KafkaFlow.Configuration;
using KafkaFlow.Serializer;
using KafkaFlow.TypedHandler;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SpendManagement.Topics;

namespace CrossCutting.Extensions
{
    public static class KafkaExtension
    {
        public static IApplicationBuilder ShowKafkaDashboard(this IApplicationBuilder app) => app.UseKafkaFlowDashboard();

        public static IServiceCollection AddKafka(this IServiceCollection services, KafkaSettings kafkaSettings)
        {
            services.AddKafka(kafka => kafka
                .UseConsoleLog()
                .AddCluster(cluster => cluster
                    .AddBrokers(kafkaSettings)
                    .AddTelemetry()
                    .AddConsumers(kafkaSettings)
                    )
                );
            services.AddHostedService<KafkaBusHostedService>();
            return services;
        }

        private static IClusterConfigurationBuilder AddTelemetry(
            this IClusterConfigurationBuilder builder)
        {
            builder
                .EnableAdminMessages(KafkaTopics.Events.ReceiptTelemetry)
                .EnableTelemetry(KafkaTopics.Events.ReceiptTelemetry);

            return builder;
        }

        private static IClusterConfigurationBuilder AddBrokers(
            this IClusterConfigurationBuilder builder,
            KafkaSettings? settings)
        {
            if (settings?.Sasl_Enabled ?? false)
            {
                builder
                    .WithBrokers(settings.Sasl_Brokers)
                    .WithSecurityInformation(si =>
                    {
                        si.SecurityProtocol = KafkaFlow.Configuration.SecurityProtocol.SaslSsl;
                        si.SaslUsername = settings.Sasl_UserName;
                        si.SaslPassword = settings.Sasl_Password;
                        si.SaslMechanism = KafkaFlow.Configuration.SaslMechanism.Plain;
                        si.SslCaLocation = string.Empty;
                    });
            }
            else
            {
                builder.WithBrokers(new[] { settings?.Brokers });
            }

            return builder;
        }

        private static IClusterConfigurationBuilder AddConsumers(
            this IClusterConfigurationBuilder builder,
            KafkaSettings? settings)
        {
            builder.AddConsumer(
                consumer => consumer
                     .Topics(KafkaTopics.Events.ReceiptEventTopicName)
                     .WithGroupId("receipts-events")
                     .WithName("Receipt events")
                     .WithWorkersCount(settings?.WorkerCount ?? 0)
                     .WithBufferSize(settings?.BufferSize ?? 0)
                     .WithAutoOffsetReset(KafkaFlow.AutoOffsetReset.Latest)
                     .WithInitialState(Enum.Parse<ConsumerInitialState>(settings?.ConsumerInitialState))
                     .AddMiddlewares(
                        middlewares =>
                            middlewares
                            .AddSerializer<JsonCoreSerializer>()
                            .Add<ConsumerLoggingMiddleware>()
                            .Add<ConsumerRetryMiddleware>()
                            .AddTypedHandlers(
                                h => h
                                    .WithHandlerLifetime(InstanceLifetime.Scoped)
                                    .AddHandler<CreateReceiptEventHandler>()
                                    )
                            )
                     );

            return builder;
        }

    }
}
