using ChapeauProject.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace ChapeauProject.Repositories
{
    public class MenuRepository : RepositoryBase, IMenuRepository
    {
        public MenuRepository(IConfiguration configuration) : base(configuration) { }

        public List<MenuItem> GetCourseFiltered(MenuCard cardFilter, string courseFilter)
        {
            List<MenuItem> items = new List<MenuItem>();

            using (SqlConnection connection = GetConnection())
            {
                string query = @"
                    SELECT MI.MenuItemID, MI.ItemName, MI.Price, ISNULL(S.Quantity, 0) AS Quantity, 
                           ISNULL(C.CourseName, 'N/A') AS CourseName, MI.MenuCard 
                    FROM MenuItems MI
                    LEFT JOIN Stock S ON MI.MenuItemID = S.MenuItemID
                    LEFT JOIN Courses C ON MI.CourseID = C.CourseID
                    WHERE UPPER(MI.MenuCard) = UPPER(@MenuCard)";

                if (courseFilter != "All")
                {
                    query += " AND UPPER(C.CourseName) = UPPER(@CourseName)";
                }

                query += " ORDER BY ISNULL(S.Quantity, 0) ASC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MenuCard", cardFilter.ToString());

                    if (courseFilter != "All")
                    {
                        command.Parameters.AddWithValue("@CourseName", courseFilter);
                    }

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(new MenuItem(
                                (int)reader["MenuItemID"],
                                (string)reader["ItemName"],
                                (decimal)reader["Price"],
                                (int)reader["Quantity"],
                                (string)reader["CourseName"],
                                (string)reader["MenuCard"]
                            ));
                        }
                    }
                }
            }
            return items;
        }

   
        public MenuItem GetById(int menuItemID)
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

        public void DeductStockQuantity(int menuItemID, int amountToDeduct)
        {
            using (SqlConnection connection = GetConnection())
            {
                string query = "UPDATE Stock SET Quantity = Quantity - @Amount WHERE MenuItemID = @MenuItemID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Amount", amountToDeduct);
                    command.Parameters.AddWithValue("@MenuItemID", menuItemID);

              
                    if (connection.State != System.Data.ConnectionState.Open)
                    {
                        connection.Open();
                    }

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}