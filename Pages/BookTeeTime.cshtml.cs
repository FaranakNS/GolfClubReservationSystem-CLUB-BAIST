using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;

namespace GolfClubReservationSystem.Pages
{
    public class BookTeeTimeModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public BookTeeTimeModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty]
        [Required(ErrorMessage = "Member ID is required.")]
        public int MemberId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Member name is required.")]
        public string MemberName { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Membership level is required.")]
        [StringLength(50)]
        public string MembershipLevel { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Standing status is required.")]
        [StringLength(50)]
        public string StandingStatus { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Date is required.")]
        public DateTime Date { get; set; } = DateTime.Today;

        [BindProperty]
        [Required(ErrorMessage = "Time is required.")]
        public TimeSpan Time { get; set; }

        [BindProperty]
        public string submit { get; set; } = string.Empty;

        [BindProperty]
        [Range(0, 4, ErrorMessage = "Number of carts must be between 0 and 4.")]
        public int NumberOfPlayers { get; set; }

        [BindProperty]
        public int NumberOfCarts { get; set; }

        public string Message { get; set; }

        public void OnPost()
        {
            switch (submit)
            {
                case "MemberDetails":
                    FetchMemberDetails();
                    break;

                case "Submit":
                    SubmitBooking();
                    break;
            }
        }

        private void FetchMemberDetails()
        {
            if (MemberId <= 0)
            {
                Message = "Please enter a valid Member ID.";
                return;
            }

            using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            string query = "SELECT MemberName, MembershipLevel FROM Member WHERE MemberId = @MemberId";

            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@MemberId", MemberId);

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                MemberName = reader["MemberName"].ToString();
                MembershipLevel = reader["MembershipLevel"].ToString();

                TempData["MemberName"] = MemberName;
                TempData["MembershipLevel"] = MembershipLevel;

                Message = "Member details fetched successfully.";
            }
            else
            {
                Message = "No member found with the given Member ID.";
            }

            connection.Close();
        }

        private void SubmitBooking()
        {
            if (!ModelState.IsValid)
            {
                Message = "Please fill in all required fields correctly.";
                return;
            }

            if (StandingStatus != "Good")
            {
                Message = "Booking is only allowed for members with a 'Good' standing status.";
                return;
            }

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                string levelQuery = "SELECT MembershipLevel FROM Member WHERE MemberId = @MemberId";
                using SqlCommand cmd = new SqlCommand(levelQuery, connection);
                cmd.Parameters.AddWithValue("@MemberId", MemberId);
                connection.Open();
                var result = cmd.ExecuteScalar();
                if (result != null) MembershipLevel = result.ToString();
                connection.Close();
            }

            var today = DateTime.Today;
            var maxDate = today.AddDays(7);
            if (Date < today || Date > maxDate)
            {
                Message = "You can only book tee times from today up to 7 days in advance.";
                return;
            }

            if (!IsTimeValidForMember(MembershipLevel, Date, Time))
            {
                Message = $"Invalid time for this membership on this day. Please choose a valid time.";
                return;
            }

            using SqlConnection connection2 = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            string query = @"IF NOT EXISTS (SELECT 1 FROM BookTeeTime WHERE Date = @Date AND Time = @Time)
                BEGIN
                    INSERT INTO BookTeeTime 
                    (MemberId, MemberName, MembershipLevel, StandingStatus, Date, Time, NumberOfPlayers, NumberOfCarts) 
                    VALUES 
                    (@MemberId, @MemberName, @MembershipLevel, @StandingStatus, @Date, @Time, @NumberOfPlayers, @NumberOfCarts)
                END
                ELSE
                BEGIN
                    RAISERROR('This time slot is already booked.', 16, 1);
                END";

            using SqlCommand command = new SqlCommand(query, connection2);
            command.Parameters.AddWithValue("@MemberId", MemberId);
            command.Parameters.AddWithValue("@MemberName", MemberName);
            command.Parameters.AddWithValue("@MembershipLevel", MembershipLevel);
            command.Parameters.AddWithValue("@StandingStatus", StandingStatus);
            command.Parameters.AddWithValue("@Date", Date);
            command.Parameters.AddWithValue("@Time", Time.ToString(@"hh\:mm\:ss"));
            command.Parameters.AddWithValue("@NumberOfPlayers", NumberOfPlayers);
            command.Parameters.AddWithValue("@NumberOfCarts", NumberOfCarts);

            try
            {
                connection2.Open();
                command.ExecuteNonQuery();
                Message = $"Tee time booked successfully for {MemberName} on {Date.ToShortDateString()} at {Time}.";
            }
            catch (SqlException ex)
            {
                Message = ex.Message;
            }
            finally
            {
                connection2.Close();
            }
        }

        private bool IsTimeValidForMember(string level, DateTime date, TimeSpan time)
        {
            var day = date.DayOfWeek;
            bool isWeekend = day == DayOfWeek.Saturday || day == DayOfWeek.Sunday;
            string membership = level?.Trim().ToLower();

            return membership switch
            {
                "gold" => true,

                "shareholder" => true,

                "silver" => isWeekend
                    ? time >= new TimeSpan(11, 0, 0)
                    : (time < new TimeSpan(15, 0, 0) || time >= new TimeSpan(17, 30, 0)),

                "bronze" => isWeekend
                    ? time >= new TimeSpan(13, 0, 0) || time >= new TimeSpan(18, 0, 0)
                    : (time < new TimeSpan(15, 0, 0)),

                _ => false
            };
        }
    }
}
