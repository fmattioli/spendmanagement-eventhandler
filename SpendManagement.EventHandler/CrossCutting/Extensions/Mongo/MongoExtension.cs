using Crosscutting.Models;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace CrossCutting.Extensions.Mongo
{
    public static class MongoExtension
    {
        public static IServiceCollection AddMongo(this IServiceCollection services, MongoSettings mongoSettings)
        {
            services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoSettings.ConnectionString));

            services.AddSingleton(sp =>
            {
                var mongoClient = sp.GetService<IMongoClient>() ?? throw new Exception("MongoDB was not injectable.");
                var db = mongoClient.GetDatabase(mongoSettings.Database);
                return db;
            });

            return services;
        }
    }
}
