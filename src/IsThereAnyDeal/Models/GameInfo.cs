using Asiago.Core.JsonConverters;
using Newtonsoft.Json;

namespace IsThereAnyDeal.Models
{
    /// <summary>
    /// Game information.
    /// </summary>
    public class GameInfo
    {
        public required Guid Id { get; set; }
        public required string Slug { get; set; }
        public required string Title { get; set; }
        [JsonConverter(typeof(StrictStringEnumConverter))]
        public required GameType? Type { get; set; }
        public required bool Mature { get; set; }
        public required Assets Assets { get; set; }
        public required bool EarlyAccess { get; set; }
        public required bool Achievements { get; set; }
        public required bool TradingCards { get; set; }
        public required int? AppId { get; set; }
        public required List<string> Tags { get; set; }
        public required DateTimeOffset? ReleaseDate { get; set; }
        public required List<Developer> Developers { get; set; }
        public required List<Publisher> Publishers { get; set; }
        public required List<Review> Reviews { get; set; }
        public required Stats Stats { get; set; }
        public required PlayerStats? Players { get; set; }
        public required Urls Urls { get; set; }
    }
}
