using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PatternSpider_Discord.Config;
using PatternSpider_Discord.Plugins;

namespace PatternSpider_Discord
{
    class Program
    {
        private PluginManager _pluginManager;
        private PatternSpiderConfig _patternSpiderConfig;

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            LoadConfiguration();
            _pluginManager = new PluginManager(_patternSpiderConfig.CommandSymbol);

            var client = new DiscordSocketClient();

            client.Log += Log;
            
            string token = _patternSpiderConfig.Token;

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            client.MessageReceived += MessageReceived;            

            await Task.Delay(-1);
        }

        private async Task MessageReceived(SocketMessage m)
        {
            await _pluginManager.DispatchMessage(m);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private void LoadConfiguration()
        {

            if (File.Exists(PatternSpiderConfig.FullPath))
            {
                _patternSpiderConfig = PatternSpiderConfig.Load();
            }
            else
            {
                Console.WriteLine("Could not load {0}, creating new config file.", PatternSpiderConfig.FullPath);
                _patternSpiderConfig = new PatternSpiderConfig();

                _patternSpiderConfig.Save();
            }
        }
    }
}

