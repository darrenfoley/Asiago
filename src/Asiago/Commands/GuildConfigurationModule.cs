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

        public GuildConfigurationModule(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        [Command]
        [RequireOwner]
        public async Task SetAdminRole(CommandContext ctx, DiscordRole role)
        {
            int rowsAffected;

            using (var dbContext = _dbContextFactory.CreateDbContext())
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

            DiscordEmbedBuilder embedBuilder = new();
            if (rowsAffected == 1)
            {
                embedBuilder.Color = Colours.EmbedColourDefault;
                embedBuilder.Description = $"Admin role has been set to {role.Mention}";
            }
            else
            {
                embedBuilder.Color = Colours.EmbedColourError;
                embedBuilder.Description = "Something went wrong!";
            }

            await ctx.RespondAsync(embedBuilder);
        }

        [Command]
        [RequireAdmin]
        public async Task SetModRole(CommandContext ctx, DiscordRole role)
        {
            int rowsAffected;

            using (var dbContext = _dbContextFactory.CreateDbContext())
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

            DiscordEmbedBuilder embedBuilder = new();
            if (rowsAffected == 1)
            {
                embedBuilder.Color = Colours.EmbedColourDefault;
                embedBuilder.Description = $"Mod role has been set to {role.Mention}";
            }
            else
            {
                embedBuilder.Color = Colours.EmbedColourError;
                embedBuilder.Description = "Something went wrong!";
            }

            await ctx.RespondAsync(embedBuilder);
        }

        [Command]
        [RequireAdmin]
        public async Task SetTwitchUpdateChannel(CommandContext ctx, DiscordChannel channel)
        {
            DiscordEmbedBuilder embedBuilder = new();

            if (channel.Type != DSharpPlus.ChannelType.Text)
            {
                embedBuilder.Color = Colours.EmbedColourError;
                embedBuilder.Description = $"Cannot set twitch update channel to non-text channel {channel.Mention}";
                await ctx.RespondAsync(embedBuilder);
                return;
            }

            int rowsAffected;

            using (var dbContext = _dbContextFactory.CreateDbContext())
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
                embedBuilder.Color = Colours.EmbedColourDefault;
                embedBuilder.Description = $"Twitch update channel has been set to {channel.Mention}";
            }
            else
            {
                embedBuilder.Color = Colours.EmbedColourError;
                embedBuilder.Description = "Something went wrong!";
            }

            await ctx.RespondAsync(embedBuilder);
        }
    }
}
