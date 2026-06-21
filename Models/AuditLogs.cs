using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AleksandarIvanov_NexaCore.Models
{
    public class AuditLog
    {
        [Key]
        public int LogId { get; set; }

        [ForeignKey("AdminUser")]
        public int AdminUserId { get; set; }
        public virtual AdminUser? AdminUser { get; set; }

        // New property to track the type of action (e.g., "Create", "Update", "Delete")
        [Required(ErrorMessage = "Action is required.")]
        public string? Action { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        
        [Required(ErrorMessage = "Description is required.")]
        public string? Description { get; set; }
    }
}
