using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Asiago.ResponseModels
{
    internal class GameInfo
    {
        public Dictionary<string, GameInfoData> Data { get; set; } = null!;

        internal Models.GameInfo? Extract(string plain)
        {
            if (!Data.TryGetValue(plain, out GameInfoData? gameInfoData) || gameInfoData.Title is null)
            {
                return null;
            }

            return new(
                gameInfoData.Title,
                gameInfoData.Image,
                gameInfoData.Urls.Game,
                gameInfoData.Reviews?.ToDictionary(
                    item => item.Key,
                    item => new Models.GameReview(item.Value.PercentPositive, item.Value.Total, item.Value.Text)
                ));
        }
    }

    internal class GameInfoData
    {
        public string? Title { get; set; }
        public string? Image { get; set; }
        [JsonProperty("is_package")]
        public bool Package { get; set; }
        [JsonProperty("is_dlc")]

        public bool Dlc { get; set; }
        public bool Achievements { get; set; }
        [JsonProperty("trading_cards")]

        public bool TradingCards { get; set; }
        [JsonProperty("early_access")]

        public bool EarlyAccess { get; set; }
        public Dictionary<string, GameInfoReview>? Reviews { get; set; }
        public GameInfoUrls Urls { get; set; } = null!;
    }

    internal class GameInfoReview
    {
        [JsonProperty("perc_positive")]

        public int PercentPositive { get; set; }
        public int Total { get; set; }
        public string Text { get; set; } = null!;

        // Timestamp isn't in the api spec but it seems to usually be returned, so we'll make it nullable just in case
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime? Timestamp { get; set; }
    }

    internal class GameInfoUrls
    {
        public string Game { get; set; } = null!;

        // History isn't in the api spec but it seems to usually be returned, so we'll make it nullable just in case
        public string? History { get; set; }
        public string Package { get; set; } = null!;
        public string Dlc { get; set; } = null!;
    }
}