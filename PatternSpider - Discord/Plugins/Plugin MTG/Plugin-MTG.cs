using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.WebSocket;
using Newtonsoft.Json;
using PatternSpider_Discord.Config;
using PatternSpider_Discord.Plugins.Plugin_MTG;
using Serilog;


namespace PatternSpider_Discord.Plugins.MTG
{
    public class PluginMTG : IPatternSpiderPlugin
    {
        public string Name => "MTG";
        public List<string> Commands => new List<string> { "mtg" };

        public PatternSpiderConfig ClientConfig { get; set; }
        public DiscordSocketClient DiscordClient { get; set; }

        public async Task Command(string command, string message, SocketMessage m)
        {
            var text = message.Trim();
            var messageParts = text.Split(' ');
            var searchString = string.Join(" ", messageParts.Skip(1));

            var searchResult = await SearchMagic(searchString);

            await m.Channel.SendMessageAsync(searchResult);
        }

        public Task Message(string message, SocketMessage m)
        {
            return Task.CompletedTask;
        }


        public async Task<string> SearchMagic(string searchString)
        {
            List<MtgCard> cards;
            var searchTerm = WebUtility.UrlEncode(searchString.ToLower());
            var searchUrl = $"https://api.magicthegathering.io/v1/cards?name={searchTerm}";
            string jsonData;

            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();

            var stringTask = client.GetStringAsync(searchUrl);
            
            try
            {
                jsonData = await stringTask;
            }
            catch
            {
                return "Error Occured trying to search for card.";
            }

            if (string.IsNullOrWhiteSpace(jsonData))
                return "No Results found for: " + searchString;

            var errorRegex = @"{""error"":""(.+)""}";
            var errorMatch = Regex.Match(jsonData, errorRegex);
            if (errorMatch.Success)
            {
                var errorString = errorMatch.Groups.FirstOrDefault().ToString();

                Log.Warning($"Plugin-MTG: Error while quering magicthegathering.io: {jsonData}");

                return $"Error while looking up MTG Card: {errorString}";                
            }

            try
            {
                cards = ParseJson(jsonData);
            }
            catch (Exception e)
            {
                Log.Warning("Plugin-MTG: Cannot parse magicthegathering.io API Response. Request: {requestSTring}", searchUrl);
                Log.Debug(e, "Plugin-MTG: Cannot parse magicthegathering.io API Response. Request: {requestSTring}", searchUrl);
                throw;
            }

            if (cards.Count == 1)
                return CardToString(cards.FirstOrDefault());

            if (cards.Count == 0)
            {
                return $"Could not find any cards named: {searchString}";
            }

            var nameBuffer = cards.FirstOrDefault().name;
            foreach (var card in cards)
            {
                if( card.name != nameBuffer)
                {
                    return $"[<https://scryfall.com/search?q={searchTerm}&unique=cards&as=grid&order=name>] Found {cards.Count} results.";
                }
                
            }

            return CardToString(cards.LastOrDefault());
        }

        private static List<MtgCard> ParseJson(string data)
        {
            var jsonData = JsonConvert.DeserializeObject<MtgCards>(data);
            return jsonData.cards.ToList();
        }

        private static string CardToString(MtgCard card)
        {
            var cardString = new StringBuilder();

            var cardImage = card.imageUrl;
            var cardName = card.name;
            var searchTerm = WebUtility.UrlEncode(card.name.ToLower());
            var cardUrl = $"https://scryfall.com/search?q={searchTerm}&unique=cards&as=grid&order=name";

            cardString.Append($"[<{cardUrl}>] {cardName}\n");

            if (card.loyalty != null)
            {
                //Card is probably a planeswalker
                cardString.Append($"[{card.setName}][{card.rarity}] {card.type}: {card.loyalty} loyalty for {MTG_EmoteTable.ReplaceSymbols(card.manaCost)}\n");

            }
            else if(!string.IsNullOrWhiteSpace(card.toughness) && !string.IsNullOrWhiteSpace(card.power))
            {
                //card is probably some form of creature
                cardString.Append($"[{card.setName}][{card.rarity}] {card.type}: {card.power}/{card.toughness} for {MTG_EmoteTable.ReplaceSymbols(card.manaCost)}\n");
            }
            else
            {
                //default
                cardString.Append($"[{card.setName}][{card.rarity}] {card.type}: costs: {MTG_EmoteTable.ReplaceSymbols(card.manaCost)}\n");
            }

            cardString.Append($"*{MTG_EmoteTable.ReplaceSymbols(card.text)}*\n");
            cardString.Append($"{cardImage}\n");

            return cardString.ToString();
        }
    }
}
