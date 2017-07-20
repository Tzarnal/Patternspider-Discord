using System.IO;
using Newtonsoft.Json;

namespace PatternSpider_Discord.Config
{
    public class PatternSpiderConfig
    {
        public static string ConfigPath = "Configuration/";
        public static string ConfigFileName = "Pattern Spider Configuration.json";

        public static string FullPath 
            => Directory.GetCurrentDirectory() + "/"+ ConfigPath + ConfigFileName;

        //Actual config elements
        public char CommandSymbol = '!';
        public string Token = "Example-Token-Replac-With-Real";

        public void Save()
        {
            var data = JsonConvert.SerializeObject(this, Formatting.Indented);

            if (!Directory.Exists(ConfigPath))
            {
                Directory.CreateDirectory(ConfigPath);
            }

            File.WriteAllText(FullPath, data);
        }

        public static PatternSpiderConfig Load()
        {
            var data = File.ReadAllText(FullPath);
            return JsonConvert.DeserializeObject<PatternSpiderConfig>(data);

        }
    }
}
