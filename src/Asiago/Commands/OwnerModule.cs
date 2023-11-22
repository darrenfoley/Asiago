using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace Asiago.Commands
{
    internal class OwnerModule : BaseCommandModule
    {
        // Dependency-injected members
        public DiscordClient Discord { private get; set; } = null!;

        [Command("say")]
        [RequireOwner]
        public async Task Say(CommandContext _, ulong channelId, [RemainingText] string message)
        {
            var channel = await Discord.GetChannelAsync(channelId);

            DiscordEmbedBuilder embedBuilder = new()
            {
                Color = Colours.EmbedColour,
                Description = message,
            };

            await channel.SendMessageAsync(embedBuilder);
        }
    }
}
