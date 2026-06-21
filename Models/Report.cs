namespace AleksandarIvanov_NexaCore.Models
{
    public class PlanSubscriptionDetail
    {
        public string CustomerName { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Days { get; set; }
        public decimal BaseAmount { get; set; }
        public int Multiplier { get; set; }
        public bool DiscountApplied { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
    }

    public class PlanSubscriptionsViewModel
    {
        public int PlanId { get; set; }
        public string PlanName { get; set; } = null!;
        public List<PlanSubscriptionDetail> Details { get; set; } = new();
    }
}
