using System.Collections.Generic;


namespace PatternSpider_Discord.Plugins
{
    class PluginPing : IPatternSpiderPlugin
    {
        public string Name => "ping";
        public List<string> Commands=> new List<string>{"ping"};

        public string Command()
        {
            return "Pong";
        }

        public string Message()
        {
            return null;
        }
    }
}
