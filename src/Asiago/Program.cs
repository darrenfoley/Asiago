using Asiago.Commands;
using Asiago.Core.IsThereAnyDeal;
using Asiago.Core.Twitch;
using Asiago.Core.Web;
using Asiago.Data;
using Asiago.Data.Extensions;
using Asiago.Extensions;
using Asiago.SlashCommands;
using Coravel;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;
using TwitchLib.Api;
using TwitchLib.Api.Core;

var builder = WebApplication.CreateBuilder();

builder.Logging.AddConsole();

string token = builder.Configuration.GetRequiredValue<string>("DISCORD_TOKEN");
string commandPrefix = builder.Configuration.GetRequiredValue<string>("DISCORD_COMMANDPREFIX");
Uri baseUrl = builder.Configuration.GetRequiredValue<Uri>("BASEURL");
string isThereAnyDealApiKey = builder.Configuration.GetRequiredValue<string>("ISTHEREANYDEAL_APIKEY");
string postgresConnectionString = builder.Configuration.GetRequiredPostgresConnectionString();
string twitchWebhookSecret = builder.Configuration.GetRequiredValue<string>("TWITCH_WEBHOOKSECRET");
ApiSettings twitchApiSettings = new()
{
    ClientId = builder.Configuration.GetRequiredValue<string>("TWITCH_CLIENTID"),
    Secret = builder.Configuration.GetRequiredValue<string>("TWITCH_CLIENTSECRET")
};

DiscordConfiguration discordConfig = new()
{
    Token = token,
    TokenType = TokenType.Bot,
    // TODO: narrow down needed intents
    Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
};
DiscordClient discord = new(discordConfig);

builder.Services.AddOptions<WebOptions>().Configure(options => options.BaseUrl = baseUrl);
builder.Services.AddOptions<IsThereAnyDealOptions>().Configure(options => options.ApiKey = isThereAnyDealApiKey);
builder.Services.AddOptions<TwitchOptions>().Configure(options => options.WebhookSecret = twitchWebhookSecret);
builder.Services.AddSingleton<HttpClient>();
builder.Services.AddDbContextFactory<ApplicationDbContext>(options => options.UseNpgsql(postgresConnectionString));
builder.Services.AddSingleton(_ => new TwitchAPI(settings: twitchApiSettings));
builder.Services.AddSingleton(discord);
builder.Services.AddInvocablesFromNamespace("Asiago.Invocables", typeof(Program).Assembly);
builder.Services.AddQueue();
builder.Services.AddCache();

builder.Services.AddControllers()
    .AddNewtonsoftJson();

var app = builder.Build();

app.EnableQueueLogging();
app.UseRequestBodyBuffering();
app.MapControllers();

var slashCommands = discord.UseSlashCommands(new SlashCommandsConfiguration
{
    Services = app.Services,
});

slashCommands.RegisterCommands<IsThereAnyDealModule>(builder.Configuration.GetValue<ulong?>("DISCORD_GUILDID"));

// Remember: need to add DiscordIntents.MessageContents privileged intent if prefix commands need to work outside of DMs with the bot
var commands = discord.UseCommandsNext(new CommandsNextConfiguration
{
    Services = app.Services,
    StringPrefixes = new[] { commandPrefix },
});

commands.RegisterCommands<OwnerModule>();
commands.RegisterCommands<GuildConfigurationModule>();
commands.RegisterCommands<TwitchModule>();

await discord.ConnectAsync();

await app.RunAsync();
