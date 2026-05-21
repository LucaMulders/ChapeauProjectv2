using ChapeauProject.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ChapeauProject.Repositories
{
    public class StaffRepository : IStaffRepository
    {
        private readonly string? _connectionString;

        public StaffRepository(IConfiguration configuration)
        {
            // get (database) connection string from appsettings
            _connectionString = configuration.GetConnectionString("ChapeauProject");
        }

        public Staff? GetByLoginCredentials(int staffID, string password)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                const string query = "SELECT StaffID, FirstName, LastName, Role, Password FROM Staff WHERE StaffID = @StaffID";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StaffID", staffID);

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return ReadUser(reader);
                        }
                    }
                }
            }
            return null;
        }

        public List<Staff> GetAll()
        {
            var staffList = new List<Staff>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT StaffID, FirstName, LastName, Role, Password FROM Staff";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            staffList.Add(ReadUser(reader));
                        }
                    }
                }
            }
            return staffList;
        }

        private Staff ReadUser(SqlDataReader reader)
        {
            int id = reader.GetInt32(reader.GetOrdinal("StaffID"));
            string firstName = reader.GetString(reader.GetOrdinal("FirstName"));
            string lastName = reader.GetString(reader.GetOrdinal("LastName"));
            string role = reader.GetString(reader.GetOrdinal("Role"));
            string password = reader.IsDBNull(reader.GetOrdinal("Password")) ? "" : reader.GetString(reader.GetOrdinal("Password"));

            return new Staff(id, firstName, lastName, role, password);
        }

        public Staff? GetById(int staffId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                const string query = "SELECT StaffID, FirstName, LastName, Role, Password FROM Staff WHERE StaffID = @StaffID";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StaffID", staffId);

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return ReadUser(reader);
                        }
                    }
                }
            }
            return null;
        }
    }
}
