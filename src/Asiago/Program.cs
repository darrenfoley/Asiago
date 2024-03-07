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
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TwitchLib.Api;
using TwitchLib.Api.Core;

var builder = Host.CreateApplicationBuilder();

builder.Logging.AddConsole();

string token = builder.Configuration.GetRequiredValue<string>("DISCORD_TOKEN");
string commandPrefix = builder.Configuration.GetRequiredValue<string>("DISCORD_COMMANDPREFIX");
string isThereAnyDealApiKey = builder.Configuration.GetRequiredValue<string>("ISTHEREANYDEAL_APIKEY");
string postgresConnectionString = builder.Configuration.GetRequiredPostgresConnectionString();
ApiSettings twitchApiSettings = new()
{
    ClientId = builder.Configuration.GetRequiredValue<string>("TWITCH_CLIENTID"),
    Secret = builder.Configuration.GetRequiredValue<string>("TWITCH_CLIENTSECRET")
};

builder.Services.AddOptions<IsThereAnyDealOptions>().Configure(options => options.ApiKey = isThereAnyDealApiKey);
builder.Services.AddSingleton<HttpClient>();
builder.Services.AddDbContextFactory<ApplicationDbContext>(options => options.UseNpgsql(postgresConnectionString));
builder.Services.AddSingleton(_ => new TwitchAPI(settings: twitchApiSettings));

var host = builder.Build();

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
    Services = host.Services,
});

slashCommands.RegisterCommands<IsThereAnyDealModule>(builder.Configuration.GetValue<ulong?>("DISCORD_GUILDID"));

// Remember: need to add DiscordIntents.MessageContents privileged intent if prefix commands need to work outside of DMs with the bot
var commands = discord.UseCommandsNext(new CommandsNextConfiguration
{
    Services = host.Services,
    StringPrefixes = new[] { commandPrefix },
});

commands.RegisterCommands<OwnerModule>();
commands.RegisterCommands<GuildConfigurationModule>();
commands.RegisterCommands<TwitchModule>();

await discord.ConnectAsync();

await host.RunAsync();
