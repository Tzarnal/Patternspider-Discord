using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace PatternSpider_Discord.Plugins
{
    class PluginExalted : IPatternSpiderPlugin
    {
        public string Name => "Exalted";
        public List<string> Commands => new List<string> {"e", "ed", "et"};

        private readonly DiceRoller _fate = new DiceRoller();

        public async Task Command(string command, string message, SocketMessage m)
        {
            switch (command)
            {
                case "et":
                    await RollTarget(message, m);
                    break;

                case "ed":
                    await RollDamage(message, m);
                    break;

                case "e":
                default:
                    await Roll(message, m);
                    break;

            }
        }

        private async Task Roll(string message, SocketMessage m)
        {
            var response = new StringBuilder();            
            var messageParts = message.Split(' ');

            if (messageParts.Length < 2)
            {
                await m.Channel.SendMessageAsync("Usage: ed <poolsize> [poolsize]...");
                return;
            }

            foreach (var messagePart in messageParts)
            {
                int poolSize;
                if (int.TryParse(messagePart, out poolSize))
                {
                    response.AppendLine(poolSize > 2000
                        ? "No Pools over 2000."
                        : string.Format("{0}", RollPool(poolSize)));
                }
            }

            await m.Channel.SendMessageAsync($"{m.Author.Mention} -- Result for: {message}\n" +
                                             $"{response}");
        }

        private async Task RollTarget(string message, SocketMessage m)
        {
            var response = new StringBuilder();
            var messageParts = message.Split(' ');
            int targetNumber;

            if (messageParts.Length < 3)
            {
                await m.Channel.SendMessageAsync("Usage: e <target number> <poolsize> [poolsize]...");
                return;                
            }

            if (!int.TryParse(messageParts[1], out targetNumber))
            {
                await m.Channel.SendMessageAsync("Usage: e <target number> <poolsize> [poolsize]...");
                return;
            }

            foreach (var messagePart in messageParts.Skip(2))
            {
                int poolSize;
                if (int.TryParse(messagePart, out poolSize))
                {
                    response.AppendLine(poolSize > 2000
                        ? "No Pools over 2000."
                        : string.Format("{0}", RollPool(poolSize,targetNumber)));
                }
            }

            await m.Channel.SendMessageAsync($"{m.Author.Mention} -- Result for: {message}\n" +
                                             $"{response}");
        }

        private async Task RollDamage(string message, SocketMessage m)
        {
            var response = new StringBuilder();
            var messageParts = message.Split(' ');

            if (messageParts.Length < 2)
            {
                await m.Channel.SendMessageAsync("Usage: ed <poolsize> [poolsize]...");
                return;
            }

            foreach (var messagePart in messageParts)
            {
                int poolSize;
                if (int.TryParse(messagePart, out poolSize))
                {
                    response.AppendLine(poolSize > 2000
                        ? "No Pools over 2000."
                        : string.Format("{0}", RollDamagePool(poolSize)));
                }
            }

            await m.Channel.SendMessageAsync($"{m.Author.Mention} -- Result for: {message}\n" +
                                             $"{response}");
        }

        public Task Message(string messsage, SocketMessage m)
        {
            return Task.CompletedTask;
        }

        private string RollPool(int poolSize, int targetNumber=7)
        {
            int[] rolls = new int[poolSize];
            var successes = 0;
            var response = "";

            for (var i = 0; i < poolSize; i++)
            {
                rolls[i] = _fate.RollDice(10);
                if (rolls[i] >= targetNumber)
                {
                    successes++;
                }

                if (rolls[i] == 10)
                {
                    successes++;
                }
            }

            if (rolls.Length <= 50)
            {
                response += String.Format("[{0}]", string.Join(", ", rolls.ToList().OrderBy(r => r)));
            }
            else
            {
                response += "Over 50 rolls, truncated to reduce spam";
            }

            if (successes == 0 && rolls.Contains(1))
            {
                response += " -- BOTCH";
            }
            else
            {
                response += string.Format(" -- {0} success(es).", successes);
            }

            return response;
        }

        private string RollDamagePool(int poolSize)
        {
            int[] rolls = new int[poolSize];
            var successes = 0;
            var response = "";

            for (var i = 0; i < poolSize; i++)
            {
                rolls[i] = _fate.RollDice(10);
                if (rolls[i] >= 7)
                {
                    successes++;
                }

            }

            if (rolls.Length <= 50)
            {
                response += String.Format("[{0}]", string.Join(", ", rolls.ToList().OrderBy(r => r)));
            }
            else
            {
                response += "Over 50 rolls, truncated to reduce spam";
            }

            response += string.Format(" -- {0} success(es).", successes);

            return response;
        }
    }
}
