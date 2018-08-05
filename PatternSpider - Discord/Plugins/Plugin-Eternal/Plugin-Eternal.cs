using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PatternSpider_Discord.Config;
using Serilog;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;

namespace PatternSpider_Discord.Plugins.Eternal
{
    class PluginEternal : IPatternSpiderPlugin
    {
        private HttpClient _httpClient;

        public string Name => "Eternal";
        public List<string> Commands => new List<string> { "eternal" };

        public PatternSpiderConfig ClientConfig { get; set; }
        public DiscordSocketClient DiscordClient { get; set; }


        public PluginEternal()
        {
            _httpClient = new HttpClient();

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.Timeout = TimeSpan.FromSeconds(2);
        }

        public async Task Command(string command, string message, SocketMessage m)
        {
            var text = message.Trim();
            var messageParts = text.Split(' ');
            var searchString = string.Join(" ", messageParts.Skip(1));

            DiscordMessage resultMesage;

            resultMesage = await SearchEternalWarcry(searchString);

            await resultMesage.SendMessageToChannel(m.Channel);            
        }

        public Task Message(string message, SocketMessage m)
        {
            return Task.CompletedTask;
        }

        private async Task<DiscordMessage> SearchEternalWarcry(string searchString)
        {
            var searchTerm = WebUtility.UrlEncode(searchString.ToLower());
            var searchUrl = $"https://eternalwarcry.com/cards?Query={searchTerm}&DraftPack=";

            HtmlDocument document;

            try
            {
                document = await UrlReqest(searchUrl);
            }
            catch (Exception e)
            {
                Log.Error(e, $"Plugin-Eternal: Encountered an error searching for the following {searchString}");
                return new DiscordMessage($"Encountered an error trying to search for the card {searchString}.");
            }            

            var paginator = document.QuerySelector("span.pagination-info");

            if (paginator != null)
            {
                var results = paginator.InnerText.Split(" ").Last();

                return new DiscordMessage($"[<{searchUrl}>] Found {results} results.");
            }

            var searchItems = document.QuerySelectorAll("div.card-search-item");

            if (searchItems == null)
            {
                return new DiscordMessage("Nothing Found");
            }

            if (searchItems.Count == 0)
            {
                return new DiscordMessage($"Could not find any cards named {searchString}.");
            }

            if (searchItems.Count > 1)
            {
                return new DiscordMessage($"[<{searchUrl}>] Found {searchItems.Count} results.");
            }

            var card = searchItems.First();
            var cardLink = card.QuerySelector("a").Attributes["href"].Value;


            Embed embed;

            try
            {
                embed = await CardToEmbed($"https://eternalwarcry.com{cardLink}");
            }
            catch (Exception e)
            {
                Log.Error(e, $"Plugin-Eternal: Encountered an error searching for the following {searchString}");
                return new DiscordMessage($"Encountered an error trying to display the card {searchString}");
            }

            return new DiscordMessage(embed);
        }

        private async Task<Embed> CardToEmbed(string cardUrl)
        {
            var document = await UrlReqest(cardUrl);

            var cardName = document.DocumentNode.SelectSingleNode("/html/body/div[3]/div/h1").InnerText;
            var cardText = document.DocumentNode.SelectSingleNode("/html/body/div[3]/div/div[2]/div[1]/div/div[2]/p").InnerText;
            var cardImage = document.DocumentNode.SelectSingleNode("/html/body/div[3]/div/div[2]/div[1]/div/div[1]/p/img").Attributes["src"].Value;

            var raceNode = document.DocumentNode
                .SelectNodes("//td").FirstOrDefault(n => n.InnerText == "Race");

            var race = "";

            if (raceNode != null)
            {
                race = raceNode.ParentNode.SelectNodes("td")[1].InnerText.Trim();
                race = $" - {race}";
            }

            var cardType = document.DocumentNode.SelectSingleNode("/html[1]/body[1]/div[3]/div[1]/div[2]/div[2]/table[2]/tr[2]/td[2]").InnerText;
            var cardRarity = document.DocumentNode.SelectSingleNode("/html[1]/body[1]/div[3]/div[1]/div[2]/div[2]/table[2]/tr[3]/td[2]").InnerText;
            var cardSet = document.DocumentNode.SelectSingleNode("/html[1]/body[1]/div[3]/div[1]/div[2]/div[2]/table[2]/tr[4]/td[2]").InnerText;

            var metaTags = document.DocumentNode.SelectNodes("//meta");

            var descriptionRegex = @"Eternal card: [A-Za-z0-9_ ]+\. (.+)";

            foreach (var tag in metaTags)
            {
                if (tag.Attributes.Contains("name") && tag.Attributes["name"].Value == "description")
                {
                    var description = tag.Attributes["content"].Value;
                    var match = Regex.Match(description, descriptionRegex);

                    if (match.Success)
                    {
                        cardText = match.Groups[1].ToString();
                        cardText = Eternal_EmoteTable.ReplaceSymbols(cardText);
                        cardText = cardText.Replace(";", "\n");
                    }

                    break;
                }
            }

            var cardEmbed = new EmbedBuilder()
                .WithTitle(cardName)
                .WithUrl(cardUrl)
                .WithThumbnailUrl(cardImage)
                .AddInlineField("Set", cardSet)
                .AddInlineField("Rarity", cardRarity)
                .AddInlineField("Type", $"{cardType}{race}")
                .AddField("Text", cardText)
                .WithColor(Color.Gold);

            return cardEmbed;
        }

        private async Task<HtmlDocument> UrlReqest(string url)
        {            
            var documentString = "";

            var stringTask = _httpClient.GetStringAsync(url);

            documentString = await stringTask;

            if (string.IsNullOrWhiteSpace(documentString))
            {
                throw new IOException("Empty page.");
            }

            var document = new HtmlDocument();
            document.LoadHtml(documentString);

            return document;
        }
    }
}
