using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace STO.Wiki;

/// <summary>
/// HttpClient-based client for querying the MediaWiki API at stowiki.net.
/// </summary>
public class WikiApiClient
{
    private const string BaseUrl = "https://stowiki.net/api.php";
    private static readonly TimeSpan RequestDelay = TimeSpan.FromMilliseconds(500);

    private readonly HttpClient _http;
    private readonly ILogger<WikiApiClient> _logger;
    private DateTime _lastRequestTime = DateTime.MinValue;

    public WikiApiClient(HttpClient httpClient, ILogger<WikiApiClient> logger)
    {
        _http = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Get all pages in a category, automatically handling cmcontinue pagination.
    /// </summary>
    public async Task<List<CategoryMember>> GetCategoryMembersAsync(string category, int limit = 500, CancellationToken ct = default)
    {
        var results = new List<CategoryMember>();
        string? cmcontinue = null;

        do
        {
            var url = $"{BaseUrl}?action=query&list=categorymembers&cmtitle=Category:{Uri.EscapeDataString(category)}&cmlimit={limit}&format=json";
            if (cmcontinue is not null)
                url += $"&cmcontinue={Uri.EscapeDataString(cmcontinue)}";

            var jsonNullable = await GetJsonAsync(url, ct);
            if (jsonNullable is not { } json) break;

            if (json.TryGetProperty("query", out var query) &&
                query.TryGetProperty("categorymembers", out var members))
            {
                foreach (var member in members.EnumerateArray())
                {
                    results.Add(new CategoryMember
                    {
                        PageId = member.GetProperty("pageid").GetInt64(),
                        Title = member.GetProperty("title").GetString() ?? string.Empty
                    });
                }
            }

            cmcontinue = null;
            if (json.TryGetProperty("continue", out var cont) &&
                cont.TryGetProperty("cmcontinue", out var cmcont))
            {
                cmcontinue = cmcont.GetString();
            }
        }
        while (cmcontinue is not null && !ct.IsCancellationRequested);

        _logger.LogDebug("Fetched {Count} members from Category:{Category}", results.Count, category);
        return results;
    }

    /// <summary>
    /// Get the raw wikitext content of a page.
    /// </summary>
    public async Task<string?> GetPageContentAsync(string title, CancellationToken ct = default)
    {
        var url = $"{BaseUrl}?action=query&prop=revisions&rvprop=content&format=json&titles={Uri.EscapeDataString(title)}";
        var jsonNullable = await GetJsonAsync(url, ct);
        if (jsonNullable is not { } json) return null;

        if (json.TryGetProperty("query", out var query) &&
            query.TryGetProperty("pages", out var pages))
        {
            foreach (var page in pages.EnumerateObject())
            {
                // Skip missing pages (id = -1)
                if (page.Value.TryGetProperty("missing", out _)) continue;

                if (page.Value.TryGetProperty("revisions", out var revisions))
                {
                    var rev = revisions[0];
                    // MediaWiki returns content under "*" key
                    if (rev.TryGetProperty("*", out var content))
                        return content.GetString();
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Search pages by term.
    /// </summary>
    public async Task<List<SearchResult>> SearchPagesAsync(string searchTerm, CancellationToken ct = default)
    {
        var url = $"{BaseUrl}?action=query&list=search&srsearch={Uri.EscapeDataString(searchTerm)}&format=json";
        var jsonNullable = await GetJsonAsync(url, ct);
        var results = new List<SearchResult>();

        if (jsonNullable is not { } json) return results;

        if (json.TryGetProperty("query", out var query) &&
            query.TryGetProperty("search", out var search))
        {
            foreach (var item in search.EnumerateArray())
            {
                results.Add(new SearchResult
                {
                    Title = item.GetProperty("title").GetString() ?? string.Empty,
                    Snippet = item.TryGetProperty("snippet", out var snippet) ? snippet.GetString() ?? string.Empty : string.Empty,
                    PageId = item.GetProperty("pageid").GetInt64()
                });
            }
        }

        return results;
    }

    private async Task<JsonElement?> GetJsonAsync(string url, CancellationToken ct)
    {
        await ThrottleAsync(ct);

        try
        {
            using var response = await _http.GetAsync(url, ct);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(5);
                _logger.LogWarning("Rate limited, waiting {Seconds}s", retryAfter.TotalSeconds);
                await Task.Delay(retryAfter, ct);
                using var retryResponse = await _http.GetAsync(url, ct);
                retryResponse.EnsureSuccessStatusCode();
                var retryDoc = await JsonDocument.ParseAsync(await retryResponse.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
                return retryDoc.RootElement.Clone();
            }

            response.EnsureSuccessStatusCode();
            var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            return doc.RootElement.Clone();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error requesting {Url}", url);
            return null;
        }
        catch (TaskCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting {Url}", url);
            return null;
        }
    }

    private async Task ThrottleAsync(CancellationToken ct)
    {
        var elapsed = DateTime.UtcNow - _lastRequestTime;
        if (elapsed < RequestDelay)
        {
            await Task.Delay(RequestDelay - elapsed, ct);
        }
        _lastRequestTime = DateTime.UtcNow;
    }
}

public class CategoryMember
{
    public long PageId { get; set; }
    public string Title { get; set; } = string.Empty;
}

public class SearchResult
{
    public long PageId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Snippet { get; set; } = string.Empty;
}
