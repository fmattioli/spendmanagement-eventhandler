using CrossCutting.Extensions.Kafka;
using KafkaFlow;
using Newtonsoft.Json;
using Serilog;
using System.Diagnostics;

namespace Crosscutting.Middlewares
{
    public class ConsumerLoggingMiddleware : IMessageMiddleware
    {
        private readonly ILogger log;

        public ConsumerLoggingMiddleware(ILogger log)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
        {
            var sw = Stopwatch.StartNew();

            var kafkaMessageInfo = new
            {
                context.ConsumerContext.GroupId,
                context.ConsumerContext.Topic,
                PartitionNumber = context.ConsumerContext.Partition,
                PartitionKey = context.GetPartitionKey(),
                Headers = context.Headers.ToJsonString(),
                MessageType = context.Message.Value.GetType().FullName,
                Message = JsonConvert.SerializeObject(context.Message)
            };

            this.log.Information($"[{nameof(ConsumerLoggingMiddleware)}] " + "Kafka message received. {@kafkaMessageInfo}", kafkaMessageInfo);

            try
            {
                await next(context);

                var kafkaMessageProcessed = new
                {
                    context.ConsumerContext.WorkerId,
                    context.ConsumerContext.GroupId,
                    context.ConsumerContext.Topic,
                    PartitionNumber = context.ConsumerContext.Partition,
                    PartitionKey = context.GetPartitionKey(),
                    context.ConsumerContext.Offset,
                    Headers = context.Headers.ToJsonString(),
                    MessageType = context.Message.Value.GetType().FullName,
                    Message = JsonConvert.SerializeObject(context.Message),
                    ProcessingTime = sw.ElapsedMilliseconds
                };

                this.log.Information(
                    $"[{nameof(ConsumerLoggingMiddleware)}] - Kafka message processed." + "{kafkaMessageInfo}", kafkaMessageProcessed);
            }
            catch (Exception ex)
            {
                this.log.Error(
                    $"[{nameof(ConsumerLoggingMiddleware)}] - Failed to process message." + ex.Message,
                    ex,
                    () => new
                    {
                        context.ConsumerContext.WorkerId,
                        context.ConsumerContext.GroupId,
                        context.ConsumerContext.Topic,
                        PartitionNumber = context.ConsumerContext.Partition,
                        PartitionKey = context.GetPartitionKey(),
                        context.ConsumerContext.Offset,
                        Headers = context.Headers.ToJsonString(),
                        MessageType = context.Message.Value.GetType().FullName,
                        Message = JsonConvert.SerializeObject(context.Message),
                        ProcessingTime = sw.ElapsedMilliseconds
                    });
            }
        }
    }
}
