using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Asiago.Commands
{
    internal class OwnerModule : BaseCommandModule
    {
        [Command]
        [RequireOwner]
        public async Task Say(CommandContext ctx, ulong channelId, [RemainingText] string message)
        {
            var channel = await ctx.Client.GetChannelAsync(channelId);

            await channel.SendMessageAsync(message);
        }
    }
}
