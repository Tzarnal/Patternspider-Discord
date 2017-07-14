using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Serilog;

namespace PatternSpider_Discord.Plugins.Weather
{
    class Weather : IPatternSpiderPlugin
    {
        public string Name => "Weather";        
        public List<string> Commands => new List<string> { "weather" };

        private readonly UsersLocations _usersLocations;
        private readonly ApiKeys _apiKeys;
        private readonly GeoCodeLookup _lookup;

        public Weather()
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

        public async Task Command(string command, string messsage, SocketMessage m)
        {
            var text = messsage.Trim();
            var messageParts = text.Split(' ');
            List<string> response = new List<string>();
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

            var finalResponse = "";
            foreach (var line in response)
            {
                finalResponse += line + Environment.NewLine;
            }

            await m.Channel.SendMessageAsync(finalResponse);
        }

        public Task Message(string messsage, SocketMessage m)
        {
            return Task.CompletedTask;
        }

        private async Task<List<string>> WeatherToday(string location)
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

                return new List<string> { "Could not find " + location };
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

                return new List<string> { "Found " + coordinates.Name + " but could not find any weather there." };
            }

            var wToday = weather.currently;

            var output = new List<string> {
                $"Weather for {coordinates.Name}: {Temp(wToday.temperature)} and {wToday.summary}, {wToday.humidity * 100}% Humidity and {Windspeed(wToday.windSpeed)} Winds."
            };

            return output;
        }

        private async Task<List<string>> WeatherForecast(string location)
        {
            List<string> output = new List<string>();
            Coordinates coordinates;

            try
            {
                coordinates = await _lookup.Lookup(location);
            }
            catch
            {
                return new List<string> { "Could not find " + location };
            }

            var weatherRequest = new WeatherLookup(_apiKeys.ForecastIoKey, coordinates.Latitude, coordinates.Longitude);
            WeatherData weather;

            try
            {
                weather = await weatherRequest.Get();
            }
            catch
            {
                return new List<string> { "Found " + coordinates.Name + " but could not find any weather there." };
            }


            output.Add("3 day forecast for: " + coordinates.Name);

            var dailyWeather = weather.daily.data.Skip(2).Take(3);

            foreach (var dayWeather in dailyWeather)
            {
                output.Add(
                    $"{TimeFromEpoch(dayWeather.time).DayOfWeek}: {dayWeather.summary} {Temp(dayWeather.temperatureMin)} to {Temp(dayWeather.temperatureMax)}");
            }

            return output;
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

        private string Temp(double temperatureC)
        {
            var temperatureF = temperatureC * 9 / 5 + 32;

            return string.Format("{0}°C ({1}°F)",
                Math.Round(temperatureC, MidpointRounding.AwayFromZero),
                Math.Round(temperatureF, MidpointRounding.AwayFromZero));
        }

        private List<string> Remember(string user, string location)
        {
            if (_usersLocations.UserLocations.ContainsKey(user))
            {
                _usersLocations.UserLocations[user] = location;
                _usersLocations.Save();
                return new List<string> { "Remembering new location for: " + user };

            }
            _usersLocations.UserLocations.Add(user, location);
            _usersLocations.Save();
            return new List<string> { "Remembering location for: " + user };
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
