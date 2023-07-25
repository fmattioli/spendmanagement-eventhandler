using Domain.Entities;
using CategoryEvent = SpendManagement.Contracts.V1.Entities;
namespace Application.Kafka.Mappers
{
    public static class CategoryMapper
    {
        public static Category ToDomain(this CategoryEvent.Category category)
        {
            return new Category(category.Id, category.Name, category.CreatedDate);
        } 
    }
}
