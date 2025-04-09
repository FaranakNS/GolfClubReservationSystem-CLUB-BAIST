using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;

namespace GolfClubReservationSystem.Pages
{
    public class ModifyTeeTimeModel : PageModel
    {
 
            private readonly IConfiguration _configuration;

            public ModifyTeeTimeModel(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            [BindProperty]
            public int BookingId { get; set; }

            [BindProperty, Required(ErrorMessage = "Member ID is required.")]
            public int MemberId { get; set; }

            [BindProperty, Required(ErrorMessage = "Member name is required.")]
            public string MemberName { get; set; }

            [BindProperty, Required(ErrorMessage = "Membership level is required.")]
            public string MembershipLevel { get; set; }

            [BindProperty, Required(ErrorMessage = "Standing status is required.")]
            public string StandingStatus { get; set; }

            [BindProperty, Required(ErrorMessage = "Date is required.")]
            public DateTime Date { get; set; } = DateTime.Today;

            [BindProperty, Required(ErrorMessage = "Time is required.")]
            public TimeSpan Time { get; set; }
            [BindProperty]
            [Range(0, 4, ErrorMessage = "Number of carts must be between 0 and 4.")]
            public int NumberOfPlayers { get; set; }

            [BindProperty]

            public int NumberOfCarts { get; set; }

            public string Message { get; set; }
            public bool IsBookingFound { get; set; } = false;

            public void OnGet()
            {
                Message = string.Empty;
            }

            public void OnPost(string submit)
            {
                Message = string.Empty;

                if (submit == "FetchDetails")
                {
                    if (MemberId <= 0)
                    {
                        Message = "Please enter a valid Member ID.";
                        return;
                    }

                    try
                    {
                        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                        string query = @"SELECT TOP 1 BookingId, MemberName, MembershipLevel, StandingStatus, Date, Time ,NumberOfPlayers,NumberOfCarts
                                     FROM BookTeeTime 
                                     WHERE MemberId = @MemberId 
                                     ORDER BY Date DESC";

                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@MemberId", MemberId);
                        connection.Open();

                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            BookingId = Convert.ToInt32(reader["BookingId"]);
                            MemberName = reader["MemberName"].ToString();
                            MembershipLevel = reader["MembershipLevel"].ToString();
                            StandingStatus = reader["StandingStatus"].ToString();
                            Date = Convert.ToDateTime(reader["Date"]);
                            Time = TimeSpan.Parse(reader["Time"].ToString());
                            NumberOfPlayers = Convert.ToInt32(reader["NumberOfPlayers"]);
                            NumberOfCarts = Convert.ToInt32(reader["NumberOfCarts"]);
                            IsBookingFound = true;
                            Message = "Booking details fetched. You can now modify and submit updates.";
                        }
                        else
                        {
                            Message = "No booking found for the given Member ID.";
                        }

                        connection.Close();
                    }
                    catch (Exception ex)
                    {
                        Message = "Error fetching booking: " + ex.Message;
                    }
                }
                else if (submit == "UpdateBooking")
                {
                    if (!ModelState.IsValid)
                    {
                        Message = "Validation failed. Please complete all required fields.";
                        return;
                    }

                    try
                    {
                        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                        string query = @"UPDATE BookTeeTime 
                 SET MemberName = @MemberName, 
                     MembershipLevel = @MembershipLevel, 
                     StandingStatus = @StandingStatus, 
                     Date = @Date, 
                     Time = @Time,
                     NumberOfPlayers = @NumberOfPlayers,
                     NumberOfCarts = @NumberOfCarts
                 WHERE BookingId = @BookingId";


                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@MemberName", MemberName);
                        command.Parameters.AddWithValue("@MembershipLevel", MembershipLevel);
                        command.Parameters.AddWithValue("@StandingStatus", StandingStatus);
                        command.Parameters.AddWithValue("@Date", Date);
                        command.Parameters.AddWithValue("@Time", Time.ToString());
                        command.Parameters.AddWithValue("@NumberOfPlayers", NumberOfPlayers);
                        command.Parameters.AddWithValue("@NumberOfCarts", NumberOfCarts);
                        command.Parameters.AddWithValue("@BookingId", BookingId);
                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        connection.Close();

                        if (rowsAffected > 0)
                        {
                            Message = "Booking updated successfully!";
                        }
                        else
                        {
                            Message = "Update failed. Booking ID might be incorrect or data unchanged.";
                        }
                    }
                    catch (Exception ex)
                    {
                        Message = "Error updating booking: " + ex.Message;
                    }
                }
            }
        }
    }

