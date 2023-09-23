using Domain.Entities;

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
    }
}
