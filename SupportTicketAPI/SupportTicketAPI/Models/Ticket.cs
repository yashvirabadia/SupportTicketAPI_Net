using Microsoft.AspNetCore.Authentication.Cookies;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupportTicketAPI.Models
{
    public class Ticket
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(255)]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Column(TypeName = "nvarchar(20)")]
        public TicketStatus Status { get; set; } = TicketStatus.Open;
        [Column(TypeName = "nvarchar(10)")]
        public TicketPriority Priority { get; set; } = TicketPriority.Medium;
        [Required]
        public int CreatedById { get; set; }
        [ForeignKey("CreatedById")]
        public User CreatedBy { get; set; }
        public int? AssignedToId { get; set; }
        [ForeignKey("AssignedToId")]
        public User AssignedTo { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<TicketComment> Comments { get; set; }
        public ICollection<TicketStatusLog> StatusLogs { get; set; }
    }

    public enum TicketStatus { Open, In_Progress, Resolved, Closed }
    public enum TicketPriority { Low, Medium, High }
    }

