using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PatternSpider_Discord.Plugins.Weather
{
    class WeatherLookup
    {
        private string _key;
        private string _lat;
        private string _long;

        public WeatherLookup(string key, float lat, float lon)
        {
            _key = key;
            _lat = lat.ToString(CultureInfo.InvariantCulture);
            _long = lon.ToString(CultureInfo.InvariantCulture);
        }

        public async Task<WeatherData> Get(bool extend = false)
        {           
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();            

            var requestString = $"https://api.darksky.net/forecast/{_key}/{_lat},{_long}?units=si";
            if (extend)
            {
                requestString += "&extend=hourly";
            }

            var stringTask = client.GetStringAsync(requestString);

            string json;
            try
            {
                json = await stringTask;
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message} -- Request: {requestString}");                
                return null;                
            }
            
            var locationData = JsonConvert.DeserializeObject<WeatherData>(json);

            return locationData;
        }
    }
}
