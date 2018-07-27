﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
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

            Log.Information($"Plugin-MTG: Output - {searchResult}");

            await m.Channel.SendMessageAsync(searchResult);
        }

        public Task Message(string message, SocketMessage m)
        {
            return Task.CompletedTask;
        }


        public async Task<string> SearchMagic(string searchString)
        {
            List<MtgCard> cards;
            string searchUrl = $"https://api.magicthegathering.io/v1/cards?name={searchString}";
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

            if (cards.Count > 1)
            {
                return
                    $"[http://gatherer.wizards.com/Pages/Search/Default.aspx?name=+[{searchString}]] Found {cards.Count} results.";
            }

            return CardToString(cards.FirstOrDefault());
        }

        private static List<MtgCard> ParseJson(string data)
        {
            var jsonData = JsonConvert.DeserializeObject<MtgCards>(data);

            Log.Information($"{jsonData.cards.Count}");
            return jsonData.cards.ToList();
        }

        private static string CardToString(MtgCard card)
        {
            var cardString = new StringBuilder();

            var cardImage = card.imageUrl;
            var cardName = card.name;
            var cardUrl = $"http://gatherer.wizards.com/Pages/Card/Details.aspx?multiverseid={card.multiverseid}";

            cardString.Append($"[<{cardUrl}>] {cardName}\n");
            cardString.Append($"```{card.text}```\n");
            cardString.Append($"{cardImage}\n");

            return cardString.ToString();
        }
    }
}
