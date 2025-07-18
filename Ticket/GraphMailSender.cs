using Microsoft.Graph;
using Microsoft.Graph.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TicketingApp.Graph
{
    public class GraphMailSender
    {
        private readonly GraphServiceClient _graph;

        public GraphMailSender()
        {
            _graph = GraphAuthProvider.GraphClient;
        }

        public async Task SendTicketCreatedNotificationAsync(
            string fromSharedMailbox,
            string toAddress,
            int ticketId,
            string originalSubject,
            string originalBody)
        {
            var mail = new Message
            {
                Subject = $"TICKET NUMERO {ticketId:D4}  {originalSubject}",
                Body = new ItemBody
                {
                    ContentType = BodyType.Text,
                    Content =
                        $"Il tuo ticket [TICKET NUMERO {ticketId:D4}] è stato aperto con successo." +
                        "\n\n"+"Oggetto: " + originalSubject + "\n" + "Corpo del messaggio: "+ originalBody
                },
                ToRecipients = new List<Recipient>
                {
                    new Recipient { EmailAddress = new EmailAddress { Address = toAddress } }
                }
            };
            var requestBody = new Microsoft.Graph.Users.Item.SendMail.SendMailPostRequestBody
            {
                Message = mail,
                SaveToSentItems = true
            };

            await _graph
                .Users[fromSharedMailbox]
                 .SendMail
                .PostAsync(requestBody);
        }
        public async Task SendTicketReopenedNotificationAsync(
           string fromSharedMailbox,
           string toAddress,
           int ticketId)
        {
            var mail = new Message
            {
                Subject = $"TICKET NUMERO {ticketId:D4} riaperto",
                Body = new ItemBody
                {
                    ContentType = BodyType.Text,
                    Content = $"Il tuo ticket [TICKET NUMERO {ticketId:D4}] è stato riaperto."
                },
                ToRecipients = new List<Recipient>
                {
                    new Recipient { EmailAddress = new EmailAddress { Address = toAddress } }
                }
            };
            var requestBody = new Microsoft.Graph.Users.Item.SendMail.SendMailPostRequestBody
            {
                Message = mail,
                SaveToSentItems = true
            };

            await _graph
                .Users[fromSharedMailbox]
                .SendMail
                .PostAsync(requestBody);
        }
        public async Task SendTicketClosedAutoReplyAsync(
    string fromSharedMailbox,
    string toAddress,
    int ticketId)
        {
            var mail = new Message
            {
                Subject = $"TICKET NUMERO {ticketId:D4} chiuso",
                Body = new ItemBody
                {
                    ContentType = BodyType.Text,
                    Content = "Il ticket è chiuso: attendere che l’amministratore legga il messaggio e lo riapra"
                },
                ToRecipients = new List<Recipient>
                {
                    new Recipient { EmailAddress = new EmailAddress { Address = toAddress } }
                }
            };
            var requestBody = new Microsoft.Graph.Users.Item.SendMail.SendMailPostRequestBody
            {
                Message = mail,
                SaveToSentItems = true
            };

            await _graph
                .Users[fromSharedMailbox]
                .SendMail
                .PostAsync(requestBody);
        }
    }
}