// Controllers/HomeController.cs
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AleksandarIvanov_NexaCore.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AleksandarIvanov_NexaCore.Models;

namespace AleksandarIvanov_NexaCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        // GET: /
        public async Task<IActionResult> Index()
        {
            // 1) Load all plans into memory
            var plansList = await _db.SubscriptionPlans
                                     .AsNoTracking()
                                     .ToListAsync();

            // 2) Order by decimal Price in‐memory
            var plans = plansList
                        .OrderBy(p => p.Price)
                        .ToList();

            return View(plans);
        }

        public IActionResult Privacy()
            => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
            => View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
    }
}
