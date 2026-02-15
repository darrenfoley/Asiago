using Asiago.Commands.Attributes;
using Asiago.Data;
using Asiago.Data.Models;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace Asiago.Commands
{
    internal class GuildConfigurationModule : BaseCommandModule
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly ILogger<GuildConfigurationModule> _logger;

        public GuildConfigurationModule(
            IDbContextFactory<ApplicationDbContext> dbContextFactory,
            ILogger<GuildConfigurationModule> logger
            )
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        [Command]
        [RequireOwner]
        [RequireGuild]
        public async Task SetAdminRole(CommandContext ctx, DiscordRole role)
        {
            await using var dbContext = _dbContextFactory.CreateDbContext();

            var guildConfiguration = await FindOrCreateGuildConfigurationAsync(dbContext, ctx.Guild.Id);
            guildConfiguration.AdminRoleId = role.Id;

            try
            {
                await dbContext.SaveChangesAsync();
                await ctx.RespondAsync($"Admin role has been set to {role.Mention}");
            }
            catch (DbUpdateException ex)
            {
                await ctx.RespondAsync("Something went wrong...");
                _logger.LogWarning(
                    ex,
                    "Failed to set admin role to [{roleId}][{roleName}] for guild [{guildId}][{guildName}]",
                    role.Id,
                    role.Name,
                    ctx.Guild.Id,
                    ctx.Guild.Name
                    );
            }
        }

        [Command]
        [RequireAdmin]
        public async Task SetModRole(CommandContext ctx, DiscordRole role)
        {
            await using var dbContext = _dbContextFactory.CreateDbContext();

            var guildConfiguration = await FindOrCreateGuildConfigurationAsync(dbContext, ctx.Guild.Id);
            guildConfiguration.ModRoleId = role.Id;

            try
            {
                await dbContext.SaveChangesAsync();
                await ctx.RespondAsync($"Mod role has been set to {role.Mention}");
            }
            catch (DbUpdateException ex)
            {
                await ctx.RespondAsync("Something went wrong...");
                _logger.LogWarning(
                    ex,
                    "Failed to set mod role to [{roleId}][{roleName}] for guild [{guildId}][{guildName}]",
                    role.Id,
                    role.Name,
                    ctx.Guild.Id,
                    ctx.Guild.Name
                    );
            }
        }

        [Command]
        [RequireAdmin]
        public async Task SetTwitchUpdateChannel(CommandContext ctx, DiscordChannel channel)
        {
            if (channel.Type != DiscordChannelType.Text)
            {
                await ctx.RespondAsync($"Cannot set twitch update channel to non-text channel {channel.Mention}");
                return;
            }

            await using var dbContext = _dbContextFactory.CreateDbContext();

            var guildConfiguration = await FindOrCreateGuildConfigurationAsync(dbContext, ctx.Guild.Id);
            guildConfiguration.TwitchUpdateChannelId = channel.Id;

            try
            {
                await dbContext.SaveChangesAsync();
                await ctx.RespondAsync($"Twitch update channel has been set to {channel.Mention}");
            }
            catch (DbUpdateException ex)
            {
                await ctx.RespondAsync("Something went wrong...");
                _logger.LogWarning(
                    ex,
                    "Failed to set twitch update channel to [{channelId}][{channelName}] for guild [{guildId}][{guildName}]",
                    channel.Id,
                    channel.Name,
                    ctx.Guild.Id,
                    ctx.Guild.Name
                    );
            }
        }

        private static async Task<GuildConfiguration> FindOrCreateGuildConfigurationAsync(ApplicationDbContext dbContext, ulong guildId)
        {
            var guildConfiguration = await dbContext.GuildConfigurations.FindAsync(guildId);
            if (guildConfiguration == null)
            {
                guildConfiguration = new GuildConfiguration
                {
                    GuildId = guildId
                };
                dbContext.GuildConfigurations.Add(guildConfiguration);
            }

            return guildConfiguration;
        }
    }
}
