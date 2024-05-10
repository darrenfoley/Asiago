namespace IsThereAnyDeal.Models
{
    public class PriceOverview
    {
        public required Guid Id { get; set; }
        public required OverviewDeal? Current { get; set; }
        public required HistoricalLow? Lowest { get; set; }
        public required int Bundled { get; set; }
        public required Urls Urls { get; set; }
    }
}
