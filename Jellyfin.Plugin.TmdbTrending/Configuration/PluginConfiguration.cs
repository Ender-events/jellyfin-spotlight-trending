using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.TmdbTrending.Configuration;

public class PluginConfiguration : BasePluginConfiguration
{
    public string TmdbApiKey { get; set; } = string.Empty;
    public List<string> TrendingItemIds { get; set; } = new();
}
