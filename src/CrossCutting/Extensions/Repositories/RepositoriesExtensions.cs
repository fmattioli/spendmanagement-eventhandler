using Data.Persistence.Repositories;
using Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CrossCutting.Extensions.Repositories
{
    public static class RepositoriesExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IReceiptRepository, ReceiptRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ISpendManagementEventRepository, SpendManagementEventRepository>();
            return services;
        }
    }
}
