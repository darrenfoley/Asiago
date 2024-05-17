using Asiago.Core.IsThereAnyDeal.Models;
using Asiago.Extensions;
using Newtonsoft.Json;

namespace Asiago.Core.IsThereAnyDeal
{
    public class IsThereAnyDealClient(string apiKey, HttpClient httpClient)
    {
        private static readonly Uri BaseUrl = new("https://api.isthereanydeal.com");

        private readonly string _apiKey = apiKey;
        private readonly HttpClient _httpClient = httpClient;

        /// <summary>
        /// Looks up a game by its title.
        /// </summary>
        public async Task<Game?> LookupGameAsync(string title)
        {
            Dictionary<string, string> queryParameters = new()
            {
                { "key", _apiKey },
                { "title", title }
            };

            var lookupResult = await GetAsync<GameLookupResult>("/games/lookup/v1", queryParameters);

            if (lookupResult != null && lookupResult.Found)
            {
                return lookupResult.Game;
            }

            return null;
        }

        /// <summary>
        /// Gets price overview information for a game by id.
        /// </summary>
        public async Task<PriceOverview?> GetGamePriceOverviewAsync(Guid id, string countryCode)
        {
            Dictionary<string, string> queryParameters = new()
            {
                { "key", _apiKey },
                { "country", countryCode }
            };

            var gameOverview = await PostAsync<GameOverview, List<Guid>>("/games/overview/v2", queryParameters, [id]);
            return gameOverview?.Prices.SingleOrDefault(p => p.Id == id);
        }

        /// <summary>
        /// Get game information.
        /// </summary>
        public async Task<GameInfo?> GetGameInfoAsync(Guid id)
        {
            Dictionary<string, string> queryParameters = new()
            {
                { "key", _apiKey },
                { "id", id.ToString() }
            };

            return await GetAsync<GameInfo>("/games/info/v2", queryParameters);
        }

        /// <summary>
        /// Search for games by title.
        /// </summary>
        public async Task<List<Game>?> SearchGamesAsync(string title, int limit)
        {
            Dictionary<string, string> queryParameters = new()
            {
                { "key", _apiKey },
                { "title", title },
                { "limit", limit.ToString() }
            };

            return await GetAsync<List<Game>>("/games/search/v1", queryParameters);
        }

        private async Task<T?> GetAsync<T>(string path, Dictionary<string, string> queryParameters) where T : class
        {
            Uri url = BuildUrl(path, queryParameters);
            var response = await _httpClient.GetAsync(url);
            return await ParseResponse<T>(response);
        }

        private async Task<T?> PostAsync<T, B>(string path, Dictionary<string, string> queryParameters, B body) where T : class
        {
            Uri url = BuildUrl(path, queryParameters);
            var response = await _httpClient.PostAsJsonAsync(url, body);
            return await ParseResponse<T>(response);
        }

        private static Uri BuildUrl(string path, Dictionary<string, string> queryParameters)
            => new UriBuilder(BaseUrl)
            {
                Path = path,
                Query = queryParameters.ToQueryString()
            }.Uri;

        private static async Task<T?> ParseResponse<T>(HttpResponseMessage response) where T : class
        {
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content);
        }
    }
}
