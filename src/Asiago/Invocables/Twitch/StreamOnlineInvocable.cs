﻿using Asiago.Core.Discord;
using Asiago.Core.Twitch;
using Asiago.Core.Twitch.EventSub;
using Asiago.Core.Twitch.EventSub.Models;
using Asiago.Data;
using Asiago.Data.Models;
using Asiago.Extensions;
using Coravel.Cache.Interfaces;
using Coravel.Invocable;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TwitchLib.Api;

namespace Asiago.Invocables.Twitch
{
    /// <summary>
    /// An <see cref="IInvocable"/> that handles <see cref="StreamOnlineEvent"/> notifications from Twitch.
    /// </summary>
    internal class StreamOnlineInvocable : IInvocable, IInvocableWithPayload<EventNotificationPayload<StreamOnlineEvent>>
    {
        public required EventNotificationPayload<StreamOnlineEvent> Payload { get; set; }

        private const string _streamOnlineCacheKeyPrefix = "twitchStreamOnline-";

        private readonly TwitchAPI _twitchApi;
        private readonly DiscordClient _discordClient;
        private readonly ILogger<StreamOnlineInvocable> _logger;
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly ICache _cache;
        private readonly TwitchOptions _twitchOptions;

        public StreamOnlineInvocable(
            TwitchAPI twitchApi,
            DiscordClient discordClient,
            ILogger<StreamOnlineInvocable> logger,
            IDbContextFactory<ApplicationDbContext> dbContextFactory,
            ICache cache,
            IOptions<TwitchOptions> twitchOptions
            )
        {
            _twitchApi = twitchApi;
            _discordClient = discordClient;
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _cache = cache;
            _twitchOptions = twitchOptions.Value;
        }

        public async Task Invoke()
        {
            _logger.LogInformation(
                "Processing [{notificationType}] notification for Twitch channel [{userId}][{userDisplayName}]",
                EventSubTypes.StreamOnline,
                Payload.Event.BroadcasterUserId,
                Payload.Event.BroadcasterUserName
                );

            TwitchChannel? twitchChannel;
            await using (var dbContext = _dbContextFactory.CreateDbContext())
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

            var cacheKey = _streamOnlineCacheKeyPrefix + Payload.Event.BroadcasterUserId;
            if (await _cache.HasAsync(cacheKey))
            {
                _logger.LogInformation(
                    "Ignoring [{notificationType}] notifications for Twitch channel [{userId}][{userDisplayName}]. Cooldown is active",
                    EventSubTypes.StreamOnline,
                    Payload.Event.BroadcasterUserId,
                    Payload.Event.BroadcasterUserName
                    );
                return;
            }
            _cache.Remember(cacheKey, () => true, _twitchOptions.StreamOnlineCooldown);

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

            Uri twitchBaseUrl = new("https://twitch.tv/");
            string streamUrl = new Uri(twitchBaseUrl, stream.UserLogin).ToString();
            // 440x248 are the dimensions Twitch uses for its stream previews on their site as of now, so that seems like a good place to start
            string imageUrl = stream.ThumbnailUrl.Replace("{width}", "440").Replace("{height}", "248");
            var embedBuilder = new DiscordEmbedBuilder()
            {
                Color = Colours.EmbedColourDefault,
                Title = stream.Title,
                Url = streamUrl,
                ImageUrl = imageUrl,
            }.WithAsiagoNotificationFooter();

            var getUsersResponses = await _twitchApi.Helix.Users.GetUsersAsync([Payload.Event.BroadcasterUserId]);
            var user = getUsersResponses.Users.SingleOrDefault();
            string streamerLiveMessage = $"{stream.UserName} is live on Twitch!";
            embedBuilder.WithAuthor(streamerLiveMessage, streamUrl, user?.ProfileImageUrl);

            // If they are streaming a game, add game image and info
            if (stream.GameId != null)
            {
                var getGamesResponse = await _twitchApi.Helix.Games.GetGamesAsync([stream.GameId]);
                var game = getGamesResponse.Games.SingleOrDefault();
                if (game == null)
                {
                    _logger.LogWarning("Unable to find game info for game ID [{id}]", stream.GameId);
                }
                else
                {
                    string thumbnailUrl = game.BoxArtUrl.Replace("{width}", "128").Replace("{height}", "128");
                    embedBuilder.WithThumbnail(thumbnailUrl);
                }
            }
            if (stream.GameName != null)
            {
                embedBuilder.AddField("Streaming", stream.GameName);
            }

            foreach (var guildConfig in twitchChannel.SubscribedGuilds)
            {
                try
                {
                    var guild = await _discordClient.GetGuildAsync(guildConfig.GuildId);
                    if (guildConfig.TwitchUpdateChannelId == null)
                    {
                        _logger.LogWarning("Guild [{guildId}][{guildName}] has Twitch subscriptions but no Twitch update channel set", guild.Id, guild.Name);
                    }
                    else
                    {
                        var channel = guild.GetChannel(guildConfig.TwitchUpdateChannelId.Value);
                        await channel.SendMessageAsync(embedBuilder);
                    }
                }
                catch (DiscordException ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Unable to post [{notificationType}] notification for Twitch channel [{userId}][{userDisplayName}] in guild [{guildId}] in channel [{channelId}].",
                        EventSubTypes.StreamOnline,
                        Payload.Event.BroadcasterUserId,
                        Payload.Event.BroadcasterUserName,
                        guildConfig.GuildId,
                        guildConfig.TwitchUpdateChannelId
                        );
                }
            }
        }
    }
}
