namespace Asiago.Core.Twitch.EventSub.Models
{
    /// <summary>
    /// Base class for notification payloads.
    /// </summary>
    public abstract class NotificationPayloadBase
    {
        /// <summary>
        /// Information about the EventSub subscription.
        /// </summary>
        public required Subscription Subscription { get; set; }
    }

    /// <summary>
    /// Payload sent by Twitch when an event you have subscribed to occurs.
    /// </summary>
    /// <typeparam name="T">The event type</typeparam>
    public class EventNotificationPayload<T> : NotificationPayloadBase
    {
        /// <summary>
        /// The event's data.
        /// </summary>
        public required T Event { get; set; }
    }

    /// <summary>
    /// Payload sent by Twitch as a challenge request to make sure you own the event handler specified when creating a subscription.
    /// </summary>
    public class VerificationNotificationPayload : NotificationPayloadBase
    {
        /// <summary>
        /// The challenge value you must respond with.
        /// </summary>
        public required string Challenge { get; set; }
    }

    /// <summary>
    /// Payload sent by Twitch when it revokes a subscription.
    /// </summary>
    public class RevocationNotificationPayload : NotificationPayloadBase
    {
        /// <summary>
        /// The reason why Twitch revoked the subscription.
        /// </summary>
        public string Reason
        {
            get
            {
                return Subscription.Status;
            }
        }
    }
}
