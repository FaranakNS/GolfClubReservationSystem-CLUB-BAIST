//using GolfClubReservationSystem.Models;
//using Microsoft.Data.SqlClient;

//namespace GolfClubReservationSystem.TechnicalServices
//{
//    public class RegisterServices
//    {
//        private readonly string _connectionString;
//        private readonly ILogger<RegisterServices> _logger;

//        public RegisterServices(IConfiguration config, ILogger<RegisterServices> logger)
//        {
//            _connectionString = config.GetConnectionString("ClubBAISTConnection");
//            _logger = logger;
//        }

//        public bool SubmitApplication(MembershipApplication application)
//        {
//            try
//            {
//                using var connection = new SqlConnection(_connectionString);
//                const string query = @"
//            INSERT INTO [golf].[MembershipApplications] 
//            (FirstName, LastName, Email, Phone, Address, PostalCode, 
//             DateOfBirth, Occupation, CompanyName, DesiredMembershipType,
//             ApplicationDate, Status, Notes)
//            VALUES 
//            (@FirstName, @LastName, @Email, @Phone, @Address, @PostalCode,
//             @DateOfBirth, @Occupation, @CompanyName, @DesiredMembershipType,
//             GETDATE(), 'Pending', @Notes)"; // Added PostalCode and Notes

//                using var command = new SqlCommand(query, connection);

//                // Required fields
//                command.Parameters.AddWithValue("@FirstName", application.FirstName);
//                command.Parameters.AddWithValue("@LastName", application.LastName);
//                command.Parameters.AddWithValue("@DateOfBirth", application.DateOfBirth);
//                command.Parameters.AddWithValue("@DesiredMembershipType", application.DesiredMembershipType);

//                // Optional fields (with null handling)
//                AddNullableParameter(command, "@Email", application.Email);
//                AddNullableParameter(command, "@Phone", application.Phone);
//                AddNullableParameter(command, "@Address", application.Address);
//                AddNullableParameter(command, "@PostalCode", application.PostalCode); // Added
//                AddNullableParameter(command, "@Occupation", application.Occupation);
//                AddNullableParameter(command, "@CompanyName", application.CompanyName);
//                AddNullableParameter(command, "@Notes", application.Notes); // Added

//                connection.Open();
//                return command.ExecuteNonQuery() > 0;
//            }
//            catch
//            {
//                return false;
//            }
//        }

//        private void AddNullableParameter(SqlCommand command, string paramName, object value)
//        {
//            command.Parameters.AddWithValue(paramName, value ?? DBNull.Value);
//        }

//        public List<MembershipApplication> GetPendingApplications()
//        {
//            List<MembershipApplication> applications = new();

//            using (SqlConnection connection = new(_connectionString))
//            {
//                string query = @"
//            SELECT 
//               FirstName
//      ,LastName
//      ,Email
//      ,Phone
//      ,Address
//      ,PostalCode
//      ,DateOfBirth
//      ,Occupation
//      ,CompanyName
//      ,DesiredMembershipType
//      ,ApplicationDate
//      ,Status
//      ,StatusUpdatedDate
//      ,Notes
//            FROM [golf].[MembershipApplications]
//            WHERE Status = 'Pending'
//            ORDER BY ApplicationDate DESC";

//                SqlCommand command = new(query, connection);

//                connection.Open();
//                using SqlDataReader reader = command.ExecuteReader();

//                while (reader.Read())
//                {
//                    applications.Add(new MembershipApplication
//                    {
                   
//                        FirstName = reader["FirstName"].ToString(),
//                        LastName = reader["LastName"].ToString(),
//                        Email = reader["Email"]?.ToString(),
//                        Phone = reader["Phone"]?.ToString(),
//                        Address = reader["Address"]?.ToString(),
//                        PostalCode = reader["PostalCode"]?.ToString(),
//                        DateOfBirth = (DateTime)reader["DateOfBirth"],
//                        Occupation = reader["Occupation"]?.ToString(),
//                        CompanyName = reader["CompanyName"]?.ToString(),
//                        DesiredMembershipType = reader["DesiredMembershipType"].ToString(),
//                        ApplicationDate = (DateTime)reader["ApplicationDate"],
//                        Status = reader["Status"].ToString(),
//                        Notes = reader["Notes"]?.ToString()
//                    });
//                }
//            }

//            return applications;
//        }
//    }

//}