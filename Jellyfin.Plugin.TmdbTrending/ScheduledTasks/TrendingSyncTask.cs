using Jellyfin.Plugin.TmdbTrending.Services;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Runtime.Loader;

namespace Jellyfin.Plugin.TmdbTrending.ScheduledTasks;

public class TrendingSyncTask : IScheduledTask
{
    private readonly TmdbService _tmdbService;
    private readonly LibraryMatchService _libraryMatchService;
    private readonly ILogger<TrendingSyncTask> _logger;

    public string Name => "TMDB Trending Synchronisation";
    public string Key => "TmdbTrendingSync";
    public string Description => "Update the trending list from TMDB";
    public string Category => "TMDB Trending";

    public TrendingSyncTask(TmdbService tmdbService, LibraryMatchService libraryMatchService, ILogger<TrendingSyncTask> logger)
    {
        _tmdbService = tmdbService;
        _libraryMatchService = libraryMatchService;
        _logger = logger;
    }

    private static string GetTmdbApiKey()
    {
        try
        {
            Assembly? tmdbAssembly =
                AssemblyLoadContext.All
                    .SelectMany(x => x.Assemblies)
                    .FirstOrDefault(x => x.FullName?.Contains("MediaBrowser.Providers") ?? false);

            var pluginClassType = tmdbAssembly?.GetType("MediaBrowser.Providers.Plugins.Tmdb.Plugin");
            var instanceProp = pluginClassType?.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            var pluginInstance = instanceProp?.GetValue(null);

            var configProp = pluginInstance?.GetType().GetProperty("Configuration");
            var config = configProp?.GetValue(pluginInstance);

            var tmdbApiKeyProp = config?.GetType().GetProperty("TmdbApiKey");
            var customApiKey = tmdbApiKeyProp?.GetValue(config) as string;

            string apiKey;
            if (!string.IsNullOrEmpty(customApiKey))
            {
                apiKey = customApiKey;
            }
            else
            {
                var tmdbUtils = tmdbAssembly?.GetType("MediaBrowser.Providers.Plugins.Tmdb.TmdbUtils");
                var apiKeyField = tmdbUtils?.GetField("ApiKey", BindingFlags.Public | BindingFlags.Static);
                apiKey = (string?)apiKeyField?.GetValue(null) ?? string.Empty;
            }
            return apiKey;
        }
        catch
        {
            throw new InvalidOperationException("API key TMDB cannot be retrieved via reflection.");
        }
    }

    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        var config = Plugin.Instance!.Configuration;

        var apiKey = config.TmdbApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            apiKey = GetTmdbApiKey();
        }

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("API key TMDB not configured.");

        try
        {
            var movieTmdbIds = await _tmdbService.FetchTrendingMovieIdsAsync(apiKey);
            var tvTmdbIds = await _tmdbService.FetchTrendingTvIdsAsync(apiKey);
            var allTmdbIds = movieTmdbIds.Concat(tvTmdbIds).ToList();
            progress.Report(25);

            _logger.LogInformation("TMDB Trending: {MovieCount} movies and {TvCount} shows retrieved", movieTmdbIds.Count, tvTmdbIds.Count);

            var jellyfinGuids = _libraryMatchService
                .FindJellyfinIdsByTmdbIds(allTmdbIds)
                .Select(g => g.ToString())
                .ToList();
            progress.Report(50);

            _logger.LogInformation("TMDB Trending: {MatchCount} elements found in Jellyfin", jellyfinGuids.Count);

            var updated = MergeIds(jellyfinGuids, config.TrendingItemIds);

            if (updated.Count < 5)
            {
                _logger.LogWarning("TMDB Trending: Only {Count} elements available (minimum recommended: 5)", updated.Count);
            }

            config.TrendingItemIds = updated;
            Plugin.Instance.SaveConfiguration();
            progress.Report(100);

            _logger.LogInformation("TMDB Trending: Updated with {Count} elements", updated.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TMDB Trending: Error during synchronization");
            throw;
        }
    }

    private static List<string> MergeIds(List<string> fresh, List<string> existing)
    {
        const int Max = 15;

        return fresh
            .Union(existing, StringComparer.OrdinalIgnoreCase)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(Max)
            .ToList();
    }

    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers() =>
    [
        new TaskTriggerInfo
        {
            Type = TaskTriggerInfoType.DailyTrigger,
            TimeOfDayTicks = TimeSpan.FromHours(3).Ticks // 3h du matin
        }
    ];
}
