using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace PatternSpider_Discord.Plugins.Weather
{
    class UsersLocations
    {
        public static string DataPath = "Configuration/Weather/";
        public static string DataFileName = "User Locations.json";
        public static string FullPath => DataPath + DataFileName;

        public Dictionary<string, string> UserLocations;

        public UsersLocations()
        {
            UserLocations = new Dictionary<string, string>();
        }

        public void Save()
        {
            var data = JsonConvert.SerializeObject(this, Formatting.Indented);

            if (!Directory.Exists(DataPath))
            {
                Directory.CreateDirectory(DataPath);
            }

            File.WriteAllText(FullPath, data);
        }

        public static UsersLocations Load()
        {
            var data = File.ReadAllText(FullPath);
            return JsonConvert.DeserializeObject<UsersLocations>(data);
        }
    }
}
