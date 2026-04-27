using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using STO.Core.Enums;
using STO.Core.Models;
using STO.Core.Services;

namespace STO.Discord.Commands;

[Command("valuable")]
[Description("Track valuable items (keys, lobi, promo tokens, etc.)")]
public class ValuableCommands
{
    private readonly IValuableItemService _valuableService;

    public ValuableCommands(IValuableItemService valuableService)
    {
        _valuableService = valuableService;
    }

    [Command("list")]
    [Description("List all valuable items")]
    public async ValueTask ListAsync(SlashCommandContext ctx)
    {
        var items = await _valuableService.GetAllAsync();

        if (items.Count == 0)
        {
            await ctx.RespondAsync("No valuable items tracked. Use `/valuable add` to start tracking.");
            return;
        }

        var embed = new DiscordEmbedBuilder()
            .WithTitle("💎 Valuable Items")
            .WithColor(new DiscordColor("#9B59B6"));

        var grouped = items.GroupBy(i => i.ItemType);
        foreach (var group in grouped)
        {
            var list = string.Join("\n", group.Take(10).Select(i =>
                $"• **{i.Name}** x{i.Quantity}{(i.Character != null ? $" ({i.Character.Name})" : i.Account != null ? $" ({i.Account.Gamertag})" : "")}"));
            embed.AddField($"{group.Key} ({group.Sum(i => i.Quantity)})", list);
        }

        await ctx.RespondAsync(embed.Build());
    }

    [Command("summary")]
    [Description("Show valuable item summary with totals")]
    public async ValueTask SummaryAsync(SlashCommandContext ctx)
    {
        var summary = await _valuableService.GetSummaryAsync();

        if (summary.Count == 0)
        {
            await ctx.RespondAsync("No valuable items tracked.");
            return;
        }

        var embed = new DiscordEmbedBuilder()
            .WithTitle("📊 Valuable Items Summary")
            .WithColor(new DiscordColor("#9B59B6"));

        foreach (var (name, count) in summary.OrderByDescending(kv => kv.Value))
        {
            embed.AddField(name, count.ToString(), inline: true);
        }

        await ctx.RespondAsync(embed.Build());
    }

    [Command("add")]
    [Description("Add a valuable item to track")]
    public async ValueTask AddAsync(
        SlashCommandContext ctx,
        [Parameter("name")][Description("Item name")] string name,
        [Parameter("account_id")][Description("Account ID")] long accountId,
        [Parameter("type")][Description("Item type")] string type,
        [Parameter("quantity")][Description("Quantity")] long quantity = 1,
        [Parameter("character_id")][Description("Character ID (optional)")] long? characterId = null,
        [Parameter("notes")][Description("Optional notes")] string? notes = null)
    {
        if (!Enum.TryParse<ItemType>(type, true, out var itemType))
        {
            await ctx.RespondAsync($"❌ Invalid type. Use: {string.Join(", ", Enum.GetNames<ItemType>().Take(10))}...");
            return;
        }

        var item = await _valuableService.AddAsync(new ValuableItem
        {
            Name = name,
            AccountId = (int)accountId,
            ItemType = itemType,
            Quantity = (int)quantity,
            CharacterId = characterId.HasValue ? (int)characterId.Value : null,
            Notes = notes
        });

        await ctx.RespondAsync($"✅ Tracking **{item.Name}** x{item.Quantity} (ID: {item.Id}).");
    }

    [Command("search")]
    [Description("Search valuable items")]
    public async ValueTask SearchAsync(
        SlashCommandContext ctx,
        [Parameter("query")][Description("Search term")] string query)
    {
        var items = await _valuableService.SearchAsync(query);

        if (items.Count == 0)
        {
            await ctx.RespondAsync($"No valuable items matching `{query}`.");
            return;
        }

        var embed = new DiscordEmbedBuilder()
            .WithTitle($"🔍 Valuable Items — \"{query}\"")
            .WithColor(new DiscordColor("#9B59B6"));

        foreach (var item in items.Take(15))
        {
            embed.AddField(
                $"{item.Name} x{item.Quantity}",
                $"**Type:** {item.ItemType} | **Location:** {item.Location}\n{(item.Notes != null ? $"*{item.Notes}*" : "")}",
                inline: true);
        }

        await ctx.RespondAsync(embed.Build());
    }

    [Command("remove")]
    [Description("Remove a valuable item")]
    public async ValueTask RemoveAsync(
        SlashCommandContext ctx,
        [Parameter("id")][Description("Valuable item ID")] long id)
    {
        var removed = await _valuableService.RemoveAsync((int)id);

        if (removed)
            await ctx.RespondAsync($"✅ Valuable item {id} removed.");
        else
            await ctx.RespondAsync($"❌ Valuable item {id} not found.");
    }
}
