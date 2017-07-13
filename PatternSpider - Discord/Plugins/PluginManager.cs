using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace PatternSpider_Discord.Plugins
{
    public class PluginManager
    {
        private List<IPatternSpiderPlugin> _plugins;
        private char _comandSymbol;

        public PluginManager(char commandSymbol)
        {
            _plugins = new List<IPatternSpiderPlugin>();
            _comandSymbol = commandSymbol;

            //Load Plugins through reflection.
            System.Reflection.Assembly ass = System.Reflection.Assembly.GetEntryAssembly();

            foreach (System.Reflection.TypeInfo ti in ass.DefinedTypes)
            {
                if (ti.ImplementedInterfaces.Contains(typeof(IPatternSpiderPlugin)))
                {
                    _plugins.Add((IPatternSpiderPlugin) ass.CreateInstance(ti.FullName));                    
                }
            }
        }

        public async Task DispatchMessage(SocketMessage m)
        {            
            foreach (IPatternSpiderPlugin plugin in _plugins)
            {
                var response = plugin.Message();

                if (response != null)
                {
                    await m.Channel.SendMessageAsync(response);
                }
            }


            if (m.Content.First() != _comandSymbol)
            {
                return;
            }

            var commandWord = m.Content.Split(' ').First().TrimStart(_comandSymbol);

            foreach (IPatternSpiderPlugin plugin in _plugins)
            {
                if (plugin.Commands.Contains(commandWord))
                {
                    var response = plugin.Command();

                    if (response != null)
                    {
                        await m.Channel.SendMessageAsync(response);
                    }
                }                
            }
        }
    }
}
