using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;

namespace Jellyfin.Plugin.TmdbTrending.Services;

public class LibraryMatchService
{
    private readonly ILibraryManager _libraryManager;

    public LibraryMatchService(ILibraryManager libraryManager)
        => _libraryManager = libraryManager;

    public List<Guid> FindJellyfinIdsByTmdbIds(IEnumerable<string> tmdbIds)
    {
        var tmdbIdSet = new HashSet<string>(tmdbIds, StringComparer.OrdinalIgnoreCase);

        var items = _libraryManager.GetItemList(new InternalItemsQuery
        {
            IncludeItemTypes = new[] { BaseItemKind.Movie, BaseItemKind.Series },
            HasTmdbId = true,
            DtoOptions = new DtoOptions(false) { EnableImages = false, Fields = new[] { ItemFields.ProviderIds } }
        });

        return items
            .Where(i => tmdbIdSet.Contains(
                i.GetProviderId(MetadataProvider.Tmdb) ?? string.Empty))
            .Select(i => i.Id)
            .ToList();
    }
}
