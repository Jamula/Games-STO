using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using STO.Core.Enums;
using STO.Core.Models;
using STO.Core.Services;

namespace STO.Discord.Commands;

[Command("build")]
[Description("Manage character builds")]
public class BuildCommands
{
    private readonly IBuildService _buildService;
    private readonly ICharacterService _characterService;

    public BuildCommands(IBuildService buildService, ICharacterService characterService)
    {
        _buildService = buildService;
        _characterService = characterService;
    }

    [Command("list")]
    [Description("List builds for a character")]
    public async ValueTask ListAsync(
        SlashCommandContext ctx,
        [Parameter("character_id")][Description("Character ID")] long characterId)
    {
        var builds = await _buildService.GetByCharacterIdAsync((int)characterId);

        if (builds.Count == 0)
        {
            await ctx.RespondAsync("No builds found for this character.");
            return;
        }

        var character = await _characterService.GetByIdAsync((int)characterId);

        var embed = new DiscordEmbedBuilder()
            .WithTitle($"🔧 Builds for {character?.Name ?? $"Character {characterId}"}")
            .WithColor(new DiscordColor("#F1C40F"));

        foreach (var build in builds)
        {
            var slotCount = build.Slots?.Count ?? 0;
            embed.AddField(
                $"{(build.IsActive ? "⭐ " : "")}{build.Name}",
                $"**Type:** {build.Type}\n**Ship:** {build.ShipName ?? "N/A"}\n**Slots:** {slotCount}\n{(build.Notes != null ? $"*{build.Notes}*" : "")}",
                inline: true);
        }

        await ctx.RespondAsync(embed.Build());
    }

    [Command("info")]
    [Description("Show build details with slots")]
    public async ValueTask InfoAsync(
        SlashCommandContext ctx,
        [Parameter("id")][Description("Build ID")] long id)
    {
        var build = await _buildService.GetByIdAsync((int)id);

        if (build is null)
        {
            await ctx.RespondAsync($"❌ Build {id} not found.");
            return;
        }

        var embed = new DiscordEmbedBuilder()
            .WithTitle($"🔧 {build.Name}")
            .WithColor(new DiscordColor("#F1C40F"))
            .AddField("Type", build.Type.ToString(), true)
            .AddField("Ship", build.ShipName ?? "N/A", true)
            .AddField("Active", build.IsActive ? "Yes ⭐" : "No", true);

        if (build.Notes != null)
            embed.AddField("Notes", build.Notes);

        if (build.Slots?.Count > 0)
        {
            var slotList = string.Join("\n", build.Slots.OrderBy(s => s.Position).Select(s =>
                $"`{s.Position}.` **{s.SlotName}** — {s.Item?.Name ?? "Empty"}{(s.Notes != null ? $" *({s.Notes})*" : "")}"));
            embed.AddField($"Slots ({build.Slots.Count})", slotList);
        }

        await ctx.RespondAsync(embed.Build());
    }

    [Command("add")]
    [Description("Create a new build")]
    public async ValueTask AddAsync(
        SlashCommandContext ctx,
        [Parameter("character_id")][Description("Character ID")] long characterId,
        [Parameter("name")][Description("Build name")] string name,
        [Parameter("type")][Description("Build type: Space or Ground")] string type,
        [Parameter("ship")][Description("Ship name")] string? ship = null,
        [Parameter("notes")][Description("Optional notes")] string? notes = null)
    {
        if (!Enum.TryParse<BuildType>(type, true, out var buildType))
        {
            await ctx.RespondAsync($"❌ Invalid type `{type}`. Use: {string.Join(", ", Enum.GetNames<BuildType>())}");
            return;
        }

        var build = await _buildService.CreateAsync(new Build
        {
            CharacterId = (int)characterId,
            Name = name,
            Type = buildType,
            ShipName = ship,
            Notes = notes
        });

        await ctx.RespondAsync($"✅ Build **{build.Name}** created (ID: {build.Id}).");
    }

    [Command("active")]
    [Description("Show all active builds")]
    public async ValueTask ActiveAsync(SlashCommandContext ctx)
    {
        var builds = await _buildService.GetActiveBuildsAsync();

        if (builds.Count == 0)
        {
            await ctx.RespondAsync("No active builds found.");
            return;
        }

        var embed = new DiscordEmbedBuilder()
            .WithTitle("⭐ Active Builds")
            .WithColor(new DiscordColor("#F1C40F"));

        foreach (var build in builds)
        {
            embed.AddField(
                build.Name,
                $"**Type:** {build.Type} | **Ship:** {build.ShipName ?? "N/A"}\n**Character:** {build.Character?.Name ?? "Unknown"}",
                inline: true);
        }

        await ctx.RespondAsync(embed.Build());
    }

    [Command("delete")]
    [Description("Delete a build")]
    public async ValueTask DeleteAsync(
        SlashCommandContext ctx,
        [Parameter("id")][Description("Build ID to delete")] long id)
    {
        var deleted = await _buildService.DeleteAsync((int)id);

        if (deleted)
            await ctx.RespondAsync($"✅ Build {id} deleted.");
        else
            await ctx.RespondAsync($"❌ Build {id} not found.");
    }
}
