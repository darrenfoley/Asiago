namespace IsThereAnyDeal.Models
{
    /// <summary>
    /// Result returned when looking up a game.
    /// </summary>
    public class GameLookupResult
    {
        public required bool Found { get; set; }
        public Game? Game { get; set; }
    }
}
