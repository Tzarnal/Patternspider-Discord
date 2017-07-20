using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using PatternSpider_Discord.Config;

namespace PatternSpider_Discord.Plugins
{
    class PluginChatReactions : IPatternSpiderPlugin
    {
        public string Name => "Chat Reactions";
        public List<string> Commands => new List<string>();

        public PatternSpiderConfig ClientConfig { get; set; }
        public DiscordSocketClient DiscordClient { get; set; }

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
