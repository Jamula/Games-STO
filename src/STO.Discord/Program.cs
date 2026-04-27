using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using STO.Data;
using STO.Data.Context;
using STO.Discord.Commands;

var builder = Host.CreateApplicationBuilder(args);

var token = builder.Configuration["Discord:Token"]
    ?? throw new InvalidOperationException("Discord:Token is required in appsettings.json or environment.");

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Data Source=sto.db";

builder.Services.AddStoData(connectionString);

var discordBuilder = DiscordClientBuilder.CreateDefault(token, DiscordIntents.Guilds | DiscordIntents.GuildMessages);

discordBuilder.ConfigureServices(services =>
{
    services.AddStoData(connectionString);
});

discordBuilder.UseCommands((provider, ext) =>
{
    ext.AddCommands(typeof(AccountCommands));
    ext.AddCommands(typeof(CharacterCommands));
    ext.AddCommands(typeof(BuildCommands));
    ext.AddCommands(typeof(InventoryCommands));
    ext.AddCommands(typeof(ReputationCommands));
    ext.AddCommands(typeof(ValuableCommands));
    ext.AddProcessor<SlashCommandProcessor>();
}, new CommandsConfiguration
{
    RegisterDefaultCommandProcessors = false
});

var client = discordBuilder.Build();

// Seed database on startup
using (var scope = client.ServiceProvider.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StoDbContext>();
    await db.Database.EnsureCreatedAsync();
    await SeedData.SeedDemoDataAsync(db);
}

await client.ConnectAsync();
await Task.Delay(-1);
