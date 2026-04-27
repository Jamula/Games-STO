using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using STO.Core.Enums;
using STO.Core.Models;
using STO.Core.Services;
using STO.Data;

namespace STO.Discord.Commands;

[Command("character")]
[Description("Manage STO characters")]
public class CharacterCommands
{
    private readonly ICharacterService _characterService;
    private readonly IAccountService _accountService;

    public CharacterCommands(ICharacterService characterService, IAccountService accountService)
    {
        _characterService = characterService;
        _accountService = accountService;
    }

    [Command("list")]
    [Description("List all characters")]
    public async ValueTask ListAsync(SlashCommandContext ctx)
    {
        var characters = await _characterService.GetAllAsync();

        if (characters.Count == 0)
        {
            await ctx.RespondAsync("No characters found. Use `/character add` to create one.");
            return;
        }

        var embed = new DiscordEmbedBuilder()
            .WithTitle("👤 STO Characters")
            .WithColor(new DiscordColor("#5865F2"));

        foreach (var c in characters.Take(25))
        {
            embed.AddField(
                c.Name,
                $"**Career:** {c.Career} | **Faction:** {c.Faction}\n**Level:** {c.Level} | **Ship:** {c.ActiveShip ?? "None"}",
                inline: true);
        }

        if (characters.Count > 25)
            embed.WithFooter($"Showing 25 of {characters.Count} characters");

        await ctx.RespondAsync(embed.Build());
    }

    [Command("info")]
    [Description("Show character details")]
    public async ValueTask InfoAsync(
        SlashCommandContext ctx,
        [Parameter("id")][Description("Character ID")] long id)
    {
        var character = await _characterService.GetWithFullDetailsAsync((int)id);

        if (character is null)
        {
            await ctx.RespondAsync($"❌ Character {id} not found.");
            return;
        }

        var embed = new DiscordEmbedBuilder()
            .WithTitle($"👤 {character.Name}")
            .WithColor(GetFactionColor(character.Faction))
            .AddField("Career", character.Career.ToString(), true)
            .AddField("Faction", character.Faction.ToString(), true)
            .AddField("Level", character.Level.ToString(), true)
            .AddField("Active Ship", character.ActiveShip ?? "None", true);

        if (character.Account != null)
            embed.AddField("Account", character.Account.Gamertag, true);

        if (character.Builds?.Count > 0)
        {
            var buildList = string.Join("\n", character.Builds.Select(b =>
                $"• **{b.Name}** ({b.Type}){(b.ShipName != null ? $" — {b.ShipName}" : "")}"));
            embed.AddField($"Builds ({character.Builds.Count})", buildList);
        }

        if (character.Notes != null)
            embed.AddField("Notes", character.Notes);

        await ctx.RespondAsync(embed.Build());
    }

    [Command("add")]
    [Description("Create a new character")]
    public async ValueTask AddAsync(
        SlashCommandContext ctx,
        [Parameter("name")][Description("Character name")] string name,
        [Parameter("account_id")][Description("Account ID")] long accountId,
        [Parameter("career")][Description("Career: Tactical, Engineering, or Science")] string career,
        [Parameter("faction")][Description("Faction: Federation, KlingonDefenseForce, RomulanRepublic, etc.")] string faction,
        [Parameter("level")][Description("Character level")] long level = 65,
        [Parameter("ship")][Description("Active ship name")] string? ship = null)
    {
        if (!Enum.TryParse<Career>(career, true, out var careerEnum))
        {
            await ctx.RespondAsync($"❌ Invalid career `{career}`. Use: {string.Join(", ", Enum.GetNames<Career>())}");
            return;
        }

        if (!Enum.TryParse<Faction>(faction, true, out var factionEnum))
        {
            await ctx.RespondAsync($"❌ Invalid faction `{faction}`. Use: {string.Join(", ", Enum.GetNames<Faction>())}");
            return;
        }

        var character = await _characterService.CreateAsync(new Character
        {
            Name = name,
            AccountId = (int)accountId,
            Career = careerEnum,
            Faction = factionEnum,
            Level = (int)level,
            ActiveShip = ship
        });

        SeedData.InitializeReputations(character);

        await ctx.RespondAsync($"✅ Character **{character.Name}** created (ID: {character.Id}).");
    }

    [Command("delete")]
    [Description("Delete a character")]
    public async ValueTask DeleteAsync(
        SlashCommandContext ctx,
        [Parameter("id")][Description("Character ID to delete")] long id)
    {
        var deleted = await _characterService.DeleteAsync((int)id);

        if (deleted)
            await ctx.RespondAsync($"✅ Character {id} deleted.");
        else
            await ctx.RespondAsync($"❌ Character {id} not found.");
    }

    private static DiscordColor GetFactionColor(Faction faction) => faction switch
    {
        Faction.Federation => new DiscordColor("#3498DB"),
        Faction.KlingonDefenseForce => new DiscordColor("#E74C3C"),
        Faction.RomulanRepublic => new DiscordColor("#2ECC71"),
        Faction.JemHadar => new DiscordColor("#9B59B6"),
        Faction.Dominion => new DiscordColor("#9B59B6"),
        _ => new DiscordColor("#95A5A6")
    };
}
