using Crosscutting.Models;
using CrossCutting.Extensions.Kafka;
using KafkaFlow;
using Polly;
using Serilog;

namespace Crosscutting.Middlewares
{
    public class ConsumerRetryMiddleware(ILogger log, ISettings settings) : IMessageMiddleware
    {
        private readonly ILogger log = log ?? throw new ArgumentNullException(nameof(log));

        private readonly int retryCount = settings.KafkaSettings!.ConsumerRetryCount;

        private readonly TimeSpan retryInterval = TimeSpan.FromSeconds(settings.KafkaSettings.ConsumerRetryInterval);

        public Task Invoke(IMessageContext context, MiddlewareDelegate next)
        {
            return Policy
                .Handle<Exception>(ex => ex is not InvalidOperationException)
                .WaitAndRetryAsync(
                    this.retryCount,
                    _ => this.retryInterval,
                    (ex, _, retryAttempt, __) =>
                    {
                        this.log.Warning(
                            $"[{nameof(ConsumerRetryMiddleware)}] - Failed to process message, retrying... " + ex.Message,
                            ex,
                            () => new
                            {
                                context.ConsumerContext.Topic,
                                context.ConsumerContext.Offset,
                                PartitionNumber = context.ConsumerContext.Partition,
                                PartitionKey = context.GetPartitionKey(),
                                Headers = context.Headers.ToJsonString(),
                                RetryAttempt = retryAttempt,
                                ex.Message
                            });
                    })
                .ExecuteAsync(() => next(context));
        }
    }
}
