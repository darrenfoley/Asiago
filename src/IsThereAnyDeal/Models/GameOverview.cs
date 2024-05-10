namespace IsThereAnyDeal.Models
{
    /// <summary>
    /// Basic price overview for selected games.
    /// </summary>
    public class GameOverview
    {
        public required List<PriceOverview> Prices { get; set; }

        // I don't see us caring about bundles anytime soon...or probably ever?
        // public required List<BundleOverview> Bundles { get; set; }
    }
}
