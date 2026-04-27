using Microsoft.EntityFrameworkCore;
using STO.Core.Enums;
using STO.Core.Models;
using STO.Core.Services;
using STO.Data.Context;

namespace STO.Data.Services;

public class CsvImportService(StoDbContext db) : ICsvImportService
{
    public async Task<ImportResult> ImportCharactersAsync(Stream csvStream, int accountId)
    {
        var result = new ImportResult();
        var (headers, rows) = CsvHelper.Parse(csvStream);

        if (headers.Length == 0)
        {
            result.Errors.Add("CSV file is empty or has no header row.");
            return result;
        }

        var colMap = BuildColumnMap(headers);
        if (!RequireColumns(colMap, result, "Name", "Career", "Faction"))
            return result;

        var existingNames = await db.Characters
            .Where(c => c.AccountId == accountId)
            .Select(c => c.Name)
            .ToListAsync();
        var existingSet = new HashSet<string>(existingNames, StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < rows.Count; i++)
        {
            int rowNum = i + 2; // 1-based, accounting for header
            var fields = rows[i];
            try
            {
                string name = GetField(fields, colMap, "Name");
                if (string.IsNullOrWhiteSpace(name))
                {
                    result.Errors.Add($"Row {rowNum}: Name is required.");
                    continue;
                }

                if (existingSet.Contains(name))
                {
                    result.Skipped++;
                    continue;
                }

                if (!TryParseEnum<Career>(GetField(fields, colMap, "Career"), out var career))
                {
                    result.Errors.Add($"Row {rowNum}: Invalid Career '{GetField(fields, colMap, "Career")}'.");
                    continue;
                }

                if (!TryParseEnum<Faction>(GetField(fields, colMap, "Faction"), out var faction))
                {
                    result.Errors.Add($"Row {rowNum}: Invalid Faction '{GetField(fields, colMap, "Faction")}'.");
                    continue;
                }

                int level = 65;
                string levelStr = GetField(fields, colMap, "Level");
                if (!string.IsNullOrWhiteSpace(levelStr) && int.TryParse(levelStr, out var parsed))
                    level = parsed;

                var character = new Character
                {
                    Name = name,
                    Career = career,
                    Faction = faction,
                    Level = level,
                    ActiveShip = GetFieldOrNull(fields, colMap, "ActiveShip"),
                    AccountId = accountId
                };

                db.Characters.Add(character);
                existingSet.Add(name);
                result.Imported++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Row {rowNum}: {ex.Message}");
            }
        }

        if (result.Imported > 0)
            await db.SaveChangesAsync();

        return result;
    }

    public async Task<ImportResult> ImportInventoryAsync(Stream csvStream, int characterId)
    {
        var result = new ImportResult();
        var (headers, rows) = CsvHelper.Parse(csvStream);

        if (headers.Length == 0)
        {
            result.Errors.Add("CSV file is empty or has no header row.");
            return result;
        }

        var colMap = BuildColumnMap(headers);
        if (!RequireColumns(colMap, result, "ItemName", "Quantity"))
            return result;

        // Cache existing items for name lookups
        var items = await db.Items.ToListAsync();
        var itemLookup = items
            .GroupBy(i => i.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < rows.Count; i++)
        {
            int rowNum = i + 2;
            var fields = rows[i];
            try
            {
                string itemName = GetField(fields, colMap, "ItemName");
                if (string.IsNullOrWhiteSpace(itemName))
                {
                    result.Errors.Add($"Row {rowNum}: ItemName is required.");
                    continue;
                }

                if (!itemLookup.TryGetValue(itemName, out var item))
                {
                    item = new Item { Name = itemName };
                    db.Items.Add(item);
                    itemLookup[itemName] = item;
                }

                int qty = 1;
                string qtyStr = GetField(fields, colMap, "Quantity");
                if (!string.IsNullOrWhiteSpace(qtyStr) && int.TryParse(qtyStr, out var parsedQty))
                    qty = parsedQty;

                InventoryLocation location = InventoryLocation.Inventory;
                string locStr = GetField(fields, colMap, "Location");
                if (!string.IsNullOrWhiteSpace(locStr))
                    TryParseEnum(locStr, out location);

                ItemRarity? rarity = null;
                string rarityStr = GetField(fields, colMap, "Rarity");
                if (!string.IsNullOrWhiteSpace(rarityStr) && TryParseEnum<ItemRarity>(rarityStr, out var parsedRarity))
                    rarity = parsedRarity;

                var inventoryItem = new InventoryItem
                {
                    Item = item,
                    CharacterId = characterId,
                    Quantity = qty,
                    Location = location,
                    Rarity = rarity,
                    Notes = GetFieldOrNull(fields, colMap, "Notes")
                };

                db.InventoryItems.Add(inventoryItem);
                result.Imported++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Row {rowNum}: {ex.Message}");
            }
        }

        if (result.Imported > 0)
            await db.SaveChangesAsync();

        return result;
    }

