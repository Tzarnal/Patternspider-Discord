using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace PatternSpider_Discord.Plugins
{
    class PluginChatReactions : IPatternSpiderPlugin
    {
        public string Name => "Chat Reactions";
        public List<string> Commands => new List<string>();

        public Task Command(string command, string message, SocketMessage m)
        {
            return Task.CompletedTask;
        }

        public async Task Message(string message, SocketMessage m)
        {
            if (message.Contains("(╯°□°)╯︵ ┻━┻"))
                await m.Channel.SendMessageAsync("┬──┬◡ﾉ(° -°ﾉ)" );            
        }
    }
}
