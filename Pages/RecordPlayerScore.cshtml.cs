//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using Microsoft.Data.SqlClient;
//using System;
//using System.ComponentModel.DataAnnotations;
//using System.Data;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Configuration;

//namespace GolfClubReservationSystem.Pages
//{
//    public class RecordPlayerScoreModel : PageModel
//    {
//        private readonly IConfiguration _configuration;

//        public RecordPlayerScoreModel(IConfiguration configuration)
//        {
//            _configuration = configuration;
//        }

//        [BindProperty]
//        public Scorecard Scorecard { get; set; }

//        public string Message { get; set; }

//        public void OnGet()
//        {
//            // Initialize Scorecard with today's date.
//            Scorecard = new Scorecard { Date = DateTime.Today };
//            Message = string.Empty;
//        }

//        public async Task<IActionResult> OnPostAsync(string submit)
//        {
//            Message = string.Empty;

//            if (submit == "FetchDetails")
//            {
//                // Validate that a MemberID is provided.
//                if (!Scorecard.MemberID.HasValue || Scorecard.MemberID.Value <= 0)
//                {
//                    Message = "Please enter a valid Member ID.";
//                    return Page();
//                }

//                try
//                {
//                    using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
//                    {
//                        await connection.OpenAsync();

//                        // Query the Member table using the provided MemberID.
//                        string fetchQuery = "SELECT MemberID, MemberName FROM [Member] WHERE MemberID = @MemberID";
//                        using (SqlCommand command = new SqlCommand(fetchQuery, connection))
//                        {
//                            command.Parameters.AddWithValue("@MemberID", Scorecard.MemberID.Value);
//                            using (SqlDataReader reader = await command.ExecuteReaderAsync())
//                            {
//                                if (reader.Read())
//                                {
//                                    // Assign the fetched member name to Scorecard.
//                                    Scorecard.MemberName = reader["MemberName"].ToString();
//                                    Message = $"Member found: {Scorecard.MemberName}";
//                                }
//                                else
//                                {
//                                    Message = "Member not found.";
//                                }
//                            }
//                        }
//                        connection.Close();
//                    }
//                }
//                catch (Exception ex)
//                {
//                    Message = "Error fetching member: " + ex.Message;
//                }

//                return Page();
//            }
//            else if (submit == "RecordScore")
//            {
//                // Validate model state.
//                if (!ModelState.IsValid)
//                {
//                    return Page();
//                }

//                try
//                {
//                    using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
//                    {
//                        await connection.OpenAsync();

//                        using (SqlCommand command = new SqlCommand("usp_AddScorecardAndReturnJoin", connection))
//                        {
//                            command.CommandType = CommandType.StoredProcedure;

//                            command.Parameters.AddWithValue("@Date", Scorecard.Date);
//                            command.Parameters.AddWithValue("@GolfCourse", Scorecard.GolfCourse);
//                            command.Parameters.AddWithValue("@CourseRating", Scorecard.CourseRating);
//                            command.Parameters.AddWithValue("@SlopeRating", Scorecard.SlopeRating);
//                            command.Parameters.AddWithValue("@TotalScore", Scorecard.TotalScore);
//                            command.Parameters.AddWithValue("@HoleScores",
//                                string.IsNullOrEmpty(Scorecard.HoleScores) ? (object)DBNull.Value : Scorecard.HoleScores);
//                            command.Parameters.AddWithValue("@MemberName", Scorecard.MemberName);
//                            command.Parameters.AddWithValue("@MemberID",
//                                Scorecard.MemberID.HasValue ? (object)Scorecard.MemberID.Value : DBNull.Value);

//                            using (SqlDataReader reader = await command.ExecuteReaderAsync())
//                            {
//                                if (reader.Read())
//                                {
//                                    int scorecardId = Convert.ToInt32(reader["ScorecardID"]);
//                                    string golfCourse = reader["GolfCourse"].ToString();
//                                    string registeredMemberName = reader["RegisteredMemberName"] as string;

//                                    // Build a confirmation message with the joined result.
//                                    Message = $"Score recorded successfully! Scorecard ID: {scorecardId}, " +
//                                              $"Golf Course: {golfCourse}, " +
//                                              "Member: " + (string.IsNullOrEmpty(registeredMemberName) ? "Not Registered" : registeredMemberName);
//                                }
//                            }
//                        }
//                        connection.Close();
//                    }
//                }
//                catch (Exception ex)
//                {
//                    Message = "Error recording score: " + ex.Message;
//                }

//                return Page();
//            }

//            return Page();
//        }
//    }

//    public class Scorecard
//    {
//        public int Id { get; set; }

//        [Required(ErrorMessage = "Date is required.")]
//        public DateTime Date { get; set; }

//        [Required(ErrorMessage = "Golf Course is required.")]
//        public string GolfCourse { get; set; }

//        [Required(ErrorMessage = "Course Rating is required.")]
//        public double CourseRating { get; set; }

//        [Required(ErrorMessage = "Slope Rating is required.")]
//        public int SlopeRating { get; set; }

//        [Required(ErrorMessage = "Total Score is required.")]
//        public int TotalScore { get; set; }

//        // Optional: Comma-separated string for hole scores.
//        public string HoleScores { get; set; }

//        [Required(ErrorMessage = "Member Name is required.")]
//        public string MemberName { get; set; }

//        // Optional MemberID; used to fetch member details if provided.
//        public int? MemberID { get; set; }
//    }
//}
