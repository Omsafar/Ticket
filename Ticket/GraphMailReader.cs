using Microsoft.Graph;
using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketingApp.Graph
{
    public class GraphMailReader
    {
        private readonly GraphServiceClient _client;
        private readonly string _sharedMailboxAddress;
        private readonly string _risposteFolderId;

        public GraphMailReader(string sharedMailboxAddress, string risposteFolderId)
        {
            _client = GraphAuthProvider.GraphClient;
            _sharedMailboxAddress = sharedMailboxAddress;
            _risposteFolderId = risposteFolderId;
        }

        public string RisposteFolderId => _risposteFolderId;

        public async Task<IEnumerable<Message>> GetNewMessagesAsync(DateTimeOffset since, string? fromEmail = null, string folderId = "Inbox")
        {
            // Filtro: ricevute dopo `since`, solo Inbox.
            var filter = $"receivedDateTime ge {since.UtcDateTime:o}";
            if (!string.IsNullOrEmpty(fromEmail))
            {
                var emailEscaped = fromEmail.Replace("'", "''");
                filter += $" and from/emailAddress/address eq '{emailEscaped}'";
            }

            var messages = await _client
                .Users[_sharedMailboxAddress]
                .MailFolders[folderId]
                .Messages
                .GetAsync(req =>
                {
                    req.QueryParameters.Filter = filter;
                    req.QueryParameters.Top = 50;
                    req.QueryParameters.Select = new[]
                    {
                        "id",
                        "subject",
                        "body",
                        "conversationId",
                        "receivedDateTime",
                        "sender",
                        "from",
                        "toRecipients"
                    };
                });

            return messages?.Value ?? Enumerable.Empty<Message>();
        }
    }
}