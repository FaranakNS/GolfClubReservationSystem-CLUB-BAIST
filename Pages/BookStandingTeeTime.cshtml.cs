using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;

namespace GolfClubReservationSystem.Pages
{
public class BookStandinTeeTimeModel : PageModel
        {
            private readonly IConfiguration _configuration;

            public BookStandinTeeTimeModel(IConfiguration configuration)
            {
                _configuration = configuration;
            }
            [BindProperty]
            [Required(ErrorMessage = "MemberID is required.")]
            public int MemberID { get; set; }
            [BindProperty]
            [Required(ErrorMessage = "MemberName is required.")]
            public string MemberName { get; set; }

            [BindProperty]
            [Required(ErrorMessage = "MemberID2  is required.")]

            public int MemberID2 { get; set; }
            [BindProperty]
            [Required(ErrorMessage = "MemberName2 is required.")]

            public string MemberName2 { get; set; }
            [BindProperty]
            [Required(ErrorMessage = "MemberID3 is required.")]

            public int MemberID3 { get; set; }
            [BindProperty]
            [Required(ErrorMessage = "MemberName3 is required.")]

            public string MemberName3 { get; set; }
            [BindProperty]
            [Required(ErrorMessage = "MemberID4 is required.")]

            public int MemberID4 { get; set; }
            [BindProperty]
            [Required(ErrorMessage = "MemberName4 is required.")]

            public string MemberName4 { get; set; }

            [BindProperty]
            [Required(ErrorMessage = "Requested day of the week is required.")]
            public string RequestedDayOfWeek { get; set; }

            [BindProperty]
            [Required(ErrorMessage = "Please select a time interval.")]
            public int RequestedTeeTime { get; set; }
            [BindProperty]
            public DateTime RequestedStartDate { get; set; } = DateTime.Today;
            [BindProperty]
            public DateTime RequestedEndDate { get; set; } = DateTime.Today;




            public string Message { get; set; }

            public void OnGet()
            {

                Message = string.Empty;
            }

            public void OnPost()
            {
                if (!ModelState.IsValid)
                {
                    return;
                }

                try
                {
                    using (SqlConnection connection = new SqlConnection("Persist Security Info=False;Database=fnasoori1;User ID=fnasoori1;Password=Imfarah101;server=dev1.baist.ca;"))
                    {
                        string query = "INSERT INTO StandingTeeTimebooking(MemberID,MemberName,MemberID2,MemberName2,MemberID3,MemberName3,MemberID4,MemberName4,RequestedDayOfWeek,RequestedTeeTime,RequestedStartDate,RequestedEndDate) " +
                       "VALUES(@MemberID,@MemberName,@MemberID2,@MemberName2,@MemberID3,@MemberName3,@MemberID4,@MemberName4,@RequestedDayOfWeek,@RequestedTeeTime,@RequestedStartDate,@RequestedEndDate)";

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

                    Message = $"Standing Tee time request submitted successfully! " +
                          $"Selected interval: {RequestedTeeTime} minutes.";
                    return;

                }
                catch (SqlException sqlEx)
                {
                    Console.WriteLine($"SQL Error: {sqlEx.Message}");

                    Message = "An unexpected error occurred. Please try again with valid values.";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");

                    Message = "An unexpected error occurred. Please try again with valid values.";
                }
            }



        }
    }





