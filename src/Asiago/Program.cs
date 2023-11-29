using Asiago;
using Asiago.Commands;
using Asiago.Exceptions;
using Asiago.SlashCommands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = new ConfigurationBuilder()
    .AddEnvironmentVariables();

var config = builder.Build();

if (config.GetValue<string>("DISCORD_TOKEN") is not string token)
{
    throw new ConfigurationException("The environment variable DISCORD_TOKEN must be set");
}
if (config.GetValue<string>("DISCORD_COMMANDPREFIX") is not string commandPrefix)
{
    throw new ConfigurationException("The environment variable DISCORD_COMMANDPREFIX must be set");
}
if (config.GetValue<string>("ISTHEREANYDEAL_APIKEY") is not string isThereAnyDealApiKey)
{
    throw new ConfigurationException("The environment variable ISTHEREANYDEAL_APIKEY must be set");
}

DiscordConfiguration discordConfig = new()
{
    Token = token,
    TokenType = TokenType.Bot,
    // TODO: narrow down needed intents
    Intents = DiscordIntents.AllUnprivileged
};
DiscordClient discord = new(discordConfig);

var serviceCollection = new ServiceCollection();

serviceCollection.AddOptions<IsThereAnyDealOptions>().Configure(options => options.ApiKey = isThereAnyDealApiKey);
serviceCollection.AddSingleton<HttpClient>()
    .AddSingleton(discord);

var serviceProvider = serviceCollection.BuildServiceProvider();

var slashCommands = discord.UseSlashCommands(new SlashCommandsConfiguration
{
    Services = serviceProvider,
});

slashCommands.RegisterCommands<IsThereAnyDealModule>(config.GetValue<ulong?>("DISCORD_GUILDID"));

// Remember: need to add DiscordIntents.MessageContents privileged intent if prefix commands need to work outside of DMs with the bot
var commands = discord.UseCommandsNext(new CommandsNextConfiguration
{
    Services = serviceProvider,
    StringPrefixes = new[] { commandPrefix },
});

commands.RegisterCommands<OwnerModule>();

await discord.ConnectAsync();
await Task.Delay(-1);
