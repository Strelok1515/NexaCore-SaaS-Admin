using System.Linq;
using System.Threading.Tasks;
using AleksandarIvanov_NexaCore.Data;
using AleksandarIvanov_NexaCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AleksandarIvanov_NexaCore.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _ctx;
        public ReportsController(ApplicationDbContext ctx) => _ctx = ctx;

        // GET: /Reports
        public async Task<IActionResult> Index()
        {
            // 1) Totals
            var totalPlans = await _ctx.SubscriptionPlans.CountAsync();
            var totalCustomers = await _ctx.Customers.CountAsync();
            var totalSubscriptions = await _ctx.Subscriptions.CountAsync();
            var totalRevenue = await _ctx.Invoices.SumAsync(i => i.Amount);

            // 2) Revenue grouped by plan
            var revenueByPlan = await _ctx.Invoices
                // project into flat anonymous, avoid grouping by NAV directly
                .Select(i => new {
                    Amount = i.Amount,
                    PlanId = i.Subscription.PlanId,
                    PlanName = i.Subscription.SubscriptionPlan.PlanName
                })
                .GroupBy(x => new { x.PlanId, x.PlanName })
                .Select(g => new PlanRevenue
                {
                    PlanId = g.Key.PlanId,
                    PlanName = g.Key.PlanName,
                    Revenue = g.Sum(x => x.Amount)
                })
                .ToListAsync();

            var vm = new ReportsViewModel
            {
                TotalPlans = totalPlans,
                TotalCustomers = totalCustomers,
                TotalSubscriptions = totalSubscriptions,
                TotalRevenue = totalRevenue,
                RevenueByPlan = revenueByPlan
            };

            return View(vm);
        }
        // NEW: GET /Reports/PlanSubscriptions/{planId}
        public async Task<IActionResult> PlanSubscriptions(int planId)
        {
            var plan = await _ctx.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.PlanId == planId);
            if (plan == null) return NotFound();

            var subs = await _ctx.Subscriptions
                .Include(s => s.Customer).ThenInclude(c => c.User)
                .Where(s => s.PlanId == planId)
                .ToListAsync();

            var details = subs.Select(s =>
            {
                var days = (s.EndDate - s.StartDate).Days;
                decimal baseAmt = plan.Price;
                int multiplier = days > 60 ? 3 : days > 30 ? 2 : 1;
                decimal amount = baseAmt * multiplier;
                bool discount = days > 90 && days <= 365;
                decimal discountAmt = discount ? amount * 0.2m : 0m;
                decimal finalAmt = amount - discountAmt;

                return new PlanSubscriptionDetail
                {
                    CustomerName = s.Customer.User.UserName,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    Days = days,
                    BaseAmount = baseAmt,
                    Multiplier = multiplier,
                    DiscountApplied = discount,
                    DiscountAmount = Math.Round(discountAmt, 2),
                    FinalAmount = Math.Round(finalAmt, 2)
                };
            }).ToList();

            var vm = new PlanSubscriptionsViewModel
            {
                PlanId = plan.PlanId,
                PlanName = plan.PlanName,
                Details = details
            };
            return View(vm);
        }
    }
}
    

