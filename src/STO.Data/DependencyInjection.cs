using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using STO.Core.Services;
using STO.Data.Context;
using STO.Data.Services;

namespace STO.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddStoData(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<StoDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ICharacterService, CharacterService>();
        services.AddScoped<IBuildService, BuildService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IReputationService, ReputationService>();
        services.AddScoped<IValuableItemService, ValuableItemService>();
        services.AddScoped<ICsvImportService, CsvImportService>();

        return services;
    }
}
