namespace IsThereAnyDeal.Models
{
    public class GamePrice
    {
        public decimal CurrentPrice { get; }
        public decimal RegularPrice { get; }
        public int PriceCutPercent { get; }
        public string Store { get; }
        public string Url { get; }

        public GamePrice(decimal currentPrice, decimal regularPrice, int priceCutPercent, string store, string url)
        {
            CurrentPrice = currentPrice;
            RegularPrice = regularPrice;
            PriceCutPercent = priceCutPercent;
            Store = store;
            Url = url;
        }
    }
}
