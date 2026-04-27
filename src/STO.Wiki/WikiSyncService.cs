using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STO.Core.Enums;
using STO.Core.Models;
using STO.Data.Context;

namespace STO.Wiki;

/// <summary>
/// Orchestrates syncing items and traits from the STO Wiki into the local database.
/// </summary>
public class WikiSyncService
{
    private static readonly Dictionary<string, ItemType> ItemCategories = new()
    {
        ["Space_weapons"] = ItemType.SpaceWeapon,
        ["Ground_weapons"] = ItemType.GroundWeapon,
        ["Shields"] = ItemType.SpaceShield,
        ["Deflectors"] = ItemType.SpaceDeflector,
        ["Impulse_engines"] = ItemType.SpaceEngine,
        ["Warp_cores"] = ItemType.SpaceWarpCore,
        ["Engineering_consoles"] = ItemType.SpaceConsoleEngineering,
        ["Science_consoles"] = ItemType.SpaceConsoleScience,
        ["Tactical_consoles"] = ItemType.SpaceConsoleTactical,
        ["Universal_consoles"] = ItemType.SpaceConsoleUniversal,
        ["Hangar_pets"] = ItemType.SpaceHangarPet,
        ["Kit_modules"] = ItemType.GroundKitModule,
    };

    private static readonly Dictionary<string, TraitType> TraitCategories = new()
    {
        ["Personal_ground_traits"] = TraitType.PersonalGround,
        ["Personal_space_traits"] = TraitType.PersonalSpace,
        ["Starship_traits"] = TraitType.Starship,
        ["Active_reputation_traits"] = TraitType.ActiveReputation,
    };

    private readonly WikiApiClient _api;
    private readonly WikiParserService _parser;
    private readonly StoDbContext _db;
    private readonly ILogger<WikiSyncService> _logger;

    public WikiSyncService(
        WikiApiClient api,
        WikiParserService parser,
        StoDbContext db,
        ILogger<WikiSyncService> logger)
    {
        _api = api;
        _parser = parser;
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Sync all items and traits from the wiki.
    /// </summary>
    public async Task SyncAllAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Starting full wiki sync");
        await SyncItemsAsync(ct);
        await SyncTraitsAsync(ct);
        _logger.LogInformation("Full wiki sync completed");
    }

    /// <summary>
    /// Sync items from key wiki categories into the database.
    /// </summary>
    public async Task SyncItemsAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Starting item sync from wiki");
        int created = 0, updated = 0;

        foreach (var (category, itemType) in ItemCategories)
        {
            ct.ThrowIfCancellationRequested();
            _logger.LogInformation("Syncing category: {Category}", category);

            List<CategoryMember> members;
            try
            {
                members = await _api.GetCategoryMembersAsync(category, ct: ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch category {Category}", category);
                continue;
            }

            _logger.LogInformation("Found {Count} pages in {Category}", members.Count, category);

            foreach (var member in members)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    var wikitext = await _api.GetPageContentAsync(member.Title, ct);
                    if (wikitext is null) continue;

                    var parsed = _parser.ParseItemFromWikitext(member.Title, wikitext);
                    if (parsed is null) continue;

                    // Override type from category mapping
                    parsed.Type = itemType;

                    var (wasCreated, _) = await UpsertItemAsync(parsed, ct);
                    if (wasCreated) created++; else updated++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to sync item: {Title}", member.Title);
                }
            }
        }

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Item sync complete: {Created} created, {Updated} updated", created, updated);
    }

    /// <summary>
    /// Sync traits from wiki categories into the database.
    /// </summary>
    public async Task SyncTraitsAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Starting trait sync from wiki");
        int created = 0, updated = 0;

        foreach (var (category, traitType) in TraitCategories)
        {
            ct.ThrowIfCancellationRequested();
            _logger.LogInformation("Syncing trait category: {Category}", category);

            List<CategoryMember> members;
            try
            {
                members = await _api.GetCategoryMembersAsync(category, ct: ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch trait category {Category}", category);
                continue;
            }

            _logger.LogInformation("Found {Count} pages in {Category}", members.Count, category);

            foreach (var member in members)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    var wikitext = await _api.GetPageContentAsync(member.Title, ct);
                    if (wikitext is null) continue;

                    var parsed = _parser.ParseTraitFromWikitext(member.Title, wikitext);
                    if (parsed is null) continue;

                    // Override type from category mapping
                    parsed.Type = traitType;

                    var (wasCreated, _) = await UpsertTraitAsync(parsed, ct);
                    if (wasCreated) created++; else updated++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to sync trait: {Title}", member.Title);
                }
            }
        }

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Trait sync complete: {Created} created, {Updated} updated", created, updated);
    }

    private async Task<(bool Created, Item Item)> UpsertItemAsync(Item parsed, CancellationToken ct)
    {
        var existing = await _db.Items
            .FirstOrDefaultAsync(i => i.Name == parsed.Name, ct);

        if (existing is null)
        {
            _db.Items.Add(parsed);
            return (true, parsed);
        }

        // Only update wiki-sourced fields if this is newer
        existing.Type = parsed.Type;
        existing.Description = parsed.Description ?? existing.Description;
        existing.DefaultRarity = parsed.DefaultRarity ?? existing.DefaultRarity;
        existing.Source = parsed.Source ?? existing.Source;
        existing.SetName = parsed.SetName ?? existing.SetName;
        existing.WikiUrl = parsed.WikiUrl;
        existing.IsFromWiki = true;
        existing.LastWikiSync = DateTime.UtcNow;

        return (false, existing);
    }

    private async Task<(bool Created, Trait Trait)> UpsertTraitAsync(Trait parsed, CancellationToken ct)
    {
        var existing = await _db.Traits
            .FirstOrDefaultAsync(t => t.Name == parsed.Name, ct);

        if (existing is null)
        {
            _db.Traits.Add(parsed);
            return (true, parsed);
        }

        existing.Type = parsed.Type;
        existing.Description = parsed.Description ?? existing.Description;
        existing.Source = parsed.Source ?? existing.Source;
        existing.WikiUrl = parsed.WikiUrl;
        existing.IsFromWiki = true;

        return (false, existing);
    }
}
