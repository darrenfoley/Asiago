namespace Asiago.Models
{
    public class GameBestCurrentPrice
    {
        public string Store { get; }
        public decimal Price { get; }
        public int PriceCutPercent { get; }
        public string Url { get; }

        public GameBestCurrentPrice(string store, decimal price, int priceCutPercent, string url)
        {
            Store = store;
            Price = price;
            PriceCutPercent = priceCutPercent;
            Url = url;
        }
    }
}
