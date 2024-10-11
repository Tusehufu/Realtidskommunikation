using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Realtidskommunikation.Hubs;
using Newtonsoft.Json;

namespace Realtidskommunikation.Services
{
    public class WeatherBackgroundService : BackgroundService
    {
        private readonly WeatherService _weatherService;
        private readonly IHubContext<WeatherHub> _hubContext;
        private readonly ConcurrentDictionary<string, (double latitude, double longitude, Timer timer)> _userLocations = new();

        public WeatherBackgroundService(WeatherService weatherService, IHubContext<WeatherHub> hubContext)
        {
            _weatherService = weatherService;
            _hubContext = hubContext;
        }

        // Starta uppdateringar för en specifik användare baserat på deras latitud och longitud
        public void StartLocationUpdates(string connectionId, double latitude, double longitude)
        {
            // Om användaren redan har en timer, stoppa den
            if (_userLocations.TryGetValue(connectionId, out var existingData))
            {
                existingData.timer.Dispose();
            }

            // Starta en timer för att uppdatera vädret var 5:e minut för denna användare
            var timer = new Timer(async _ => await SendWeatherUpdateAsync(connectionId, latitude, longitude),
                                  null, TimeSpan.Zero, TimeSpan.FromMinutes(5));  // Omedelbar uppdatering och sedan varje 5:e minut

            _userLocations[connectionId] = (latitude, longitude, timer);
        }

        // Funktion för att hämta och skicka väderdata till en specifik användare
        private async Task SendWeatherUpdateAsync(string connectionId, double latitude, double longitude)
        {
            try
            {
                // Hämta väderdata baserat på latitud och longitud
                var weatherData = await _weatherService.GetWeatherByCoordinatesAsync(latitude, longitude);

                if (weatherData != null)
                {
                    // Serialisera objektet till JSON och skicka det till SignalR-klienten
                    var jsonData = JsonConvert.SerializeObject(weatherData);
                    await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveWeatherUpdate", jsonData);  // Skicka väderdata till specifik användare
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending weather update: {ex.Message}");
            }
        }

        // Ta bort användarens timer när de kopplar från
        public void StopLocationUpdates(string connectionId)
        {
            if (_userLocations.TryRemove(connectionId, out var data))
            {
                data.timer.Dispose();
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Här behöver vi inte göra något eftersom uppdateringar hanteras för varje användare
            return Task.CompletedTask;
        }
    }
}
