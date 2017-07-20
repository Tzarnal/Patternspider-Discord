using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using PatternSpider_Discord.Config;

namespace PatternSpider_Discord.Plugins
{
    class PluginCoinflip : IPatternSpiderPlugin
    {
        public string Name => "Coin Flip";
        public List<string> Commands => new List<string> { "coin" };

        public PatternSpiderConfig ClientConfig { get; set; }
        public DiscordSocketClient DiscordClient { get; set; }

        private readonly DiceRoller _genie;

        public PluginCoinflip()
        {
            _genie = new DiceRoller();
        }

        public async Task Command(string command, string message, SocketMessage m)
        {
            var roll = _genie.RollDice(2);

            if (roll == 1)
            {
                await m.Channel.SendMessageAsync("Heads.");
            }
            else
            {
                await m.Channel.SendMessageAsync("Tails.");
            }
        }

        public Task Message(string message, SocketMessage m)
        {
            return Task.CompletedTask;
        }
    }
}
