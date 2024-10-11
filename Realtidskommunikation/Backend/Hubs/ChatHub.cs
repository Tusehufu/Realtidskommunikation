using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Realtidskommunikation.Hubs
{

    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(ILogger<ChatHub> logger)
        {
            _logger = logger;
        }

        // Metod justerad för att ta emot både message och username som parametrar
        public async Task SendMessage(string message, string username)
        {
            try
            {
                _logger.LogInformation("SendMessage called with message: {Message} and username: {Username}", message, username);

                if (string.IsNullOrEmpty(username))
                {
                    _logger.LogError("SendMessage failed: Username is null or empty");
                    throw new InvalidOperationException("Username is required");
                }

                // Skicka meddelandet till alla anslutna klienter
                await Clients.All.SendAsync("ReceiveMessage", message, username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in SendMessage");
                throw;
            }
        }
    }
}