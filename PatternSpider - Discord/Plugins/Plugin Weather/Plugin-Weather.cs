using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PatternSpider_Discord.Config;
using Serilog;

namespace PatternSpider_Discord.Plugins.Weather
{
    class PluginWeather : IPatternSpiderPlugin
    {
        public string Name => "Weather";
        public List<string> Commands => new List<string> {"weather"};

        public PatternSpiderConfig ClientConfig { get; set; }
        public DiscordSocketClient DiscordClient { get; set; }

        private readonly UsersLocations _usersLocations;
        private readonly ApiKeys _apiKeys;
        private readonly GeoCodeLookup _lookup;

        private static Dictionary<string, string> _weatherIcons = new Dictionary<string, string>
        {
            {"clear-day", "https://ssl.gstatic.com/onebox/weather/48/sunny.png"},
            {"clear-night", "https://cdn1.iconfinder.com/data/icons/weather-169/24/weather_forcast_night_clear_moon_star-512.png"},
            {"rain", "https://ssl.gstatic.com/onebox/weather/48/rain_s_cloudy.png"},
            {"snow", "https://ssl.gstatic.com/onebox/weather/48/snow_light.png"},
            {"sleet", "https://ssl.gstatic.com/onebox/weather/48/sleet.png"},
            {"wind", "https://ssl.gstatic.com/onebox/weather/48/windy.png"},
            {"fog", "https://ssl.gstatic.com/onebox/weather/64/fog.png"},
            {"cloudy", "https://ssl.gstatic.com/onebox/weather/64/cloudy.png"},
            {"partly-cloudy-day", "https://ssl.gstatic.com/onebox/weather/64/partly_cloudy.png"},
            {"partly-cloudy-night", "https://cdn.iconscout.com/public/images/icon/free/png-512/cloud-and-moon-cloudy-night-weather-3990e862204a2361-512x512.png"},
            {"hail", "https://cdn4.iconfinder.com/data/icons/heavy-weather/100/Weather_Icons_05_hail-512.png"},
            {"thunderstorm", "https://ssl.gstatic.com/onebox/weather/64/thunderstorms.png"},
            {"tornado", "https://cdn3.iconfinder.com/data/icons/weather-16/256/Tornado-512.png"}
        };


        private static Dictionary<string, string> _weatherEmoji = new Dictionary<string, string>
        {
            { "clear-day", "☀️" } ,
            { "clear-night", "🌃" } ,
            { "rain", "🌧️" } ,
            { "snow", "❄️" } ,
            { "sleet", "🌨️" } ,
            { "wind", "💨" } ,
            { "fog", "🌁" } ,
            { "cloudy", "☁️" } ,
            { "partly-cloudy-day", "⛅" } ,
            { "partly-cloudy-night", "⛅" } ,
            { "hail", "🌨️" } ,
            { "thunderstorm", "⛈️" } ,
            { "tornado", "🌪️" }
        };
        private static string[] windBearingText = new string[16]
        {
            "N",
            "NNE",
            "NE",
            "ENE",
            "E",
            "ESE",
            "SE",
            "SSE",
            "S",
            "SSW",
            "SW",
            "WSW",
            "W",
            "WNW",
            "NW",
            "NNW"
        };

        public PluginWeather()
        {
            if (File.Exists(UsersLocations.FullPath))
            {
                _usersLocations = UsersLocations.Load();
            }
            else
            {
                Log.Warning("Plugin-Weather: Could not load {0}, creating an empty one.", UsersLocations.FullPath);
                _usersLocations = new UsersLocations();
                _usersLocations.Save();
            }

            if (File.Exists(ApiKeys.FullPath))
            {
                _apiKeys = ApiKeys.Load();
            }
            else
            {
                Log.Warning("Plugin-Weather: Could not load {0}, creating a default one.", ApiKeys.FullPath);
                _apiKeys = new ApiKeys();
                _apiKeys.Save();
            }

            _lookup = new GeoCodeLookup(_apiKeys.MapQuestKey);            
        }

