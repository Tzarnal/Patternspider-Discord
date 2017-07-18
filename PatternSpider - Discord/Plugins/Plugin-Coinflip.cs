using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace PatternSpider_Discord.Plugins
{
    class PluginCoinflip : IPatternSpiderPlugin
{
    public string Name => "Coin Flip";
    public List<string> Commands => new List<string> { "coin" };

    private readonly DiceRoller _genie;

    public PluginCoinflip()
    {
        _genie = new DiceRoller();
    }

    public async Task Command(string command, string messsage, SocketMessage m)
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

    public Task Message(string messsage, SocketMessage m)
    {
        return Task.CompletedTask;
    }
}
}
