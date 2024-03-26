namespace Asiago.Core.Twitch.EventSub
{
    /// <summary>
    /// Twitch EventSub webhook request headers.
    /// </summary>
    internal static class WebhookRequestHeaders
    {
        /// <summary>
        /// An ID that uniquely identifies this message. This is an opaque ID, and is not required to be in any particular format.
        /// </summary>
        public const string MessageId = "Twitch-Eventsub-Message-Id";
        /// <summary>
        /// A counter to track the number of retries Twitch has made to send this notification. If Twitch is unsure of whether you received
        /// a notification, it’ll resend the event.
        /// </summary>
        public const string MessageRetry = "Twitch-Eventsub-Message-Retry";
        /// <summary>
        /// The type of notification.
        /// </summary>
        public const string MessageType = "Twitch-Eventsub-Message-Type";
        /// <summary>
        /// The HMAC signature which can be used to verify that Twitch sent the message.
        /// </summary>
        public const string MessageSignature = "Twitch-Eventsub-Message-Signature";
        /// <summary>
        /// The UTC date and time (in RFC3339 format) that Twitch sent the notification.
        /// </summary>
        public const string MessageTimestamp = "Twitch-Eventsub-Message-Timestamp";
        /// <summary>
        /// The subscription type you subscribed to.
        /// </summary>
        public const string SubscriptionType = "Twitch-Eventsub-Subscription-Type";
        /// <summary>
        /// The version number that identifies the definition of the subscription request. This version matches the version number that you
        /// specified in your subscription request.
        /// </summary>
        public const string SubscriptionVersion = "Twitch-Eventsub-Subscription-Version";
    }
}
