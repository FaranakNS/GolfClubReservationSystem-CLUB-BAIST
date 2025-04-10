using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;

namespace GolfClubReservationSystem.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public RegisterModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty]
        [Required(ErrorMessage = "Member name is required.")]
        public string MemberName { get; set; }

        [BindProperty]
        public string Occupation { get; set; }

        [BindProperty]
        public string CompanyName { get; set; }

        [BindProperty]
        public string Address { get; set; }

        [BindProperty]
        public string PostalCode { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Phone is required.")]
        public string Phone { get; set; }

        [BindProperty]
        public string AlternatePhone { get; set; }

        [BindProperty]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Date of Birth is required.")]
        public DateTime DateOfBirth { get; set; } = DateTime.Today;

        // New Membership Level property
        [BindProperty]
        [Required(ErrorMessage = "Membership Level is required.")]
        public string MembershipLevel { get; set; }

        public string Message { get; set; }

        public void OnGet()
        {
            Message = string.Empty; // Initialize the page
        }

        public void OnPost()
        {
            if (!ModelState.IsValid)
            {
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    // Wrap the table name and column names in square brackets to avoid reserved word issues.
                    string query = "INSERT INTO [Member_New] ([MemberName], [Occupation], [CompanyName], [Address], [PostalCode], [Phone], [AlternatePhone], [Email], [DateOfBirth], [MembershipLevel]) " +
                                   "VALUES (@MemberName, @Occupation, @CompanyName, @Address, @PostalCode, @Phone, @AlternatePhone, @Email, @DateOfBirth, @MembershipLevel)";
                    SqlCommand command = new SqlCommand(query, connection);

                    command.Parameters.AddWithValue("@MemberName", MemberName);
                    command.Parameters.AddWithValue("@Occupation", string.IsNullOrEmpty(Occupation) ? (object)DBNull.Value : Occupation);
                    command.Parameters.AddWithValue("@CompanyName", string.IsNullOrEmpty(CompanyName) ? (object)DBNull.Value : CompanyName);
                    command.Parameters.AddWithValue("@Address", string.IsNullOrEmpty(Address) ? (object)DBNull.Value : Address);
                    command.Parameters.AddWithValue("@PostalCode", string.IsNullOrEmpty(PostalCode) ? (object)DBNull.Value : PostalCode);
                    command.Parameters.AddWithValue("@Phone", Phone);
                    command.Parameters.AddWithValue("@AlternatePhone", string.IsNullOrEmpty(AlternatePhone) ? (object)DBNull.Value : AlternatePhone);
                    command.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(Email) ? (object)DBNull.Value : Email);
                    command.Parameters.AddWithValue("@DateOfBirth", DateOfBirth);
                    command.Parameters.AddWithValue("@MembershipLevel", MembershipLevel);

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                Message = "Registration successful!";
            }
            catch (SqlException sqlEx)
            {
                // Log the detailed SQL error message for debugging.
                Console.WriteLine($"SQL Error: {sqlEx.Message}");
                // Optionally, display the error message in a development environment:
                Message = "SQL Error: " + sqlEx.Message;
            }

            catch (Exception ex)
            {
                // Log the error message
                Console.WriteLine($"Error: {ex.Message}");
                Message = "An unexpected error occurred. Please try again with valid values.";
            }
        }
    }
}
