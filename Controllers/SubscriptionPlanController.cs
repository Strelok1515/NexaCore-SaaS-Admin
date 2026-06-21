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
    public class SubscriptionPlanController : Controller
    {
        private readonly ApplicationDbContext _context;
        public SubscriptionPlanController(ApplicationDbContext context) => _context = context;

        // GET: /SubscriptionPlan
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var plans = await _context.SubscriptionPlans.ToListAsync();
            return View(plans);
        }

        // GET: /SubscriptionPlan/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var plan = await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.PlanId == id.Value);
            if (plan == null) return NotFound();

            return View(plan);
        }

        // GET: /SubscriptionPlan/Create
        [Authorize(Policy = "AdminsOnly")]
        public IActionResult Create() => View();

        // POST: /SubscriptionPlan/Create
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminsOnly")]
        public async Task<IActionResult> Create([Bind("PlanName,Price,Duration,Features")] SubscriptionPlan plan)
        {
            if (!ModelState.IsValid) return View(plan);
            _context.Add(plan);
            await _context.SaveChangesAsync();
            TempData["ToastMessage"] = "Subscription plan created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /SubscriptionPlan/Edit/5
        [Authorize(Policy = "AdminsOnly")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var plan = await _context.SubscriptionPlans.FindAsync(id.Value);
            if (plan == null) return NotFound();
            return View(plan);
        }

        // POST: /SubscriptionPlan/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminsOnly")]
        public async Task<IActionResult> Edit(int id, [Bind("PlanId,PlanName,Price,Duration,Features")] SubscriptionPlan plan)
        {
            if (id != plan.PlanId) return NotFound();
            if (!ModelState.IsValid) return View(plan);

            try
            {
                _context.Update(plan);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.SubscriptionPlans.AnyAsync(p => p.PlanId == id))
                    return NotFound();
                throw;
            }
            TempData["ToastMessage"] = "Subscription plan updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /SubscriptionPlan/Delete/5
        [Authorize(Policy = "AdminsOnly")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var plan = await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.PlanId == id.Value);
            if (plan == null) return NotFound();
            return View(plan);
        }

        // POST: /SubscriptionPlan/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminsOnly")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var plan = await _context.SubscriptionPlans.FindAsync(id);
            if (plan != null)
            {
                _context.SubscriptionPlans.Remove(plan);
                await _context.SaveChangesAsync();
            }
            TempData["ToastMessage"] = "Subscription plan deleted.";

            return RedirectToAction(nameof(Index));
        }
    }
}
