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
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("config.json", false);

var config = builder.Build();

if (config.GetRequiredSection("Discord").GetValue<string>("Token") is not string token)
{
    throw new ConfigurationException("Discord.Token must be set to a string in config.json");
}
if (config.GetRequiredSection("Discord").GetValue<string>("CommandPrefix") is not string commandPrefix)
{
    throw new ConfigurationException("Discord.CommandPrefix must be set to a string in config.json");
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

serviceCollection.AddOptions<IsThereAnyDealOptions>().Bind(config.GetRequiredSection("IsThereAnyDeal")).ValidateDataAnnotations();
serviceCollection.AddSingleton<HttpClient>()
    .AddSingleton(discord);

var serviceProvider = serviceCollection.BuildServiceProvider();

var slashCommands = discord.UseSlashCommands(new SlashCommandsConfiguration
{
    Services = serviceProvider,
});

// TODO: conditionally register commands for guild/globally based on config.json
slashCommands.RegisterCommands<IsThereAnyDealModule>();

// Remember: need to add DiscordIntents.MessageContents privileged intent if prefix commands need to work outside of DMs with the bot
var commands = discord.UseCommandsNext(new CommandsNextConfiguration
{
    Services = serviceProvider,
    StringPrefixes = new[] { commandPrefix },
});

commands.RegisterCommands<OwnerModule>();

await discord.ConnectAsync();
await Task.Delay(-1);
