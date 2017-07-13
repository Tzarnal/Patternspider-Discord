using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace PatternSpider_Discord.Plugins
{
    public interface IPatternSpiderPlugin
    {
        string Name { get; }
        List<string> Commands { get; }

        Task Command(string command, string messsage, SocketMessage m);
        Task Message(string messsage, SocketMessage m);
    }
}
