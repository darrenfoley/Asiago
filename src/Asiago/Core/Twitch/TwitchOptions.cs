namespace Asiago.Core.Twitch
{
    /// <summary>
    /// Twitch options.
    /// </summary>
    public class TwitchOptions
    {
        /// <summary>
        /// The secret used to verify the signature of Twitch EventSub webhook notifications.
        /// </summary>
        /// <remarks>
        /// The secret must be an ASCII string that’s a minimum of 10 characters long and a maximum of 100 characters long.
        /// </remarks>
        public required string WebhookSecret { get; set; }
    }
}
