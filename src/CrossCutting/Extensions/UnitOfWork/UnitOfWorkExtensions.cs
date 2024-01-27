﻿using Data.Persistence.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Data.SqlClient;

namespace CrossCutting.Extensions.UnitOfWork
{
    public static class UnitOfWorkExtensions
    {
        public static IServiceCollection AddUnitOfWork(this IServiceCollection services, string connectionString)
        {
            services.AddScoped<IUnitOfWork, Data.Persistence.UnitOfWork.UnitOfWork>();
            services.AddScoped((s) => new SqlConnection(connectionString));
            services.AddScoped<IDbTransaction>(s =>
            {
                SqlConnection conn = s.GetRequiredService<SqlConnection>();
                conn.Open();
                return conn.BeginTransaction();
            });

            return services;
        }
    }
}
