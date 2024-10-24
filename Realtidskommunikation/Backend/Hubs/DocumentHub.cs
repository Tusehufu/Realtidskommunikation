using Microsoft.AspNetCore.SignalR;
using Backend.Models;


namespace Realtidskommunikation.Hubs
{
    public class DocumentHub : Hub
    {
        public async Task UpdateDocument(DocumentData documentData)
        {
            await Clients.Others.SendAsync("ReceiveDocumentUpdate", documentData);
        }
    }

}
