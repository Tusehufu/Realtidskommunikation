using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Realtidskommunikation.Hubs
{
    public class GreenhouseHub : Hub
    {
        private static bool _isWindowOpen = false;
        private static bool _isWatering = false;
        private readonly GreenhouseService _greenhouseService;

        public GreenhouseHub(GreenhouseService greenhouseService)
        {
            _greenhouseService = greenhouseService;
        }

        public async Task ControlDevice(string device, string command)
        {
            // Anropa tjänsten för att utföra kommandot
            _greenhouseService.ExecuteCommand(device, command);

            // Uppdatera det globala tillståndet
            switch (device.ToLower())
            {
                case "window":
                    _isWindowOpen = command == "open";
                    break;
                case "watering":
                    _isWatering = command == "start";
                    break;
            }

            // Skicka det uppdaterade tillståndet till alla anslutna klienter
            await Clients.All.SendAsync("UpdateState", _isWindowOpen, _isWatering);
        }
    }

}