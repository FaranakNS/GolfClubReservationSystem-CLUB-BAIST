using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace GolfClubReservationSystem.Pages
{
    public class ViewMemberAccountModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public ViewMemberAccountModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // We'll assume that the MemberID is passed as a query parameter.
        // In production, you might store the MemberID in session after login.
        [BindProperty(SupportsGet = true)]
        public int MemberID { get; set; }

        public Member Member { get; set; }
        public bool IsMemberFound { get; set; }
        public string Message { get; set; }

        public IActionResult OnGet()
        {
            // Ensure the user is logged in; otherwise, redirect to login.
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToPage("Login");
            }

            if (MemberID <= 0)
            {
                Message = "Member ID not provided.";
                IsMemberFound = false;
                return Page();
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    string query = @"SELECT MemberID, MemberName, Occupation, CompanyName, Address, PostalCode, 
                                            Phone, AlternatePhone, Email, DateOfBirth, MembershipLevel, ApplicationStatus
                                     FROM Member
                                     WHERE MemberID = @MemberID";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@MemberID", MemberID);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Member = new Member
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
                                MembershipLevel = reader["MembershipLevel"]?.ToString(),
                                ApplicationStatus = reader["ApplicationStatus"]?.ToString()
                            };
                            IsMemberFound = true;
                        }
                        else
                        {
                            IsMemberFound = false;
                            Message = "Member not found.";
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                IsMemberFound = false;
                Message = "Error fetching member data: " + ex.Message;
            }

            return Page();
        }
    }

    public class Member
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
        public string ApplicationStatus { get; set; }
    }
}

