// Controllers/CustomerController.cs
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
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public CustomerController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Customer
        public async Task<IActionResult> Index()
        {
            bool isAdmin = User.HasClaim("IsAdmin", "True");
            if (isAdmin)
            {
                // Admin sees all customers
                var allCustomers = await _context.Customers
                    .Include(c => c.User)
                    .ToListAsync();
                return View(allCustomers);
            }
            else
            {
                // Regular user only sees their own
                int currentUserId = int.Parse(_userManager.GetUserId(User));
                var myCustomer = await _context.Customers
                    .Include(c => c.User)
                    .Where(c => c.Id == currentUserId)
                    .ToListAsync();
                return View(myCustomer);
            }
        }

        // GET: /Customer/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id.Value);

            if (customer == null) return NotFound();

            // Regular users can only view their own
            if (!User.HasClaim("IsAdmin", "True"))
            {
                int currentUserId = int.Parse(_userManager.GetUserId(User));
                if (customer.Id != currentUserId)
                    return Forbid();
            }

            return View(customer);
        }

        // GET: /Customer/Create
        [Authorize(Policy = "AdminsOnly")]
        public IActionResult Create()
        {
            // Only list Users who aren't already customers
            var usedIds = _context.Customers.Select(c => c.Id).ToList();
            var availableUsers = _context.Users
                .Where(u => !usedIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName })
                .ToList();

            ViewData["UserId"] = new SelectList(availableUsers, "Id", "UserName");
            return View();
        }

        // POST: /Customer/Create
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminsOnly")]
        public async Task<IActionResult> Create([Bind("Id,IsActive")] Customer customer)
        {
            // Prevent duplicates
            if (_context.Customers.Any(c => c.Id == customer.Id))
                ModelState.AddModelError(nameof(customer.Id), "That user is already a customer.");

            if (ModelState.IsValid)
            {
                _context.Add(customer);
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "Customer created successfully!";
                return RedirectToAction(nameof(Index));
            }

            // On error, re‑populate dropdown
            var usedIds = _context.Customers.Select(c => c.Id).ToList();
            var availableUsers = _context.Users
                .Where(u => !usedIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName })
                .ToList();
            ViewData["UserId"] = new SelectList(availableUsers, "Id", "UserName", customer.Id);
            return View(customer);
        }

        // GET: /Customer/Edit/5
        [Authorize(Policy = "AdminsOnly")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var customer = await _context.Customers.FindAsync(id.Value);
            if (customer == null) return NotFound();

            // Show the user dropdown (so an admin could reassign, if you want)
            var users = _context.Users
                .Select(u => new { u.Id, u.UserName })
                .ToList();
            ViewData["UserId"] = new SelectList(users, "Id", "UserName", customer.Id);
            return View(customer);
        }

        // POST: /Customer/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminsOnly")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IsActive")] Customer customer)
        {
            if (id != customer.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(customer);
                    await _context.SaveChangesAsync();
                    TempData["ToastMessage"] = "Customer updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Customers.AnyAsync(c => c.Id == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            // On error, re‑populate dropdown
            var users = _context.Users
                .Select(u => new { u.Id, u.UserName })
                .ToList();
            ViewData["UserId"] = new SelectList(users, "Id", "UserName", customer.Id);
            return View(customer);
        }

        // GET: /Customer/Delete/5
        [Authorize(Policy = "AdminsOnly")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var customer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id.Value);
            if (customer == null) return NotFound();
            return View(customer);
        }

        // POST: /Customer/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminsOnly")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "Customer deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
