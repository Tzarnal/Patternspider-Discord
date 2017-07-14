using System;
using System.IO;
using System.Threading.Tasks;
using Serilog;
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
        private DiscordSocketClient _client;

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            LoadConfiguration();
            _pluginManager = new PluginManager(_patternSpiderConfig.CommandSymbol);

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            _client = new DiscordSocketClient();

            _client.Log += LogClientMessage;
            
            string token = _patternSpiderConfig.Token;

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            _client.MessageReceived += MessageReceived;            

            await Task.Delay(-1);
        }

        private async Task MessageReceived(SocketMessage m)
        {
            await _pluginManager.DispatchMessage(m);
        }

        private Task LogClientMessage(LogMessage msg)
        {
            if (msg.Exception != null)
            {
                Log.Error(msg.Exception,"Exception Caught by Discord.net");

                return Task.CompletedTask;
            }

            switch (msg.Severity)
            {
                case LogSeverity.Verbose:
                    Log.Verbose($"{msg.Source} - {msg.Message}");
                    break;
                case LogSeverity.Info:
                    Log.Information($"{msg.Source} - {msg.Message}");
                    break;
                case LogSeverity.Critical:
                    Log.Warning($"{msg.Source} - {msg.Message}");
                    break;
                case LogSeverity.Debug:
                    Log.Debug($"{msg.Source} - {msg.Message}");
                    break;
                case LogSeverity.Error:
                    Log.Error($"{msg.Source} - {msg.Message}");
                    break;
                case LogSeverity.Warning:
                    Log.Warning($"{msg.Source} - {msg.Message}");
                    break;                
            }

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

