using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using STO.Core.Services;

namespace STO.Discord.Commands;

[Command("reputation")]
[Description("View and manage reputation progress")]
public class ReputationCommands
{
    private readonly IReputationService _reputationService;
    private readonly ICharacterService _characterService;

    public ReputationCommands(IReputationService reputationService, ICharacterService characterService)
    {
        _reputationService = reputationService;
        _characterService = characterService;
    }

    [Command("show")]
    [Description("Show reputation progress for a character")]
    public async ValueTask ShowAsync(
        SlashCommandContext ctx,
        [Parameter("character_id")][Description("Character ID")] long characterId)
    {
        var reps = await _reputationService.GetByCharacterIdAsync((int)characterId);

        if (reps.Count == 0)
        {
            await ctx.RespondAsync("No reputation data found for this character.");
            return;
        }

        var character = await _characterService.GetByIdAsync((int)characterId);

        var embed = new DiscordEmbedBuilder()
            .WithTitle($"🏅 Reputation — {character?.Name ?? $"Character {characterId}"}")
            .WithColor(new DiscordColor("#1ABC9C"));

        foreach (var rep in reps)
        {
            var bar = GetProgressBar(rep.Tier, 6);
            embed.AddField(
                rep.Faction.ToString().Replace("_", " "),
                $"**Tier {rep.Tier}/6** {bar}\nProgress: {rep.Progress:N0} XP",
                inline: true);
        }

        await ctx.RespondAsync(embed.Build());
    }

    [Command("summary")]
    [Description("Show reputation summary for a character")]
    public async ValueTask SummaryAsync(
        SlashCommandContext ctx,
        [Parameter("character_id")][Description("Character ID")] long characterId)
    {
        var reps = await _reputationService.GetReputationSummaryAsync((int)characterId);
        var character = await _characterService.GetByIdAsync((int)characterId);

        var maxed = reps.Count(r => r.Tier >= 6);
        var total = reps.Count;

        var embed = new DiscordEmbedBuilder()
            .WithTitle($"📊 Reputation Summary — {character?.Name ?? $"Character {characterId}"}")
            .WithColor(new DiscordColor("#1ABC9C"))
            .AddField("Total Reputations", total.ToString(), true)
            .AddField("Maxed (T6)", maxed.ToString(), true)
            .AddField("Completion", $"{(total > 0 ? maxed * 100 / total : 0)}%", true);

        var incomplete = reps.Where(r => r.Tier < 6).OrderByDescending(r => r.Tier).Take(5);
        if (incomplete.Any())
        {
            var list = string.Join("\n", incomplete.Select(r =>
                $"• **{r.Faction.ToString().Replace("_", " ")}** — Tier {r.Tier}"));
            embed.AddField("Highest Incomplete", list);
        }

        await ctx.RespondAsync(embed.Build());
    }

    [Command("account")]
    [Description("Show reputation across all characters in an account")]
    public async ValueTask AccountAsync(
        SlashCommandContext ctx,
        [Parameter("account_id")][Description("Account ID")] long accountId)
    {
        var reps = await _reputationService.GetAllForAccountAsync((int)accountId);

        if (reps.Count == 0)
        {
            await ctx.RespondAsync("No reputation data found for this account.");
            return;
        }

        var embed = new DiscordEmbedBuilder()
            .WithTitle("🏅 Account Reputation Overview")
            .WithColor(new DiscordColor("#1ABC9C"));

        var grouped = reps.GroupBy(r => r.Character?.Name ?? "Unknown");
        foreach (var group in grouped.Take(10))
        {
            var maxed = group.Count(r => r.Tier >= 6);
            var total = group.Count();
            embed.AddField(group.Key, $"**{maxed}/{total}** maxed", inline: true);
        }

        await ctx.RespondAsync(embed.Build());
    }

    private static string GetProgressBar(int current, int max)
    {
        var filled = current;
        var empty = max - current;
        return new string('█', filled) + new string('░', empty);
    }
}
