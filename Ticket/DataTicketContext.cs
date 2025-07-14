using Microsoft.EntityFrameworkCore;
using TicketingApp.Models;

namespace TicketingApp.Data
{
    public class TicketContext : DbContext
    {
        public TicketContext(DbContextOptions<TicketContext> options) : base(options) { }

        public DbSet<Ticket> Tickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Imposta il seed iniziale IDENTITY = 82
            modelBuilder.Entity<Ticket>()
                .Property(t => t.TicketId)
                .UseIdentityColumn(seed: 82, increment: 1);
        }
    }
}
