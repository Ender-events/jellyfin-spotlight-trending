using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace Jellyfin.Plugin.TmdbTrending.Services;

public class TmdbService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://api.themoviedb.org/3";

    public TmdbService(HttpClient httpClient)
        => _httpClient = httpClient;

    public async Task<List<string>> FetchTrendingMovieIdsAsync(string apiKey)
    {
        var url = $"{BaseUrl}/trending/movie/day?language=us-US&api_key={apiKey}";

        var response = await _httpClient.GetStringAsync(url);
        var json = JObject.Parse(response);

        return json["results"]!
            .Select(r => r["id"]!.ToString())
            .Where(id => !string.IsNullOrEmpty(id))
            .ToList();
    }

    public async Task<List<string>> FetchTrendingTvIdsAsync(string apiKey)
    {
        var url = $"{BaseUrl}/trending/tv/day?language=us-US&api_key={apiKey}";

        var response = await _httpClient.GetStringAsync(url);
        var json = JObject.Parse(response);

        return json["results"]!
            .Select(r => r["id"]!.ToString())
            .Where(id => !string.IsNullOrEmpty(id))
            .ToList();
    }
}
