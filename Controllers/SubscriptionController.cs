// Controllers/SubscriptionController.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using AleksandarIvanov_NexaCore.Data;
using AleksandarIvanov_NexaCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AleksandarIvanov_NexaCore.Controllers
{
    [Authorize] // Require login for all actions
    public class SubscriptionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public SubscriptionController(
            ApplicationDbContext context,
            UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Subscription
        public async Task<IActionResult> Index()
        {
            var isAdmin = User.HasClaim("IsAdmin", "True");
            var query = _context.Subscriptions
                .Include(s => s.Customer).ThenInclude(c => c.User)
                .Include(s => s.SubscriptionPlan)
                .AsQueryable();

            if (!isAdmin)
            {
                var currentUserId = int.Parse(_userManager.GetUserId(User));
                query = query.Where(s => s.CustomerId == currentUserId);
            }

            var subs = await query.ToListAsync();
            return View(subs);
        }

        // GET: /Subscription/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var subscription = await _context.Subscriptions
                .Include(s => s.Customer).ThenInclude(c => c.User)
                .Include(s => s.SubscriptionPlan)
                .FirstOrDefaultAsync(s => s.SubscriptionId == id.Value);

            if (subscription == null) return NotFound();

            if (!User.HasClaim("IsAdmin", "True"))
            {
                var currentUserId = int.Parse(_userManager.GetUserId(User));
                if (subscription.CustomerId != currentUserId)
                    return Forbid();
            }

            return View(subscription);
        }

        // GET: /Subscription/Create
        [Authorize(Policy = "AdminsOnly")]
        public async Task<IActionResult> Create()
        {
            ViewData["CustomerId"] = new SelectList(
                await _context.Customers.Include(c => c.User)
                    .Select(c => new { c.Id, Display = c.User.UserName })
                    .ToListAsync(),
                "Id", "Display");
            ViewData["PlanId"] = new SelectList(
                await _context.SubscriptionPlans.ToListAsync(),
                "PlanId", "PlanName");
            return View();
        }

        // POST: /Subscription/Create
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminsOnly")]
        public async Task<IActionResult> Create(
            [Bind("CustomerId,PlanId,StartDate,EndDate")] Subscription subscription)
        {
            if (!ModelState.IsValid)
            {
                // re‑populate dropdowns on validation error
                ViewData["CustomerId"] = new SelectList(
                    await _context.Customers.Include(c => c.User)
                        .Select(c => new { c.Id, Display = c.User.UserName })
                        .ToListAsync(),
                    "Id", "Display", subscription.CustomerId);
                ViewData["PlanId"] = new SelectList(
                    await _context.SubscriptionPlans.ToListAsync(),
                    "PlanId", "PlanName", subscription.PlanId);
                return View(subscription);
            }

            // 1) save the subscription
            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            // 2) fetch the plan
            var plan = await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.PlanId == subscription.PlanId);

            if (plan != null)
            {
                // 3) calculate the invoice amount
                var amount = CalculateAmount(plan.Price, subscription.StartDate, subscription.EndDate);

                // 4) create & save invoice
                _context.Invoices.Add(new Invoice
                {
                    SubscriptionId = subscription.SubscriptionId,
                    Amount = amount,
                    DueDate = subscription.EndDate,
                    PaymentStatus = "Pending"
                });
                await _context.SaveChangesAsync();
            }

            TempData["ToastMessage"] = "Subscription created—and invoice generated!";
            return RedirectToAction(nameof(Index));
        }

        // helper: full price for first 3 blocks, 20% off thereafter (up to 12 blocks)
        private decimal CalculateAmount(decimal monthlyPrice, DateTime start, DateTime end)
        {
            var totalDays = (end - start).Days;
            var totalBlocks = (int)Math.Ceiling(totalDays / 30.0);
            totalBlocks = Math.Min(totalBlocks, 12); // cap at one year

            if (totalBlocks <= 3)
            {
                return Math.Round(monthlyPrice * totalBlocks, 2);
            }
            else
            {
                var fullBlocks = 3;
                var discountedBlocks = totalBlocks - fullBlocks;
                var fullAmount = monthlyPrice * fullBlocks;
                var discountedAmount = monthlyPrice * 0.8m * discountedBlocks;
                return Math.Round(fullAmount + discountedAmount, 2);
            }
        }

        // GET: /Subscription/Edit/5
        [Authorize(Policy = "AdminsOnly")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var subscription = await _context.Subscriptions.FindAsync(id.Value);
            if (subscription == null) return NotFound();

            ViewData["CustomerId"] = new SelectList(
                await _context.Customers.Include(c => c.User)
                    .Select(c => new { c.Id, Display = c.User.UserName })
                    .ToListAsync(),
                "Id", "Display", subscription.CustomerId);
            ViewData["PlanId"] = new SelectList(
                await _context.SubscriptionPlans.ToListAsync(),
                "PlanId", "PlanName", subscription.PlanId);

            return View(subscription);
        }

        // POST: /Subscription/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminsOnly")]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("SubscriptionId,CustomerId,PlanId,StartDate,EndDate")] Subscription subscription)
        {
            if (id != subscription.SubscriptionId) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["CustomerId"] = new SelectList(
                    await _context.Customers.Include(c => c.User)
                        .Select(c => new { c.Id, Display = c.User.UserName })
                        .ToListAsync(),
                    "Id", "Display", subscription.CustomerId);
                ViewData["PlanId"] = new SelectList(
                    await _context.SubscriptionPlans.ToListAsync(),
                    "PlanId", "PlanName", subscription.PlanId);
                return View(subscription);
            }

            try
            {
                _context.Update(subscription);
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "Subscription updated successfully!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Subscriptions.AnyAsync(e => e.SubscriptionId == id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Subscription/Delete/5
        [Authorize(Policy = "AdminsOnly")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var subscription = await _context.Subscriptions
                .Include(s => s.Customer).ThenInclude(c => c.User)
                .Include(s => s.SubscriptionPlan)
                .FirstOrDefaultAsync(s => s.SubscriptionId == id.Value);

            if (subscription == null) return NotFound();
            return View(subscription);
        }

        // POST: /Subscription/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminsOnly")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subscription = await _context.Subscriptions.FindAsync(id);
            if (subscription != null)
            {
                _context.Subscriptions.Remove(subscription);
                await _context.SaveChangesAsync();
            }
            TempData["ToastMessage"] = "Subscription deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
