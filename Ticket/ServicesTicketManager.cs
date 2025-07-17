using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using TicketingApp.Graph;
using TicketingApp.Data;
using TicketingApp.Models;
using System.Text.RegularExpressions;
using TicketingApp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace TicketingApp.Services
{
    public class TicketManager
    {
        private readonly GraphMailReader _mailReader;
        private readonly IServiceScopeFactory _scopeFactory;
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

        public TicketManager(GraphMailReader mailReader, IServiceScopeFactory scopeFactory, GraphMailSender mailSender)
        {
            _mailReader = mailReader;
            _scopeFactory = scopeFactory;
            _mailSender = mailSender;
            // All'avvio leggiamo tutta la casella
            _lastSync = DateTimeOffset.MinValue;
        }

        public async Task SendTicketReopenedNotificationAsync(string toAddress, int ticketId)
        {
            await _mailSender.SendTicketReopenedNotificationAsync(
                "support.ticket@paratorispa.it",
                toAddress,
                ticketId);
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

            var tasks = new List<Task>();
            foreach (var msg in newMessages)
            {
                if (msg.ReceivedDateTime <= _lastSync)
                    continue;

                tasks.Add(ProcessMessageAsync(msg));
            }

            await Task.WhenAll(tasks);

            _lastSync = DateTimeOffset.UtcNow;

            if (newMessages != null && newMessages.Any())
                TicketsSynced?.Invoke();
        }

        private static bool IsUniqueViolation(DbUpdateException ex)
        {
            return ex.InnerException is SqlException sql && (sql.Number == 2601 || sql.Number == 2627);
        }

        private async Task ProcessMessageAsync(Microsoft.Graph.Models.Message msg)
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<TicketRepository>();
            var ctx = scope.ServiceProvider.GetRequiredService<TicketContext>();

            await using var tx = await ctx.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            if (await repo.FindByGraphMessageIdAsync(msg.Id) != null)
            {
                await tx.CommitAsync();
                return;
            }

            var convId = msg.ConversationId ?? string.Empty;
            var existing = await repo.FindByConversationIdAsync(convId);
            if (existing != null)
            {
                if (string.Equals(existing.Stato, "Chiuso", StringComparison.OrdinalIgnoreCase))
                {
                    await _mailSender.SendTicketClosedInfoAsync(
                        "support.ticket@paratorispa.it",
                        msg.From?.EmailAddress?.Address ?? "unknown",
                        existing.TicketId);
                    await tx.CommitAsync();
                    return;
                }

                existing.DataUltimaModifica = msg.ReceivedDateTime?.UtcDateTime ?? DateTime.UtcNow;
                existing.Corpo += "\n---\n" + HtmlUtils.ToPlainText(msg.Body?.Content);
                await repo.UpdateAsync(existing);
                await tx.CommitAsync();
                return;
            }

            if (TryGetTicketId(msg.Subject, out var subjectTicketId))
            {
                var byId = await repo.FindByIdAsync(subjectTicketId);
                if (byId != null && string.Equals(byId.Stato, "Chiuso", StringComparison.OrdinalIgnoreCase))
                {
                    await _mailSender.SendTicketClosedInfoAsync(
                        "support.ticket@paratorispa.it",
                        msg.From?.EmailAddress?.Address ?? "unknown",
                        byId.TicketId);
                    await tx.CommitAsync();
                    return;
                }
            }

            var ticket = new Ticket
            {
                GraphMessageId = msg.Id,
                ConversationId = convId,
                MittenteEmail = msg.From?.EmailAddress?.Address ?? "unknown",
                Oggetto = msg.Subject,
                Corpo = HtmlUtils.ToPlainText(msg.Body?.Content),
                Stato = "Aperto",
                DataApertura = msg.ReceivedDateTime?.UtcDateTime ?? DateTime.UtcNow,
                DataUltimaModifica = msg.ReceivedDateTime?.UtcDateTime ?? DateTime.UtcNow
            };

            var created = false;
            try
            {
                await repo.CreateAsync(ticket);
                created = true;
                await tx.CommitAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                await tx.RollbackAsync();
                var other = await repo.FindByGraphMessageIdAsync(msg.Id);
                if (other != null)
                {
                    ticket = other;
                }
            }

            if (created)
            {
                await _mailSender.SendTicketCreatedNotificationAsync(
                    "support.ticket@paratorispa.it",
                    ticket.MittenteEmail,
                    ticket.TicketId,
                    ticket.Oggetto,
                    ticket.Corpo);
            }
        }
        public async Task<Ticket> CreateManualTicketAsync(string email, string subject, string body)
        {
            var ticket = new Ticket
            {
                GraphMessageId = Guid.NewGuid().ToString(),
                ConversationId = Guid.NewGuid().ToString(),
                MittenteEmail = email,
                Oggetto = subject,
                Corpo = body,
                Stato = "Aperto",
                DataApertura = DateTime.UtcNow,
                DataUltimaModifica = DateTime.UtcNow
            };

            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<TicketRepository>();
            var ctx = scope.ServiceProvider.GetRequiredService<TicketContext>();
            await using var tx = await ctx.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            await repo.CreateAsync(ticket);
            await tx.CommitAsync();

            await _mailSender.SendTicketCreatedNotificationAsync(
                "support.ticket@paratorispa.it",
                ticket.MittenteEmail,
                ticket.TicketId,
                ticket.Oggetto,
                ticket.Corpo);

            NotifyTicketsChanged();
            return ticket;
        }
    }
}