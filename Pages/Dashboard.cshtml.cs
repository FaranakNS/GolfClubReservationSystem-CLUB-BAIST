using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GolfClubReservationSystem.Pages
{
    public class DashboardModel : PageModel
    {
        public string Username { get; set; }
        public string UserRole { get; set; }

        public IActionResult OnGet()
        {
            Username = HttpContext.Session.GetString("Username");
            UserRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(UserRole))
            {
                return RedirectToPage("Login");
            }
            return Page();
        }
    }
}
