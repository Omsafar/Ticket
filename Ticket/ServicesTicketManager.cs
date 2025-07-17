using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using TicketingApp.Graph;
using TicketingApp.Data;
using TicketingApp.Models;
using System.Text.RegularExpressions;

namespace TicketingApp.Services
{
    public class TicketManager
    {
        private readonly GraphMailReader _mailReader;
        private readonly TicketRepository _repo;
        private readonly GraphMailSender _mailSender;
        private DateTimeOffset _lastSync;

        private static bool TryGetTicketId(string? subject, out int ticketId)
        {
            ticketId = 0;
            if (string.IsNullOrEmpty(subject))
                return false;

            var match = Regex.Match(subject, @"TICKET\s+NUMERO\s*(\d+)", RegexOptions.IgnoreCase);
            return match.Success && int.TryParse(match.Groups[1].Value, out ticketId);
        }

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

                var convId = msg.ConversationId ?? string.Empty;
                if (TryGetTicketId(msg.Subject, out var ticketId))
                {
                    var openTicket = await _repo.FindOpenByIdAsync(ticketId);
                    if (openTicket != null && openTicket.ConversationId == convId)
                    {
                        await _repo.AppendMessageAsync(openTicket.TicketId,
                            msg.ReceivedDateTime?.UtcDateTime ?? DateTime.UtcNow,
                            msg.Body?.Content ?? string.Empty);
                        continue;
                    }
                }

                var existing = await _repo.FindByConversationIdAsync(convId);
                if (existing != null)
                {
                    await _repo.AppendMessageAsync(existing.TicketId,
                        msg.ReceivedDateTime?.UtcDateTime ?? DateTime.UtcNow,
                        msg.Body?.Content ?? string.Empty);
                    continue;
                }

                var ticket = new Ticket
                {
                    GraphMessageId = msg.Id,
                    ConversationId = convId,
                    MittenteEmail = msg.From?.EmailAddress?.Address ?? "unknown",
                    Oggetto = msg.Subject,
                    Corpo = msg.Body?.Content ?? string.Empty,
                    Stato = "Aperto",
                    DataApertura = msg.ReceivedDateTime?.UtcDateTime ?? DateTime.UtcNow,
                    DataUltimaModifica = msg.ReceivedDateTime?.UtcDateTime ?? DateTime.UtcNow
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