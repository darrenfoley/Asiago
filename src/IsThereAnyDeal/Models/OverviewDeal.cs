using Asiago.Common.JsonConverters;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace IsThereAnyDeal.Models
{
    public class OverviewDeal
    {
        public required Shop Shop { get; set; }
        public required Price Price { get; set; }
        public required Price Regular { get; set; }
        public required int Cut { get; set; }
        public required string? Voucher { get; set; }

        public enum DealFlag
        {
            [EnumMember(Value = "H")]
            HistoricalLow,
            [EnumMember(Value = "N")]
            NewHistoricalLow,
            [EnumMember(Value = "S")]
            StoreLow
        }
        [JsonConverter(typeof(StrictStringEnumConverter))]
        public required DealFlag? Flag { get; set; }
        public required List<Drm> Drm { get; set; }
        public required List<Platform> Platforms { get; set; }
        public required DateTimeOffset Timestamp { get; set; }
        public required DateTimeOffset? Expiry { get; set; }
        public required Uri Url { get; set; }
    }
}
