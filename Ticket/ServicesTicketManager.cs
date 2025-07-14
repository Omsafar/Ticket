using System;
using System.Threading;
using System.Threading.Tasks;
using TicketingApp.Graph;
using TicketingApp.Data;
using TicketingApp.Models;

namespace TicketingApp.Services
{
    public class TicketManager
    {
        private readonly GraphMailReader _mailReader;
        private readonly TicketRepository _repo;
        private DateTimeOffset _lastSync;

        public TicketManager(GraphMailReader mailReader, TicketRepository repo)
        {
            _mailReader = mailReader;
            _repo = repo;
            _lastSync = DateTimeOffset.UtcNow.AddMinutes(-10); // prima sync ultimi 10 minuti
        }

        public async Task SyncAsync(CancellationToken ct)
        {
            var newMessages = await _mailReader.GetNewMessagesAsync(_lastSync);
            foreach (var msg in newMessages)
            {
                // Crea ticket per ogni mail
                var ticket = new Ticket
                {
                    MittenteEmail = msg.From?.EmailAddress?.Address ?? "unknown",
                    Oggetto = msg.Subject,
                    Corpo = msg.Body?.Content ?? string.Empty,
                    Stato = "Aperto",
                    DataApertura = msg.ReceivedDateTime?.UtcDateTime ?? DateTime.UtcNow,
                    DataUltimaModifica = DateTime.UtcNow
                };

                await _repo.CreateAsync(ticket);
            }
            _lastSync = DateTimeOffset.UtcNow;
        }
    }
}