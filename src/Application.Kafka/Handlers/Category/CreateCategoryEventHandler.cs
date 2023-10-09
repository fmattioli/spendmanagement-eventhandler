using Application.Kafka.Mappers;
using Domain.Interfaces;
using KafkaFlow;
using KafkaFlow.TypedHandler;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SpendManagement.Contracts.V1.Events.CategoryEvents;
using Serilog;

namespace Application.Kafka.Handlers.Category
{
    public class CreateCategoryEventHandler :
        IMessageHandler<CreateCategoryEvent>,
        IMessageHandler<UpdateCategoryEvent>,
        IMessageHandler<DeleteCategoryEvent>
    {
        private readonly ICategoryRepository _categoryRepository;

        public CreateCategoryEventHandler(ICategoryRepository categoryRepository) => _categoryRepository = categoryRepository;

        public async Task Handle(IMessageContext context, CreateCategoryEvent message)
        {
            var categoryDomain = message.Category.ToDomain();

            await _categoryRepository.AddOne(categoryDomain);
        }

        public async Task Handle(IMessageContext context, UpdateCategoryEvent message)
        {
            var categoryDomain = message.ToDomain();

            var filter = new FilterDefinitionBuilder<Domain.Entities.Category>()
                .Where(m => m.Id == categoryDomain.Id);

            await _categoryRepository.ReplaceOneAsync(_ => filter.Inject(), categoryDomain);
        }

        public async Task Handle(IMessageContext context, DeleteCategoryEvent message)
        {
            var filter = new FilterDefinitionBuilder<Domain.Entities.Category>()
                .Where(ev => ev.Id == message.Id);

            await _categoryRepository.DeleteAsync(_ => filter.Inject());
        }
    }
}
