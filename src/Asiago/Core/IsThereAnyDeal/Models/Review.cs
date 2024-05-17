namespace Asiago.Core.IsThereAnyDeal.Models
{
    public class Review
    {
        public required int? Score { get; set; }
        public required string Source { get; set; }
        public required int? Count { get; set; }
        public required Uri Url { get; set; }
    }
}
