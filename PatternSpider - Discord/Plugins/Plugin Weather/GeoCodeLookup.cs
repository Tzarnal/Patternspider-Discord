using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PatternSpider_Discord.Plugins.Weather
{
    struct Coordinates
    {
        public float Latitude;
        public float Longitude;
        public string Name;
    }

    class GeoCodeLookup
    {
        private string _key;

        private Dictionary<string, Coordinates> _cache;

        public GeoCodeLookup(string key)
        {
            _cache = new Dictionary<string, Coordinates>();
            _key = key;
        }

        public async Task<Coordinates> Lookup(string location)
        {
            if (_cache.ContainsKey(location))
            {
                return _cache[location];
            }

            var coordinates = new Coordinates();

            var requestString = $"http://www.mapquestapi.com/geocoding/v1/address?key={_key}&location={location}";

            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();

            var stringTask = client.GetStringAsync(requestString);

            string json;
            try
            {
                json = await stringTask;
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message} -- Request: {requestString}");
                throw;
            }

            var locationData = JsonConvert.DeserializeObject<GeoLocationData>(json);
            var locationResult = locationData.results.First().locations.First();

            coordinates.Latitude = (float)locationResult.latLng.lat;
            coordinates.Longitude = (float)locationResult.latLng.lng;
            coordinates.Name = locationResult.ToString();

            _cache.Add(location, coordinates);

            return coordinates;
        }
    }
}
