using Asiago.Core.Twitch.EventSub.Models;

namespace Asiago.Core.Twitch.EventSub
{
    /// <summary>
    /// Possible values for the <see cref="WebhookRequestHeaders.MessageType"/> request header.
    /// </summary>
    /// <remarks>
    /// Used to differentiate the type of notification payload.
    /// </remarks>
    internal static class WebhookRequestMessageTypes
    {
        /// <summary>
        /// The notification payload contains event data.
        /// </summary>
        /// <remarks>
        /// See <see cref="EventNotificationPayload{T}"/>.
        /// </remarks>
        public const string Notification = "notification";
        /// <summary>
        /// The notification payload contains a challenge request used to prove that you own the event handler.
        /// </summary>
        /// <remarks>
        /// See <see cref="VerificationNotificationPayload"/>.
        /// </remarks>
        public const string WebhookCallbackVerification = "webhook_callback_verification";
        /// <summary>
        /// The notification payload contains the reason why Twitch revoked your subscription.
        /// </summary>
        /// <remarks>
        /// See <see cref="RevocationNotificationPayload"/>.
        /// </remarks>
        public const string Revocation = "revocation";
    }
}
