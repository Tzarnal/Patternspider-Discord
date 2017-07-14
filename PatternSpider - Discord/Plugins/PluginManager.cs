using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Serilog;

namespace PatternSpider_Discord.Plugins
{
    public class PluginManager
    {
        private readonly List<IPatternSpiderPlugin> _plugins;
        private readonly char _commandSymbol;

        public PluginManager(char commandSymbol)
        {
            _plugins = new List<IPatternSpiderPlugin>();
            _commandSymbol = commandSymbol;

            //Load Plugins through reflection.
            System.Reflection.Assembly ass = System.Reflection.Assembly.GetEntryAssembly();

            foreach (System.Reflection.TypeInfo ti in ass.DefinedTypes)
            {
                if (ti.ImplementedInterfaces.Contains(typeof(IPatternSpiderPlugin)))
                {
                    _plugins.Add((IPatternSpiderPlugin) ass.CreateInstance(ti.FullName));                    
                }
            }

            var pluginNames = _plugins.Select(plugin => plugin.Name).ToList();

            Log.Information("PatternSpider: Loaded {plugins}", pluginNames);
        }

        public async Task DispatchMessage(SocketMessage m)
        {
            foreach (IPatternSpiderPlugin plugin in _plugins)
            {
                await plugin.Message(m.Content, m);
            }

            if (m.Content.First() != _commandSymbol)
            {
                return;
            }

            var commandWord = m.Content.Split(' ').First().TrimStart(_commandSymbol);

            foreach (IPatternSpiderPlugin plugin in _plugins)
            {
                if (plugin.Commands.Contains(commandWord))
                {
                    await plugin.Command(commandWord,m.Content,m);  
                }                
            }
        }
    }
}
