using Domain.Entities;
using Newtonsoft.Json;
using SpendManagement.Contracts.V1.Events.CategoryEvents;
using CategoryEvent = SpendManagement.Contracts.V1.Entities;

namespace Application.Kafka.Mappers
{
    public static class CategoryMapper
    {
        public static Category ToDomain(this CategoryEvent.Category category)
        {
            return new Category(category.Id, category.Name, category.CreatedDate);
        }

        public static Category ToDomain(this UpdateCategoryEvent updateCategoryEvent)
        {
            return new Category(updateCategoryEvent.Category.Id, updateCategoryEvent.Category.Name, updateCategoryEvent.Category.CreatedDate);
        }

        public static SpendManagementEvent ToSpendManagementEvent(this CreateCategoryEvent createCategoryEvent)
        {
            return new SpendManagementEvent(
                createCategoryEvent.RoutingKey,
                createCategoryEvent.EventCreatedDate,
                nameof(CreateCategoryEvent),
                JsonConvert.SerializeObject(createCategoryEvent));
        }

        public static SpendManagementEvent ToSpendManagementEvent(this UpdateCategoryEvent updateCategoryEvent)
        {
            return new SpendManagementEvent(
                updateCategoryEvent.RoutingKey,
                updateCategoryEvent.EventCreatedDate,
                nameof(UpdateCategoryEvent),
                JsonConvert.SerializeObject(updateCategoryEvent));
        }

        public static SpendManagementEvent ToSpendManagementEvent(this DeleteCategoryEvent deleteCategoryEvent)
        {
            return new SpendManagementEvent(
                deleteCategoryEvent.RoutingKey,
                deleteCategoryEvent.EventCreatedDate,
                nameof(DeleteCategoryEvent),
                JsonConvert.SerializeObject(deleteCategoryEvent));
        }
    }
}
