using Application.Kafka.Constants;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace CrossCutting.Extensions.Tracing
{
    public static class TracingExtensions
    {
        public static IServiceCollection AddTracing(this IServiceCollection services)
        {
            services.AddOpenTelemetry().WithTracing(tcb =>
            {
                tcb
                .AddSource(Constants.ApplicationName)
                .SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                        .AddService(serviceName: Constants.ApplicationName))
                .AddAspNetCoreInstrumentation()
                .AddMongoDBInstrumentation()
                .AddOtlpExporter();
            });

            services.AddSingleton(TracerProvider.Default.GetTracer(Constants.ApplicationName));
            return services;
        }
    }
}
