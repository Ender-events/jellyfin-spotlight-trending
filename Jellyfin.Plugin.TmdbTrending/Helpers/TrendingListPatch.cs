using Newtonsoft.Json.Linq;
using System.IO;

namespace Jellyfin.Plugin.TmdbTrending.Helpers;

/// <summary>
/// Callback pour jellyfin-plugin-media-bar
/// Intercepte la demande de avatars/list.txt et injecte les IDs trendings
/// </summary>
public static class TrendingListPatch
{
    public static JObject BuildPayloads()
    {
        return new JObject
        {
            ["id"] = "a8f0b126-a8d2-4667-b2d4-6e5c5cfe7e78",
            ["fileNamePattern"] = "avatars/list.txt",
            ["callbackAssembly"] = typeof(TrendingListPatch).Assembly.FullName,
            ["callbackClass"] = typeof(TrendingListPatch).FullName!,
            ["callbackMethod"] = nameof(GetTrendingList)
        };
    }

    public static string GetTrendingList(object payload)
    {
        var config = Plugin.Instance?.Configuration;
        var ids = config?.TrendingItemIds;

        if (ids == null || ids.Count == 0)
            return string.Empty;

        using var writer = new StringWriter();
        writer.WriteLine("TMDB Trending"); // Nom de la section

        foreach (var id in ids)
        {
            // Media Bar expects GUIDs without dashes
            writer.WriteLine(id.Replace("-", ""));
        }

        return writer.ToString();
    }
}
