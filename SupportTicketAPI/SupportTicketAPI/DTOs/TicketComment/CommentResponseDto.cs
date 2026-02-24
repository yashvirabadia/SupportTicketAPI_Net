using SupportTicketAPI.DTOs.User;

namespace SupportTicketAPI.DTOs.TicketComment
{
    public class CommentResponseDto
    {
        public int Id { get; set; }
        public string Comment { get; set; } = string.Empty;
        public UserResponseDto User { get; set; }
        public DateTime CreatedAt { get; set; } 
    }
}
