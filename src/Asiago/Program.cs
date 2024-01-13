using Asiago;
using Asiago.Commands;
using Asiago.Data;
using Asiago.Data.Extensions;
using Asiago.Extensions;
using Asiago.SlashCommands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = new ConfigurationBuilder()
    .AddEnvironmentVariables();

var config = builder.Build();

string token = config.GetRequiredValue<string>("DISCORD_TOKEN");
string commandPrefix = config.GetRequiredValue<string>("DISCORD_COMMANDPREFIX");
string isThereAnyDealApiKey = config.GetRequiredValue<string>("ISTHEREANYDEAL_APIKEY");
string postgresConnectionString = config.GetRequiredPostgresConnectionString();

DiscordConfiguration discordConfig = new()
{
    Token = token,
    TokenType = TokenType.Bot,
    // TODO: narrow down needed intents
    Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
};
DiscordClient discord = new(discordConfig);

var serviceCollection = new ServiceCollection();

serviceCollection.AddOptions<IsThereAnyDealOptions>().Configure(options => options.ApiKey = isThereAnyDealApiKey);
serviceCollection.AddSingleton<HttpClient>()
    .AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(postgresConnectionString));

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
commands.RegisterCommands<GuildConfigurationModule>();

await discord.ConnectAsync();
await Task.Delay(-1);
