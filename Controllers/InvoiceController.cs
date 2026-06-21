using AleksandarIvanov_NexaCore.Data;
using AleksandarIvanov_NexaCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AleksandarIvanov_NexaCore.Controllers
{
    [Authorize]
    public class InvoiceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public InvoiceController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var isAdmin = User.HasClaim("IsAdmin", "True");
            int? currentUserId = isAdmin ? null : int.Parse(_userManager.GetUserId(User));

            var invoicesQuery = _context.Invoices
                .Include(i => i.Subscription).ThenInclude(s => s.Customer).ThenInclude(c => c.User)
                .Include(i => i.Subscription).ThenInclude(s => s.SubscriptionPlan)
                .AsQueryable();

            if (currentUserId.HasValue)
                invoicesQuery = invoicesQuery.Where(i => i.Subscription.CustomerId == currentUserId.Value);

            var invoices = await invoicesQuery.ToListAsync();

            var planStatsQuery = _context.Invoices
                .Include(i => i.Subscription).ThenInclude(s => s.SubscriptionPlan)
                .AsQueryable();

            if (currentUserId.HasValue)
                planStatsQuery = planStatsQuery.Where(i => i.Subscription.CustomerId == currentUserId.Value);

            var planStats = await planStatsQuery
                .GroupBy(i => new { i.Subscription.PlanId, i.Subscription.SubscriptionPlan.PlanName })
                .Select(g => new
                {
                    g.Key.PlanId,
                    g.Key.PlanName,
                    Revenue = g.Sum(i => i.Amount),
                    Count = g.Count()
                })
                .ToListAsync();

            ViewBag.PlanStats = planStats;

            return View(invoices);
        }

        [Authorize] 
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var invoice = await _context.Invoices
                .Include(i => i.Subscription)
                    .ThenInclude(s => s.Customer)
                        .ThenInclude(c => c.User)
                .Include(i => i.Subscription)
                    .ThenInclude(s => s.SubscriptionPlan)
                .FirstOrDefaultAsync(i => i.InvoiceId == id.Value);

            if (invoice == null) return NotFound();

            // if regular user, ensure they only see their own
            var isAdmin = User.HasClaim("IsAdmin", "True");
            if (!isAdmin)
            {
                var currentUserId = int.Parse(_userManager.GetUserId(User));
                if (invoice.Subscription.CustomerId != currentUserId)
                    return Forbid();
            }

            return View(invoice);
        }



    }
}
