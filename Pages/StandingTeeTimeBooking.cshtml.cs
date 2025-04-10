using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace GolfClubReservationSystem.Pages
{
    public class StandingTeeTimeBookingModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public StandingTeeTimeBookingModel(IConfiguration configuration)
        {
            _configuration = configuration;
            Requests = new List<StandingTeeTimeRequest>();
        }

        // List of standing tee time requests.
        public List<StandingTeeTimeRequest> Requests { get; set; }

        // Feedback message to the user.
        public string Message { get; set; }

        public async Task OnGetAsync()
        {
            Requests.Clear();
            Message = string.Empty;
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();

                    string query = @"
                        SELECT TOP (1000)
                              StandingTeeTimeID,
                              MemberID,
                              MemberName,
                              MemberID2,
                              MemberName2,
                              MemberID3,
                              MemberName3,
                              MemberID4,
                              MemberName4,
                              RequestedDayOfWeek,
                              RequestedTeeTime,
                              RequestedStartDate,
                              RequestedEndDate
                        FROM [fnasoori1].[dbo].[StandingTeeTimebooking]
                        ORDER BY RequestedStartDate, RequestedTeeTime";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Requests.Add(new StandingTeeTimeRequest
                                {
                                    StandingTeeTimeID = reader["StandingTeeTimeID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["StandingTeeTimeID"]),
                                    MemberID = reader["MemberID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["MemberID"]),
                                    MemberName = reader["MemberName"]?.ToString(),
                                    MemberID2 = reader["MemberID2"] == DBNull.Value ? 0 : Convert.ToInt32(reader["MemberID2"]),
                                    MemberName2 = reader["MemberName2"]?.ToString(),
                                    MemberID3 = reader["MemberID3"] == DBNull.Value ? 0 : Convert.ToInt32(reader["MemberID3"]),
                                    MemberName3 = reader["MemberName3"]?.ToString(),
                                    MemberID4 = reader["MemberID4"] == DBNull.Value ? 0 : Convert.ToInt32(reader["MemberID4"]),
                                    MemberName4 = reader["MemberName4"]?.ToString(),
                                    RequestedDayOfWeek = reader["RequestedDayOfWeek"]?.ToString(),
                                    RequestedTeeTime = reader["RequestedTeeTime"]?.ToString(),
                                    RequestedStartDate = reader["RequestedStartDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["RequestedStartDate"]),
                                    RequestedEndDate = reader["RequestedEndDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["RequestedEndDate"])
                                });
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Message = "Error loading standing tee time requests: " + ex.Message;
            }
        }

        // Handler when the staff accepts a standing tee time request.
        public async Task<IActionResult> OnPostAcceptAsync(int standingTeeTimeID)
        {
            Message = string.Empty;
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();

                    // Retrieve the request details
                    string fetchQuery = @"
                        SELECT MemberID, MemberName, RequestedDayOfWeek, RequestedTeeTime, RequestedStartDate, RequestedEndDate 
                        FROM [fnasoori1].[dbo].[StandingTeeTimebooking]
                        WHERE StandingTeeTimeID = @StandingTeeTimeID";

                    int memberID = 0;
                    string memberName = string.Empty;
                    string requestedDay = string.Empty;
                    string requestedTeeTime = string.Empty;
                    DateTime startDate = DateTime.MinValue;
                    DateTime endDate = DateTime.MinValue;

                    using (SqlCommand fetchCommand = new SqlCommand(fetchQuery, connection))
                    {
                        fetchCommand.Parameters.AddWithValue("@StandingTeeTimeID", standingTeeTimeID);
                        using (SqlDataReader reader = await fetchCommand.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                memberID = reader["MemberID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["MemberID"]);
                                memberName = reader["MemberName"]?.ToString();
                                requestedDay = reader["RequestedDayOfWeek"]?.ToString();
                                requestedTeeTime = reader["RequestedTeeTime"]?.ToString();
                                startDate = reader["RequestedStartDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["RequestedStartDate"]);
                                endDate = reader["RequestedEndDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["RequestedEndDate"]);
                            }
                        }
                    }

                    // Insert into BookTeeTime table using the retrieved details.
                    string insertQuery = @"
                        INSERT INTO BookTeeTime 
                        (MemberID, MemberName, Date, Time, GolfCourse, NumberOfPlayers, NumberOfCarts, StandingStatus, EmployeeID)
                        VALUES 
                        (@MemberID, @MemberName, @Date, @Time, @GolfCourse, @NumberOfPlayers, @NumberOfCarts, @StandingStatus, @EmployeeID)";

                    // For this example, we assume default values for some fields
                    // and that the booking date is chosen from the requested start date.
                    using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@MemberID", memberID);
                        insertCommand.Parameters.AddWithValue("@MemberName", memberName);
                        // We'll use the start date (or any logic you require) for the booking date.
                        insertCommand.Parameters.AddWithValue("@Date", startDate);
                        // Use the requested tee time as the booking time.
                        insertCommand.Parameters.AddWithValue("@Time", requestedTeeTime);
                        // Assume a default golf course value, or set it from additional data.
                        insertCommand.Parameters.AddWithValue("@GolfCourse", "Default Course");
                        // Default numeric values for players, carts, and employee if not provided.
                        insertCommand.Parameters.AddWithValue("@NumberOfPlayers", 4);
                        insertCommand.Parameters.AddWithValue("@NumberOfCarts", 2);
                        // Set StandingStatus as confirmed or active.
                        insertCommand.Parameters.AddWithValue("@StandingStatus", "Confirmed");
                        // Optionally, set EmployeeID (if the clerk is recorded); use 0 if none.
                        insertCommand.Parameters.AddWithValue("@EmployeeID", 0);
                        await insertCommand.ExecuteNonQueryAsync();
                    }

                    // Delete the request from StandingTeeTimebooking.
                    string deleteQuery = "DELETE FROM [fnasoori1].[dbo].[StandingTeeTimebooking] WHERE StandingTeeTimeID = @StandingTeeTimeID";
                    using (SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection))
                    {
                        deleteCommand.Parameters.AddWithValue("@StandingTeeTimeID", standingTeeTimeID);
                        await deleteCommand.ExecuteNonQueryAsync();
                    }

                    connection.Close();
                    Message = $"Request {standingTeeTimeID} accepted; booking created successfully.";
                }
            }
            catch (Exception ex)
            {
                Message = "Error accepting request: " + ex.Message;
            }
            await OnGetAsync(); // Reload data.
            return Page();
        }

        // Handler to reject a standing tee time request (delete the row)
        public async Task<IActionResult> OnPostRejectAsync(int standingTeeTimeID)
        {
            Message = string.Empty;
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();
                    string deleteQuery = "DELETE FROM [fnasoori1].[dbo].[StandingTeeTimebooking] WHERE StandingTeeTimeID = @StandingTeeTimeID";
                    using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@StandingTeeTimeID", standingTeeTimeID);
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                            Message = $"Request {standingTeeTimeID} rejected and deleted successfully.";
                        else
                            Message = $"Request {standingTeeTimeID} not found.";
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Message = "Error rejecting request: " + ex.Message;
            }
            await OnGetAsync();
            return Page();
        }
    }

    // Model representing a standing tee time request.
    public class StandingTeeTimeRequest
    {
        public int StandingTeeTimeID { get; set; }
        public int MemberID { get; set; }
        public string MemberName { get; set; }
        public int MemberID2 { get; set; }
        public string MemberName2 { get; set; }
        public int MemberID3 { get; set; }
        public string MemberName3 { get; set; }
        public int MemberID4 { get; set; }
        public string MemberName4 { get; set; }
        public string RequestedDayOfWeek { get; set; }
        public string RequestedTeeTime { get; set; }
        public DateTime RequestedStartDate { get; set; }
        public DateTime RequestedEndDate { get; set; }
    }
}
