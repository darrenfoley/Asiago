using Asiago.Commands.Attributes;
using Asiago.Data;
using Asiago.Data.Models;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.EntityFrameworkCore;
using TwitchLib.Api;

namespace Asiago.Commands
{
    [Group]
    [RequireMod]
    internal class TwitchModule : BaseCommandModule
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly TwitchAPI _twitchApi;

        public TwitchModule(IDbContextFactory<ApplicationDbContext> dbContextFactory, TwitchAPI twitchApi)
        {
            _dbContextFactory = dbContextFactory;
            _twitchApi = twitchApi;
        }

        [Command]
        public async Task Add(CommandContext ctx, string twitchChannelName)
        {
            var twitchGetUsersResponse = await _twitchApi.Helix.Users.GetUsersAsync(logins: [twitchChannelName]);
            var twitchUser = twitchGetUsersResponse.Users.FirstOrDefault();
            if (twitchUser == null)
            {
                await ctx.RespondAsync($"Unable to find twitch channel [{twitchChannelName}]!");
                return;
            }

            TwitchChannel? twitchChannel;
            bool addedChannel = false;

            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                // we either need to do the include here or enable lazy loading
                var guildConfig = await dbContext.GuildConfigurations.Include(gc => gc.TwitchChannels).SingleOrDefaultAsync(gc => gc.GuildId == ctx.Guild.Id);
                if (guildConfig == null)
                {
                    guildConfig = new GuildConfiguration { GuildId = ctx.Guild.Id };
                    dbContext.GuildConfigurations.Add(guildConfig);
                }

                if (guildConfig.TwitchChannels.Any(tc => tc.UserId == twitchUser.Id))
                {
                    await ctx.RespondAsync($"Already added twitch channel [{twitchChannelName}]!");
                    return;
                }

                twitchChannel = await dbContext.TwitchChannels.SingleOrDefaultAsync(tc => tc.UserId == twitchUser.Id);
                if (twitchChannel == null)
                {
                    twitchChannel = new TwitchChannel { UserId = twitchUser.Id };
                    dbContext.TwitchChannels.Add(twitchChannel);
                    addedChannel = true;
                }

                guildConfig.TwitchChannels.Add(twitchChannel);
                await dbContext.SaveChangesAsync();
            }

            if (addedChannel)
            {
                // update TwitchWebsocketHostedService to watch for online events from this channel
            }

            await ctx.RespondAsync($"Added twitch channel [{twitchChannelName}] with id [{twitchChannel.UserId}]!");
        }

        [Command]
        public async Task Remove(CommandContext ctx, string twitchChannelName)
        {
            var twitchGetUsersResponse = await _twitchApi.Helix.Users.GetUsersAsync(logins: [twitchChannelName]);
            var twitchUser = twitchGetUsersResponse.Users.FirstOrDefault();
            if (twitchUser == null)
            {
                await ctx.RespondAsync($"Unable to find a twitch channel named [{twitchChannelName}]!");
                return;
            }

            bool deletedChannel = false;
            bool removedSubscription = false;

            using (var dbContext = _dbContextFactory.CreateDbContext())
            {

                var twitchChannel = await dbContext.TwitchChannels
                    .Include(tc => tc.SubscribedGuilds)
                    .SingleOrDefaultAsync(tc => tc.UserId == twitchUser.Id);

                if (twitchChannel != null)
                {
                    var guild = twitchChannel.SubscribedGuilds.FirstOrDefault(g => g.GuildId == ctx.Guild.Id);
                    if (guild != null)
                    {
                        // If only the current guild subscribes to this twitch channel, then just delete the twitch channel and let things cascade
                        if (twitchChannel.SubscribedGuilds.Count == 1)
                        {
                            dbContext.Remove(twitchChannel);
                            deletedChannel = true;
                        }
                        else
                        {
                            twitchChannel.SubscribedGuilds.Remove(guild);
                        }
                        await dbContext.SaveChangesAsync();
                        removedSubscription = true;
                    }
                }
            }

            if (deletedChannel)
            {
                // update TwitchWebsocketHostedService to stop watching for online events from this channel
            }

            if (removedSubscription)
            {
                await ctx.RespondAsync($"Removed twitch channel [{twitchChannelName}] subscription for this guild");
            }
            else
            {
                await ctx.RespondAsync($"Couldn't find twitch channel [{twitchChannelName}] subscription for this guild");
            }
        }

        [Command]
        public async Task List(CommandContext ctx)
        {
            List<string> twitchUserIds;
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                twitchUserIds = await dbContext.TwitchChannels
                    .Where(tc => tc.SubscribedGuilds.Any(g => g.GuildId == ctx.Guild.Id))
                    .Select(tc => tc.UserId)
                    .ToListAsync();
            }

            if (twitchUserIds.Count > 0)
            {
                var twitchGetUsersResponse = await _twitchApi.Helix.Users.GetUsersAsync(twitchUserIds);
                if (twitchGetUsersResponse.Users.Count() > 0)
                {
                    string message = "This guild is subscribed to the following twitch channels:";
                    foreach (var twitchUser in twitchGetUsersResponse.Users)
                    {
                        message += $"\n* {twitchUser.DisplayName}";
                    }
                    await ctx.RespondAsync(message);
                    return;
                }
            }

            await ctx.RespondAsync("This guild is not subscribed to any twitch channels");
        }

    }
}
