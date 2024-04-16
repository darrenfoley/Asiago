using DSharpPlus.Entities;

namespace Asiago.Extensions
{
    /// <summary>
    /// Extension methods on <see cref="DiscordEmbedBuilder"/>.
    /// </summary>
    internal static class DiscordEmbedBuilderExtensions
    {
        /// <summary>
        /// Sets the embed's footer for Asiago Notifications.
        /// </summary>
        public static DiscordEmbedBuilder WithAsiagoNotificationFooter(this DiscordEmbedBuilder embedBuilder)
            => embedBuilder.WithFooter("Asiago Notifications").WithTimestamp(DateTimeOffset.Now);
    }
}
