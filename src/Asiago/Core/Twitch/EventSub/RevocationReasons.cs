namespace Asiago.Core.Twitch.EventSub
{
    /// <summary>
    /// Possible reasons for Twitch to revoke a subscription.
    /// </summary>
    internal static class RevocationReasons
    {
        /// <summary>
        /// The user mentioned in the subscription no longer exists.
        /// </summary>
        public static string UserRemoved => "user_removed";
        /// <summary>
        /// The user revoked the authorization token or simply changed their password.
        /// </summary>
        public static string AuthorizationRevoked => "authorization_revoked";
        /// <summary>
        /// The callback failed to respond in a timely manner too many times.
        /// </summary>
        public static string NotificationFailuresExceeded => "notification_failures_exceeded";
        /// <summary>
        /// The subscribed to subscription type and version is no longer supported.
        /// </summary>
        public static string VersionRemoved => "version_removed";

    }
}
