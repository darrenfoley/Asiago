namespace IsThereAnyDeal.Models
{
    public class GameReview
    {
        public int PercentPositive { get; }
        public int TotalVotes { get; }
        public string Text { get; }

        public GameReview(int percentPositive, int totalVotes, string text)
        {
            PercentPositive = percentPositive;
            TotalVotes = totalVotes;
            Text = text;
        }
    }
}