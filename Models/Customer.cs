using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AleksandarIvanov_NexaCore.Models
{
    public class Customer
    {
        [Key]
        [ForeignKey("User")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public virtual User? User { get; set; }

        // Additional customer-specific property.
        public bool IsActive { get; set; }
        
        // one‑to‑many to Subscription
        public virtual ICollection<Subscription> Subscriptions { get; set; }
                = new List<Subscription>();
    }
}
