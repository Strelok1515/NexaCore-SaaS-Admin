using System.ComponentModel.DataAnnotations;

namespace AleksandarIvanov_NexaCore.Models
{
    public class SubscriptionPlan
    {
        [Key]
        public int PlanId { get; set; }

        [Required(ErrorMessage = "Plan name is required.")]
        public string? PlanName { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Price cannot be negative.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Duration is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Duration must be at least 1 day.")]
        public int Duration { get; set; }

        [MaxLength(2000, ErrorMessage = "Features text is too long.")]
        public string? Features { get; set; }
        
        // ← Add this collection navigation
        public virtual ICollection<Subscription> Subscriptions { get; set; }
                 = new List<Subscription>();
    }
}
