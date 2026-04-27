using System.Text;
using Microsoft.EntityFrameworkCore;
using STO.Core.Enums;
using STO.Core.Models;
using STO.Core.Services;
using STO.Data.Context;

namespace STO.Data.Services;

public class MarkdownSyncService : IMarkdownSyncService
{
    private readonly StoDbContext _db;

    public MarkdownSyncService(StoDbContext db)
    {
        _db = db;
    }

    public async Task ExportBuildToMarkdownAsync(int buildId, string repoRoot)
    {
        var build = await _db.Builds
            .Include(b => b.Slots).ThenInclude(s => s.Item)
            .Include(b => b.Character)
            .FirstOrDefaultAsync(b => b.Id == buildId);

        if (build is null) return;

        var markdown = await GenerateBuildMarkdownAsync(buildId);
        var careerDir = build.Character.Career switch
        {
            Career.Engineering => "Engineering",
            Career.Science => "Science",
            Career.Tactical => "Tactical",
            _ => "Other"
        };

        var buildType = build.Type == BuildType.Space ? "space" : "ground";
        var fileName = $"{buildType}.md";
        var filePath = Path.Combine(repoRoot, careerDir, fileName);

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await File.WriteAllTextAsync(filePath, markdown);
    }

    public async Task ExportAllBuildsAsync(int characterId, string repoRoot)
    {
        var builds = await _db.Builds
            .Where(b => b.CharacterId == characterId)
            .Select(b => b.Id)
            .ToListAsync();

        foreach (var buildId in builds)
        {
            await ExportBuildToMarkdownAsync(buildId, repoRoot);
        }
    }

    public async Task<string> GenerateBuildMarkdownAsync(int buildId)
    {
        var build = await _db.Builds
            .Include(b => b.Slots).ThenInclude(s => s.Item)
            .Include(b => b.Character).ThenInclude(c => c.Account)
            .FirstOrDefaultAsync(b => b.Id == buildId);

        if (build is null) return string.Empty;

        var sb = new StringBuilder();
        var character = build.Character;

        sb.AppendLine($"# {character.Career} {build.Type} Build");
        sb.AppendLine();
        sb.AppendLine($"**Character:** {character.Name}");
        sb.AppendLine($"**Account:** {character.Account.Gamertag}");
        sb.AppendLine($"**Career:** {character.Career}");
        sb.AppendLine($"**Faction:** {character.Faction}");

        if (build.Type == BuildType.Space && !string.IsNullOrEmpty(build.ShipName))
        {
            sb.AppendLine($"**Ship:** {build.ShipName}");
        }

        sb.AppendLine();

        if (!string.IsNullOrEmpty(build.Notes))
        {
            sb.AppendLine($"> {build.Notes}");
            sb.AppendLine();
        }

        // Group slots by name prefix for organized output
        var slots = build.Slots.OrderBy(s => s.Position).ToList();
        if (slots.Count > 0)
        {
            sb.AppendLine("## Equipment");
            sb.AppendLine();
            sb.AppendLine("| Slot | Item |");
            sb.AppendLine("|------|------|");
            foreach (var slot in slots)
            {
                var itemName = slot.Item?.Name ?? "_empty_";
                sb.AppendLine($"| {slot.SlotName} | {itemName} |");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
