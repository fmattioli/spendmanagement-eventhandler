using Application.Kafka.Mappers;
using Data.Persistence.UnitOfWork;
using Domain.Interfaces;
using KafkaFlow;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SpendManagement.Contracts.V1.Events.CategoryEvents;

namespace Application.Kafka.Handlers.Category
{
    public class CategoryEventHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork) :
        IMessageHandler<CreateCategoryEvent>,
        IMessageHandler<UpdateCategoryEvent>,
        IMessageHandler<DeleteCategoryEvent>
    {
        private readonly ICategoryRepository _categoryRepository = categoryRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task Handle(IMessageContext context, CreateCategoryEvent message)
        {
            var categoryDomain = message.Category.ToDomain();

            await _categoryRepository.AddOneAsync(categoryDomain);

            var spendManagementDomainEvent = message.ToSpendManagementEvent();
            await _unitOfWork.SpendManagementEventRepository.Add(spendManagementDomainEvent);
            _unitOfWork.Commit();
        }

        public async Task Handle(IMessageContext context, UpdateCategoryEvent message)
        {
            var categoryDomain = message.ToDomain();

            var filter = new FilterDefinitionBuilder<Domain.Entities.Category>()
                .Where(m => m.Id == categoryDomain.Id);

            await _categoryRepository.ReplaceOneAsync(_ => filter.Inject(), categoryDomain);

            var spendManagementDomainEvent = message.ToSpendManagementEvent();
            await _unitOfWork.SpendManagementEventRepository.Add(spendManagementDomainEvent);
            _unitOfWork.Commit();
        }

        public async Task Handle(IMessageContext context, DeleteCategoryEvent message)
        {
            var filter = new FilterDefinitionBuilder<Domain.Entities.Category>()
                .Where(ev => ev.Id == message.Id);

            await _categoryRepository.DeleteAsync(_ => filter.Inject());

            var spendManagementDomainEvent = message.ToSpendManagementEvent();
            await _unitOfWork.SpendManagementEventRepository.Add(spendManagementDomainEvent);
            _unitOfWork.Commit();
        }
    }
}
