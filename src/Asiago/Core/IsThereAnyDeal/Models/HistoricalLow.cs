namespace Asiago.Core.IsThereAnyDeal.Models
{
    public class HistoricalLow
    {
        public required Shop Shop { get; set; }
        public required Price Price { get; set; }
        public required Price Regular { get; set; }
        public required int Cut { get; set; }
        public required DateTimeOffset Timestamp { get; set; }
    }
}
