using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Emit;

namespace TicketingApp.Models
{
    public class Ticket
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TicketId { get; set; }

        [NotMapped]
        public string TicketCode => $"TICKET NUMERO {TicketId:D4}"; // es. 0082, 0083...

        [Required]
        public string MittenteEmail { get; set; }

        [Required]
        [MaxLength(255)]
        public string Oggetto { get; set; }

        public string Corpo { get; set; }

        [Required]
        [MaxLength(20)]
        public string Stato { get; set; } = "Aperto"; // default

        public DateTime DataApertura { get; set; } = DateTime.UtcNow;
        public DateTime DataUltimaModifica { get; set; } = DateTime.UtcNow;

        public string? GestoreEmail { get; set; }
    }
}

// ================================
// 4. Data/TicketContext.cs
// ================================
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