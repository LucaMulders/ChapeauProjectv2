using ChapeauProject.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace ChapeauProject.Repositories
{
    public class MenuRepository : RepositoryBase, IMenuRepository
    {
        public MenuRepository(IConfiguration configuration) : base(configuration) { }

        public List<MenuItem> GetCourseFiltered(MenuCard cardFilter, string courseFilter)
        {
            string query = GetSqlForFilteredMenu(courseFilter);
            List<MenuItem> items = new List<MenuItem>();

          
            try
            {
                using (SqlConnection connection = GetConnection())
                {
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
                                items.Add(ReadMenuItem(reader));
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
               
                throw new Exception("Database error occurred while fetching the filtered menu: " + ex.Message);
            }

            return items;
        }

        private string GetSqlForFilteredMenu(string courseFilter)
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

            return query;
        }

        private MenuItem ReadMenuItem(SqlDataReader reader)
        {
            string cardString = (string)reader["MenuCard"];

            MenuCard cardEnum;
            if (!Enum.TryParse(cardString, true, out cardEnum))
            {
                cardEnum = MenuCard.Lunch; // default 
            }

            Menu associatedMenu = new Menu(cardEnum);

            return new MenuItem(
                (int)reader["MenuItemID"],
                (string)reader["ItemName"],
                (decimal)reader["Price"],
                (int)reader["Quantity"],
                associatedMenu
            );
        }

        public MenuItem GetById(int menuItemID)
        {
            // Search Single row can fail if connection drops
            try
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
                                return ReadMenuItem(reader);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Database error occurred while fetching menu item ID " + menuItemID + ": " + ex.Message);
            }

            return null;
        }

        public void DeductStockQuantity(int menuItemID, int amountToDeduct)
        {
            
            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    string query = "UPDATE Stock SET Quantity = Quantity - @Amount WHERE MenuItemID = @MenuItemID";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Amount", amountToDeduct);
                        command.Parameters.AddWithValue("@MenuItemID", menuItemID);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Failed to update stock database quantity for item " + menuItemID + ": " + ex.Message);
            }
        }
    }
}