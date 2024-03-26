using Asiago.Core.JsonConverters;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Asiago.Core.Twitch.EventSub.Models
{
    /// <summary>
    /// Event for when a stream goes online.
    /// </summary>
    public class StreamOnlineEvent
    {
        /// <summary>
        /// The id of the stream.
        /// </summary>
        public required string Id { get; set; }
        /// <summary>
        /// The broadcaster’s user id.
        /// </summary>
        [JsonProperty("broadcaster_user_id")]
        public required string BroadcasterUserId { get; set; }
        /// <summary>
        /// The broadcaster’s user login.
        /// </summary>
        [JsonProperty("broadcaster_user_login")]
        public required string BroadcasterUserLogin { get; set; }
        /// <summary>
        /// The broadcaster’s user display name.
        /// </summary>
        [JsonProperty("broadcaster_user_name")]
        public required string BroadcasterUserName { get; set; }
        /// <summary>
        /// The stream type.
        /// </summary>
        [JsonConverter(typeof(StrictStringEnumConverter))]
        public required StreamType Type { get; set; }
        /// <summary>
        /// The timestamp at which the stream went online at.
        /// </summary>
        [JsonProperty("started_at")]
        public required DateTime StartedAt;

        /// <summary>
        /// Possible stream types.
        /// </summary>
        public enum StreamType
        {
            Live,
            Playlist,
            [EnumMember(Value = "watch_party")]
            WatchParty,
            Premiere,
            Rerun
        }
    }
}
