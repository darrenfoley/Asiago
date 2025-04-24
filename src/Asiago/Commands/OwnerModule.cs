using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Exceptions;

namespace Asiago.Commands
{
    [RequireOwner]
    internal class OwnerModule : BaseCommandModule
    {
        private readonly ILogger<OwnerModule> _logger;

        public OwnerModule(ILogger<OwnerModule> logger)
        {
            _logger = logger;
        }

        [Command]
        public async Task Say(CommandContext ctx, ulong channelId, [RemainingText] string message)
        {
            var channel = await ctx.Client.GetChannelAsync(channelId);

            await channel.SendMessageAsync(message);
        }

        [Command]
        [RequireDirectMessage]
        public async Task Leave(CommandContext ctx, params ulong[] guildIds)
        {
            if (guildIds.Length == 0)
            {
                await ctx.RespondAsync("You must provide at least 1 guild");
                return;
            }

            foreach (ulong guildId in guildIds)
            {
                try
                {
                    var guild = await ctx.Client.GetGuildAsync(guildId);
                    await guild.LeaveAsync();
                }
                catch (DiscordException ex)
                {
                    await ctx.RespondAsync($"Something went wrong when trying to leave guild [{guildId}]");
                    _logger.LogError(ex, "Failed to leave Discord guild [{guildId}] from owner command 'leave'", guildId);
                }
            }
        }
    }
}
