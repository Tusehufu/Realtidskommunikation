using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Realtidskommunikation.Hubs
{
    public class GreenhouseHub : Hub
    {
        private readonly GreenhouseService _greenhouseService;

        // Injektera GreenhouseService via konstruktorn
        public GreenhouseHub(GreenhouseService greenhouseService)
        {
            _greenhouseService = greenhouseService;
        }

        public async Task SendSensorData(string sensorData)
        {
            // Skicka sensoruppdateringar till alla klienter
            await Clients.All.SendAsync("ReceiveSensorData", sensorData);
        }

        public async Task ControlDevice(string device, string command)
        {
            // Skicka kommandot till alla anslutna klienter för loggning och visning
            await Clients.All.SendAsync("DeviceControl", device, command);

            // Använd instansen av GreenhouseService för att hantera kommandot
            _greenhouseService.ExecuteCommand(device, command);
        }
    }
}