using Newtonsoft.Json;

namespace Asiago.Core.Twitch.EventSub.Models
{
    /// <summary>
    /// EventSub subscription data.
    /// </summary>
    public class Subscription
    {
        /// <summary>
        /// Your client ID.
        /// </summary>
        public required string Id { get; set; }
        /// <summary>
        /// The notification’s subscription type.
        /// </summary>
        public required string Type { get; set; }
        /// <summary>
        /// The version of the subscription.
        /// </summary>
        public required string Version { get; set; }
        /// <summary>
        /// The status of the subscription.
        /// </summary>
        public required string Status { get; set; }
        /// <summary>
        /// How much the subscription counts against your limit.
        /// </summary>
        public required int Cost { get; set; }
        /// <summary>
        /// Subscription-specific parameters.
        /// </summary>
        public required Dictionary<string, string> Condition { get; set; }
        /// <summary>
        /// The transport details Twitch used this notification.
        /// </summary>
        public required Transport Transport { get; set; }
        /// <summary>
        /// The time the notification was created.
        /// </summary>
        [JsonProperty("created_at")]
        public required DateTime CreatedAt { get; set; }
    }
}
