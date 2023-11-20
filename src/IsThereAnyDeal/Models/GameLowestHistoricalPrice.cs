namespace Asiago.Models
{
    public class GameLowestHistoricalPrice
    {
        public string Store { get; }
        public decimal Price { get; }
        public int PriceCutPercent { get; }
        public DateTime Recorded { get; }
        public string FormattedRecorded { get; }

        public GameLowestHistoricalPrice(string store, decimal price, int priceCutPercent, DateTime recorded, string formattedRecorded)
        {
            Store = store;
            Price = price;
            PriceCutPercent = priceCutPercent;
            Recorded = recorded;
            FormattedRecorded = formattedRecorded;
        }
    }
}
