using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace PatternSpider_Discord.Plugins
{
    class PluginFudge : IPatternSpiderPlugin
    {
        public string Name => "Fudge Dice";
        public List<string> Commands => new List<string> { "fudge" };

        private readonly DiceRoller _genie = new DiceRoller();

        public PluginFudge()
        {
            _genie = new DiceRoller();
        }

        public async Task Command(string command, string message, SocketMessage m)
        {            
            var name = m.Author.Username;
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
                    results[i - 1] = "➖";
                }
                else if (roll == 2)
                {
                    results[i - 1] = "🔲";
                }
                else
                {
                    rollTotal++;
                    results[i - 1] = "➕";
                }
            }

            await m.Channel.SendMessageAsync(m.Author.Mention + " -- [" + string.Join(" ", results) + "] -- Sum: " + rollTotal);
        }

        public Task Message(string message, SocketMessage m)
        {
            return Task.CompletedTask;
        }
    }
}
