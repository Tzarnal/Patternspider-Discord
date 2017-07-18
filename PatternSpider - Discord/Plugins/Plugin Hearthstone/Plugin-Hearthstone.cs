using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.WebSocket;
using Newtonsoft.Json;
using Serilog;

namespace PatternSpider_Discord.Plugins.Hearthstone
{
    class PluginHearthstone : IPatternSpiderPlugin
    {
        public string Name => "Hearthstone";
        public List<string> Commands => new List<string> { "hs" };

        public async Task Command(string command, string messsage, SocketMessage m)
        {
            var text = messsage.Trim();
            var messageParts = text.Split(' ');
            var searchString = string.Join(" ", messageParts.Skip(1));

            var searchResult = await SearchHearthHead(searchString);

            await m.Channel.SendMessageAsync(searchResult);
        }

        public Task Message(string messsage, SocketMessage m)
        {
            return Task.CompletedTask;
        }

        private async Task<string> SearchHearthHead(string searchString)
        {
            List<Card> cards;
            string searchUrl = $"http://hearthstone.services.zam.com/v1/card?sort=cost,name&search={searchString}&cost=0,1,2,3,4,5,6,7&type=MINION,SPELL,WEAPON&collectible=true";
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

            if (cards.Count == 1)
            {
                var card = cards[0];
                return CardToString(card);
            }

            return $"[http://www.hearthhead.com/cards] Found {cards.Count} results.";
        }

        private static List<Card> ParseJson(string data)
        {
            return JsonConvert.DeserializeObject<List<Card>>(data);
        }

        private string CardToString(Card card)
        {
            string cardText;
            var zamName = card.name.ToLower().Replace(" ", "-");
            var cardClass = ReCapitilize(card.card_class);
            card.text = CorrectCardText(card.text);

            var cardSet = card.set;
            if (Tables.BlockNameCorrection.ContainsKey(cardSet))
            {
                cardSet = Tables.BlockNameCorrection[cardSet];
            }

            var block = "Unknown";
            if (Tables.Block.ContainsKey(cardSet))
            {
                block = Tables.Block[cardSet];
            }

            var format = "Wild";
            if (Tables.StandardLegal.Contains(block))
            {
                format = "Standard";
            }

            card.text = card.text.Replace("\n", " ");

            switch (card.type)
            {
                case "MINION":
                    cardText = string.Format("[{7}] [{8}] http://www.hearthhead.com/cards/{5}\n",
                                            card.name, card.attack, card.health, card.cost, card.text, zamName, cardClass, cardSet, format);
                    break;
                case "SPELL":
                    cardText = string.Format("[{5}] [{6}] http://www.hearthhead.com/cards/{3}\n",
                                            card.name, card.cost, card.text, zamName, cardClass, cardSet, format);
                    break;
                case "WEAPON":
                    cardText = string.Format("[{6}] [{7}] http://www.hearthhead.com/cards/{4}\n",
                                             card.name, card.attack, card.durability, card.cost, zamName, cardClass, cardSet, format);

                    if (!string.IsNullOrWhiteSpace(card.text))
                    {
                        cardText += " - " + card.text;
                    }
                    break;
                default:
                    cardText = string.Format("" +
                                             "[{4}] [{5}] http://www.hearthhead.com/cards/{3}\n",
                                             card.name, card.cost, card.text, zamName, card.set, block);
                    break;
            }

            return cardText;
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
