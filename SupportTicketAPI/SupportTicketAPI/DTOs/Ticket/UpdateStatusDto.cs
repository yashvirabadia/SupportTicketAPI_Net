using SupportTicketAPI.Models;

namespace SupportTicketAPI.DTOs.Ticket
{
    public class UpdateStatusDto
    {
        public TicketStatus Status { get; set; }
    }
}
