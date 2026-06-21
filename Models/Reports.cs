using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AleksandarIvanov_NexaCore.Models
{
    public class ReportsViewModel
    {
        public int TotalPlans { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalSubscriptions { get; set; }
        public decimal TotalRevenue { get; set; }

        public List<PlanRevenue> RevenueByPlan { get; set; }
    }

    public class PlanRevenue
    {
        // New: carry the ID so we can link to Details
        public int PlanId { get; set; }

        public string PlanName { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Revenue { get; set; }
    }
}
