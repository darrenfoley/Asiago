namespace Asiago.Core.Twitch.EventSub.Models
{
    /// <summary>
    /// Information about EventSub notification transport details.
    /// </summary>
    public class Transport
    {
        /// <summary>
        /// The transport method. Possible values are webhook and websocket.
        /// </summary>
        public required string Method { get; set; }
        /// <summary>
        /// The callback URL where the notifications are being sent.
        /// This is only specified if method is set to webhook, which is always the case for us.
        /// </summary>
        public required string Callback { get; set; }
    }
}
