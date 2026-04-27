using Microsoft.Extensions.DependencyInjection;

namespace STO.Wiki;

public static class DependencyInjection
{
    /// <summary>
    /// Registers all STO Wiki sync services and the named HttpClient.
    /// </summary>
    public static IServiceCollection AddWikiServices(this IServiceCollection services)
    {
        services.AddHttpClient<WikiApiClient>(client =>
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("STO-WikiSync/1.0 (Games-STO; .NET)");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddSingleton<WikiParserService>();
        services.AddScoped<WikiSyncService>();

        return services;
    }
}
