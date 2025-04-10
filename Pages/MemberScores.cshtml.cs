using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;

namespace GolfClubReservationSystem.Pages
{
    public class MemberScoresModel : PageModel
    {

            private readonly IConfiguration _configuration;

        [BindProperty]
        public Scorecard Scorecard { get; set; }
        public MemberScoresModel(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            // Bind MemberID from the query string (GET) so the user can enter it.
            [BindProperty(SupportsGet = true)]
            [Required(ErrorMessage = "Member ID is required.")]
            public int MemberID { get; set; }

            // This property will hold the list of scorecards for the given MemberID.
            public List<Scorecard> MemberScorecards { get; set; } = new List<Scorecard>();

            public async Task OnGetAsync()
            {
                // If no valid MemberID is entered, no scores are fetched.
                if (MemberID <= 0)
                {
                    return;
                }

                try
                {
                    using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                    {
                        string query = @"
                        SELECT TOP (1000) 
                            ScorecardID,
                            Date,
                            GolfCourse,
                            CourseRating,
                            SlopeRating,
                            TotalScore,
                            HoleScores,
                            MemberName,
                            MemberID
                        FROM Scorecards  
                        WHERE MemberID = @MemberID";

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@MemberID", MemberID);
                            await connection.OpenAsync();

                            using (SqlDataReader reader = await command.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    MemberScorecards.Add(new Scorecard
                                    {
                                        Id = Convert.ToInt32(reader["ScorecardID"]),
                                        Date = Convert.ToDateTime(reader["Date"]),
                                        GolfCourse = reader["GolfCourse"].ToString(),
                                        CourseRating = Convert.ToDouble(reader["CourseRating"]),
                                        SlopeRating = Convert.ToInt32(reader["SlopeRating"]),
                                        TotalScore = Convert.ToInt32(reader["TotalScore"]),
                                        HoleScores = reader["HoleScores"] == DBNull.Value ? null : reader["HoleScores"].ToString(),
                                        MemberName = reader["MemberName"].ToString(),
                                        MemberID = reader["MemberID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["MemberID"])
                                    });
                                }
                            }
                            connection.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Error loading scores: " + ex.Message);
                }
            }
        }

      
    }
