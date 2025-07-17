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
        [MaxLength(200)]
        public string GraphMessageId { get; set; }

        [Required]
        [MaxLength(200)]
        public string ConversationId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Stato { get; set; } = "Aperto"; // default

        [MaxLength(20)]
        public string? StatoPrecedente { get; set; }

        public DateTime DataApertura { get; set; } = DateTime.UtcNow;
        public DateTime DataUltimaModifica { get; set; } = DateTime.UtcNow;

        public string? GestoreEmail { get; set; }
    }
}