    public async Task<ImportResult> ImportValuableItemsAsync(Stream csvStream, int accountId)
    {
        var result = new ImportResult();
        var (headers, rows) = CsvHelper.Parse(csvStream);

        if (headers.Length == 0)
        {
            result.Errors.Add("CSV file is empty or has no header row.");
            return result;
        }

        var colMap = BuildColumnMap(headers);
        if (!RequireColumns(colMap, result, "Name", "ItemType"))
            return result;

        // Cache characters for optional CharacterName matching
        var characters = await db.Characters
            .Where(c => c.AccountId == accountId)
            .ToListAsync();
        var charLookup = characters
            .GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < rows.Count; i++)
        {
            int rowNum = i + 2;
            var fields = rows[i];
            try
            {
                string name = GetField(fields, colMap, "Name");
                if (string.IsNullOrWhiteSpace(name))
                {
                    result.Errors.Add($"Row {rowNum}: Name is required.");
                    continue;
                }

                if (!TryParseEnum<ItemType>(GetField(fields, colMap, "ItemType"), out var itemType))
                {
                    result.Errors.Add($"Row {rowNum}: Invalid ItemType '{GetField(fields, colMap, "ItemType")}'.");
                    continue;
                }

                int? characterId = null;
                string charName = GetField(fields, colMap, "CharacterName");
                if (!string.IsNullOrWhiteSpace(charName))
                {
                    if (charLookup.TryGetValue(charName, out var character))
                        characterId = character.Id;
                    else
                        result.Errors.Add($"Row {rowNum}: Character '{charName}' not found; importing without character link.");
                }

                InventoryLocation location = InventoryLocation.Inventory;
                string locStr = GetField(fields, colMap, "Location");
                if (!string.IsNullOrWhiteSpace(locStr))
                    TryParseEnum(locStr, out location);

                int qty = 1;
                string qtyStr = GetField(fields, colMap, "Quantity");
                if (!string.IsNullOrWhiteSpace(qtyStr) && int.TryParse(qtyStr, out var parsedQty))
                    qty = parsedQty;

                var valuableItem = new ValuableItem
                {
                    Name = name,
                    ItemType = itemType,
                    AccountId = accountId,
                    CharacterId = characterId,
                    Location = location,
                    Quantity = qty,
                    Notes = GetFieldOrNull(fields, colMap, "Notes")
                };

                db.ValuableItems.Add(valuableItem);
                result.Imported++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Row {rowNum}: {ex.Message}");
            }
        }

        if (result.Imported > 0)
            await db.SaveChangesAsync();

        return result;
    }

    public async Task<Stream> ExportCharactersAsync(int accountId)
    {
        var characters = await db.Characters
            .Where(c => c.AccountId == accountId)
            .OrderBy(c => c.Name)
            .ToListAsync();

        string[] headers = ["Name", "Career", "Faction", "Level", "ActiveShip"];
        var rows = characters.Select(c => new[]
        {
            c.Name,
            c.Career.ToString(),
            c.Faction.ToString(),
            c.Level.ToString(),
            c.ActiveShip ?? string.Empty
        });

        return CsvHelper.Write(headers, rows);
    }

    public async Task<Stream> ExportInventoryAsync(int characterId)
    {
        var items = await db.InventoryItems
            .Include(ii => ii.Item)
            .Where(ii => ii.CharacterId == characterId)
            .OrderBy(ii => ii.Item.Name)
            .ToListAsync();

        string[] headers = ["ItemName", "Quantity", "Location", "Rarity", "Notes"];
        var rows = items.Select(ii => new[]
        {
            ii.Item.Name,
            ii.Quantity.ToString(),
            ii.Location.ToString(),
            ii.Rarity?.ToString() ?? string.Empty,
            ii.Notes ?? string.Empty
        });

        return CsvHelper.Write(headers, rows);
    }

    #region Helpers

    private static Dictionary<string, int> BuildColumnMap(string[] headers)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < headers.Length; i++)
            map[headers[i]] = i;
        return map;
    }

    private static bool RequireColumns(Dictionary<string, int> colMap, ImportResult result, params string[] required)
    {
        var missing = required.Where(r => !colMap.ContainsKey(r)).ToList();
        if (missing.Count > 0)
        {
            result.Errors.Add($"Missing required columns: {string.Join(", ", missing)}");
            return false;
        }
        return true;
    }

    private static string GetField(string[] fields, Dictionary<string, int> colMap, string column)
    {
        if (!colMap.TryGetValue(column, out int idx) || idx >= fields.Length)
            return string.Empty;
        return fields[idx].Trim();
    }

    private static string? GetFieldOrNull(string[] fields, Dictionary<string, int> colMap, string column)
    {
        var value = GetField(fields, colMap, column);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static bool TryParseEnum<T>(string value, out T parsed) where T : struct, Enum
    {
        return Enum.TryParse(value, ignoreCase: true, out parsed);
    }

    #endregion
}
