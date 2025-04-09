using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;

namespace GolfClubReservationSystem.Pages
{
    public class CancelTeeTimeModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public CancelTeeTimeModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty, Required(ErrorMessage = "StandingTeeTimeID is required.")]
        public int StandingTeeTimeID { get; set; }

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
        public TimeSpan RequestedTeeTime { get; set; }
        [BindProperty]
        public DateTime RequestedStartDate { get; set; }
        [BindProperty]
        public DateTime RequestedEndDate { get; set; }

        public string Message { get; set; }
        public bool IsBookingFound { get; set; } = false;

        public void OnGet()
        {
            Message = string.Empty;
        }

        public void OnPost(string submit)
        {
            Message = string.Empty;

            if (submit == "FetchBooking")
            {
                if (StandingTeeTimeID <= 0)
                {
                    Message = "Please enter a valid Standing Tee Time ID.";
                    return;
                }
                try
                {
                    using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                    {
                        string query = @"SELECT TOP 1 
                                            StandingTeeTimeID, MemberName, MemberID2, MemberName2, 
                                            MemberID3, MemberName3, MemberID4, MemberName4, 
                                            RequestedDayOfWeek, RequestedTeeTime, RequestedStartDate, RequestedEndDate 
                                         FROM StandingTeeTimebooking 
                                         WHERE StandingTeeTimeID = @StandingTeeTimeID 
                                         ORDER BY RequestedStartDate DESC";
                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@StandingTeeTimeID", StandingTeeTimeID);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            StandingTeeTimeID = Convert.ToInt32(reader["StandingTeeTimeID"]);
                            MemberName = reader["MemberName"].ToString();
                            MemberID2 = Convert.ToInt32(reader["MemberID2"]);
                            MemberName2 = reader["MemberName2"].ToString();
                            MemberID3 = Convert.ToInt32(reader["MemberID3"]);
                            MemberName3 = reader["MemberName3"].ToString();
                            MemberID4 = Convert.ToInt32(reader["MemberID4"]);
                            MemberName4 = reader["MemberName4"].ToString();
                            RequestedDayOfWeek = reader["RequestedDayOfWeek"].ToString();
                            RequestedTeeTime = (TimeSpan)reader["RequestedTeeTime"];
                            RequestedStartDate = Convert.ToDateTime(reader["RequestedStartDate"]);
                            RequestedEndDate = Convert.ToDateTime(reader["RequestedEndDate"]);
                            IsBookingFound = true;
                            Message = "Booking found. Click 'Cancel Booking' to cancel your standing tee time request.";
                        }
                        else
                        {
                            IsBookingFound = false;
                            Message = "No standing tee time booking found for the given ID.";
                        }
                        connection.Close();
                    }
                }
                catch (Exception ex)
                {
                    Message = "Error fetching booking: " + ex.Message;
                }
            }
            else if (submit == "CancelBooking")
            {
                if (StandingTeeTimeID <= 0)
                {
                    Message = "Invalid request.";
                    return;
                }
                try
                {
                    using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                    {
                        string query = @"DELETE FROM StandingTeeTimebooking WHERE StandingTeeTimeID = @StandingTeeTimeID";
                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@StandingTeeTimeID", StandingTeeTimeID);
                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        connection.Close();
                        if (rowsAffected > 0)
                        {
                            Message = "Standing tee time booking cancelled successfully!";
                            IsBookingFound = false;
                        }
                        else
                        {
                            Message = "No booking was cancelled. Please verify your details.";
                        }
                    }
                }
                catch (Exception ex)
                {
                    Message = "Error cancelling booking: " + ex.Message;
                }
            }
        }
    }
}
