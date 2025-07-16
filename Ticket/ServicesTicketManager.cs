using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using TicketingApp.Graph;
using TicketingApp.Data;
using TicketingApp.Models;

namespace TicketingApp.Services
{
    public class TicketManager
    {
        private readonly GraphMailReader _mailReader;
        private readonly TicketRepository _repo;
        private readonly GraphMailSender _mailSender;
        private DateTimeOffset _lastSync;

        public bool CanSync { get; set; } = true;
        public string CurrentUserEmail { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }

        public event Action? TicketsSynced;

        public void NotifyTicketsChanged() => TicketsSynced?.Invoke();

        public TicketManager(GraphMailReader mailReader, TicketRepository repo, GraphMailSender mailSender)
        {
            _mailReader = mailReader;
            _repo = repo;
            _mailSender = mailSender;
            // All'avvio leggiamo tutta la casella
            _lastSync = DateTimeOffset.MinValue;
        }

        public async Task SyncAsync(CancellationToken ct)
        {
            if (!CanSync)
            {
                TicketsSynced?.Invoke();
                return;
            }

            var filterEmail = IsAdmin ? null : CurrentUserEmail;
            var newMessages = await _mailReader.GetNewMessagesAsync(_lastSync, filterEmail);
            foreach (var msg in newMessages)
            {
                if (await _repo.FindByGraphMessageIdAsync(msg.Id) != null)
                    continue;
                var ticket = new Ticket
                {
                    GraphMessageId = msg.Id,
                    MittenteEmail = msg.From?.EmailAddress?.Address ?? "unknown",
                    Oggetto = msg.Subject,
                    Corpo = msg.Body?.Content ?? string.Empty,
                    Stato = "Aperto",
                    DataApertura = msg.ReceivedDateTime?.UtcDateTime ?? DateTime.UtcNow,
                    DataUltimaModifica = DateTime.UtcNow
                };

                await _repo.CreateAsync(ticket);
                await _mailSender.SendTicketCreatedNotificationAsync(
                "support.ticket@paratorispa.it",
                ticket.MittenteEmail,
                ticket.TicketId,
                ticket.Oggetto);
            }
            _lastSync = DateTimeOffset.UtcNow;


            if (newMessages != null && newMessages.Any())
                TicketsSynced?.Invoke();
        }
    }
}