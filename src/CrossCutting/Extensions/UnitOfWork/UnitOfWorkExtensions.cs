using Data.Persistence.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Data;

namespace CrossCutting.Extensions.UnitOfWork
{
    public static class UnitOfWorkExtensions
    {
        public static IServiceCollection AddUnitOfWork(this IServiceCollection services, string connectionString)
        {
            services.AddScoped<IUnitOfWork, Data.Persistence.UnitOfWork.UnitOfWork>();
            services.AddScoped((s) => new NpgsqlConnection(connectionString));
            services.AddScoped<IDbTransaction>(s =>
            {
                NpgsqlConnection conn = s.GetRequiredService<NpgsqlConnection>();
                conn.Open();
                return conn.BeginTransaction();
            });

            return services;
        }
    }
}
