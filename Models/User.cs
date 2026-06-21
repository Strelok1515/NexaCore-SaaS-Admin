using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AleksandarIvanov_NexaCore.Models
{
    public class User : IdentityUser<int>
    {
        // The inherited 'Id' property serves as UserId.
        [Key]
        public override int Id { get; set; }  // UserId from the ER diagram

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(30, ErrorMessage = "Name cannot exceed 30 characters.")]
        public override required string? UserName { get; set; }

        // Email and PasswordHash are inherited from IdentityUser<int>.
        // Email validation and storage are handled by Identity.

        [StringLength(100, ErrorMessage = "Address cannot exceed 100 characters.")]
        public string? Address { get; set; }

        public bool IsAdmin { get; set; }

        // Optional: If you want a one-to-one relationship with a Customer profile.
        public virtual Customer? CustomerProfile { get; set; }
    }
}
