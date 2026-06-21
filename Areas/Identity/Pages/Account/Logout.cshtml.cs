// Areas/Identity/Pages/Account/Logout.cshtml.cs
#nullable disable

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AleksandarIvanov_NexaCore.Models;

namespace AleksandarIvanov_NexaCore.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;

        public LogoutModel(SignInManager<User> signInManager)
        {
            _signInManager = signInManager;
        }

        /// <summary>
        /// Holds the logout confirmation message.
        /// </summary>
        [TempData]
        public string LogoutMessage { get; set; }

        public void OnGet()
        {
            // This page only responds to POST for actual logout,
            // so a GET can just show a confirmation if you want.
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            // Perform the sign-out
            await _signInManager.SignOutAsync();

            // Set the flash message
            LogoutMessage = "You have been successfully logged out.";

            // Redirect back to home or to returnUrl
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                // Redirect to the home page where the message will display
                return RedirectToPage("/Index");
            }
        }
    }
}
