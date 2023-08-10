using Domain.Interfaces;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Linq.Expressions;

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

        public async Task ReplaceOneAsync(Expression<Func<TEntity, bool>> filterExpression, TEntity update)
        {
            await this.collection.ReplaceOneAsync(filterExpression, update);
        }

        public async Task DeleteAsync(Expression<Func<TEntity, bool>> filterExpression)
        {
            await this.collection.DeleteOneAsync(filterExpression);
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
