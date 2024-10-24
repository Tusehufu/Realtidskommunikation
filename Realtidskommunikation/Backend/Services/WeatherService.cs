using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Backend.Models;

namespace Realtidskommunikation.Services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "8R9REPZB4WMM54L4P4AAUTX7S";  

        public WeatherService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // metod för att hämta väderdata baserat på latitud och longitud
        public async Task<WeatherInfo> GetWeatherByCoordinatesAsync(double latitude, double longitude)
        {
            var formattedLatitude = latitude.ToString("F6", CultureInfo.InvariantCulture);
            var formattedLongitude = longitude.ToString("F6", CultureInfo.InvariantCulture);

            // Bygg upp URL:en
            var url = $"https://weather.visualcrossing.com/VisualCrossingWebServices/rest/services/timeline/{formattedLatitude},{formattedLongitude}?unitGroup=metric&key={_apiKey}&contentType=json";

            return await FetchWeatherData(url);
        }


        // Gemensam metod för att hämta och parsa väderdata från Visual Crossing API
        private async Task<WeatherInfo> FetchWeatherData(string url)
        {
            try
            {
                var response = await _httpClient.GetStringAsync(url);

                var weatherData = JObject.Parse(response);

                var currentConditions = weatherData["currentConditions"];

                return new WeatherInfo
                {
                    Temperature = currentConditions["temp"].ToString(),
                    Condition = currentConditions["conditions"].ToString(),
                    Humidity = currentConditions["humidity"].ToString(),
                    WindSpeed = currentConditions["windspeed"].ToString()
                };
            }
            catch (HttpRequestException ex)
            {
                // Hantera fel vid API-anrop och logga det
                Console.WriteLine($"Error fetching weather data from API: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                // Hantera eventuella andra fel och logga det
                Console.WriteLine($"General error in FetchWeatherData: {ex.Message}");
                return null;
            }
        }
    }
}
