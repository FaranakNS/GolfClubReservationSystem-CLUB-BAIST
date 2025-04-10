using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace GolfClubReservationSystem.Pages
{
    public class RecordPlayerScoreModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public RecordPlayerScoreModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Bind the Scorecard details for the score submission form.
        [BindProperty]
        public Scorecard Scorecard { get; set; }

        // Bind a separate MemberId for the lookup.
        [BindProperty]
        public int MemberId { get; set; }

        // Flag to signal that the member (and details) were found.
        public bool IsBookingFound { get; set; } = false;

        // Message to show feedback to the user.
        public string Message { get; set; }

        public void OnGet()
        {
            // Initialize Scorecard with today's date.
            Scorecard = new Scorecard { Date = DateTime.Today };
            Message = string.Empty;
            IsBookingFound = false;
        }

        // Handler for fetching member details by MemberId.
        public async Task<IActionResult> OnPostFetchDetailsAsync()
        {
            Message = string.Empty;
            if (MemberId <= 0)
            {
                Message = "Please enter a valid Member ID.";
                IsBookingFound = false;
                return Page();
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();
                    // Query the Member table using the provided MemberId.
                    string fetchQuery = "SELECT MemberID, MemberName FROM [Member] WHERE MemberID = @MemberID";
                    using (SqlCommand command = new SqlCommand(fetchQuery, connection))
                    {
                        command.Parameters.AddWithValue("@MemberID", MemberId);
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (reader.Read())
                            {
                                // Populate the Scorecard with the fetched member information.
                                Scorecard.MemberName = reader["MemberName"].ToString();
                                Scorecard.MemberID = Convert.ToInt32(reader["MemberID"]);
                                IsBookingFound = true;
                                Message = $"Member found: {Scorecard.MemberName}";
                            }
                            else
                            {
                                IsBookingFound = false;
                                Message = "Member not found.";
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Message = "Error fetching member: " + ex.Message;
                IsBookingFound = false;
            }
            return Page();
        }

        // Handler for recording the score using the stored procedure.
        public async Task<IActionResult> OnPostRecordScoreAsync()
        {
            Message = string.Empty;
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand("usp_AddScorecardAndReturnJoin", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@Date", Scorecard.Date);
                        command.Parameters.AddWithValue("@GolfCourse", Scorecard.GolfCourse);
                        command.Parameters.AddWithValue("@CourseRating", Scorecard.CourseRating);
                        command.Parameters.AddWithValue("@SlopeRating", Scorecard.SlopeRating);
                        command.Parameters.AddWithValue("@TotalScore", Scorecard.TotalScore);
                        command.Parameters.AddWithValue("@HoleScores",
                            string.IsNullOrEmpty(Scorecard.HoleScores) ? (object)DBNull.Value : Scorecard.HoleScores);
                        command.Parameters.AddWithValue("@MemberName", Scorecard.MemberName);
                        command.Parameters.AddWithValue("@MemberID",
                            Scorecard.MemberID.HasValue ? (object)Scorecard.MemberID.Value : DBNull.Value);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (reader.Read())
                            {
                                int scorecardId = Convert.ToInt32(reader["ScorecardID"]);
                                string golfCourse = reader["GolfCourse"].ToString();
                                string registeredMemberName = reader["RegisteredMemberName"] as string;
                                Message = $"Score recorded successfully! Scorecard ID: {scorecardId}, " +
                                          $"Golf Course: {golfCourse}, " +
                                          "Member: " + (string.IsNullOrEmpty(registeredMemberName) ? "Not Registered" : registeredMemberName);
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Message = "Error recording score: " + ex.Message;
            }
            return Page();
        }
    }

    public class Scorecard
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Golf Course is required.")]
        public string GolfCourse { get; set; }

        [Required(ErrorMessage = "Course Rating is required.")]
        public double CourseRating { get; set; }

        [Required(ErrorMessage = "Slope Rating is required.")]
        public int SlopeRating { get; set; }

        [Required(ErrorMessage = "Total Score is required.")]
        public int TotalScore { get; set; }

        // Optional: A comma-separated string of hole scores.
        public string HoleScores { get; set; }

        [Required(ErrorMessage = "Member Name is required.")]
        public string MemberName { get; set; }

        // Optional member identifier (fetched from Member lookup).
        public int? MemberID { get; set; }
    }
}
