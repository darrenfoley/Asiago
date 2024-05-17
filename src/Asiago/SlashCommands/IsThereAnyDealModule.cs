using Asiago.Core.Discord;
using Asiago.Core.IsThereAnyDeal;
using Asiago.Core.IsThereAnyDeal.Models;
using Asiago.Extensions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Options;

namespace Asiago.SlashCommands
{
    internal class IsThereAnyDealModule : ApplicationCommandModule
    {
        private readonly IsThereAnyDealOptions _itadOptions;
        private readonly HttpClient _httpClient;

        public IsThereAnyDealModule(IOptions<IsThereAnyDealOptions> itadOptions, HttpClient httpClient)
        {
            _itadOptions = itadOptions.Value;
            _httpClient = httpClient;
        }

        [SlashCommand("gamedeals", "Get game deals")]
        public async Task GameDeals(
            InteractionContext ctx,
            [Option("title", "The title of the game"), Autocomplete(typeof(GameTitleAutocompleteProvider))] string title,
            [Option("country", "The country for which you want deals")] Country country
            )
        {
            await ctx.DeferAsync();

            IsThereAnyDealClient itadClient = new(_itadOptions.ApiKey, _httpClient);
            Game? game = await itadClient.LookupGameAsync(title);
            if (game is null)
            {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder { Content = $"Unable to find game [{title}]" });
                return;
            }

            Task<GameInfo?> gameInfoTask = itadClient.GetGameInfoAsync(game.Id);
            Task<PriceOverview?> priceOverviewTask = itadClient.GetGamePriceOverviewAsync(game.Id, country.ToString());

            GameInfo? gameInfo = await gameInfoTask;
            PriceOverview? priceOverview = await priceOverviewTask;

            if (gameInfo is null || priceOverview is null || priceOverview.Current is null)
            {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder { Content = $"Deal info unavailable for [{title}]" });
                return;
            }

            DiscordEmbedBuilder embedBuilder = new()
            {
                Color = Colours.EmbedColourDefault,
                Title = gameInfo.Title,
                Url = gameInfo.Urls.Game.ToString(),
            };

            string bestPriceString = GetCurrentBestPriceString(priceOverview.Current, country);
            embedBuilder.AddField("Current Best Price", bestPriceString);

            if (priceOverview.Lowest is not null)
            {
                string historicalPriceString = GetHistoricalLowestPriceString(priceOverview.Lowest, country);
                embedBuilder.AddField("Lowest Historical Price", historicalPriceString);
            }

            Review? steamReview = gameInfo.Reviews
                .SingleOrDefault(r => string.Equals(r.Source, "steam", StringComparison.OrdinalIgnoreCase));
            if (steamReview?.Score != null)
            {
                string countString = steamReview.Count != null ? " of " + steamReview.Count : "";
                string formattedSteamReview = $"[{steamReview.Score}%{countString}]({steamReview.Url})";
                embedBuilder.AddField("Steam Review", formattedSteamReview);
            }

            if (gameInfo.Tags.Count != 0)
            {
                string tags = string.Join(", ", gameInfo.Tags);
                embedBuilder.AddField("Tags", tags);
            }

            // Pick the best image url available
            Uri? imageUrl = gameInfo.Assets.Banner600
                ?? gameInfo.Assets.Banner400
                ?? gameInfo.Assets.Banner300
                ?? gameInfo.Assets.BoxArt
                ?? gameInfo.Assets.Banner145;
            if (imageUrl != null)
            {
                embedBuilder.WithImageUrl(imageUrl);
            }

            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(embedBuilder));
        }

        private static string GetHistoricalLowestPriceString(HistoricalLow historicalLow, Country country)
        {
            string historicalPriceCutString = historicalLow.Cut > 0 ? $" (-{historicalLow.Cut}%)" : "";

            string historicalPriceString = historicalLow.Price.Amount.ToFormattedPrice(country.ToString()) + historicalPriceCutString
                + " on " + historicalLow.Shop.Name
                + "\n" + Formatter.Timestamp(historicalLow.Timestamp);

            return historicalPriceString;
        }

        private static string GetCurrentBestPriceString(OverviewDeal currentDealInfo, Country country)
        {
            string priceCutString = currentDealInfo.Cut > 0 ? $" (-{currentDealInfo.Cut}%)" : "";
            string expiryString = currentDealInfo.Expiry != null ? $"\nExpires {Formatter.Timestamp(currentDealInfo.Expiry.Value)}" : "";

            string bestPriceString = currentDealInfo.Price.Amount.ToFormattedPrice(country.ToString()) + priceCutString
                + $" on [{currentDealInfo.Shop.Name}]({currentDealInfo.Url})"
                + expiryString
                + "\nRegular " + currentDealInfo.Regular.Amount.ToFormattedPrice(country.ToString());

            return bestPriceString;
        }
    }

    internal enum Country
    {
        [ChoiceName("Canada")]
        CA,
        [ChoiceName("USA")]
        US,
    }

    internal class GameTitleAutocompleteProvider : IAutocompleteProvider
    {
        private readonly IsThereAnyDealOptions _itadOptions;
        private readonly HttpClient _httpClient;

        public GameTitleAutocompleteProvider(IOptions<IsThereAnyDealOptions> itadOptions, HttpClient httpClient)
        {
            _itadOptions = itadOptions.Value;
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
        {
            var value = ctx.FocusedOption.Value.ToString();
            if (!string.IsNullOrWhiteSpace(value))
            {
                IsThereAnyDealClient itadClient = new(_itadOptions.ApiKey, _httpClient);
                List<Game>? games = await itadClient.SearchGamesAsync(value, 25);
                if (games is not null)
                {
                    return games.ConvertAll(game => new DiscordAutoCompleteChoice(game.Title, game.Title));
                }
            }
            return [];
        }
    }
}
