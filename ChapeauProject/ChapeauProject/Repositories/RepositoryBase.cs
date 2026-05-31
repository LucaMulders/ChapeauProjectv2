using ChapeauProject.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ChapeauProject.Repositories
{
    public abstract class RepositoryBase
    {
        protected readonly string? _connectionString;

        protected RepositoryBase(IConfiguration configuration)
        {
            // get the (database) connection string from appsettings
            _connectionString = configuration.GetConnectionString("ChapeauProject");
        }

        public MenuItem? GetById(int menuItemID)
        {
            using (SqlConnection connection = GetConnection())
            {
                string query = @"
                    SELECT MI.MenuItemID, MI.ItemName, MI.Price, ISNULL(S.Quantity, 0) AS Quantity, 
                           ISNULL(C.CourseName, 'N/A') AS CourseName, MI.MenuCard 
                    FROM MenuItems MI
                    LEFT JOIN Stock S ON MI.MenuItemID = S.MenuItemID
                    LEFT JOIN Courses C ON MI.CourseID = C.CourseID
                    WHERE MI.MenuItemID = @MenuItemID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MenuItemID", menuItemID);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new MenuItem(
                                (int)reader["MenuItemID"],
                                (string)reader["ItemName"],
                                (decimal)reader["Price"],
                                (int)reader["Quantity"],
                                (string)reader["CourseName"],
                                (string)reader["MenuCard"]
                            );
                        }
                    }
                }
            }
            return null;
        }

        protected SqlConnection GetConnection()
        {
            try
            {
                SqlConnection connection = new SqlConnection(_connectionString);
                connection.Open();
                return connection;
            }
            catch (SqlException ex)
            {
                throw new Exception("Could not open a connection to the database.", ex);
            }
        }
    }
}
