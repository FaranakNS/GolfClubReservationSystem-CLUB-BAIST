using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace GolfClubReservationSystem.Pages
{
    public class TeesheetModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public TeesheetModel(IConfiguration configuration)
        {
            _configuration = configuration;
            Bookings = new List<TeeTimeBooking>();
        }

        // List to hold all bookings from the BookTeeTime table.
        public List<TeeTimeBooking> Bookings { get; set; }

        // Optional message for error or informational messages.
        public string Message { get; set; }

        public async Task OnGetAsync()
        {
            Bookings.Clear();
            Message = string.Empty;

            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();

                    string query = @"
                        SELECT TOP (1000)
                              BookingId,
                              MemberID,
                              MemberName,
                              NumberOfPlayers,
                              NumberOfCarts,
                              MembershipLevel,
                              StandingStatus,
                              Date,
                              Time,
                              EmployeeID
                        FROM [fnasoori1].[dbo].[BookTeeTime]
                        ORDER BY Date, Time";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Bookings.Add(new TeeTimeBooking
                                {
                                    BookingId = reader["BookingId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["BookingId"]),
                                    MemberID = reader["MemberID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["MemberID"]),
                                    MemberName = reader["MemberName"]?.ToString(),
                                    NumberOfPlayers = reader["NumberOfPlayers"] == DBNull.Value ? 0 : Convert.ToInt32(reader["NumberOfPlayers"]),
                                    NumberOfCarts = reader["NumberOfCarts"] == DBNull.Value ? 0 : Convert.ToInt32(reader["NumberOfCarts"]),
                                    MembershipLevel = reader["MembershipLevel"]?.ToString(),
                                    StandingStatus = reader["StandingStatus"]?.ToString(),
                                    Date = reader["Date"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["Date"]),
                                    Time = reader["Time"]?.ToString(),
                                    EmployeeID = reader["EmployeeID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["EmployeeID"])
                                });
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Message = "Error loading bookings: " + ex.Message;
            }
        }
    }

    // Model representing each tee time booking record.
    public class TeeTimeBooking
    {
        public int BookingId { get; set; }
        public int MemberID { get; set; }
        public string MemberName { get; set; }
        public int NumberOfPlayers { get; set; }
        public int NumberOfCarts { get; set; }
        public string MembershipLevel { get; set; }
        public string StandingStatus { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public int EmployeeID { get; set; }
    }
}
