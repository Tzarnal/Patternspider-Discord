using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace PatternSpider_Discord
{
    static class Extensions
    {
        public static string CapitalizeOnlyFirstLetter(this string str)
        {
            return char.ToUpper(str[0]) + str.Substring(1).ToLower();
        }

        public static string RemoveMarkup(this string str)
        {
            var output = Regex.Replace(str, @"</?.>", "");
            output = Regex.Replace(output, @"\[.\]", "");
            output = Regex.Replace(output, @"\{.*?}", "");

            return output;
        }
    }
}