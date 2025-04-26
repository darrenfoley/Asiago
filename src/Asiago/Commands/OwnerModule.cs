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
            try
            {
                var channel = await ctx.Client.GetChannelAsync(channelId);
                await channel.SendMessageAsync(message);
                await ctx.RespondAsync($"Sent message [{message}] in channel [{channelId}][{channel.Name}]");
            }
            catch (DiscordException ex)
            {
                await ctx.RespondAsync("Something went wrong...");
                _logger.LogError(ex, "Feiled to send message [{message}] in channel [{channelId}]", message, channelId);
            }
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

            int leftCount = 0;
            string response = "Left the following guilds:";
            foreach (ulong guildId in guildIds)
            {
                try
                {
                    var guild = await ctx.Client.GetGuildAsync(guildId);
                    await guild.LeaveAsync();
                    ++leftCount;
                    response += $"\n* [{guildId}][{guild.Name}]";
                }
                catch (DiscordException ex)
                {
                    await ctx.RespondAsync($"Something went wrong when trying to leave guild [{guildId}]");
                    _logger.LogError(ex, "Failed to leave Discord guild [{guildId}] from owner command 'leave'", guildId);
                }
            }

            if (leftCount > 0)
            {
                await ctx.RespondAsync(response);
            }
        }
    }
}
