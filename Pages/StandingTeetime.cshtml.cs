using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace GolfClubReservationSystem.Pages
{
    public class StandingTeetimeModel : PageModel
    {

        private readonly IConfiguration _configuration;

        public StandingTeetimeModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty]
        public int MemberID { get; set; }

        [BindProperty]
        public string MemberName { get; set; }

        [BindProperty]
        public int MemberID2 { get; set; }

        [BindProperty]
        public string MemberName2 { get; set; }

        [BindProperty]
        public int MemberID3 { get; set; }

        [BindProperty]
        public string MemberName3 { get; set; }

        [BindProperty]
        public int MemberID4 { get; set; }

        [BindProperty]
        public string MemberName4 { get; set; }

        [BindProperty]
        public string RequestedDayOfWeek { get; set; }

        [BindProperty]
        public string RequestedTeeTime { get; set; }

        [BindProperty]
        public DateTime RequestedStartDate { get; set; }

        [BindProperty]
        public DateTime RequestedEndDate { get; set; }

        public string Message { get; set; }

        public void OnPost()
        {
            string membershipLevel = string.Empty;

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                string query = "SELECT MembershipLevel FROM Member WHERE MemberID = @MemberID";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@MemberID", MemberID);
                connection.Open();
                var result = command.ExecuteScalar();
                if (result != null)
                {
                    membershipLevel = result.ToString().Trim().ToLower();
                }
                connection.Close();
            }

            if (membershipLevel != "shareholder")
            {
                Message = "Only Shareholders are allowed to book Standing Tee Times.";
                return;
            }

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                string query = @"INSERT INTO StandingTeeTimebooking 
                        (MemberID, MemberName, MemberID2, MemberName2, MemberID3, MemberName3, MemberID4, MemberName4, 
                         RequestedDayOfWeek, RequestedTeeTime, RequestedStartDate, RequestedEndDate) 
                    VALUES 
                        (@MemberID, @MemberName, @MemberID2, @MemberName2, @MemberID3, @MemberName3, @MemberID4, @MemberName4, 
                         @RequestedDayOfWeek, @RequestedTeeTime, @RequestedStartDate, @RequestedEndDate)";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@MemberID", MemberID);
                command.Parameters.AddWithValue("@MemberName", MemberName);
                command.Parameters.AddWithValue("@MemberID2", MemberID2);
                command.Parameters.AddWithValue("@MemberName2", MemberName2);
                command.Parameters.AddWithValue("@MemberID3", MemberID3);
                command.Parameters.AddWithValue("@MemberName3", MemberName3);
                command.Parameters.AddWithValue("@MemberID4", MemberID4);
                command.Parameters.AddWithValue("@MemberName4", MemberName4);
                command.Parameters.AddWithValue("@RequestedDayOfWeek", RequestedDayOfWeek);
                command.Parameters.AddWithValue("@RequestedTeeTime", RequestedTeeTime);
                command.Parameters.AddWithValue("@RequestedStartDate", RequestedStartDate);
                command.Parameters.AddWithValue("@RequestedEndDate", RequestedEndDate);

                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }

            Message = "Standing tee time booked successfully.";
        }
    }
}
