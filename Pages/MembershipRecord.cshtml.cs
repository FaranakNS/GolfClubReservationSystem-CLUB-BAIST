using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace GolfClubReservationSystem.Pages
{
    public class MembershipRecordModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public List<RegisteredMember> Members { get; set; }

        public MembershipRecordModel(IConfiguration configuration)
        {
            _configuration = configuration;
            Members = new List<RegisteredMember>();
        }

        public IActionResult OnGet()
        {
            // Ensure user is logged in as membership committee.
            var username = HttpContext.Session.GetString("Username");
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(username) || userRole != "membership committee")
            {
                return RedirectToPage("Login");
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                // Retrieve all registered members ordered by MembershipLevel.
                string query = @"SELECT MemberID, MemberName, Occupation, CompanyName, Address, PostalCode, Phone, AlternatePhone, Email, DateOfBirth, MembershipLevel 
                                 FROM Member_New
                                 ORDER BY MembershipLevel";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var member = new RegisteredMember
                            {
                                MemberID = Convert.ToInt32(reader["MemberID"]),
                                MemberName = reader["MemberName"].ToString(),
                                Occupation = reader["Occupation"]?.ToString(),
                                CompanyName = reader["CompanyName"]?.ToString(),
                                Address = reader["Address"]?.ToString(),
                                PostalCode = reader["PostalCode"]?.ToString(),
                                Phone = reader["Phone"]?.ToString(),
                                AlternatePhone = reader["AlternatePhone"]?.ToString(),
                                Email = reader["Email"]?.ToString(),
                                DateOfBirth = Convert.ToDateTime(reader["DateOfBirth"]),
                                MembershipLevel = reader["MembershipLevel"]?.ToString()
                            };
                            Members.Add(member);
                        }
                    }
                }
                connection.Close();
            }
            return Page();
        }
    }

    public class RegisteredMember
    {
        public int MemberID { get; set; }
        public string MemberName { get; set; }
        public string Occupation { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string Phone { get; set; }
        public string AlternatePhone { get; set; }
        public string Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string MembershipLevel { get; set; }
    }
}
