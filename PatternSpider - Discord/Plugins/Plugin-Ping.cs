using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace PatternSpider_Discord.Plugins
{
    class PluginPing : IPatternSpiderPlugin
    {
        public string Name => "Ping";
        public List<string> Commands=> new List<string>{"ping"};

        public async Task Command(string command, string messsage, SocketMessage m)
        {
            await m.Channel.SendMessageAsync("Pong.");
        }

        public Task Message(string messsage, SocketMessage m)
        {
            return Task.CompletedTask;
        }
    }
}
