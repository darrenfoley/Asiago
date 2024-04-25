using Asiago.Data;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Asiago.Commands.Attributes
{
    /// <summary>
    /// Checks that the member who triggered the command has the guild's admin role or is the guild owner or bot owner.
    /// The command must be executed from a guild.
    /// </summary>
    internal class RequireAdminAttribute : CheckBaseAttribute
    {
        public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            // Command must be executed from a guild
            if (ctx.Guild is null || ctx.Member is null)
            {
                return false;
            }

            // Check if current user is the guild owner or the bot owner
            RequireOwnerAttribute requireOwnerAttr = new();
            if (ctx.Member.IsOwner || await requireOwnerAttr.ExecuteCheckAsync(ctx, help))
            {
                return true;
            }

            ApplicationDbContext dbContext = ctx.Services.GetRequiredService<ApplicationDbContext>();
            var guildConfiguration = await dbContext.GuildConfigurations.SingleOrDefaultAsync(gc => gc.GuildId == ctx.Guild.Id);
            if (guildConfiguration is null)
            {
                return false;
            }

            return ctx.Member.Roles.Any(r => r.Id == guildConfiguration.AdminRoleId);
        }
    }

    /// <summary>
    /// Checks that the member who triggered the command has the guild's mod role, admin role, or is the guild owner or bot owner.
    /// The command must be executed from a guild.
    /// </summary>
    internal class RequireModAttribute : CheckBaseAttribute
    {
        public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            // Command must be executed from a guild
            if (ctx.Guild is null || ctx.Member is null)
            {
                return false;
            }

            // Check if current user is the guild owner or the bot owner
            RequireOwnerAttribute requireOwnerAttr = new();
            if (ctx.Member.IsOwner || await requireOwnerAttr.ExecuteCheckAsync(ctx, help))
            {
                return true;
            }

            ApplicationDbContext dbContext = ctx.Services.GetRequiredService<ApplicationDbContext>();
            var guildConfiguration = await dbContext.GuildConfigurations.SingleOrDefaultAsync(gc => gc.GuildId == ctx.Guild.Id);
            if (guildConfiguration is null)
            {
                return false;
            }

            return ctx.Member.Roles.Any(r => r.Id == guildConfiguration.ModRoleId || r.Id == guildConfiguration.AdminRoleId);
        }
    }
}
