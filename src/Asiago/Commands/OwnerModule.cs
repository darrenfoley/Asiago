﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace Asiago.Commands
{
    internal class OwnerModule : BaseCommandModule
    {
        [Command("say")]
        [RequireOwner]
        public async Task Say(CommandContext ctx, ulong channelId, [RemainingText] string message)
        {
            var channel = await ctx.Client.GetChannelAsync(channelId);

            DiscordEmbedBuilder embedBuilder = new()
            {
                Color = Colours.EmbedColour,
                Description = message,
            };

            await channel.SendMessageAsync(embedBuilder);
        }
    }
}
