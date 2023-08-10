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
        private readonly ILogger _logger;

        public CreateCategoryEventHandler(ICategoryRepository categoryRepository, ILogger logger) => (_categoryRepository, _logger) = (categoryRepository, logger);

        public async Task Handle(IMessageContext context, CreateCategoryEvent message)
        {
            var categoryDomain = message.Category.ToDomain();

            await _categoryRepository.AddOne(categoryDomain);

            _logger.Information($"Category created with sucessfully on database");
        }

        public async Task Handle(IMessageContext context, UpdateCategoryEvent message)
        {
            var categoryDomain = message.ToDomain();

            var filter = new FilterDefinitionBuilder<Domain.Entities.Category>()
                .Where(m => m.Id == categoryDomain.Id);

            await _categoryRepository.ReplaceOneAsync(dm => filter.Inject(), categoryDomain);

            _logger.Information($"Category updated with sucessfully on database");
        }

        public async Task Handle(IMessageContext context, DeleteCategoryEvent message)
        {
            var filter = new FilterDefinitionBuilder<Domain.Entities.Category>()
                .Where(ev => ev.Id == message.Id);

            await _categoryRepository.DeleteAsync(dm => filter.Inject());

            _logger.Information($"Category deleted with sucessfully on database");
        }
    }
}
