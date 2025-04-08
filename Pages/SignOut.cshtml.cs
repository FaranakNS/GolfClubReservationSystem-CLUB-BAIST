using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GolfClubReservationSystem.Pages
{
    public class SignOutModel : PageModel
    {
        public IActionResult OnGet()
        {
          
            HttpContext.Session.Clear();
        
            return RedirectToPage("Login");
        }
    }
}
