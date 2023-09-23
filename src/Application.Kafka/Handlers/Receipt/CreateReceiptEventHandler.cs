using Application.Kafka.Mappers;
using Domain.Interfaces;
using KafkaFlow;
using KafkaFlow.TypedHandler;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Serilog;
using SpendManagement.Contracts.V1.Events.ReceiptEvents;

namespace Application.Kafka.Handlers.Receipt
{
    public class CreateReceiptEventHandler :
        IMessageHandler<CreatedReceiptEvent>,
        IMessageHandler<UpdateReceiptEvent>,
        IMessageHandler<DeleteReceiptEvent>
    {
        private readonly IReceiptRepository _receiptRepository;
        private readonly ILogger _logger;

        public CreateReceiptEventHandler(IReceiptRepository receiptRepository, ILogger logger)
        {
            _receiptRepository = receiptRepository;
            _logger = logger;
        }

        public async Task Handle(IMessageContext context, CreatedReceiptEvent message)
        {
            var domainEntity = message.ToDomain();

            await _receiptRepository.AddOne(domainEntity);

            _logger.Information("Receipt created with sucessfully on database");
        }

        public async Task Handle(IMessageContext context, UpdateReceiptEvent message)
        {
            var domainEntity = message.ToDomain();

            var filter = new FilterDefinitionBuilder<Domain.Entities.Receipt>()
                .Where(m => m.Id == domainEntity.Id);

            await _receiptRepository.ReplaceOneAsync(_ => filter.Inject(), domainEntity);

            _logger.Information("Receipt updated with sucessfully on database");
        }

        public async Task Handle(IMessageContext context, DeleteReceiptEvent message)
        {
            var filter = new FilterDefinitionBuilder<Domain.Entities.Receipt>()
                .Where(ev => ev.Id == message.Id);

            await _receiptRepository.DeleteAsync(_ => filter.Inject());

            _logger.Information("Receipt deleted with sucessfully on database");
        }
    }
}
