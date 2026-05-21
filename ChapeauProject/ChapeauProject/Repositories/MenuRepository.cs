using ChapeauProject.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;

namespace ChapeauProject.Repositories
{
    public class MenuRepository : IMenuRepository
    {
        private readonly string _connectionString;

        public MenuRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ChapeauProject");
        }

        public List<MenuItem> GetFiltered(string cardFilter, string courseFilter) //NOTE improve name of getfiltered
        {
            List<MenuItem> items = new List<MenuItem>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                // LEFT JOIN ensures items appear even if Stock or Course links are missing [cite: 7, 13, 19]
                // UPPER() handles potential case-sensitivity issues between C# and SQL
                string query = @"
                    SELECT 
                        MI.MenuItemID, 
                        MI.ItemName, 
                        MI.Price, 
                        ISNULL(S.Quantity, 0) AS Quantity, 
                        ISNULL(C.CourseName, 'N/A') AS CourseName, 
                        MI.MenuCard 
                    FROM MenuItems MI
                    LEFT JOIN Stock S ON MI.MenuItemID = S.MenuItemID
                    LEFT JOIN Courses C ON MI.CourseID = C.CourseID
                    WHERE UPPER(MI.MenuCard) = UPPER(@MenuCard)";

                if (courseFilter != "All")
                {
                    query += " AND UPPER(C.CourseName) = UPPER(@CourseName)";
                }

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MenuCard", cardFilter);

                    if (courseFilter != "All")
                    {
                        command.Parameters.AddWithValue("@CourseName", courseFilter);
                    }

                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Mapping database columns to the MenuItem model
                            MenuItem item = new MenuItem(
                                (int)reader["MenuItemID"],
                                (string)reader["ItemName"],
                                (decimal)reader["Price"],
                                (int)reader["Quantity"],
                                (string)reader["CourseName"],
                                (string)reader["MenuCard"]
                            );
                            items.Add(item);
                        }
                    }
                }
            }
            return items;
        }
    }
}