        public async Task Command(string command, string message, SocketMessage m)
        {
            var text = message.Trim();
            var messageParts = text.Split(' ');
            DiscordMessage response = new DiscordMessage();
            var user = m.Author.ToString();

            if (messageParts.Length == 1)
            {
                if (_usersLocations.UserLocations.ContainsKey(user))
                {
                    response = await WeatherToday(_usersLocations.UserLocations[user]);
                }
                else
                {
                    GiveHelp(m);
                }

            }
            else if (messageParts.Length == 2)
            {
                command = messageParts[1].ToLower();
                if (command.ToLower() == "forecast")
                {
                    if (_usersLocations.UserLocations.ContainsKey(user))
                    {
                        response = await WeatherForecast(_usersLocations.UserLocations[user]);
                    }
                    else
                    {
                        GiveHelp(m);
                    }
                }
                else if (command.ToLower() == "remember")
                {
                    GiveHelp(m);
                }
                else
                {
                    command = messageParts[1].ToLower();
                    if (_usersLocations.UserLocations.ContainsKey(command))
                    {
                        response = await WeatherToday(_usersLocations.UserLocations[command]);
                    }
                    else
                    {
                        response = await WeatherToday(command);
                    }
                }
            }
            else
            {
                command = messageParts[1].ToLower();
                if (command.ToLower() == "forecast")
                {
                    response = await WeatherForecast(string.Join(" ", messageParts.Skip(2)));
                }
                else if (command.ToLower() == "remember")
                {
                    response = Remember(user, string.Join(" ", messageParts.Skip(2)));
                }
                else
                {
                    response = await WeatherToday(string.Join(" ", messageParts.Skip(1)));
                }
            }

            await response.SendMessageToChannel(m.Channel);           
        }

        public Task Message(string message, SocketMessage m)
        {
            return Task.CompletedTask;
        }

        private async Task<DiscordMessage> WeatherToday(string location)
        {
            Coordinates coordinates;

            try
            {
                coordinates = await _lookup.Lookup(location);
            }
            catch (Exception e)
            {
                Log.Debug(e, "Plugin-Weather: Location Lookup failure");                                
                if (!string.IsNullOrWhiteSpace(e.InnerException?.Message))
                    Log.Debug(e.InnerException, "Plugin-Weather: Location Lookup failure details");

                return new DiscordMessage("Could not find " + location);                
            }

            var weatherRequest = new WeatherLookup(_apiKeys.ForecastIoKey, coordinates.Latitude, coordinates.Longitude);
            WeatherData weather;
           
            try
            {
                weather = await weatherRequest.Get();
            }
            catch (Exception e)
            {
                Log.Debug(e, "Plugin-Weather: Weather Lookup failure");                
                if (!string.IsNullOrWhiteSpace(e.InnerException?.Message))
                    Log.Debug(e.InnerException, "Plugin-Weather: Weather Lookup failure details");                

                return new DiscordMessage("Found " + coordinates.Name + " but could not find any weather there.");                
            }

            var weatherEmbed = new EmbedBuilder();
            
            var wToday = weather.currently;
            var moonPhase = weather.daily.data[0].MoonPhase();
            var weatherIcon = _weatherIcons[wToday.icon];

            weatherEmbed.Title = $"Weather for {coordinates.Name}";
            weatherEmbed.Description = wToday.summary;
            weatherEmbed.ThumbnailUrl = weatherIcon;

            var tempField = new EmbedFieldBuilder
            {
                Name = "Temperature",
                Value = Temp(wToday.temperature),
                IsInline = true                               
            };

            var windField = new EmbedFieldBuilder
            {
                Name = "Wind",
                Value = $"{Windspeed(wToday.windSpeed)} - {Windbearing(wToday.windBearing)}",
                IsInline = true
            };

            var humField = new EmbedFieldBuilder
            {
                Name = "Humidty",
                Value = $"{wToday.humidity * 100}%",
                IsInline = true
            };

            var moonField = new EmbedFieldBuilder
            {
                Name = "Moon",
                Value = moonPhase,
                IsInline = true
            };

            var precipitation = "None";

            if (wToday.precipIntensity > 0.4 && wToday.precipProbability > 0.2)
            {
                precipitation = $"{wToday.precipProbability * 100}% of {wToday.precipType}";
            }

            var precipField = new EmbedFieldBuilder
            {
                Name = "Precipitation",
                Value = precipitation
            };

            weatherEmbed.Fields.Add(tempField);            
            weatherEmbed.Fields.Add(moonField);
            weatherEmbed.Fields.Add(windField);
            weatherEmbed.Fields.Add(humField);
            weatherEmbed.Fields.Add(precipField);

            return new DiscordMessage(weatherEmbed);
        }

