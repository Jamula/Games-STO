using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using STO.Core.Enums;
using STO.Core.Models;
using STO.Core.Services;

namespace STO.Discord.Commands;

[Command("inventory")]
[Description("Manage character inventory")]
public class InventoryCommands
{
    private readonly IInventoryService _inventoryService;
    private readonly ICharacterService _characterService;

    public InventoryCommands(IInventoryService inventoryService, ICharacterService characterService)
    {
        _inventoryService = inventoryService;
        _characterService = characterService;
    }

    [Command("list")]
    [Description("List inventory for a character")]
    public async ValueTask ListAsync(
        SlashCommandContext ctx,
        [Parameter("character_id")][Description("Character ID")] long characterId)
    {
        var items = await _inventoryService.GetByCharacterIdAsync((int)characterId);

        if (items.Count == 0)
        {
            await ctx.RespondAsync("No inventory items found for this character.");
            return;
        }

        var character = await _characterService.GetByIdAsync((int)characterId);

        var embed = new DiscordEmbedBuilder()
            .WithTitle($"📦 Inventory for {character?.Name ?? $"Character {characterId}"}")
            .WithColor(new DiscordColor("#E67E22"));

        var grouped = items.GroupBy(i => i.Location);
        foreach (var group in grouped)
        {
            var itemList = string.Join("\n", group.Take(10).Select(i =>
                $"• **{i.Item?.Name ?? "Unknown"}** x{i.Quantity} ({i.Rarity})"));

            if (group.Count() > 10)
                itemList += $"\n*...and {group.Count() - 10} more*";

            embed.AddField($"📍 {group.Key} ({group.Count()})", itemList);
        }

        await ctx.RespondAsync(embed.Build());
    }

    [Command("search")]
    [Description("Search for items across all characters")]
    public async ValueTask SearchAsync(
        SlashCommandContext ctx,
        [Parameter("query")][Description("Search term")] string query)
    {
        var items = await _inventoryService.SearchAcrossAccountsAsync(query);

        if (items.Count == 0)
        {
            await ctx.RespondAsync($"No items matching `{query}` found.");
            return;
        }

        var embed = new DiscordEmbedBuilder()
            .WithTitle($"🔍 Search Results for \"{query}\"")
            .WithColor(new DiscordColor("#E67E22"));

        foreach (var item in items.Take(15))
        {
            embed.AddField(
                item.Item?.Name ?? "Unknown",
                $"**Character:** {item.Character?.Name ?? "?"} | **Qty:** {item.Quantity}\n**Location:** {item.Location} | **Rarity:** {item.Rarity}",
                inline: true);
        }

        if (items.Count > 15)
            embed.WithFooter($"Showing 15 of {items.Count} results");

        await ctx.RespondAsync(embed.Build());
    }

    [Command("add")]
    [Description("Add an item to character inventory")]
    public async ValueTask AddAsync(
        SlashCommandContext ctx,
        [Parameter("character_id")][Description("Character ID")] long characterId,
        [Parameter("item_id")][Description("Item ID")] long itemId,
        [Parameter("quantity")][Description("Quantity")] long quantity = 1,
        [Parameter("location")][Description("Location: Inventory, Bank, AccountBank, Overflow, Equipped")] string location = "Inventory",
        [Parameter("rarity")][Description("Rarity: Common, Uncommon, Rare, VeryRare, UltraRare, Epic")] string rarity = "VeryRare")
    {
        if (!Enum.TryParse<InventoryLocation>(location, true, out var loc))
        {
            await ctx.RespondAsync($"❌ Invalid location. Use: {string.Join(", ", Enum.GetNames<InventoryLocation>())}");
            return;
        }

        if (!Enum.TryParse<ItemRarity>(rarity, true, out var rar))
        {
            await ctx.RespondAsync($"❌ Invalid rarity. Use: {string.Join(", ", Enum.GetNames<ItemRarity>())}");
            return;
        }

        var item = await _inventoryService.AddItemAsync(new InventoryItem
        {
            CharacterId = (int)characterId,
            ItemId = (int)itemId,
            Quantity = (int)quantity,
            Location = loc,
            Rarity = rar
        });

        await ctx.RespondAsync($"✅ Added item to inventory (ID: {item.Id}).");
    }

    [Command("remove")]
    [Description("Remove an inventory item")]
    public async ValueTask RemoveAsync(
        SlashCommandContext ctx,
        [Parameter("id")][Description("Inventory item ID")] long id)
    {
        var removed = await _inventoryService.RemoveItemAsync((int)id);

        if (removed)
            await ctx.RespondAsync($"✅ Inventory item {id} removed.");
        else
            await ctx.RespondAsync($"❌ Inventory item {id} not found.");
    }
}
