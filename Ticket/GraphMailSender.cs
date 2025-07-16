using Microsoft.Graph;
using Microsoft.Graph.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TicketingApp.Graph
{
    public class GraphMailSender
    {
        private readonly GraphServiceClient _graph;

        public GraphMailSender(GraphServiceClient graphClient)
        {
            _graph = graphClient;
        }

        public async Task SendTicketCreatedNotificationAsync(
            string fromSharedMailbox,
            string toAddress,
            int ticketId,
            string originalSubject)
        {
            var mail = new Message
            {
                Subject = $"TICKET NUMERO {ticketId:D4} – {originalSubject}",
                Body = new ItemBody
                {
                    ContentType = BodyType.Text,
                    Content = $"Il tuo ticket [TICKET NUMERO {ticketId:D4}] è stato aperto con successo."
                },
                ToRecipients = new List<Recipient>
                {
                    new Recipient { EmailAddress = new EmailAddress { Address = toAddress } }
                }
            };

            await _graph
                .Users[fromSharedMailbox]
                .SendMail(mail, saveToSentItems: true)
                .Request()
                .PostAsync();
        }
    }
}