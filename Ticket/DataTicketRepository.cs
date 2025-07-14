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
            // Facoltativo: se vuoi salvare anche l'ID del messaggio per evitare duplicati
            return await _ctx.Tickets.FirstOrDefaultAsync(t => t.Corpo.Contains(graphMessageId));
        }

        public async Task<Ticket> CreateAsync(Ticket ticket)
        {
            _ctx.Tickets.Add(ticket);
            await _ctx.SaveChangesAsync();
            return ticket;
        }

        public IQueryable<Ticket> GetAll() => _ctx.Tickets.AsNoTracking();
    }
}