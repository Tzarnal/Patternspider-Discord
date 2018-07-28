using System;
using System.Collections.Generic;
using System.Text;

namespace PatternSpider_Discord.Plugins.MTG
{
    class MTG_EmoteTable
    {
        public static Dictionary<string, string> ManaSymbols = new Dictionary<string, string>()
        {
            //Basic mana types and colorless
            {"{U}", "<:manablue:472704446976229376>"},
            {"{B}", "<:manablack:472704446535565343>"},
            {"{R}", "<:manared:472704446720114709>"},
            {"{G}", "<:managreen:472704446749605899>"},
            {"{W}", "<:manawhite:472704446841749514>"},
            {"{C}", "<:manac1:472705157025759243>"},

            //Snow
            {"{S}", "<:manasnow:472704446862721034>"},

            //Generic costs
            {"{X}","<:manax:472704447089344522>"},
            {"{0}","<:manac0:472705156803330049>"},
            {"{1}","<:manac1:472705157025759243>"},
            {"{2}","<:manac2:472705156820238348>"},
            {"{3}","<:manac3:472705157130616842>"},
            {"{4}","<:manac4:472705157046730772>"},
            {"{5}","<:manac5:472705157294194688>"},
            {"{6}","<:manac6:472705157054988288>"},
            {"{7}","<:manac7:472705157067571210>"},
            {"{8}","<:manac8:472705157092737040>"},
            {"{9}","<:manac9:472705156962713611>"},
            {"{10}","<:manac10:472705156635557889>"},
            {"{11}","<:manac11:472705157021564938>"},
            {"{12}","<:manac12:472705156614717472>"},
            {"{13}","<:manac13:472705157034147840>"},
            {"{14}","<:manac14:472705156954325003>"},
            {"{15}","<:manac15:472705157310971918>"},
            {"{16}","<:manac16:472705156824432641>"},
            {"{17}","<:manac17:472705157193269248>"},
            {"{18}","<:manac18:472705157256445952>"},
            {"{19}","<:manac19:472705156954325004>"},
            {"{20}","<:manac20:472705156912250881>"},

            //Phyrexian Mana
            {"{U/P}","<:manapblue:472705721704775689>"},
            {"{R/P}","<:manapred:472705721474088971>"},
            {"{B/P}","<:manapblack:472705721666895883>"},
            {"{G/P}","<:manapgreen:472705721692061727>"},
            {"{W/P}","<:manapwhite:472705722581516289>"},

            //Hybrid Mana
            {"{W/U}","<:manawhiteblue:472704784156196865>"},
            {"{W/B}","<:manawhiteblack:472704784831479818>"},
            {"{R/W}","<:manaredwhite:472704784567107585>"},
            {"{R/G}","<:manaredgreen:472704784613507073>"},
            {"{G/W}","<:managreenwhite:472704784340746241>"},
            {"{G/U}","<:managreenblue:472704784160260097>"},
            {"{U/R}","<:manabluered:472704784411918337>"},
            {"{U/B}","<:manablueblack:472704784328294402>"},
            {"{B/R}","<:manablackred:472704784466706433>"},
            {"{B/G}","<:manablackgreen:472704784558718976>"},

            //Hybrid with 2 generic cost
            {"{2/W}","<:mana2white:472705514598563840>"},
            {"{2/R","<:mana2red:472705514413752322>"},
            {"{2/G}","<:mana2green:472705514543906826>"},
            {"{2/U}","<:mana2blue:472705514317414412>"},
            {"{2/B}","<:mana2black:472705514556358667>"},

            //Not tehnially a cost but included for completeness
            //Tap symbol
            {"{T}","<:tap:472726325350891531>"},
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
