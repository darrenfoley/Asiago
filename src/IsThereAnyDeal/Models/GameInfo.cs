namespace IsThereAnyDeal.Models
{
    public class GameInfo
    {
        public string Title { get; }
        public string? ImageUrl { get; }
        public string IsThereAnyDealGameUrl { get; }
        public Dictionary<string, GameReview>? Reviews { get; }

        public GameInfo(string title, string? imageUrl, string isThereAnyDealGameUrl, Dictionary<string, GameReview>? reviews)
        {
            Title = title;
            ImageUrl = imageUrl;
            IsThereAnyDealGameUrl = isThereAnyDealGameUrl;
            Reviews = reviews;
        }
    }
}
