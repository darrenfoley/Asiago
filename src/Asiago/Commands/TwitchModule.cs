﻿using Asiago.Commands.Attributes;
using Asiago.Core.Twitch;
using Asiago.Core.Twitch.EventSub;
using Asiago.Core.Web;
using Asiago.Data;
using Asiago.Data.Models;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TwitchLib.Api;
using TwitchLib.Api.Core.Enums;

namespace Asiago.Commands
{
    [Group, RequireMod]
    internal class TwitchModule : BaseCommandModule
    {
        private readonly ILogger<TwitchModule> _logger;
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly TwitchAPI _twitchApi;
        private readonly TwitchOptions _twitchOptions;
        private readonly WebOptions _webOptions;

        public TwitchModule(
            ILogger<TwitchModule> logger,
            IDbContextFactory<ApplicationDbContext> dbContextFactory,
            TwitchAPI twitchApi,
            IOptions<TwitchOptions> twitchOptions,
            IOptions<WebOptions> webOptions
            )
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _twitchApi = twitchApi;
            _twitchOptions = twitchOptions.Value;
            _webOptions = webOptions.Value;
        }

        [Command]
        public async Task Add(CommandContext ctx, string twitchChannelName)
        {
            var twitchGetUsersResponse = await _twitchApi.Helix.Users.GetUsersAsync(logins: [twitchChannelName]);
            var twitchUser = twitchGetUsersResponse.Users.FirstOrDefault();
            if (twitchUser == null)
            {
                await ctx.RespondAsync($"Unable to find Twitch channel [{twitchChannelName}]");
                return;
            }

            await using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var guildConfig = await dbContext.GuildConfigurations
                    .Include(gc => gc.TwitchChannels)
                    .SingleOrDefaultAsync(gc => gc.GuildId == ctx.Guild.Id);
                if (guildConfig == null)
                {
                    guildConfig = new GuildConfiguration { GuildId = ctx.Guild.Id };
                    dbContext.GuildConfigurations.Add(guildConfig);
                }

                if (guildConfig.TwitchChannels.Any(tc => tc.UserId == twitchUser.Id))
                {
                    await ctx.RespondAsync($"Already added Twitch channel [{twitchChannelName}]");
                    return;
                }

                var twitchChannel = await dbContext.TwitchChannels.SingleOrDefaultAsync(tc => tc.UserId == twitchUser.Id);
                if (twitchChannel == null)
                {
                    var createSubscriptionResponse = await _twitchApi.Helix.EventSub.CreateEventSubSubscriptionAsync(
                        EventSubTypes.StreamOnline,
                        EventSubTypes.Versions.StreamOnlineVersion,
                        Utilities.GenerateStreamOnlineCondition(twitchUser.Id),
                        EventSubTransportMethod.Webhook,
                        webhookCallback: Utilities.GenerateWebhookCallbackUrl(_webOptions.BaseUrl),
                        webhookSecret: _twitchOptions.WebhookSecret
                        );
                    var twitchSubscription = createSubscriptionResponse.Subscriptions.FirstOrDefault();
                    if (twitchSubscription == null)
                    {
                        await ctx.RespondAsync("Something went wrong...");
                        _logger.LogError(
                            "Call to Twitch API to create EventSub subscription to Twitch channel [{twitchUserId}][{twitchChannelName}] " +
                            "for guild [{guildId}][{guildName}] failed",
                            twitchUser.Id,
                            twitchChannelName,
                            ctx.Guild.Id,
                            ctx.Guild.Name
                            );
                        return;
                    }
                    twitchChannel = new TwitchChannel
                    {
                        UserId = twitchUser.Id,
                        SubscriptionId = twitchSubscription.Id
                    };
                    dbContext.TwitchChannels.Add(twitchChannel);
                }

                guildConfig.TwitchChannels.Add(twitchChannel);
                try
                {
                    await dbContext.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    // This can happen due to a race condition due to the delay between looking if an item exists in the db and adding it
                    // if not. This really shouldn't happen often.
                    await ctx.RespondAsync("Something went wrong...");
                    _logger.LogWarning(
                        ex,
                        "Failed to add subscription to Twitch channel [{twitchUserId}][{twitchChannelName}] for guild [{guildId}][{guildName}]",
                        twitchUser.Id,
                        twitchChannelName,
                        ctx.Guild.Id,
                        ctx.Guild.Name
                        );
                    return;
                }
            }

