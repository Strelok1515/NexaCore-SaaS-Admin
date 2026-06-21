using AleksandarIvanov_NexaCore.Data;
using AleksandarIvanov_NexaCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AleksandarIvanov_NexaCore.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /User
        public async Task<IActionResult> Index()
        {
            var users = await _context.User.ToListAsync();
            return View(users);
        }

        // GET: /User/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();
            return View(user);
        }

        // GET: /User/Create
        public IActionResult Create() => View();

        // POST: /User/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserName,Email,Address,IsAdmin")] User user)
        {
            if (!ModelState.IsValid)
                return View(user);

            _context.User.Add(user);
            await _context.SaveChangesAsync();

            TempData["ToastMessage"] = "User created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /User/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.User.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        // POST: /User/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserName,Email,Address,IsAdmin")] User incoming)
        {
            if (id != incoming.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(incoming);

            // 1) Re‑fetch the tracked entity (with real ConcurrencyStamp)
            var existing = await _context.User.FindAsync(id);
            if (existing == null)
                return NotFound();

            // 2) Apply only the properties you allow to change
            existing.UserName = incoming.UserName;
            existing.Email = incoming.Email;
            existing.Address = incoming.Address;
            existing.IsAdmin = incoming.IsAdmin;

            try
            {
                // 3) Save changes on the tracked entity
                await _context.SaveChangesAsync();

                TempData["ToastMessage"] = "User updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                // 4) Handle the rare race where someone else deleted/modified it
                if (!await _context.User.AnyAsync(u => u.Id == id))
                    return NotFound();

                ModelState.AddModelError(string.Empty,
                    "This record was changed by another user. Please reload and try again.");
                return View(incoming);
            }
        }

        // GET: /User/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();
            return View(user);
        }

        // POST: /User/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.User.FindAsync(id);
            if (user != null)
            {
                _context.User.Remove(user);
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "User deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
