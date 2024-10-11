using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Realtidskommunikation.Services;

namespace Realtidskommunikation.Hubs
{
    public class WeatherHub : Hub
    {
        private readonly WeatherService _weatherService;
        private readonly WeatherBackgroundService _backgroundService;

        public WeatherHub(WeatherService weatherService, WeatherBackgroundService backgroundService)
        {
            _weatherService = weatherService;
            _backgroundService = backgroundService;
        }

        public async Task SendLocation(double latitude, double longitude)
        {
            try
            {
                // Logga att vi tagit emot koordinater
                Console.WriteLine($"Received location: Latitude={latitude}, Longitude={longitude}");

                // Hämta väderdata baserat på latitud och longitud
                var weatherInfo = await _weatherService.GetWeatherByCoordinatesAsync(latitude, longitude);

                if (weatherInfo != null)
                {
                    var jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(weatherInfo);
                    // Skicka väderdata till klienten
                    await Clients.Caller.SendAsync("ReceiveWeatherUpdate", jsonData);
                }
                else
                {
                    Console.WriteLine("WeatherInfo is null. Could not retrieve weather data.");
                    throw new Exception("Could not retrieve weather data.");
                }

                // Starta bakgrundsuppdateringar för denna användare
                _backgroundService.StartLocationUpdates(Context.ConnectionId, latitude, longitude);
            }
            catch (Exception ex)
            {
                // Logga alla fel som uppstår i processen
                Console.WriteLine($"Error in SendLocation: {ex.Message}");
                throw;  // Kasta om felet för att logga det och stänga anslutningen korrekt
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _backgroundService.StopLocationUpdates(Context.ConnectionId);  // Sluta skicka uppdateringar till denna användare
            await base.OnDisconnectedAsync(exception);
        }
    }
}
