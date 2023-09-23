using Domain.Entities;
using Domain.Interfaces;
using MongoDB.Driver;

namespace Data.Persistence.Repositories
{
    public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(IMongoDatabase mongoDb) : base(mongoDb, "Categories")
        {
            
        }

    }
}
