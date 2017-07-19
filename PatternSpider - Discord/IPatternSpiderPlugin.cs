using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace PatternSpider_Discord
{
    public interface IPatternSpiderPlugin
    {
        string Name { get; }
        List<string> Commands { get; }

        Task Command(string command, string message, SocketMessage m);
        Task Message(string message, SocketMessage m);
    }
}
