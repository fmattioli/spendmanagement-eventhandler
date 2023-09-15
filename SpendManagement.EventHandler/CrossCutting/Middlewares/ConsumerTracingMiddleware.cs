using Application.Kafka.Constants;
using KafkaFlow;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using System.Diagnostics;
using System.Text;

namespace Crosscutting.Middlewares
{
    public class ConsumerTracingMiddleware : IMessageMiddleware
    {
        private static readonly ActivitySource Activity = new(Constants.ApplicationName);
        private static readonly TextMapPropagator Propagator = new TraceContextPropagator();

        public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
        {
            var parentContext = Propagator.Extract(default, context, ExtractTraceContextFromBasicProperties);
            Baggage.Current = parentContext.Baggage;
            using var activity = Activity.StartActivity("Processing message", ActivityKind.Consumer, parentContext.ActivityContext);
            await next(context);
        }

        private static IEnumerable<string> ExtractTraceContextFromBasicProperties(IMessageContext props, string key)
        {
            if (props.Headers.GetString("traceparent") is not null && props.Headers.GetString("traceparent") != string.Empty)
            {
                var bytes = Encoding.ASCII.GetBytes(props.Headers.GetString("traceparent"));
                return new[] { Encoding.UTF8.GetString(bytes) };
            }

            return Enumerable.Empty<string>();
        }
    }
}
