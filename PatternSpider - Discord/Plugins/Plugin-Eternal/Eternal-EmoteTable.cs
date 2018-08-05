using System;
using System.Collections.Generic;
using System.Text;

namespace PatternSpider_Discord.Plugins.Eternal
{
    class Eternal_EmoteTable
    {
        public static Dictionary<string, string> ManaSymbols = new Dictionary<string, string>()
        {
            //Basic mana types and colorless
            {"{T}", "<:Time:475626298811875338>"},
            {"{P}", "<:Storm:475626298849492992>"},
            {"{S}", "<:Shadow:475626298782253066>"},
            {"{J}", "<:Justice:475626298836779008>"},
            {"{F}", "<:Fire:475626298664812545>"},           
        };

        public static string ReplaceSymbols(string input)
        {
            var output = input;

            foreach (var symbol in ManaSymbols )
            {
                output = output.Replace(symbol.Key, symbol.Value);
            }

            return output;
        }
    }
}
