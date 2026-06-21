using System;
using System.Threading.Tasks;
using AleksandarIvanov_NexaCore.Data;
using AleksandarIvanov_NexaCore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace AleksandarIvanov_NexaCore.Filters
{
    public class AuditLogActionFilter : IAsyncActionFilter
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _users;

        public AuditLogActionFilter(ApplicationDbContext db, UserManager<User> users)
        {
            _db = db;
            _users = users;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            // let the action run
            var resultContext = await next();

            // only for authenticated admins
            var principal = context.HttpContext.User;
            if (principal?.Identity?.IsAuthenticated != true) return;

            var admin = await _users.GetUserAsync(principal);
            if (admin == null || !admin.IsAdmin) return;

            // build "Controller/Action[/id]" descriptor
            var ctrl = context.ActionDescriptor.RouteValues["controller"];
            var act = context.ActionDescriptor.RouteValues["action"];
            var desc = $"{ctrl}/{act}";

            // try to pull an "id" parameter
            string idSeg = null;
            if (context.ActionArguments.TryGetValue("id", out var argVal) && argVal != null)
                idSeg = argVal.ToString();
            else if (context.RouteData.Values.TryGetValue("id", out var rd) && rd != null)
                idSeg = rd.ToString();
            else if (resultContext.Result is Microsoft.AspNetCore.Mvc.RedirectToActionResult rta
                     && rta.RouteValues != null
                     && rta.RouteValues.TryGetValue("id", out var rid)
                     && rid != null)
                idSeg = rid.ToString();

            if (!string.IsNullOrEmpty(idSeg))
                desc += $"/{idSeg}";

            _db.AuditLogs.Add(new AuditLog
            {
                AdminUserId = admin.Id,
                Action = desc,
                Timestamp = DateTime.UtcNow,
                Description = $"Admin '{admin.UserName}' executed {desc}"
            });

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) { }
            catch { }
        }
    }
}
