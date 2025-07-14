using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace TicketingApp.Graph
{
    public class GraphMailReader
    {
        private readonly GraphServiceClient _client;
        private readonly string _sharedMailboxAddress;

        public GraphMailReader(string sharedMailboxAddress)
        {
            _client = GraphAuthProvider.GraphClient;
            _sharedMailboxAddress = sharedMailboxAddress;
        }

        public async Task<IEnumerable<Message>> GetNewMessagesAsync(DateTimeOffset since)
        {
            // Filtro: ricevute dopo `since`, solo Inbox.
            var filter = $"receivedDateTime ge {since.UtcDateTime:o}";

            var messages = await _client
                .Users[_sharedMailboxAddress]
                .MailFolders["Inbox"]
                .Messages
                .Request()
                .Filter(filter)
                .Select(m => new
                {
                    m.Id,
                    m.Subject,
                    m.Body,
                    m.ReceivedDateTime,
                    m.Sender,
                    m.From,
                    m.ToRecipients
                })
                .Top(50)
                .GetAsync();

            return messages.CurrentPage;
        }
    }
}