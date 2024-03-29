﻿using Application.Kafka.Mappers;
using Data.Persistence.UnitOfWork;
using Domain.Interfaces;
using KafkaFlow;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SpendManagement.Contracts.V1.Events.ReceiptEvents;

namespace Application.Kafka.Handlers.Receipt
{
    public class ReceiptEventHandler(IReceiptRepository receiptRepository, IUnitOfWork unitOfWork) :
        IMessageHandler<CreatedReceiptEvent>,
        IMessageHandler<UpdateReceiptEvent>,
        IMessageHandler<DeleteReceiptEvent>
    {
        private readonly IReceiptRepository _receiptRepository = receiptRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task Handle(IMessageContext context, CreatedReceiptEvent message)
        {
            var domainEntity = message.ToDomain();

            await _receiptRepository.AddOneAsync(domainEntity);

            var spendManagementDomainEvent = message.ToSpendManagementEvent();
            await _unitOfWork.SpendManagementEventRepository.Add(spendManagementDomainEvent);
            _unitOfWork.Commit();
        }

        public async Task Handle(IMessageContext context, UpdateReceiptEvent message)
        {
            var domainEntity = message.ToDomain();

            var filter = new FilterDefinitionBuilder<Domain.Entities.Receipt>()
                .Where(m => m.Id == domainEntity.Id);

            await _receiptRepository.ReplaceOneAsync(_ => filter.Inject(), domainEntity);

            var spendManagementDomainEvent = message.ToSpendManagementEvent();
            await _unitOfWork.SpendManagementEventRepository.Add(spendManagementDomainEvent);
            _unitOfWork.Commit();
        }

        public async Task Handle(IMessageContext context, DeleteReceiptEvent message)
        {
            var filter = new FilterDefinitionBuilder<Domain.Entities.Receipt>()
                .Where(ev => ev.Id == message.Id);

            await _receiptRepository.DeleteAsync(_ => filter.Inject());

            var spendManagementDomainEvent = message.ToSpendManagementEvent();
            await _unitOfWork.SpendManagementEventRepository.Add(spendManagementDomainEvent);
            _unitOfWork.Commit();
        }
    }
}
