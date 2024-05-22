using Asiago.Core.IsThereAnyDeal.Models;
using Asiago.Extensions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Asiago.Core.IsThereAnyDeal
{
    internal class IsThereAnyDealClient
    {
        private static readonly Uri BaseUrl = new("https://api.isthereanydeal.com");

        private readonly IsThereAnyDealOptions _itadOptions;
        private readonly HttpClient _httpClient;
        private readonly ILogger<IsThereAnyDealClient> _logger;

        public IsThereAnyDealClient(
            IOptions<IsThereAnyDealOptions> itadOptions,
            HttpClient httpClient,
            ILogger<IsThereAnyDealClient> logger
        )
        {
            _itadOptions = itadOptions.Value;
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Looks up a game by its title.
        /// </summary>
        public async Task<Game?> LookupGameAsync(string title)
        {
            Dictionary<string, string> queryParameters = new()
            {
                { "key", _itadOptions.ApiKey },
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
                { "key", _itadOptions.ApiKey },
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
                { "key", _itadOptions.ApiKey },
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
                { "key", _itadOptions.ApiKey },
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

        private async Task<T?> ParseResponse<T>(HttpResponseMessage response) where T : class
        {
            T? ret = null;

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    ret = JsonConvert.DeserializeObject<T>(content);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to deserialize response for request [{method} {path}]. Response content: [{content}]",
                        response.RequestMessage?.Method,
                        response.RequestMessage?.RequestUri?.AbsolutePath,
                        content
                        );
                }
            }

            return ret;
        }
    }
}
