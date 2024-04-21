using Asiago.Commands.Attributes;
using Asiago.Data;
using Asiago.Data.Models;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Asiago.Commands
{
    internal class GuildConfigurationModule : BaseCommandModule
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly IStringLocalizer<GuildConfigurationModule> _stringLocalizer;

        public GuildConfigurationModule(
            IDbContextFactory<ApplicationDbContext> dbContextFactory,
            IStringLocalizer<GuildConfigurationModule> stringLocalizer
            )
        {
            _dbContextFactory = dbContextFactory;
            _stringLocalizer = stringLocalizer;
        }

        [Command]
        [RequireOwner]
        [RequireGuild]
        public async Task SetAdminRole(CommandContext ctx, DiscordRole role)
        {
            int rowsAffected;

            await using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                rowsAffected = await dbContext.GuildConfigurations.Upsert(new GuildConfiguration
                {
                    GuildId = ctx.Guild.Id,
                    AdminRoleId = role.Id
                }).WhenMatched(gc => new GuildConfiguration
                {
                    AdminRoleId = role.Id
                }).RunAsync();
            }

            if (rowsAffected == 1)
            {
                await ctx.RespondAsync(_stringLocalizer["AdminRoleSet", role.Mention]);
            }
            else
            {
                await ctx.RespondAsync(_stringLocalizer["ErrorMessage"]);
            }
        }

        [Command]
        [RequireAdmin]
        public async Task SetModRole(CommandContext ctx, DiscordRole role)
        {
            int rowsAffected;

            await using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                rowsAffected = await dbContext.GuildConfigurations.Upsert(new GuildConfiguration
                {
                    GuildId = ctx.Guild.Id,
                    ModRoleId = role.Id
                }).WhenMatched(gc => new GuildConfiguration
                {
                    ModRoleId = role.Id
                }).RunAsync();
            }

            if (rowsAffected == 1)
            {
                await ctx.RespondAsync(_stringLocalizer["ModRoleSet", role.Mention]);
            }
            else
            {
                await ctx.RespondAsync(_stringLocalizer["ErrorMessage"]);
            }
        }

        [Command]
        [RequireAdmin]
        public async Task SetTwitchUpdateChannel(CommandContext ctx, DiscordChannel channel)
        {
            if (channel.Type != DiscordChannelType.Text)
            {
                await ctx.RespondAsync(_stringLocalizer["ErrorDiscordNonTextChannel", channel.Mention]);
                return;
            }

            int rowsAffected;

            await using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                rowsAffected = await dbContext.GuildConfigurations.Upsert(new GuildConfiguration
                {
                    GuildId = ctx.Guild.Id,
                    TwitchUpdateChannelId = channel.Id
                }).WhenMatched(gc => new GuildConfiguration
                {
                    TwitchUpdateChannelId = channel.Id
                }).RunAsync();
            }

            if (rowsAffected == 1)
            {
                await ctx.RespondAsync(_stringLocalizer["TwitchUpdateChannelSet", channel.Mention]);
            }
            else
            {
                await ctx.RespondAsync(_stringLocalizer["ErrorMessage"]);
            }
        }
    }
}
