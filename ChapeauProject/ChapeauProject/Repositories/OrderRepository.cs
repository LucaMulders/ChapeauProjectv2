using ChapeauProject.Models;
using ChapeauProject.ViewModels;
using Microsoft.Data.SqlClient;

namespace ChapeauProject.Repositories
{
    public class OrderRepository : RepositoryBase, IOrderRepository
    {
        public OrderRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public List<RunningOrderViewModel> GetAllOrdersByStatus()
        {
            var orders = new Dictionary<int, RunningOrderViewModel>();

            using (SqlConnection connection = GetConnection())
            {
                string query = @"
                    SELECT o.OrderID, g.TableNumber, o.OrderTimeStamp,
                        oi.OrderItemID, mi.ItemName, oi.Quantity, oi.PreparationStatus,
                        mi.MenuCard, ISNULL(c.CourseName, 'Other') AS CourseName
                    FROM Orders o
                    JOIN Guests g ON o.GuestID = g.GuestID
                    JOIN OrderItems oi ON o.OrderID = oi.OrderID
                    JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID
                    LEFT JOIN Courses c ON mi.CourseID = c.CourseID
                    WHERE o.OrderStatus = 'Pending'
                    ORDER BY o.OrderTimeStamp ASC, c.CourseID ASC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int orderID = reader.GetInt32(reader.GetOrdinal("OrderID"));

                            if (!orders.ContainsKey(orderID))
                            {
                                orders[orderID] = new RunningOrderViewModel
                                {
                                    OrderID = orderID,
                                    TableNumber = reader.GetInt32(reader.GetOrdinal("TableNumber")),
                                    OrderTime = DateTime.SpecifyKind(reader.GetDateTime(reader.GetOrdinal("OrderTimeStamp")), DateTimeKind.Utc),
                                    Items = new List<RunningOrderItemViewModel>()
                                };
                            }

                            orders[orderID].Items.Add(new RunningOrderItemViewModel
                            {
                                OrderItemID       = reader.GetInt32(reader.GetOrdinal("OrderItemID")),
                                ItemName          = reader.GetString(reader.GetOrdinal("ItemName")),
                                Quantity          = reader.GetInt32(reader.GetOrdinal("Quantity")),
                                PreparationStatus = Enum.Parse<PreparationStatus>(reader.GetString(reader.GetOrdinal("PreparationStatus"))),
                                MenuCard          = reader.GetString(reader.GetOrdinal("MenuCard")),
                                CourseName        = reader.GetString(reader.GetOrdinal("CourseName"))
                            });
                        }
                    }
                }
            }

            return orders.Values.ToList();
        }

        public void ToggleItemPreparation(int orderItemId)
        {
            using (SqlConnection connection = GetConnection())
            {
                string query = @"UPDATE OrderItems SET PreparationStatus =
                    CASE PreparationStatus
                        WHEN 'Pending'   THEN 'Preparing'
                        WHEN 'Preparing' THEN 'Ready'
                        WHEN 'Ready'     THEN 'Pending'
                    END
                    WHERE OrderItemID = @OrderItemID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OrderItemID", orderItemId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void CompleteOrder(int orderId)
        {
            using (SqlConnection connection = GetConnection())
            {
                string query = "UPDATE Orders SET OrderStatus = 'Complete' WHERE OrderID = @OrderID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OrderID", orderId);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}