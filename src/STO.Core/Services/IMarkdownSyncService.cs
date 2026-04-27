namespace STO.Core.Services;

public interface IMarkdownSyncService
{
    /// <summary>
    /// Export a character's build to a markdown file in the appropriate career directory.
    /// </summary>
    Task ExportBuildToMarkdownAsync(int buildId, string repoRoot);

    /// <summary>
    /// Export all builds for a character to markdown files.
    /// </summary>
    Task ExportAllBuildsAsync(int characterId, string repoRoot);

    /// <summary>
    /// Generate markdown content for a build.
    /// </summary>
    Task<string> GenerateBuildMarkdownAsync(int buildId);
}
