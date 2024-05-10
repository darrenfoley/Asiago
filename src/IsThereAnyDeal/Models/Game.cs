using Asiago.Core.JsonConverters;
using Newtonsoft.Json;

namespace IsThereAnyDeal.Models
{
    /// <summary>
    /// Basic information about a game.
    /// </summary>
    public class Game
    {
        public required Guid Id { get; set; }
        public required string Slug { get; set; }
        public required string Title { get; set; }
        [JsonConverter(typeof(StrictStringEnumConverter))]
        public required GameType? Type { get; set; }
        public required bool Mature { get; set; }
    }
}