        private async Task<DiscordMessage> WeatherForecast(string location)
        {            
            Coordinates coordinates;

            try
            {
                coordinates = await _lookup.Lookup(location);
            }
            catch
            {
                return new DiscordMessage("Could not find " + location);                
            }

            var weatherRequest = new WeatherLookup(_apiKeys.ForecastIoKey, coordinates.Latitude, coordinates.Longitude);
            WeatherData weather;

            try
            {
                weather = await weatherRequest.Get();
            }
            catch
            {
                return new DiscordMessage("Found " + coordinates.Name + " but could not find any weather there.");                
            }

            var forecastEmbed = new EmbedBuilder();

            forecastEmbed.Title = "3 day forecast for: " + coordinates.Name;                        
            var dailyWeather = weather.daily.data.Skip(2).Take(3);

            foreach (var dayWeather in dailyWeather)
            {
                var weatherIcon = _weatherEmoji[dayWeather.icon];

                var forecastField = new EmbedFieldBuilder
                {
                    Name = TimeFromEpoch(dayWeather.time).DayOfWeek.ToString(),
                    Value = $"{weatherIcon} {dayWeather.summary} {Temp(dayWeather.temperatureMin)} to {Temp(dayWeather.temperatureMax)}",
                };

                forecastEmbed.Fields.Add(forecastField);                                
            }

            return new DiscordMessage(forecastEmbed);
        }

        private DateTime TimeFromEpoch(int time)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(time);
        }

        private string Windspeed(double windSpeedKm)
        {
            var windSpeedM = windSpeedKm * 0.62137;

            return string.Format("{0} km/h ({1} mp/h)",
                Math.Round(windSpeedKm, MidpointRounding.AwayFromZero),
                Math.Round(windSpeedM, MidpointRounding.AwayFromZero));
        }

        private string Windbearing(double windBearing)
        {
            var windBearingIndex =  (windBearing / 22.5) + .5;

            return windBearingText[(int)windBearingIndex];
        }

        private string Temp(double temperatureC)
        {
            var temperatureF = temperatureC * 9 / 5 + 32;

            return string.Format("{0}°C ({1}°F)",
                Math.Round(temperatureC, MidpointRounding.AwayFromZero),
                Math.Round(temperatureF, MidpointRounding.AwayFromZero));
        }

        private DiscordMessage Remember(string user, string location)
        {
            if (_usersLocations.UserLocations.ContainsKey(user))
            {
                _usersLocations.UserLocations[user] = location;
                _usersLocations.Save();
                return new DiscordMessage("Remembering new location for: " + user);                

            }
            _usersLocations.UserLocations.Add(user, location);
            _usersLocations.Save();
            return new DiscordMessage("Remembering location for: " + user);            
        }

        private async void GiveHelp(SocketMessage m)
        {
            var response = HelpText();
            await m.Author.SendMessageAsync(response);
        }

        private string HelpText()
        {
            var helpText = new StringBuilder();

            helpText.AppendLine("Usage:");
            helpText.AppendLine("Weather - Gives Weather for a remembered location");
            helpText.AppendLine("Weather <location> - Gives Weather for a specificed location");
            helpText.AppendLine("Weather Forecast - Give Weather Forecast for a remembered location");
            helpText.AppendLine("Weather Forecast <location> Gives Weather Forecast for a specified location");
            helpText.AppendLine("Weather Remember <location> - Remembers a location for your nickname");

            return helpText.ToString();
        }                        
    }
}
