using Application.Kafka.Mappers;
using Data.Persistence.UnitOfWork;
using Domain.Interfaces;
using KafkaFlow;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SpendManagement.Contracts.V1.Events.RecurringReceiptEvents;

namespace Application.Kafka.Handlers.RecurringReceipt
{
    public class RecurringReceiptEventHandler(IRecurringReceiptRepository recurringReceiptRepository, IUnitOfWork unitOfWork) :
        IMessageHandler<CreateRecurringReceiptEvent>,
        IMessageHandler<UpdateRecurringReceiptEvent>,
        IMessageHandler<DeleteRecurringReceiptEvent>
    {
        private readonly IRecurringReceiptRepository _recurringReceiptRepository = recurringReceiptRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task Handle(IMessageContext context, CreateRecurringReceiptEvent message)
        {
            var domainEntity = message.ToDomain();
            await _recurringReceiptRepository.AddOneAsync(domainEntity);

            var spendManagementDomainEvent = message.ToSpendManagementEvent();
            await _unitOfWork.SpendManagementEventRepository.Add(spendManagementDomainEvent);

            _unitOfWork.Commit();
        }

        public async Task Handle(IMessageContext context, UpdateRecurringReceiptEvent message)
        {
            var domainEntity = message.ToDomain();

            var filter = new FilterDefinitionBuilder<Domain.Entities.RecurringReceipt>()
            .Where(m => m.Id == domainEntity.Id);

            await _recurringReceiptRepository.ReplaceOneAsync(_ => filter.Inject(), domainEntity);

            var spendManagementDomainEvent = message.ToSpendManagementEvent();

            await _unitOfWork.SpendManagementEventRepository.Add(spendManagementDomainEvent);
            _unitOfWork.Commit();
        }

        public async Task Handle(IMessageContext context, DeleteRecurringReceiptEvent message)
        {
            var filter = new FilterDefinitionBuilder<Domain.Entities.Receipt>()
              .Where(ev => ev.Id == message.Id);

            await _recurringReceiptRepository.DeleteAsync(_ => filter.Inject());

            var spendManagementDomainEvent = message.ToSpendManagementEvent();
            await _unitOfWork.SpendManagementEventRepository.Add(spendManagementDomainEvent);

            _unitOfWork.Commit();
        }
    }
}
