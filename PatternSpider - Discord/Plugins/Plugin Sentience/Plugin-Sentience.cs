using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.WebSocket;
using PatternSpider_Discord.Plugins.Sentience.RelayChains;

namespace PatternSpider_Discord.Plugins.Sentience
{
    class PluginSentience : IPatternSpiderPlugin
    {
        public string Name => "Sentience";
        public List<string> Commands => new List<string>();

        private Chain _brain;
        private readonly object _writeLock;
        private Settings _settings;

        public static string BrainPath = "Data/Sentience/Brain.txt";

        public PluginSentience()
        {
            _writeLock = new object();

            if (File.Exists(Settings.FullPath))
            {
                _settings = Settings.Load();
            }
            else
            {
                _settings = new Settings();
                _settings.Save();
            }

            var brainDirectory = Path.GetDirectoryName(BrainPath);

            if (!Directory.Exists(brainDirectory))
            {
                Directory.CreateDirectory(brainDirectory);
            }

            if (!File.Exists(BrainPath))
            {
                //This is a little ugly but i can't manually close a filestream
                using (var f = File.Create(BrainPath))
                {                    
                }
            }

            PruneBrain();
            LoadBrain();
        }
    

        public Task Command(string command, string message, SocketMessage m)
        {
            return Task.CompletedTask;
        }

        public async Task Message(string message, SocketMessage m)
        {
            var botName = "PatternSpider";
            var botNameMatch = string.Format("^(@)?{0}[:,;]", botName);

            if (_brain == null)
            {
                return;
            }

            var brain = _brain;
          
            if (Regex.IsMatch(message, botNameMatch))
            {
                var regex = new Regex(botNameMatch);

                message = regex.Replace(message, "");

                var response = TextSanitizer.FixInputEnds(brain.GenerateSentenceFromSentence(message));

                if (!string.IsNullOrWhiteSpace(response))
                {
                    await m.Channel.SendMessageAsync(response);
                    return;
                }

                response = RandomResponse.Reponse(m.Author.Username);
                await m.Channel.SendMessageAsync(response);
                return;
            }

            if (message.Split(' ').Length > _settings.WindowSize)
            {
                message = TextSanitizer.SanitizeInput(message);
                brain.Learn(message);

                SaveLine(message);
            }
        }

        private void SaveLine(string message)
        {
            lock (_writeLock)
            {
                var fs = File.AppendText(BrainPath);

                try
                {
                    fs.WriteLine(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to save Sentience Memory: " + e.Message);
                }

                fs.Flush();
            }
        }

        private void LoadBrain()
        {
            using (var stream = new FileStream(BrainPath, FileMode.Open))
            using (var reader = new StreamReader(stream))
            {
                var brain = new Chain(_settings.WindowSize);
                string line;
                
                while ((line = reader.ReadLine()) != null)
                {
                    brain.Learn("sup");
                    brain.Learn(TextSanitizer.SanitizeInput(line));
                }

                _brain = brain;
            }
        }

        private void PruneBrain()
        {
            var fileInfo = new FileInfo(BrainPath);
            if (fileInfo.Length <= _settings.LogSize)
            {
                return;
            }

            var rand = new Random();

            File.Copy(BrainPath, BrainPath + ".temp");
            File.Delete(BrainPath);

            using (var stream = new FileStream(BrainPath + ".temp", FileMode.Open))
            using (var oldBrain = new StreamReader(stream))
            {
                using (var brain = File.AppendText(BrainPath))
                {
                    string line;
                    while ((line = oldBrain.ReadLine()) != null)
                    {
                        if (rand.Next(0, 2) == 1)
                        {
                            brain.WriteLine(line);
                        }
                    }
                }                                           
            }
        }
    }
}
