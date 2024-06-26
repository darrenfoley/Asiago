﻿namespace Asiago.Data.Models
{
    public class TwitchChannel
    {
        public required string UserId { get; set; }
        public required string SubscriptionId { get; set; }
        public List<GuildConfiguration> SubscribedGuilds { get; set; } = [];
    }
}
