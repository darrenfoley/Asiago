using Asiago.Core.Twitch.EventSub;
using Asiago.Core.Twitch.EventSub.Models;
using Asiago.Data;
using Asiago.Data.Models;
using Coravel.Invocable;
using DSharpPlus;
using Microsoft.EntityFrameworkCore;
using TwitchLib.Api;

namespace Asiago.Invocables.Twitch
{
    /// <summary>
    /// An <see cref="IInvocable"/> that handles <see cref="StreamOnlineEvent"/> notifications from Twitch.
    /// </summary>
    public class StreamOnlineInvocable : IInvocable, IInvocableWithPayload<EventNotificationPayload<StreamOnlineEvent>>
    {
        public required EventNotificationPayload<StreamOnlineEvent> Payload { get; set; }

        private readonly TwitchAPI _twitchApi;
        private readonly DiscordClient _discordClient;
        private readonly ILogger<StreamOnlineInvocable> _logger;
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public StreamOnlineInvocable(
            TwitchAPI twitchApi,
            DiscordClient discordClient,
            ILoggerFactory loggerFactory,
            IDbContextFactory<ApplicationDbContext> dbContextFactory
            )
        {
            _twitchApi = twitchApi;
            _discordClient = discordClient;
            _logger = loggerFactory.CreateLogger<StreamOnlineInvocable>();
            _dbContextFactory = dbContextFactory;
        }

        public async Task Invoke()
        {
            _logger.LogInformation(
                "Processing {notificationType} notification for Twitch channel [{userId}][{userDisplayName}]",
                EventSubTypes.StreamOnline,
                Payload.Event.BroadcasterUserId,
                Payload.Event.BroadcasterUserName
                );

            TwitchChannel? twitchChannel;
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                twitchChannel = await dbContext.TwitchChannels
                    .Include(tc => tc.SubscribedGuilds)
                    .SingleOrDefaultAsync(tc => tc.UserId == Payload.Event.BroadcasterUserId);
            }

            // Just log unlikely errors and return
            if (twitchChannel == null)
            {
                _logger.LogError(
                    "Twitch channel [{userId}][{userDisplayName}] not found in database",
                    Payload.Event.BroadcasterUserId,
                    Payload.Event.BroadcasterUserName
                );
                return;
            }
            if (twitchChannel.SubscribedGuilds.Count == 0)
            {
                _logger.LogError(
                    "No guilds subscribed to Twitch channel [{userId}][{userDisplayName}] found in database",
                    Payload.Event.BroadcasterUserId,
                    Payload.Event.BroadcasterUserName
                );
                return;
            }

            var getStreamsResponse = await _twitchApi.Helix.Streams.GetStreamsAsync(userIds: [Payload.Event.BroadcasterUserId]);
            var stream = getStreamsResponse.Streams.SingleOrDefault();
            if (stream == null)
            {
                _logger.LogError(
                    "Unable to find stream info for Twitch channel [{userId}][{userDisplayName}]",
                    Payload.Event.BroadcasterUserId,
                    Payload.Event.BroadcasterUserName
                );
                return;
            }

            foreach (var guildConfig in twitchChannel.SubscribedGuilds)
            {
                var guild = await _discordClient.GetGuildAsync(guildConfig.GuildId);
                if (guildConfig.TwitchUpdateChannelId == null)
                {
                    _logger.LogWarning("Guild [{guildId}][{guildName}] has Twitch subscriptions but no Twitch update channel set", guild.Id, guild.Name);
                }
                else
                {
                    var channel = guild.GetChannel(guildConfig.TwitchUpdateChannelId.Value);
                    // TODO: use discord embed
                    await channel.SendMessageAsync($"{stream.UserName} is live on Twitch! Streaming {stream.Title}");
                }
            }
        }
    }
}
