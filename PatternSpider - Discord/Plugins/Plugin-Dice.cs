using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.WebSocket;
using org.mariuszgromada.math.mxparser;
using PatternSpider_Discord.Config;

namespace PatternSpider_Discord.Plugins
{
    class PluginDice : IPatternSpiderPlugin
    {
        public string Name => "Dice";
        public List<string> Commands => new List<string> { "dice", "d", "roll", "r", "d100", "d20", "d12", "d10", "d8", "d6", "d4" };

        public PatternSpiderConfig ClientConfig { get; set; }
        public DiscordSocketClient DiscordClient { get; set; }

        private string _diceResults;
        private readonly DiceRoller _genie = new DiceRoller();

        public async Task Command(string command, string message, SocketMessage m)
        {
            var messageParts = message.Split(' ');
            var processedMessage = string.Join(" ", messageParts.Skip(1));
            var name = m.Author.Username;
            var response = new StringBuilder();

            switch (command)
            {
                case "d100":
                    processedMessage = "1d100 " + processedMessage;
                    break;
                case "d20":
                    processedMessage = "1d20 " + processedMessage;
                    break;
                case "d12":
                    processedMessage = "1d12 " + processedMessage;
                    break;
                case "d10":
                    processedMessage = "1d10 " + processedMessage;
                    break;
                case "d8":
                    processedMessage = "1d8 " + processedMessage;
                    break;
                case "d6":
                    processedMessage = "1d6 " + processedMessage;
                    break;
                case "d4":
                    processedMessage = "1d4 " + processedMessage;
                    break;
            }

            do
            {
                try
                {
                    processedMessage = RollsToNumbers(processedMessage);
                }
                catch (ArgumentException ex)
                {
                    await m.Channel.SendMessageAsync(string.Format("{0} -- {1}", name, ex.Message));
                    return;
                }

                if (!string.IsNullOrWhiteSpace(_diceResults))
                {
                    if (_diceResults.Length > 140)
                    {
                        _diceResults = "rolls over 140 character, truncated to reduce spam";
                    }
                    response.AppendLine(string.Format("{2} -- {1}", name, processedMessage, _diceResults));
                }

            } while (!string.IsNullOrWhiteSpace(_diceResults));


            double total;

            try
            {
                total = CalculateString(processedMessage);
            }
            catch (Exception e)
            {
                await m.Channel.SendMessageAsync(e.Message);
                return;                
            }
       
            if (total != 0 && total.ToString(CultureInfo.InvariantCulture) != processedMessage.Trim())
            {
                response.AppendLine(string.Format("Result: {0}", total));
            }

            if (response.Length > 0)
            {
                await m.Channel.SendMessageAsync($"{m.Author.Mention} -- Result for: {message}\n" +
                                                 $"{response}");                
            }
            else
            {
                await m.Channel.SendMessageAsync( "No dice to roll and not a mathematical expression." );
            }
        }

        public Task Message(string message, SocketMessage m)
        {
            return Task.CompletedTask;
        }

        private string RollsToNumbers(string input)
        {
            _diceResults = "";
            var output = input;
            var diceRegex = new Regex(@"\d*d\d+", RegexOptions.IgnoreCase);

            foreach (Match match in diceRegex.Matches(input))
            {
                if (match.Success)
                {
                    var numbers = match.Value.ToLower().Split('d');
                    int dieSize;
                    int amountThrown;

                    if (string.IsNullOrWhiteSpace(numbers[0]))
                    {
                        numbers[0] = "1";
                    }

                    try
                    {
                        dieSize = int.Parse(numbers[1]);
                        amountThrown = int.Parse(numbers[0]);
                    }
                    catch (OverflowException)
                    {
                        throw new ArgumentException("Die size or roll size exceeds " + Int32.MaxValue);
                    }


                    var total = 0;
                    var diceResults = new List<int>();

                    if (amountThrown > 9999)
                    {
                        throw new ArgumentException("No Rolls with more than 9999 Dice");
                    }

                    for (var i = 0; i < amountThrown; i++)
                    {
                        var result = _genie.RollDice(dieSize);
                        diceResults.Add(result);
                        total += result;
                    }

                    var repRegex = new Regex(match.Value, RegexOptions.IgnoreCase);
                    output = repRegex.Replace(output, total.ToString(CultureInfo.InvariantCulture), 1);
                    _diceResults = string.Format("{0}[{1}]", _diceResults, string.Join(",", diceResults));
                }
            }

            return output;
        }

        public double CalculateString(string input)
        {
            var expresion = new Expression(input);
            double result;

            result = expresion.calculate();

            return result;
        }
    }
}
