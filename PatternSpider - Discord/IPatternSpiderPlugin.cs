using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using PatternSpider_Discord.Config;

namespace PatternSpider_Discord
{
    public interface IPatternSpiderPlugin
    {
        string Name { get; }
        List<string> Commands { get; }

        PatternSpiderConfig ClientConfig { get; set; }
        DiscordSocketClient DiscordClient { get; set; }

        Task Command(string command, string message, SocketMessage m);
        Task Message(string message, SocketMessage m);
    }
}
