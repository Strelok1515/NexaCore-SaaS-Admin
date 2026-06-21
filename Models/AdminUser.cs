using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AleksandarIvanov_NexaCore.Models
{
    public class AdminUser
    {
        [Key]
        public int AdminUserId { get; set; }

        // Foreign key to your Identity User
        [Required(ErrorMessage = "User ID is required.")]
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        // Navigation back to the AspNet‐user
        public virtual User User { get; set; } = null!;

        [Required(ErrorMessage = "Role is required.")]
        public string Role { get; set; } = null!;

        public virtual ICollection<AuditLog> AuditLogs { get; set; }
            = new List<AuditLog>();
    }
}
