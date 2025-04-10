using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace GolfClubReservationSystem.Pages
{
    public class ReviewMembershipModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public ReviewMembershipModel(IConfiguration configuration)
        {
            _configuration = configuration;
            Applications = new List<MembershipApplication>();
        }

        // List of applications loaded from the MembershipApplication table.
        public List<MembershipApplication> Applications { get; set; }

        // Message to show user feedback.
        public string Message { get; set; }

        public async Task OnGetAsync()
        {
            Applications.Clear();
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();

                    string query = @"SELECT ApplicationID, MemberName, Occupation, CompanyName, Address, PostalCode, Phone, AlternatePhone, Email, DateOfBirth, MembershipLevel, ApplicationStatus 
                                     FROM MembershipApplication";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Applications.Add(new MembershipApplication
                                {
                                    ApplicationID = Convert.ToInt32(reader["ApplicationID"]),
                                    MemberName = reader["MemberName"].ToString(),
                                    Occupation = reader["Occupation"].ToString(),
                                    CompanyName = reader["CompanyName"].ToString(),
                                    Address = reader["Address"].ToString(),
                                    PostalCode = reader["PostalCode"].ToString(),
                                    Phone = reader["Phone"].ToString(),
                                    AlternatePhone = reader["AlternatePhone"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    DateOfBirth = Convert.ToDateTime(reader["DateOfBirth"]),
                                    MembershipLevel = reader["MembershipLevel"].ToString(),
                                    ApplicationStatus = reader["ApplicationStatus"].ToString()
                                });
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Message = "Error loading applications: " + ex.Message;
            }
        }

        // Accepts an application: inserts into new_Member and deletes from MembershipApplication.
        public async Task<IActionResult> OnPostAcceptAsync(int applicationId)
        {
            try
            {
                MembershipApplication app = null;
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();

                    // Fetch the application details.
                    string fetchQuery = "SELECT * FROM MembershipApplication WHERE ApplicationID = @ApplicationID";
                    using (SqlCommand command = new SqlCommand(fetchQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ApplicationID", applicationId);
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                app = new MembershipApplication
                                {
                                    ApplicationID = Convert.ToInt32(reader["ApplicationID"]),
                                    MemberName = reader["MemberName"].ToString(),
                                    Occupation = reader["Occupation"].ToString(),
                                    CompanyName = reader["CompanyName"].ToString(),
                                    Address = reader["Address"].ToString(),
                                    PostalCode = reader["PostalCode"].ToString(),
                                    Phone = reader["Phone"].ToString(),
                                    AlternatePhone = reader["AlternatePhone"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    DateOfBirth = Convert.ToDateTime(reader["DateOfBirth"]),
                                    MembershipLevel = reader["MembershipLevel"].ToString(),
                                    ApplicationStatus = reader["ApplicationStatus"].ToString()
                                };
                            }
                        }
                    }

                    if (app == null)
                    {
                        Message = "Application not found.";
                        return RedirectToPage();
                    }

                    // Insert record into new_Member.
                    string insertQuery = @"INSERT INTO Member_New  
                                           (MemberName, Occupation, CompanyName, Address, PostalCode, Phone, AlternatePhone, Email, DateOfBirth, MembershipLevel)
                                           VALUES (@MemberName, @Occupation, @CompanyName, @Address, @PostalCode, @Phone, @AlternatePhone, @Email, @DateOfBirth, @MembershipLevel)";
                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@MemberName", app.MemberName);
                        command.Parameters.AddWithValue("@Occupation", app.Occupation);
                        command.Parameters.AddWithValue("@CompanyName", app.CompanyName);
                        command.Parameters.AddWithValue("@Address", app.Address);
                        command.Parameters.AddWithValue("@PostalCode", app.PostalCode);
                        command.Parameters.AddWithValue("@Phone", app.Phone);
                        command.Parameters.AddWithValue("@AlternatePhone", app.AlternatePhone);
                        command.Parameters.AddWithValue("@Email", app.Email);
                        command.Parameters.AddWithValue("@DateOfBirth", app.DateOfBirth);
                        command.Parameters.AddWithValue("@MembershipLevel", app.MembershipLevel);
                        await command.ExecuteNonQueryAsync();
                    }

                    // Delete the application from MembershipApplication.
                    string deleteQuery = "DELETE FROM MembershipApplication WHERE ApplicationID = @ApplicationID";
                    using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ApplicationID", applicationId);
                        await command.ExecuteNonQueryAsync();
                    }

                    connection.Close();
                    Message = "Application accepted and member added successfully.";
                }
            }
            catch (Exception ex)
            {
                Message = "Error processing acceptance: " + ex.Message;
            }

            // Reload applications.
            await OnGetAsync();
            return Page();
        }

        // Rejects an application: simply deletes it from MembershipApplication.
        public async Task<IActionResult> OnPostRejectAsync(int applicationId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();
                    string deleteQuery = "DELETE FROM MembershipApplication WHERE ApplicationID = @ApplicationID";
                    using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ApplicationID", applicationId);
                        await command.ExecuteNonQueryAsync();
                    }
                    connection.Close();
                    Message = "Application rejected and deleted successfully.";
                }
            }
            catch (Exception ex)
            {
                Message = "Error processing rejection: " + ex.Message;
            }

            await OnGetAsync();
            return Page();
        }
    }

    public class MembershipApplication
    {
        public int ApplicationID { get; set; }
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
