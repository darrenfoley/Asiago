namespace Asiago.Models
{
    public class GameOverview
    {
        public GameBestCurrentPrice BestCurrentPrice { get; }
        public GameLowestHistoricalPrice? LowestHistoricalPrice { get; }

        public GameOverview(GameBestCurrentPrice bestCurrentPrice, GameLowestHistoricalPrice? lowestHistoricalPrice)
        {
            BestCurrentPrice = bestCurrentPrice;
            LowestHistoricalPrice = lowestHistoricalPrice;
        }
    }
}
