using Jellyfin.Plugin.TmdbTrending.Helpers;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Runtime.Loader;


namespace Jellyfin.Plugin.TmdbTrending.ScheduledTasks;

public class StartupService : IScheduledTask
{
    public string Name => "TMDB Trending Startup";

    public string Key => "Jellyfin.Plugin.TmdbTrending.Startup";

    public string Description => "Startup Service for TMDB Trending";

    public string Category => "Startup Services";

    private readonly ILogger<StartupService> _logger;

    public StartupService(ILogger<StartupService> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"TMDB Trending Startup. Registering file transformations.");

        Assembly? fileTransformationAssembly =
            AssemblyLoadContext.All.SelectMany(x => x.Assemblies).FirstOrDefault(x =>
                x.FullName?.Contains(".FileTransformation") ?? false);

        if (fileTransformationAssembly != null)
        {
            Type? pluginInterfaceType = fileTransformationAssembly.GetType("Jellyfin.Plugin.FileTransformation.PluginInterface");
            pluginInterfaceType?.GetMethod("RegisterTransformation")?.Invoke(null, new object?[] { TrendingListPatch.BuildPayloads() });
        }

        return Task.CompletedTask;
    }

    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers() =>
    [
        new TaskTriggerInfo
        {
            Type = TaskTriggerInfoType.StartupTrigger
        }
    ];
}
