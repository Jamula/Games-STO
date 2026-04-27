using System.Text.RegularExpressions;
using STO.Core.Enums;
using STO.Core.Models;

namespace STO.Wiki;

/// <summary>
/// Parses MediaWiki wikitext content into STO domain models.
/// </summary>
public partial class WikiParserService
{
    private const string WikiBaseUrl = "https://stowiki.net/wiki/";

    /// <summary>
    /// Parse an Item from wikitext. Extracts data from {{itemheader}} and {{infobox}} templates.
    /// </summary>
    public Item? ParseItemFromWikitext(string title, string wikitext)
    {
        if (string.IsNullOrWhiteSpace(wikitext)) return null;

        var item = new Item
        {
            Name = CleanTitle(title),
            WikiUrl = WikiBaseUrl + Uri.EscapeDataString(title.Replace(' ', '_')),
            IsFromWiki = true,
            LastWikiSync = DateTime.UtcNow
        };

        // Try to extract rarity from {{itemheader|...|rarity}} or {{infobox ... |rarity = ...}}
        var rarity = ExtractTemplateParam(wikitext, "rarity")
                  ?? ExtractItemHeaderRarity(wikitext);
        if (rarity is not null)
            item.DefaultRarity = ParseRarity(rarity);

        // Try to extract type from infobox or itemheader
        var typeStr = ExtractTemplateParam(wikitext, "type");
        if (typeStr is not null)
            item.Type = ParseItemType(typeStr);

        // Extract description — first non-template, non-heading paragraph
        item.Description = ExtractDescription(wikitext);

        // Extract set name
        item.SetName = ExtractTemplateParam(wikitext, "set");

        // Extract source/obtained from
        item.Source = ExtractTemplateParam(wikitext, "obtained")
                   ?? ExtractTemplateParam(wikitext, "source");

        return item;
    }

    /// <summary>
    /// Parse a Trait from wikitext.
    /// </summary>
    public Trait? ParseTraitFromWikitext(string title, string wikitext)
    {
        if (string.IsNullOrWhiteSpace(wikitext)) return null;

        var trait = new Trait
        {
            Name = CleanTitle(title),
            WikiUrl = WikiBaseUrl + Uri.EscapeDataString(title.Replace(' ', '_')),
            IsFromWiki = true
        };

        // Extract description
        trait.Description = ExtractDescription(wikitext);

        // Try to determine trait type from template or categories
        var typeStr = ExtractTemplateParam(wikitext, "type");
        if (typeStr is not null)
            trait.Type = ParseTraitType(typeStr);
        else
            trait.Type = InferTraitTypeFromContent(wikitext);

        trait.Source = ExtractTemplateParam(wikitext, "obtained")
                    ?? ExtractTemplateParam(wikitext, "source");

        return trait;
    }

    /// <summary>
    /// Maps a wiki category name to our ItemType enum.
    /// </summary>
    public ItemType MapCategoryToItemType(string category) => category.ToLowerInvariant() switch
    {
        "space_weapons" or "space weapons" => ItemType.SpaceWeapon,
        "ground_weapons" or "ground weapons" => ItemType.GroundWeapon,
        "shields" or "space_shields" or "space shields" => ItemType.SpaceShield,
        "deflectors" or "deflector_dishes" or "deflector dishes" => ItemType.SpaceDeflector,
        "impulse_engines" or "impulse engines" => ItemType.SpaceEngine,
        "warp_cores" or "warp cores" or "singularity_cores" or "singularity cores" => ItemType.SpaceWarpCore,
        "engineering_consoles" or "engineering consoles" => ItemType.SpaceConsoleEngineering,
        "science_consoles" or "science consoles" => ItemType.SpaceConsoleScience,
        "tactical_consoles" or "tactical consoles" => ItemType.SpaceConsoleTactical,
        "universal_consoles" or "universal consoles" => ItemType.SpaceConsoleUniversal,
        "space_devices" or "space devices" => ItemType.SpaceDevice,
        "hangar_pets" or "hangar pets" => ItemType.SpaceHangarPet,
        "ground_armor" or "ground armor" or "body_armor" or "body armor" => ItemType.GroundArmor,
        "ground_shields" or "ground shields" or "personal_shields" or "personal shields" => ItemType.GroundShield,
        "ground_devices" or "ground devices" => ItemType.GroundDevice,
        "kits" => ItemType.GroundKit,
        "kit_modules" or "kit modules" => ItemType.GroundKitModule,
        "ships" or "playable_starships" or "playable starships" => ItemType.Ship,
        "duty_officers" or "duty officers" => ItemType.DutyOfficer,
        _ => ItemType.Other
    };

    /// <summary>
    /// Maps a wiki category name to our TraitType enum.
    /// </summary>
    public TraitType MapCategoryToTraitType(string category) => category.ToLowerInvariant() switch
    {
        "personal_ground_traits" or "ground_traits" or "ground traits" => TraitType.PersonalGround,
        "personal_space_traits" or "space_traits" or "space traits" => TraitType.PersonalSpace,
        "starship_traits" or "starship traits" => TraitType.Starship,
        "active_reputation_traits" or "reputation_traits" or "reputation traits" => TraitType.ActiveReputation,
        _ => TraitType.PersonalSpace
    };

