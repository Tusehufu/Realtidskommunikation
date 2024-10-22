using Microsoft.AspNetCore.SignalR;
using Realtidskommunikation.Hubs;

public class GreenhouseBackgroundService : BackgroundService
{
    private readonly IHubContext<GreenhouseHub> _hubContext;
    private readonly Random _random = new Random();

    public GreenhouseBackgroundService(IHubContext<GreenhouseHub> hubContext)
    {
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Simulera sensordata för temperatur och luftfuktighet
            var temperature = _random.Next(10, 28); // Slumpmässig temperatur mellan 10°C och 28°C
            var humidity = _random.Next(50, 99); // Slumpmässig luftfuktighet mellan 50% och 99%

            var sensorData = $"Temperatur: {temperature}°C, Luftfuktighet: {humidity}%";

            // Skicka sensordata till alla anslutna klienter via SignalR
            await _hubContext.Clients.All.SendAsync("ReceiveSensorData", sensorData);

            // Vänta i 5 sekunder innan nästa uppdatering
            await Task.Delay(50000, stoppingToken);
        }
    }
}
