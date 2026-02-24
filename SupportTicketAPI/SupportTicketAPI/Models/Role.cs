using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupportTicketAPI.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(20)")]
        public RoleName Name { get; set; }

        public ICollection<User> Users { get; set; }
    }

    public enum RoleName
    {
        User,
        Support,
        Manager
    }
}
