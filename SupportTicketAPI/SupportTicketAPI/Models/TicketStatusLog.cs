using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupportTicketAPI.Models
{
    public class TicketStatusLog
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int TicketId { get; set; }
        [ForeignKey(nameof(TicketId))]
        public Ticket Ticket { get; set; }
        [Column(TypeName = "nvarchar(20)")]
        public TicketStatus OldStatus { get; set; }
        [Column(TypeName = "nvarchar(20)")]
        public TicketStatus NewStatus { get; set; }
        [Required]
        public int ChangedById { get; set; }
        [ForeignKey(nameof(ChangedById))]
        public User ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    }
}
