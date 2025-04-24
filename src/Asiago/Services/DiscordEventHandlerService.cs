using Asiago.Data;
using Asiago.Data.Models;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using TwitchLib.Api;

namespace Asiago.Services
{
    internal class DiscordEventHandlerService
    {
        private readonly ILogger<DiscordEventHandlerService> _logger;
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly TwitchAPI _twitchApi;

        public DiscordEventHandlerService(
            ILogger<DiscordEventHandlerService> logger,
            IDbContextFactory<ApplicationDbContext> dbContextFactory,
            TwitchAPI twitchApi
            )
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _twitchApi = twitchApi;
        }

        public async Task HandleGuildDeleted(DiscordClient client, GuildDeleteEventArgs args)
        {
            // Guild is temporarily unavailable due to Discord outage or etc.
            if (args.Unavailable)
            {
                _logger.LogDebug("Guild [{guildId}][{guildName}] is temporarily unavailable.", args.Guild.Id, args.Guild.Name);
                return;
            }

            _logger.LogInformation("Guild [{guildId}][{guildName}] is deleted or bot has been removed. Cleaning up", args.Guild.Id, args.Guild.Name);

            await using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var guild = await dbContext.GuildConfigurations
                    .Include(g => g.TwitchChannels).ThenInclude(tc => tc.SubscribedGuilds)
                    .SingleOrDefaultAsync(g => g.GuildId == args.Guild.Id);

                // Only need to clean up if the guild has config
                if (guild != null)
                {
                    // Clean up Twitch channels which were subscribed to by only this guild
                    // The remaining join records between this guild and other Twitch channels will be cleaned up when the GuildConfiguration is deleted
                    HashSet<TwitchChannel> twitchChannelsToDelete = [.. guild.TwitchChannels.Where(tc => tc.SubscribedGuilds.Count == 1)];
                    foreach (var twitchChannel in twitchChannelsToDelete)
                    {
                        bool subscriptionDeleted = await _twitchApi.Helix.EventSub.DeleteEventSubSubscriptionAsync(twitchChannel.SubscriptionId);
                        if (subscriptionDeleted)
                        {
                            _logger.LogInformation(
                                "Successfully deleted Twitch EventSub subscription [{twitchSubscriptionId}] to user [{twitchUserId}] while handling GuildDeleted event for guild [{guildId}][{guildName}]",
                                twitchChannel.SubscriptionId,
                                twitchChannel.UserId,
                                args.Guild.Id,
                                args.Guild.Name
                                );
                        }
                        else
                        {
                            _logger.LogError(
                                "Failed to delete Twitch EventSub subscription [{twitchSubscriptionId}] to user [{twitchUserId}] while handling GuildDeleted event for guild [{guildId}][{guildName}]",
                                twitchChannel.SubscriptionId,
                                twitchChannel.UserId,
                                args.Guild.Id,
                                args.Guild.Name
                                );
                        }
                    }
                    dbContext.RemoveRange(twitchChannelsToDelete);

                    dbContext.Remove(guild);

                    try
                    {
                        await dbContext.SaveChangesAsync();
                    }
                    catch (DbUpdateException ex)
                    {
                        // This can happen due to a race condition due to the delay between looking if an item exists in the db
                        // and removing it if it does. This really shouldn't happen often.
                        _logger.LogError(
                            ex,
                            "Failed to persist changes to the database while handling GuildDeleted event for guild [{guildId}][{guildName}]",
                            args.Guild.Id,
                            args.Guild.Name
                            );
                    }
                }
            }
        }
    }
}
