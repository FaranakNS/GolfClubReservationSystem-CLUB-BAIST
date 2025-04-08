//using GolfClubReservationSystem.Models;
//using GolfClubReservationSystem.TechnicalServices;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;

//namespace GolfClubReservationSystem.Pages
//{
//    public class RegisterModel : PageModel
//    {
//        private readonly RegisterServices _registerService;

//        [BindProperty]
//        public MembershipApplication Application { get; set; } = new();

//        [TempData]
//        public string StatusMessage { get; set; } = string.Empty;

//        public RegisterModel(RegisterServices registerService)
//        {
//            _registerService = registerService;
//        }

//        public void OnGet()
//        {

//        }

//        public IActionResult OnPost()
//        {
//            if (!ModelState.IsValid)
//            {
//                return Page();
//            }

//            try
//            {
//                bool success = _registerService.SubmitApplication(Application);

//                if (success)
//                {
//                    // Clear form after successful submission
//                    Application = new MembershipApplication();
//                    StatusMessage = "Thank you! Your application has been submitted successfully.";
//                }
//                else
//                {
//                    StatusMessage = "Error submitting application. Please try again.";
//                }
//            }
//            catch (Exception ex)
//            {
//                StatusMessage = $"Error: {ex.Message}";
//            }

//            return Page();
//        }


//    }
//}
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
                    // Updated query includes MembershipLevel
                    string query = "INSERT INTO Member (MemberName, Occupation, CompanyName, Address, PostalCode, Phone, AlternatePhone, Email, DateOfBirth, MembershipLevel) " +
                                   "VALUES (@MemberName, @Occupation, @CompanyName, @Address, @PostalCode, @Phone, @AlternatePhone, @Email, @DateOfBirth, @MembershipLevel)";
                    SqlCommand command = new SqlCommand(query, connection);

                    command.Parameters.AddWithValue("@MemberName", MemberName);
                    command.Parameters.AddWithValue("@Occupation", Occupation ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@CompanyName", CompanyName ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Address", Address ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@PostalCode", PostalCode ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Phone", Phone);
                    command.Parameters.AddWithValue("@AlternatePhone", AlternatePhone ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Email", Email ?? (object)DBNull.Value);
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
                Console.WriteLine($"SQL Error: {sqlEx.Message}");
                Message = "An unexpected error occurred. Please try again with valid values.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Message = "An unexpected error occurred. Please try again with valid values.";
            }
        }
    }
}