    private static string CleanTitle(string title)
    {
        // Remove namespace prefix if present (e.g., "Trait: Something")
        var colonIdx = title.IndexOf(':');
        if (colonIdx >= 0 && colonIdx < 20)
            return title[(colonIdx + 1)..].Trim();
        return title.Trim();
    }

    private static string? ExtractTemplateParam(string wikitext, string paramName)
    {
        // Match | paramName = value (wiki template parameter syntax)
        var pattern = $@"\|\s*{Regex.Escape(paramName)}\s*=\s*([^\n|}}]+)";
        var match = Regex.Match(wikitext, pattern, RegexOptions.IgnoreCase);
        if (match.Success)
        {
            var value = match.Groups[1].Value.Trim();
            // Strip wikitext markup
            value = StripWikiMarkup(value);
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
        return null;
    }

    private static string? ExtractItemHeaderRarity(string wikitext)
    {
        // {{itemheader|Item Name|rarity}} — rarity is typically the second positional param
        var match = Regex.Match(wikitext, @"\{\{itemheader\|[^|}}]*\|([^|}}]+)", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private static string? ExtractDescription(string wikitext)
    {
        // Take the first meaningful paragraph: skip templates, headings, categories
        var lines = wikitext.Split('\n');
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) continue;
            if (trimmed.StartsWith("{{") || trimmed.StartsWith("}}")) continue;
            if (trimmed.StartsWith("[[Category:")) continue;
            if (trimmed.StartsWith("==")) continue;
            if (trimmed.StartsWith("|") || trimmed.StartsWith("!")) continue;
            if (trimmed.StartsWith("{|") || trimmed.StartsWith("|}")) continue;

            var cleaned = StripWikiMarkup(trimmed);
            if (cleaned.Length > 10)
                return cleaned.Length > 2000 ? cleaned[..2000] : cleaned;
        }
        return null;
    }

    private static string StripWikiMarkup(string text)
    {
        // Remove [[link|display]] → display, [[link]] → link
        text = Regex.Replace(text, @"\[\[[^\]]*\|([^\]]+)\]\]", "$1");
        text = Regex.Replace(text, @"\[\[([^\]]+)\]\]", "$1");
        // Remove '' and ''' (bold/italic)
        text = text.Replace("'''", "").Replace("''", "");
        // Remove <ref>...</ref> and <br/> tags
        text = Regex.Replace(text, @"<ref[^>]*>.*?</ref>", "", RegexOptions.Singleline);
        text = Regex.Replace(text, @"<[^>]+/?>", "");
        return text.Trim();
    }

    private static ItemRarity? ParseRarity(string rarity) => rarity.Trim().ToLowerInvariant() switch
    {
        "common" => ItemRarity.Common,
        "uncommon" => ItemRarity.Uncommon,
        "rare" => ItemRarity.Rare,
        "very rare" or "veryrare" => ItemRarity.VeryRare,
        "ultra rare" or "ultrarare" => ItemRarity.UltraRare,
        "epic" => ItemRarity.Epic,
        _ => null
    };

    private static ItemType ParseItemType(string type) => type.Trim().ToLowerInvariant() switch
    {
        "space weapon" or "ship weapon" => ItemType.SpaceWeapon,
        "ground weapon" => ItemType.GroundWeapon,
        "shield" or "space shield" or "shield array" => ItemType.SpaceShield,
        "deflector" or "deflector dish" => ItemType.SpaceDeflector,
        "impulse engine" or "impulse engines" => ItemType.SpaceEngine,
        "warp core" or "singularity core" or "warp engine" => ItemType.SpaceWarpCore,
        "engineering console" => ItemType.SpaceConsoleEngineering,
        "science console" => ItemType.SpaceConsoleScience,
        "tactical console" => ItemType.SpaceConsoleTactical,
        "universal console" => ItemType.SpaceConsoleUniversal,
        "device" or "space device" => ItemType.SpaceDevice,
        "hangar pet" or "hangar bay" => ItemType.SpaceHangarPet,
        "body armor" or "ground armor" => ItemType.GroundArmor,
        "personal shield" or "ground shield" => ItemType.GroundShield,
        "ground device" => ItemType.GroundDevice,
        "kit" => ItemType.GroundKit,
        "kit module" => ItemType.GroundKitModule,
        "ship" or "starship" => ItemType.Ship,
        _ => ItemType.Other
    };

    private static TraitType ParseTraitType(string type) => type.Trim().ToLowerInvariant() switch
    {
        "ground" or "personal ground" => TraitType.PersonalGround,
        "space" or "personal space" or "personal" => TraitType.PersonalSpace,
        "starship" => TraitType.Starship,
        "active reputation" or "reputation" => TraitType.ActiveReputation,
        _ => TraitType.PersonalSpace
    };

    private static TraitType InferTraitTypeFromContent(string wikitext)
    {
        var lower = wikitext.ToLowerInvariant();
        if (lower.Contains("[[category:starship traits]]") || lower.Contains("starship trait"))
            return TraitType.Starship;
        if (lower.Contains("[[category:active reputation traits]]") || lower.Contains("reputation trait"))
            return TraitType.ActiveReputation;
        if (lower.Contains("ground trait") || lower.Contains("[[category:ground traits]]"))
            return TraitType.PersonalGround;
        return TraitType.PersonalSpace;
    }
}
