using ChapeauProject.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ChapeauProject.Repositories
{
    public class StaffRepository : RepositoryBase, IStaffRepository
    {
        public StaffRepository(string connectionString) : base(connectionString)
        {
        }

        public Staff? GetByLoginCredentials(int staffID, string password)
        {
            using (var connection = GetConnection())
            {
                const string query = "SELECT StaffID, FirstName, LastName, Role, Password FROM Staff WHERE StaffID = @StaffID AND Password = @Password";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StaffID", staffID);
                    command.Parameters.AddWithValue("@Password", password);

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

        private Staff ReadUser(SqlDataReader reader)
        {
            int id = reader.GetInt32(reader.GetOrdinal("StaffID"));
            string firstName = reader.GetString(reader.GetOrdinal("FirstName"));
            string lastName = reader.GetString(reader.GetOrdinal("LastName"));
            StaffRole role = Enum.Parse<StaffRole>(reader.GetString(reader.GetOrdinal("Role")));
            string password;
            if (reader.IsDBNull(reader.GetOrdinal("Password")))
            {
                password = "";
            }
            else
            {
                password = reader.GetString(reader.GetOrdinal("Password"));
            }

            return new Staff(id, firstName, lastName, role, password);
        }

    }
}
