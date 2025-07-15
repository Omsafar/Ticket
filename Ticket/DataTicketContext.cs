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
            base.OnModelCreating(modelBuilder);

            // Fai sì che l'entità Ticket venga letta da dbo.Ticket (singolare)
            modelBuilder.Entity<Ticket>()
                .ToTable("Ticket");           // <— nome tabella esatto nel DB

            // (Già presente) imposta l'identity seed a 82
            modelBuilder.Entity<Ticket>()
                .Property(t => t.TicketId)
                .UseIdentityColumn(seed: 82, increment: 1);
        }

    }
}

