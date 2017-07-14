using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Serilog;

namespace PatternSpider_Discord.Plugins.Mumble
{
    class MumbleServers
    {
        public static string DataPath = "Configuration/Mumble/";
        public static string DataFileName = "Servers.json";      
        public static string FullPath => DataPath + DataFileName;

        public List<ServerEntry> Servers;

        public MumbleServers()
        {
            Servers = new List<ServerEntry>
            {
                new ServerEntry
                {
                    Guild = "example server",
                    DiscordChannel = "xamplez",
                    MumbleCVP = "https://example.com/servers/cvp.json"
                }
            };
        }

        public void Save()
        {
            var data = JsonConvert.SerializeObject(this);

            if (!Directory.Exists(DataPath))
            {
                Directory.CreateDirectory(DataPath);
            }

            try
            {
                File.WriteAllText(FullPath, data);
            }
            catch (Exception e)
            {
                Log.Error(e,"Plugin-Mumble: Failed to save Mumble Servers file {FullPath}",FullPath);                
            }
        }

        public static MumbleServers Load()
        {
            var data = File.ReadAllText(FullPath);
            return JsonConvert.DeserializeObject<MumbleServers>(data);
        }
    }
}
