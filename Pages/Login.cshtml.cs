using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace GolfClubReservationSystem.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty, Required]
        public string Username { get; set; }

        [BindProperty, Required]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        
        private readonly Dictionary<string, (string Password, string Role)> users = new Dictionary<string, (string, string)>
        {
            { "Golfplayer", ("player123", "player") },
            { "staff-clerk", ("clerk123", "clerk") },
            { "staff-proshop", ("proshop123", "pro shop staff") },
            { "member", ("member123", "member") },
            { "shareholder", ("shareholder123", "shareholder") },
            { "finance", ("finance123", "finance committee") },
            { "membershipcomitte", ("membership123", "membership committee") }
        };

        public void OnGet()
        {
            
            HttpContext.Session.Clear();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            if (users.ContainsKey(Username) && users[Username].Password == Password)
            {
               
                HttpContext.Session.SetString("Username", Username);
                HttpContext.Session.SetString("UserRole", users[Username].Role);

                if (users[Username].Role == "membership committee")
                {
                    return RedirectToPage("MembershipCommitteeDashboard");
                }
                else
                {
                    return RedirectToPage("Dashboard");
                }
            }
            else
            {
                ErrorMessage = "Invalid username or password.";
                return Page();
            }
        }
    }
}

