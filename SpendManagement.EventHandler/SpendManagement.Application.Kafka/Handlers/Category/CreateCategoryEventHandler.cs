using Application.Kafka.Mappers;
using Domain.Interfaces;
using KafkaFlow;
using KafkaFlow.TypedHandler;
using SpendManagement.Contracts.V1.Events.CategoryEvents;

namespace Application.Kafka.Handlers.Category
{
    public class CreateCategoryEventHandler : IMessageHandler<CreateCategoryEvents>
    {
        private ICategoryRepository _categoryRepository;

        public CreateCategoryEventHandler(ICategoryRepository categoryRepository) => _categoryRepository = categoryRepository; 
        public async Task Handle(IMessageContext context, CreateCategoryEvents message)
        {
            var categoryDomain = message.Category.ToDomain();
            await _categoryRepository.AddOne(categoryDomain);
        }
    }
}
