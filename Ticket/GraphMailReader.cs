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
        private string? _risposteFolderId;

        public GraphMailReader(string sharedMailboxAddress)
        {
            _client = GraphAuthProvider.GraphClient;
            _sharedMailboxAddress = sharedMailboxAddress;
        }

        private async Task<string?> GetRisposteFolderIdAsync()
        {
            if (_risposteFolderId != null)
                return _risposteFolderId;

            var childFolders = await _client
                .Users[_sharedMailboxAddress]
                .MailFolders["Inbox"]
                .ChildFolders
                .GetAsync();

            _risposteFolderId = childFolders?.Value?
                .FirstOrDefault(f => string.Equals(f.DisplayName, "Risposte", StringComparison.OrdinalIgnoreCase))?.Id;

            return _risposteFolderId;
        }

        public async Task<IEnumerable<Message>> GetNewMessagesAsync(DateTimeOffset since, string? fromEmail = null)
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
                .MailFolders["Inbox"]
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

            var result = messages?.Value?.ToList() ?? new List<Message>();

            var risposteId = await GetRisposteFolderIdAsync();
            if (!string.IsNullOrEmpty(risposteId))
            {
                var risposteMessages = await _client
                    .Users[_sharedMailboxAddress]
                    .MailFolders[risposteId]
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

                if (risposteMessages?.Value != null)
                    result.AddRange(risposteMessages.Value);
            }

            return result;
        }
    }
}