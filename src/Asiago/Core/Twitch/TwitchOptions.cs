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
        /// <summary>
        /// The amount of time which must pass after a stream.online event before new events will be processed for that broadcaster.
        /// </summary>
        /// <remarks>
        /// Intended to reduce spam.
        /// </remarks>
        public TimeSpan StreamOnlineCooldown { get; set; } = TimeSpan.FromMinutes(60);
    }
}
