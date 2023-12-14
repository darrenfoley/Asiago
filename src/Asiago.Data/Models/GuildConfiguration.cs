namespace Asiago.Data.Models
{
    public class GuildConfiguration
    {
        public ulong GuildId { get; set; }
        public ulong? AdminRoleId { get; set; }
        public ulong? ModRoleId { get; set; }
        public ulong? TwitchUpdateChannelId { get; set; }
    }
}
