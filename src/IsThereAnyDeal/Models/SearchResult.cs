namespace IsThereAnyDeal.Models
{
    public class SearchResult
    {
        public string Id { get; set; }
        public string Title { get; set; }

        public SearchResult(string id, string title)
        {
            Id = id;
            Title = title;
        }
    }
}
