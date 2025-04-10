using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;

namespace GolfClubReservationSystem.Pages
{
    public class PlayerHandicapModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public PlayerHandicapModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty, Required(ErrorMessage = "Member ID is required.")]
        public int MemberId { get; set; }

        // Updated property names to match the view.
        public bool HandicapCalculated { get; set; } = false;
        public string ErrorMessage { get; set; }
        public int RoundsCount { get; set; }
        public double Best8AverageDifferential { get; set; }
        public double HandicapIndex { get; set; }
        public string Message { get; set; }

        public void OnGet()
        {
            // Initial page load
        }

        public void OnPost(string submit)
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Please enter a valid Member ID.";
                return;
            }

            try
            {
                // List to store each round's handicap differential.
                List<double> differentials = new List<double>();

                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    // Fetch the last 20 rounds for the given MemberId.
                    string query = @"SELECT TOP 20 TotalScore, CourseRating, SlopeRating 
                                     FROM Scorecards 
                                     WHERE MemberId = @MemberId 
                                     ORDER BY Date DESC";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@MemberId", MemberId);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        int totalScore = Convert.ToInt32(reader["TotalScore"]);
                        double courseRating = Convert.ToDouble(reader["CourseRating"]);
                        int slopeRating = Convert.ToInt32(reader["SlopeRating"]);

                        // Calculate differential using the formula:
                        // Differential = (TotalScore - CourseRating) * 113 / SlopeRating
                        double differential = (totalScore - courseRating) * 113 / slopeRating;
                        differentials.Add(differential);
                    }
                    connection.Close();
                }

                RoundsCount = differentials.Count;
                if (RoundsCount == 0)
                {
                    ErrorMessage = "No rounds found for the given Member ID.";
                    return;
                }

                // Sort differentials in ascending order (lower is better).
                differentials.Sort();

                // Select the best 8 rounds. If there are fewer than 8, take all available.
                var best8 = differentials.Take(8).ToList();
                if (best8.Count == 0)
                {
                    ErrorMessage = "Not enough rounds to calculate handicap.";
                    return;
                }

                Best8AverageDifferential = best8.Average();
                // Handicap Index is usually 0.96 times the average differential.
                HandicapIndex = Best8AverageDifferential * 0.96;
                HandicapCalculated = true;
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error calculating handicap: " + ex.Message;
            }
        }
    }
}
