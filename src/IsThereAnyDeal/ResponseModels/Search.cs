namespace IsThereAnyDeal.ResponseModels
{
    internal class Search
    {
        public SearchData Data { get; set; } = null!;

        public List<Models.SearchResult> Extract()
        {
            return Data.Results.ConvertAll(result => new Models.SearchResult(result.Plain, result.Title));
        }
    }

    internal class SearchData
    {
        public List<SearchResult> Results { get; set; } = null!;
        public SearchUrls Urls { get; set; } = null!;
    }

    internal class SearchResult
    {
        public long Id { get; set; }
        public string Plain { get; set; } = null!;
        public string Title { get; set; } = null!;
    }

    internal class SearchUrls
    {
        public string Search { get; set; } = null!;
    }
}
