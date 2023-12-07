using IsThereAnyDeal.Extensions;
using IsThereAnyDeal.Models;
using Newtonsoft.Json;

namespace IsThereAnyDeal
{
    public class IsThereAnyDealClient
    {
        private const string BaseUrl = "https://api.isthereanydeal.com";

        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public IsThereAnyDealClient(string apiKey, HttpClient httpClient)
        {
            _apiKey = apiKey;
            _httpClient = httpClient;
        }

        public async Task<string?> GetGameId(string title)
        {
            Dictionary<string, string> queryParameters = new()
            {
                { "key", _apiKey },
                { "title", title }
            };

            var plainResponse = await GetAsync<ResponseModels.GamePlain>("/v02/game/plain/", queryParameters);

            if (plainResponse is not null
                && plainResponse.Meta.Active
                && plainResponse.Meta.Match == "title"
                && plainResponse.Data is not null)
            {
                return plainResponse.Data.Plain;
            }

            return null;
        }

        public async Task<GameOverview?> GetGameOverview(string id, string countryCode)
        {
            Dictionary<string, string> queryParameters = new()
            {
                { "key", _apiKey },
                { "plains", id },
                { "country", countryCode }
            };

            var overviewResponse = await GetAsync<ResponseModels.GameOverview>("/v01/game/overview/", queryParameters);
            return overviewResponse?.Extract(id);
        }

        public async Task<List<GamePrice>?> GetGamePrices(string id, string countryCode)
        {
            Dictionary<string, string> queryParameters = new()
            {
                { "key", _apiKey },
                { "plains", id },
                { "country", countryCode }
            };

            var pricesResponse = await GetAsync<ResponseModels.GamePrices>("/v01/game/prices/", queryParameters);
            return pricesResponse?.Extract(id);
        }

        public async Task<GameInfo?> GetGameInfo(string id)
        {
            Dictionary<string, string> queryParameters = new()
            {
                { "key", _apiKey },
                { "plains", id }
            };

            var infoResponse = await GetAsync<ResponseModels.GameInfo>("/v01/game/info/", queryParameters);
            return infoResponse?.Extract(id);
        }

        private async Task<T?> GetAsync<T>(string path, Dictionary<string, string> queryParameters) where T : class
        {
            UriBuilder uriBuilder = new($"{BaseUrl}{path}")
            {
                Query = queryParameters.ToQueryString()
            };
            var response = await _httpClient.GetAsync(uriBuilder.Uri);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var deserializedObject = JsonConvert.DeserializeObject<T>(content);
            return deserializedObject;
        }
    }
}
