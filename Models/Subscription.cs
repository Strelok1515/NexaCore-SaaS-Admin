using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AleksandarIvanov_NexaCore.Models
{
    public class Subscription
    {
        [Key]
        public int SubscriptionId { get; set; }

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }

        [ForeignKey("SubscriptionPlan")]
        public int PlanId { get; set; }
        public virtual SubscriptionPlan? SubscriptionPlan { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required.")]
        public DateTime EndDate { get; set; }

        public virtual ICollection<Invoice> Invoices { get; set; }
           = new List<Invoice>();
    }
}
