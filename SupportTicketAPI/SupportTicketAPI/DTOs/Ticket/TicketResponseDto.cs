using SupportTicketAPI.DTOs.User;

namespace SupportTicketAPI.DTOs.Ticket
{
    public class TicketResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public UserResponseDto CreatedBy { get; set; } = null!;
        public UserResponseDto? AssignedTo { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
