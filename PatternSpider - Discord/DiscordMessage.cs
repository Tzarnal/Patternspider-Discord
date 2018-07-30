using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace PatternSpider_Discord
{
    public class DiscordMessage
    {
        public string Message;
        public Embed EmbedData;


        public DiscordMessage()
        {            
        }

        public DiscordMessage(string message)
        {
            Message = message;
        }

        public DiscordMessage(Embed embed)
        {
            EmbedData = embed;
        }

        public async Task SendMessageToChannel(ISocketMessageChannel channel)
        {
            if (EmbedData == null)
            {
                await channel.SendMessageAsync(Message);
            }

            await channel.SendMessageAsync("", false, EmbedData);
        }
    }
}
