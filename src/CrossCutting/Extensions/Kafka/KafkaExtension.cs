﻿using Application.Kafka.Handlers.Receipt;
using Crosscutting.HostedService;
using Crosscutting.Middlewares;
using Crosscutting.Models;
using CrossCutting.Extensions.Kafka;
using KafkaFlow;
using KafkaFlow.Admin.Dashboard;
using KafkaFlow.Configuration;
using KafkaFlow.Serializer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SpendManagement.Topics;

namespace CrossCutting.Extensions.Kafka
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
                    .AddConsumers(kafkaSettings)
                    )
                );

            return services;
        }

        private static IClusterConfigurationBuilder AddBrokers(
            this IClusterConfigurationBuilder builder,
            KafkaSettings? settings)
        {
            if (settings?.Sasl_Enabled ?? false)
            {
                builder
                    .WithBrokers(settings.Sasl_Brokers)
                    .WithSecurityInformation(information =>
                    {
                        information.SaslMechanism = SaslMechanism.ScramSha256;
                        information.SaslUsername = settings.Sasl_Username;
                        information.SaslPassword = settings.Sasl_Password;
                        information.SecurityProtocol = SecurityProtocol.SaslSsl;
                        information.EnableSslCertificateVerification = true;
                    });
            }
            else
            {
                builder.WithBrokers(new[] { settings?.Broker });
            }

            return builder;
        }

        private static IClusterConfigurationBuilder AddConsumers(
            this IClusterConfigurationBuilder builder,
            KafkaSettings? settings)
        {
            builder.AddConsumer(
                consumer => consumer
                     .Topics(KafkaTopics.Events.GetReceiptEvents(settings!.Environment))
                     .WithGroupId("Receipts-Events")
                     .WithName("Receipt-Events")
                     .WithBufferSize(settings.BufferSize)
                     .WithWorkersCount(settings.WorkerCount)
                     .WithAutoOffsetReset(AutoOffsetReset.Latest)
                     .WithInitialState(Enum.Parse<ConsumerInitialState>(settings.ConsumerInitialState ?? "Running"))
                     .AddMiddlewares(
                        middlewares =>
                            middlewares
                            .AddDeserializer<JsonCoreDeserializer>()
                            .Add<ConsumerLoggingMiddleware>()
                            .Add<ConsumerTracingMiddleware>()
                            .Add<ConsumerRetryMiddleware>()
                            .AddTypedHandlers(
                                h => h
                                    .WithHandlerLifetime(InstanceLifetime.Scoped)
                                    .AddHandlersFromAssemblyOf<ReceiptEventHandler>()
                                    )
                            )
                     );

            return builder;
        }
    }
}
