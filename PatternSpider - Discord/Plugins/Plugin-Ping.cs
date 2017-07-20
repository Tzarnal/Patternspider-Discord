using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using PatternSpider_Discord.Config;

namespace PatternSpider_Discord.Plugins
{
    class PluginPing : IPatternSpiderPlugin
    {
        public string Name => "Ping";
        public List<string> Commands=> new List<string>{"ping"};

        public PatternSpiderConfig ClientConfig { get; set; }
        public DiscordSocketClient DiscordClient { get; set; }

        public async Task Command(string command, string message, SocketMessage m)
        {
            await m.Channel.SendMessageAsync("Pong.");
        }

        public Task Message(string message, SocketMessage m)
        {
            return Task.CompletedTask;
        }
    }
}
