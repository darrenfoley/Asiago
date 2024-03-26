using Asiago.Core.Discord;
using Asiago.Core.IsThereAnyDeal;
using Asiago.Extensions;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using IsThereAnyDeal;
using IsThereAnyDeal.Models;
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
        public async Task GetDeals(
            InteractionContext ctx,
            [Option("title", "The title of the game"), Autocomplete(typeof(GameTitleAutocompleteProvider))] string title,
            [Option("country", "The country for which you want deals")] Country country
            )
        {
            await ctx.DeferAsync();

            IsThereAnyDealClient itadClient = new(_itadOptions.ApiKey, _httpClient);
            string? gameId = await itadClient.GetGameId(title);
            if (gameId is null)
            {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(
                    new DiscordEmbedBuilder()
                    {
                        Color = Colours.EmbedColourError,
                        Description = $"I can't find a game with the title \"{title}\""
                    }));
                return;
            }

            var gameInfoTask = itadClient.GetGameInfo(gameId);
            var gameOverviewTask = itadClient.GetGameOverview(gameId, country.ToString());
            var gamePricesTask = itadClient.GetGamePrices(gameId, country.ToString());

            var gameInfo = await gameInfoTask;
            var gameOverview = await gameOverviewTask;
            var gamePrices = await gamePricesTask;

            if (gameInfo is null || gameOverview is null)
            {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(
                    new DiscordEmbedBuilder()
                    {
                        Color = Colours.EmbedColourError,
                        Description = $"I don't have deal info for \"{title}\""
                    }));
                return;
            }

            DiscordEmbedBuilder embedBuilder = new()
            {
                Color = Colours.EmbedColourDefault,
                Title = gameInfo.Title,
                Url = gameInfo.IsThereAnyDealGameUrl,
            };

            string bestPriceString = GetCurrentBestPriceString(gamePrices, gameOverview, country);
            embedBuilder.AddField("Current Best Price", bestPriceString);

            if (gameOverview.LowestHistoricalPrice is not null)
            {
                string historicalPriceCutString = gameOverview.LowestHistoricalPrice.PriceCutPercent > 0
                    ? $" (-{gameOverview.LowestHistoricalPrice.PriceCutPercent}%)"
                    : "";

                string historicalPriceString = gameOverview.LowestHistoricalPrice.Price.ToFormattedPrice(country.ToString())
                    + historicalPriceCutString
                    + $" on {gameOverview.LowestHistoricalPrice.Store}\n{gameOverview.LowestHistoricalPrice.FormattedRecorded}";

                embedBuilder.AddField("Lowest Historical Price", historicalPriceString);
            }

            if (gameInfo.Reviews is not null && gameInfo.Reviews.TryGetValue("steam", out GameReview? gameReview))
            {
                string formattedReview = $"{gameReview.Text} ({gameReview.PercentPositive}% of {gameReview.TotalVotes})";
                embedBuilder.AddField("Steam Review", formattedReview);
            }

            if (gameInfo.ImageUrl is not null)
            {
                embedBuilder.WithImageUrl(gameInfo.ImageUrl);
            }

            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(embedBuilder));
        }

        private static string GetCurrentBestPriceString(List<GamePrice>? gamePrices, GameOverview gameOverview, Country country)
        {
            string priceCutString = "";
            if (gameOverview.BestCurrentPrice.PriceCutPercent > 0)
            {
                priceCutString = $" (-{gameOverview.BestCurrentPrice.PriceCutPercent}%)";
            }

            string bestPriceString = $"{gameOverview.BestCurrentPrice.Price.ToFormattedPrice(country.ToString())}{priceCutString}"
                + $" on [{gameOverview.BestCurrentPrice.Store}]({gameOverview.BestCurrentPrice.Url})";

            // Sometimes the game prices api call returns bad/duplicate data
            // where it says all the prices are 0 in one of the copies
            GamePrice? gamePrice = gamePrices?.FirstOrDefault(
                price => price.Store == gameOverview.BestCurrentPrice.Store && price.RegularPrice > 0
                );

            bestPriceString += "\nRegular ";
            if (gamePrice is null)
            {
                // Calculate the regular price if we don't have good data for it
                decimal discountPercent = 1 - (gameOverview.BestCurrentPrice.PriceCutPercent / 100m);
                decimal regularPrice = gameOverview.BestCurrentPrice.Price / discountPercent;
                bestPriceString += regularPrice.ToFormattedPrice(country.ToString());
            }
            else
            {
                bestPriceString += gamePrice.RegularPrice.ToFormattedPrice(country.ToString());
            }

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
                var searchResults = await itadClient.GetSearch(value, 25);
                if (searchResults is not null)
                {
                    return searchResults.ConvertAll(result => new DiscordAutoCompleteChoice(result.Title, result.Title));
                }
            }
            return Enumerable.Empty<DiscordAutoCompleteChoice>();
        }
    }
}
