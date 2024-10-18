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
        // Metod för att skicka privata meddelanden till Admin
        public async Task SendMessageToAdmin(string message, string fromUser)
        {
            try
            {
                _logger.LogInformation("SendMessageToAdmin called with message: {Message} from user: {FromUser}", message, fromUser);

                if (string.IsNullOrEmpty(fromUser))
                {
                    _logger.LogError("SendMessageToAdmin failed: fromUser is null or empty");
                    throw new InvalidOperationException("Sender username is required");
                }

                // Skicka meddelandet till alla klienter i "Admin"-gruppen
                await Clients.Group("Admin").SendAsync("ReceivePrivateMessage", message, fromUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in SendMessageToAdmin");
                throw;
            }
        }
        public async Task SendMessageToUser(string toUser, string message)
        {
            try
            {
                _logger.LogInformation("SendMessageToUser called with message: {Message} to user: {ToUser}", message, toUser);

                if (string.IsNullOrEmpty(toUser))
                {
                    _logger.LogError("SendMessageToUser failed: toUser is null or empty");
                    throw new InvalidOperationException("Recipient username is required");
                }

                // Skicka meddelandet direkt till användarens grupp
                await Clients.Group(toUser).SendAsync("ReceivePrivateMessage", message, "Admin");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in SendMessageToUser");
                throw;
            }
        }

        // När en klient ansluter
        public override async Task OnConnectedAsync()
        {
            var username = Context.GetHttpContext()?.Request.Query["username"].ToString();
            _logger.LogInformation("User connected with username: {Username}", username);

            if (!string.IsNullOrEmpty(username))
            {
                // Om användaren är Admin, lägg till i Admin-gruppen
                if (username == "Admin")
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "Admin");
                    _logger.LogInformation("Admin connected and added to Admin group");
                }
                else
                {
                    // Lägg till användaren i en grupp baserat på deras användarnamn
                    await Groups.AddToGroupAsync(Context.ConnectionId, username);
                    _logger.LogInformation("User added to group: {Username}", username);
                }

                // Skicka ett välkomstmeddelande till den anslutna användaren
                await Clients.Caller.SendAsync("ReceiveMessage", $"Välkommen, {username}!", "System");
            }

            await base.OnConnectedAsync();
        }

        // När en klient kopplar från
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var username = Context.GetHttpContext()?.Request.Query["username"].ToString();
            _logger.LogInformation("User disconnected with username: {Username}", username);

            if (!string.IsNullOrEmpty(username))
            {
                // Om användaren är Admin, ta bort från Admin-gruppen
                if (username == "Admin")
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admin");
                    _logger.LogInformation("Admin disconnected and removed from Admin group");
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}