            await ctx.RespondAsync($"Added Twitch channel [{twitchChannelName}]");
        }

        [Command]
        public async Task Remove(CommandContext ctx, string twitchChannelName)
        {
            var twitchGetUsersResponse = await _twitchApi.Helix.Users.GetUsersAsync(logins: [twitchChannelName]);
            var twitchUser = twitchGetUsersResponse.Users.FirstOrDefault();
            if (twitchUser == null)
            {
                await ctx.RespondAsync($"Unable to find a Twitch channel named [{twitchChannelName}]");
                return;
            }

            await using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var twitchChannel = await dbContext.TwitchChannels
                    .Include(tc => tc.SubscribedGuilds)
                    .SingleOrDefaultAsync(tc => tc.UserId == twitchUser.Id);

                if (twitchChannel != null)
                {
                    var guild = twitchChannel.SubscribedGuilds.FirstOrDefault(g => g.GuildId == ctx.Guild.Id);
                    if (guild != null)
                    {
                        // If only the current guild subscribes to this twitch channel,
                        // then just delete the twitch channel and let things cascade
                        if (twitchChannel.SubscribedGuilds.Count == 1)
                        {
                            bool subscriptionDeleted = await _twitchApi.Helix.EventSub.DeleteEventSubSubscriptionAsync(twitchChannel.SubscriptionId);
                            if (!subscriptionDeleted)
                            {
                                await ctx.RespondAsync("Something went wrong...");
                                return;
                            }
                            dbContext.Remove(twitchChannel);
                        }
                        else
                        {
                            twitchChannel.SubscribedGuilds.Remove(guild);
                        }
                        try
                        {
                            await dbContext.SaveChangesAsync();
                            await ctx.RespondAsync($"Removed Twitch channel [{twitchChannelName}] subscription for this guild");
                            return;
                        }
                        catch (DbUpdateException ex)
                        {
                            // This can happen due to a race condition due to the delay between looking if an item exists in the db
                            // and removing it if it does. This really shouldn't happen often.
                            await ctx.RespondAsync("Something went wrong...");
                            _logger.LogWarning(
                                ex,
                                "Failed to remove subscription to Twitch channel [{twitchUserId}][{twitchChannelName}] for guild [{guildId}][{guildName}]",
                                twitchUser.Id,
                                twitchChannelName,
                                ctx.Guild.Id,
                                ctx.Guild.Name
                                );
                            return;
                        }
                    }
                }
            }

            await ctx.RespondAsync($"Not subscribed to Twitch channel [{twitchChannelName}] in this guild");
        }

        [Command]
        public async Task List(CommandContext ctx)
        {
            List<string> twitchUserIds;
            await using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                twitchUserIds = await dbContext.TwitchChannels
                    .Where(tc => tc.SubscribedGuilds.Any(g => g.GuildId == ctx.Guild.Id))
                    .Select(tc => tc.UserId)
                    .ToListAsync();
            }

            if (twitchUserIds.Count == 0)
            {
                await ctx.RespondAsync("This guild is not subscribed to any Twitch channels");
                return;
            }

            var twitchGetUsersResponse = await _twitchApi.Helix.Users.GetUsersAsync(twitchUserIds);
            if (twitchGetUsersResponse.Users.Length == 0)
            {
                await ctx.RespondAsync("Something went wrong...");
                _logger.LogError("Failed to look up Twitch users for guild [{guildId}][{guildName}]", ctx.Guild.Id, ctx.Guild.Name);
                return;
            }

            // TODO: Think about using interactivity with paging for this
            string message = "This guild is subscribed to the following Twitch channels:";
            foreach (var twitchUser in twitchGetUsersResponse.Users)
            {
                message += $"\n* {twitchUser.DisplayName}";
            }
            await ctx.RespondAsync(message);
        }
    }
}
