using System.Collections.Generic;


namespace PatternSpider_Discord.Plugins.Plugin_MTG
{
    public class Ruling
    {
        public string date { get; set; }
        public string text { get; set; }
    }

    public class ForeignName
    {
        public string name { get; set; }
        public string language { get; set; }
        public string imageUrl { get; set; }
        public int? multiverseid { get; set; }
    }

    public class Legality
    {
        public string format { get; set; }
        public string legality { get; set; }
    }

    public class MtgCard
    {
        public string name { get; set; }
        public IList<string> names { get; set; }
        public string manaCost { get; set; }
        public int cmc { get; set; }
        public IList<string> colors { get; set; }
        public IList<string> colorIdentity { get; set; }
        public string type { get; set; }
        public IList<string> supertypes { get; set; }
        public IList<string> types { get; set; }
        public IList<string> subtypes { get; set; }
        public string rarity { get; set; }
        public string set { get; set; }
        public string setName { get; set; }
        public string text { get; set; }
        public string flavor { get; set; }
        public string artist { get; set; }
        public string number { get; set; }
        public string power { get; set; }
        public string toughness { get; set; }
        public string layout { get; set; }
        public IList<Ruling> rulings { get; set; }
        public IList<ForeignName> foreignNames { get; set; }
        public IList<string> printings { get; set; }
        public IList<Legality> legalities { get; set; }
        public string source { get; set; }
        public string id { get; set; }
        public int? loyalty { get; set; }
        public string releaseDate { get; set; }
        public int? multiverseid { get; set; }
        public string imageUrl { get; set; }
        public string originalText { get; set; }
        public string originalType { get; set; }
    }

    public class MtgCards
    {
        public IList<MtgCard> cards { get; set; }
    }
}
