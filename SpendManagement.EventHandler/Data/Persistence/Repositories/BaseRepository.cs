using Domain.Interfaces;

using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Data.Persistence.Repositories
{
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
    {
        private readonly IMongoCollection<TEntity> collection;
        public BaseRepository(IMongoDatabase mongoDb, string collectionName)
        {
            MapClasses();
            this.collection = mongoDb.GetCollection<TEntity>(collectionName);
        }

        public async Task AddOne(TEntity entity)
        {
            await this.collection.InsertOneAsync(entity);
        }

        private static void MapClasses()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(TEntity)))
            {
                BsonClassMap.RegisterClassMap<TEntity>(cm =>
                {
                    cm.AutoMap();
                    cm.SetIgnoreExtraElements(true);
                });
            }
        }
    }
}
