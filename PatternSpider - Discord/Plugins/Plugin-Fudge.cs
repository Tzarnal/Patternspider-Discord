using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace PatternSpider_Discord.Plugins
{
    class PluginFudge : IPatternSpiderPlugin
    {
        public string Name => "Fudge Dice";
        public List<string> Commands => new List<string> { "fudge" };

        private readonly DiceRoller _genie;

        public PluginFudge()
        {
            _genie = new DiceRoller();
        }

        public async Task Command(string command, string message, SocketMessage m)
        {
            var messageParts = message.Split(' ');
            var diceNumber = 4;

            if (messageParts.Length > 1 && !int.TryParse(messageParts[1], out diceNumber))
            {
                await m.Channel.SendMessageAsync("Sorry, the argument must be a number");
                return;
            }

            var rollTotal = 0;
            var results = new string[diceNumber];

            for (var i = diceNumber; i > 0; i--)
            {
                var roll = _genie.RollDice(3);

                if (roll == 1)
                {
                    rollTotal--;
                    results[i - 1] = "<:FateMinus:337287430455558144>";
                }
                else if (roll == 2)
                {
                    results[i - 1] = "<:FateNull:337287451699576832>";
                }
                else
                {
                    rollTotal++;
                    results[i - 1] = "<:FatePlus:337287399509852181>";
                }
            }

            await m.Channel.SendMessageAsync(m.Author.Mention + " -- [ " + string.Join(" ", results) + " ] -- Sum: " + rollTotal);
        }

        public Task Message(string message, SocketMessage m)
        {
            return Task.CompletedTask;
        }
    }
}
