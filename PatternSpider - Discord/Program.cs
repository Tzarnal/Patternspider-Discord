using System.IO;
using System.Threading.Tasks;
using Serilog;
using Discord;
using Discord.WebSocket;
using PatternSpider_Discord.Config;

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
            Log.Logger = new LoggerConfiguration()
#if DEBUG
                    .MinimumLevel.Verbose()
#else
                    .MinimumLevel.Information()
#endif
                .WriteTo.Console()
                .CreateLogger();

            LoadConfiguration();

            _pluginManager = new PluginManager(_patternSpiderConfig.CommandSymbol);

            _client = new DiscordSocketClient();

            _client.Log += LogClientMessage;                        
            await _client.LoginAsync(TokenType.Bot, _patternSpiderConfig.Token);
            await _client.StartAsync();

            _client.MessageReceived += MessageReceived;            

            await Task.Delay(-1);
        }

        private async Task MessageReceived(SocketMessage m)
        {
            if(m.Author.Id != _client.CurrentUser.Id)
                await _pluginManager.DispatchMessage(m);
        }

        private Task LogClientMessage(LogMessage msg)
        {
            if (msg.Exception != null)
            {
                Log.Fatal(msg.Exception,"Exception Caught by Discord.net");

                return Task.CompletedTask;
            }

            switch (msg.Severity)
            {
                case LogSeverity.Verbose:
                    Log.Verbose("Discord.net - {Source}: {Message}",msg.Source,msg.Message);
                    break;
                case LogSeverity.Info:
                    Log.Information("Discord.net - {Source}: {Message}", msg.Source, msg.Message);
                    break;
                case LogSeverity.Critical:
                    Log.Warning("Discord.net - {Source}: {Message}", msg.Source, msg.Message);
                    break;
                case LogSeverity.Debug:
                    Log.Debug("Discord.net - {Source}: {Message}", msg.Source, msg.Message);
                    break;
                case LogSeverity.Error:
                    Log.Error("Discord.net - {Source}: {Message}", msg.Source, msg.Message);
                    break;
                case LogSeverity.Warning:
                    Log.Warning("Discord.net - {Source}: {Message}", msg.Source, msg.Message);
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
                Log.Fatal("PatternSpider: Could not load {0}, creating a default one.", PatternSpiderConfig.FullPath);                
                _patternSpiderConfig = new PatternSpiderConfig();

                _patternSpiderConfig.Save();
            }
        }
    }
}

