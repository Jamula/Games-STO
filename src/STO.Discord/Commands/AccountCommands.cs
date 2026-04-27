using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using STO.Core.Models;
using STO.Core.Services;

namespace STO.Discord.Commands;

[Command("account")]
[Description("Manage STO accounts")]
public class AccountCommands
{
    private readonly IAccountService _accountService;

    public AccountCommands(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [Command("list")]
    [Description("List all accounts")]
    public async ValueTask ListAsync(SlashCommandContext ctx)
    {
        var accounts = await _accountService.GetAllAsync();

        if (accounts.Count == 0)
        {
            await ctx.RespondAsync("No accounts found. Use `/account add` to create one.");
            return;
        }

        var embed = new DiscordEmbedBuilder()
            .WithTitle("📋 STO Accounts")
            .WithColor(new DiscordColor("#5865F2"));

        foreach (var account in accounts)
        {
            var charCount = account.Characters?.Count ?? 0;
            embed.AddField(
                $"{account.Nickname ?? account.Gamertag}",
                $"**Gamertag:** {account.Gamertag}\n**Characters:** {charCount}\n{(account.Notes != null ? $"*{account.Notes}*" : "")}",
                inline: true);
        }

        await ctx.RespondAsync(embed.Build());
    }

    [Command("info")]
    [Description("Show account details")]
    public async ValueTask InfoAsync(
        SlashCommandContext ctx,
        [Parameter("gamertag")][Description("The gamertag to look up")] string gamertag)
    {
        var account = await _accountService.GetByGamertagAsync(gamertag);

        if (account is null)
        {
            await ctx.RespondAsync($"❌ Account `{gamertag}` not found.");
            return;
        }

        var embed = new DiscordEmbedBuilder()
            .WithTitle($"🎮 {account.Nickname ?? account.Gamertag}")
            .WithColor(new DiscordColor("#57F287"))
            .AddField("Gamertag", account.Gamertag, true)
            .AddField("Created", account.CreatedAt.ToString("yyyy-MM-dd"), true);

        if (account.Notes != null)
            embed.AddField("Notes", account.Notes);

        if (account.Characters?.Count > 0)
        {
            var charList = string.Join("\n", account.Characters.Select(c =>
                $"• **{c.Name}** — {c.Career} {c.Faction} (Lv {c.Level})"));
            embed.AddField($"Characters ({account.Characters.Count})", charList);
        }

        await ctx.RespondAsync(embed.Build());
    }

    [Command("add")]
    [Description("Add a new account")]
    public async ValueTask AddAsync(
        SlashCommandContext ctx,
        [Parameter("gamertag")][Description("Xbox gamertag")] string gamertag,
        [Parameter("nickname")][Description("Friendly nickname")] string? nickname = null,
        [Parameter("notes")][Description("Optional notes")] string? notes = null)
    {
        var existing = await _accountService.GetByGamertagAsync(gamertag);
        if (existing != null)
        {
            await ctx.RespondAsync($"❌ Account `{gamertag}` already exists.");
            return;
        }

        var account = await _accountService.CreateAsync(new Account
        {
            Gamertag = gamertag,
            Nickname = nickname,
            Notes = notes
        });

        await ctx.RespondAsync($"✅ Account **{account.Nickname ?? account.Gamertag}** created (ID: {account.Id}).");
    }

    [Command("delete")]
    [Description("Delete an account")]
    public async ValueTask DeleteAsync(
        SlashCommandContext ctx,
        [Parameter("id")][Description("Account ID to delete")] long id)
    {
        var deleted = await _accountService.DeleteAsync((int)id);

        if (deleted)
            await ctx.RespondAsync($"✅ Account {id} deleted.");
        else
            await ctx.RespondAsync($"❌ Account {id} not found.");
    }
}
