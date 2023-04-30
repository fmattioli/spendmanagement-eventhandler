using KafkaFlow;
using KafkaFlow.TypedHandler;
using SpendManagement.Contracts.V1.Events;

namespace Application.Kafka.Handlers
{
    public class CreateReceiptEventHandler : IMessageHandler<CreatedReceiptEvent>
    {
        public Task Handle(IMessageContext context, CreatedReceiptEvent message)
        {
            throw new NotImplementedException();
        }
    }
}
