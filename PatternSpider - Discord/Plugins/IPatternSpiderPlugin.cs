using System.Collections.Generic;

namespace PatternSpider_Discord.Plugins
{
    public interface IPatternSpiderPlugin
    {
        string Name { get; }
        List<string> Commands { get; }

        string Command();
        string Message();
    }
}
