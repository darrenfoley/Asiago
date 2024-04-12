namespace Asiago.Core.Twitch.EventSub
{
    /// <summary>
    /// EventSub types.
    /// </summary>
    internal static class EventSubTypes
    {
        /// <summary>
        /// The specified broadcaster starts a stream.
        /// </summary>
        public const string StreamOnline = "stream.online";
        /// <summary>
        /// The version number that identifies the desired definition of the stream online subscription type to use.
        /// </summary>
        public const string StreamOnlineVersion = "1";
    }
}
