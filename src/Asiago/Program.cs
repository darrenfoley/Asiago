using Asiago;
using Asiago.Exceptions;
using Asiago.SlashCommands;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("config.json", false);

var config = builder.Build();

var serviceCollection = new ServiceCollection();

serviceCollection.AddOptions<IsThereAnyDealOptions>().Bind(config.GetRequiredSection("IsThereAnyDeal")).ValidateDataAnnotations();
serviceCollection.AddSingleton<HttpClient>();

var serviceProvider = serviceCollection.BuildServiceProvider();

if (config.GetRequiredSection("Discord").GetValue<string>("Token") is not string token)
{
    throw new ConfigurationException("Discord token must be set to a string in config.json");
}

DiscordConfiguration discordConfig = new()
{
    Token = token,
    TokenType = TokenType.Bot,
    // TODO: narrow down needed intents
    Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
};

DiscordClient discord = new(discordConfig);

var slashCommands = discord.UseSlashCommands(new SlashCommandsConfiguration
{
    Services = serviceProvider,
});

// TODO: conditionally register commands for guild/globally based on config.json
slashCommands.RegisterCommands<IsThereAnyDealModule>();

await discord.ConnectAsync();
await Task.Delay(-1);
