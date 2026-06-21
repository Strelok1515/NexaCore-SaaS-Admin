using System.Linq;
using System.Threading.Tasks;
using AleksandarIvanov_NexaCore.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AleksandarIvanov_NexaCore.Controllers
{
    [Authorize(Policy = "AdminsOnly")]
    public class AuditLogController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AuditLogController(ApplicationDbContext context)
            => _context = context;

        // GET: /AuditLog
        public async Task<IActionResult> Index()
        {
            var logs = await _context.AuditLogs
                .Include(l => l.AdminUser)
                    .ThenInclude(au => au.User)
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
            return View(logs);
        }

        // POST: /AuditLog/Clear
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            // 1) Delete all audit logs
            _context.AuditLogs.RemoveRange(_context.AuditLogs);
            await _context.SaveChangesAsync();

            // 2) Reset SQLite AUTOINCREMENT counter
            //    (only works on SQLite; ignores errors on other providers)
            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM sqlite_sequence WHERE name = 'AuditLogs';");
            }
            catch
            {
                // swallow any provider‐specific error
            }

            TempData["ToastMessage"] = "Audit history cleared.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /AuditLog/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var log = await _context.AuditLogs
                .Include(l => l.AdminUser)
                    .ThenInclude(au => au.User)
                .FirstOrDefaultAsync(l => l.LogId == id.Value);

            if (log == null) return NotFound();
            return View(log);
        }
    }
}
