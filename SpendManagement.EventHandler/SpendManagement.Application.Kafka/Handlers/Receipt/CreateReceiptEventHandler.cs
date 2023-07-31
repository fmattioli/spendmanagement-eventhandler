using Application.Kafka.Mappers;
using Domain.Interfaces;
using KafkaFlow;
using KafkaFlow.TypedHandler;
using SpendManagement.Contracts.V1.Events.ReceiptEvents;

namespace Application.Kafka.Handlers.Receipt
{
    public class CreateReceiptEventHandler : IMessageHandler<CreatedReceiptEvent>
    {
        private readonly IReceiptRepository _receiptRepository;

        public CreateReceiptEventHandler(IReceiptRepository receiptRepository)
        {
            _receiptRepository = receiptRepository;
        }

        public async Task Handle(IMessageContext context, CreatedReceiptEvent message)
        {
            var domainEntity = message.ToDomain();
            await _receiptRepository.AddOne(domainEntity);
        }
    }
}
