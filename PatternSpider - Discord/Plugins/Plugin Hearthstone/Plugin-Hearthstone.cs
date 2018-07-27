using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.WebSocket;
using Newtonsoft.Json;
using PatternSpider_Discord.Config;
using Serilog;

namespace PatternSpider_Discord.Plugins.Hearthstone
{
    class PluginHearthstone : IPatternSpiderPlugin
    {
        public string Name => "Hearthstone";
        public List<string> Commands => new List<string> { "hs" };

        public PatternSpiderConfig ClientConfig { get; set; }
        public DiscordSocketClient DiscordClient { get; set; }

        public async Task Command(string command, string message, SocketMessage m)
        {
            var text = message.Trim();
            var messageParts = text.Split(' ');
            var searchString = string.Join(" ", messageParts.Skip(1));

            var searchResult = await SearchHearthHead(searchString);

            await m.Channel.SendMessageAsync(searchResult);
        }

        public Task Message(string message, SocketMessage m)
        {
            return Task.CompletedTask;
        }

        private async Task<string> SearchHearthHead(string searchString)
        {
            List<Card> cards;
            string searchUrl = $"http://hearthstone.services.zam.com/v1/card?sort=cost,name&search={searchString}&cost=0,1,2,3,4,5,6,7,8,9,10&type=MINION,SPELL,WEAPON,HERO&collectible=true";
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
                Log.Warning("Plugin-Hearthstone: Cannot parse Hearthead API Response. Request: {requestSTring}", searchUrl);
                Log.Debug(e, "Plugin-Hearthstone: Cannot parse Hearthead API Response. Request: {requestSTring}", searchUrl);
                throw;
            }

            if (cards.Count == 0)
            {
                return $"could not find any cards named: {searchString}";
            }

            if (cards.Count == 1)
            {
                var card = cards[0];
                return CardToString(card);
            }

            return $"[<https://www.hearthpwn.com/cards?filter-name={searchString}&filter-premium=1&display=3>] Found {cards.Count} results.";
        }

        private static List<Card> ParseJson(string data)
        {
            return JsonConvert.DeserializeObject<List<Card>>(data);
        }

        private string CardToString(Card card)
        {
            string cardText;
            var cardString = new StringBuilder();
            var zamName = card.name.ToLower().Replace(" ", "-");
            card.text = CorrectCardText(card.text);
            
            card.text = card.text.Replace("\n", " ");

            cardString.Append($"[<http://www.hearthhead.com/cards/{zamName}>] {card.name}\n");

            switch (card.type)
            {
                case "MINION":
                    cardText = $"[{card.rarity.CapitalizeOnlyFirstLetter()}] {(card.card_class ?? "Neutral").CapitalizeOnlyFirstLetter()} {(card.race ?? "-").CapitalizeOnlyFirstLetter()} Minion: {card.attack}/{card.health} for {card.cost} mana. \n ```{card.text}```\n";
                    break;
                case "HERO":
                    cardText = $"[{card.rarity.CapitalizeOnlyFirstLetter()}] {(card.card_class ?? "Neutral").CapitalizeOnlyFirstLetter()} Hero: {card.cost} mana.\n ```{card.text}```\n";
                    break;
                case "SPELL":
                    cardText = $"[{card.rarity.CapitalizeOnlyFirstLetter()}] {(card.card_class ?? "Neutral").CapitalizeOnlyFirstLetter()} Spell: {card.cost} mana.\n ```{card.text}```\n";
                    break;
                case "WEAPON":
                    cardText =  $"[{card.rarity.CapitalizeOnlyFirstLetter()}] {(card.card_class ?? "Neutral").CapitalizeOnlyFirstLetter()} Weapon: {card.attack}/{card.durability} for {card.cost}\n ```{card.text}```\n";
                    break;
                default:
                    cardText = $"[{card.rarity.CapitalizeOnlyFirstLetter()}] {(card.card_class ?? "Neutral").CapitalizeOnlyFirstLetter()} Card: {card.cost} mana.\n ```{card.text}```\n";
                    break;
            }

            cardString.Append($"[{card.set.CapitalizeOnlyFirstLetter()}] {cardText}");
            cardString.Append($"{card.cardImage}\n");

            return cardString.ToString();
        }

        private string ReCapitilize(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return text[0].ToString().ToUpper() + text.Substring(1).ToLower();
        }

        private string CorrectCardText(string text)
        {
            //Insert points for details
            var newText = Regex.Replace(text, "{.}", string.Empty);

            //Markup
            newText = Regex.Replace(newText, "<.>", string.Empty);
            newText = Regex.Replace(newText, "</.>", string.Empty);

            //@ Symbols
            newText = newText.Replace("@", " ");

            return newText;
        }
    }
}
