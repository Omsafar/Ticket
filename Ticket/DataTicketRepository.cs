using System;
using System.Linq;
using System.Threading.Tasks;
using TicketingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace TicketingApp.Data
{
    public class TicketRepository
    {
        private readonly TicketContext _ctx;
        public TicketRepository(TicketContext ctx) => _ctx = ctx;

        public async Task<Ticket?> FindByGraphMessageIdAsync(string graphMessageId)
        {
            return await _ctx.Tickets.FirstOrDefaultAsync(t => t.GraphMessageId == graphMessageId);
        }

        public async Task<Ticket> CreateAsync(Ticket ticket)
        {
            _ctx.Tickets.Add(ticket);
            await _ctx.SaveChangesAsync();
            return ticket;
        }

        public IQueryable<Ticket> GetAll() => _ctx.Tickets.AsNoTracking();

        public async Task UpdateStatusAsync(int ticketId, string newStatus)
        {
            var ticket = await _ctx.Tickets.FindAsync(ticketId);
            if (ticket == null)
                return;

            ticket.Stato = newStatus;
            ticket.DataUltimaModifica = DateTime.UtcNow;
            await _ctx.SaveChangesAsync();
        }
    }
}