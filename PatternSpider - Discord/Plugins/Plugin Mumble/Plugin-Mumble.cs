using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Serilog;

namespace PatternSpider_Discord.Plugins.Mumble
{
    class PluginMumble : IPatternSpiderPlugin
    {
        public string Name => "Mumble";
        public List<string> Commands => new List<string> { "mumble" };

        private readonly MumbleServers _servers;

        public PluginMumble()
        {
            if (File.Exists(MumbleServers.FullPath))
            {
                _servers = MumbleServers.Load();
            }
            else
            {
                Log.Warning("Plugin-Mumble: Could not load {0}, creating an empty one.", MumbleServers.FullPath);
                _servers = new MumbleServers();
                _servers.Save();
            }
        }

        public async Task Command(string command, string messsage, SocketMessage m)
        {            
            var guildChannel = m.Channel as IGuildChannel;
            if (guildChannel == null)
            {
                return;
            }

            var candidateServer =
                _servers.Servers.FirstOrDefault(entry => entry.Guild == guildChannel.Guild.Name && entry.DiscordChannel == guildChannel.Name);

            if (candidateServer != null)
            {
                var message = await GetMumbleData(candidateServer.MumbleCVP);
                await m.Channel.SendMessageAsync(message);
            }
        }

        public Task Message(string messsage, SocketMessage m)
        {
            return Task.CompletedTask;
        }

        private String GetMumbleChannelUsers(List<User> users)
        {
            var userNames = new List<string>();

            foreach (var user in users)
            {
                userNames.Add(user.Name);
            }

            return string.Join(",", userNames);
        }

        private async Task<string> GetMumbleData(string cvpUrl)
        {
            MumbleCVP mumbleData = new MumbleCVP();
            var message = new StringBuilder();

            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();

            var stringTask = client.GetStringAsync(cvpUrl);

            string json;
            try
            {
                json = await stringTask;
            }
            catch (Exception e)
            {
                Log.Warning("Plugin-Mumble: Mumble request API Failure. Request: {requestSTring}", cvpUrl);
                Log.Debug(e,"Plugin-Mumble: Mumble request API Failure. Request: {requestSTring}", cvpUrl);
                return null;
            }

            try
            {
                mumbleData = MumbleCVP.Load(json);
                message.AppendLine($"{mumbleData.name} - {mumbleData.x_connecturl}");
            }
            catch (Exception e)
            {
                Log.Error(e,"Plugin-Mumble: Error parsing response. Response: {json}", json );                
            }

            if (mumbleData.root.users.Count != 0)
            {
                message.AppendLine($"{mumbleData.root.name} {GetMumbleChannelUsers(mumbleData.root.users)}");
            }

            foreach (var channel in mumbleData.root.channels)
            {
                if (channel.users.Count != 0)
                {
                    message.AppendLine($"{channel.name} - {GetMumbleChannelUsers(channel.users)}");
                }

                if (channel.channels.Count != 0)
                {
                    foreach (var subChannel in channel.channels)
                    {
                        if (subChannel.users.Count != 0)
                        {
                            message.AppendLine($"{subChannel.name} - {GetMumbleChannelUsers(subChannel.users)}");
                        }
                    }
                }
            }

            return message.ToString();
        }
    }
}
