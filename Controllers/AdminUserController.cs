using AleksandarIvanov_NexaCore.Data;
using AleksandarIvanov_NexaCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AleksandarIvanov_NexaCore.Controllers
{
    public class AdminUserController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AdminUserController(ApplicationDbContext context)
            => _context = context;

        // GET: /AdminUser
        // Lists all admin users with their linked Identity User.
        public async Task<IActionResult> Index()
        {
            var admins = await _context.AdminUsers
                .Include(a => a.User)
                .ToListAsync();
            return View(admins);
        }

        // GET: /AdminUser/Details/5
        // Shows details for a single admin user.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var admin = await _context.AdminUsers
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AdminUserId == id.Value);

            if (admin == null) return NotFound();
            return View(admin);
        }
    }
}
