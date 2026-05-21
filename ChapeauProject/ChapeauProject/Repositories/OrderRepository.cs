using ChapeauProject.ViewModels;
using Microsoft.Data.SqlClient;

namespace ChapeauProject.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly string? _connectionString;

        public OrderRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ChapeauProject");
        }

        public List<RunningOrderViewModel> GetRunningOrders() //NOTE getallorderbystatus and make it enum instead of bit
        {
            var orders = new Dictionary<int, RunningOrderViewModel>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT o.OrderID, g.TableNumber, o.OrderTimeStamp,
                        oi.OrderItemID, mi.ItemName, oi.Quantity, oi.PreparationStatus
                    FROM Orders o
                    JOIN Guests g ON o.GuestID = g.GuestID
                    JOIN OrderItems oi ON o.OrderID = oi.OrderID
                    JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID
                    WHERE o.OrderStatus = 0
                    ORDER BY o.OrderTimeStamp ASC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
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
                                OrderItemID = reader.GetInt32(reader.GetOrdinal("OrderItemID")),
                                ItemName = reader.GetString(reader.GetOrdinal("ItemName")),
                                Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                                IsPrepared = reader.GetBoolean(reader.GetOrdinal("PreparationStatus"))
                            });
                        }
                    }
                }
            }

            return orders.Values.ToList();
        }

        public void ToggleItemPreparation(int orderItemId) //NOTE make enum
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "UPDATE OrderItems SET PreparationStatus = ~PreparationStatus WHERE OrderItemID = @OrderItemID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OrderItemID", orderItemId);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public void CompleteOrder(int orderId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "UPDATE Orders SET OrderStatus = 1 WHERE OrderID = @OrderID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OrderID", orderId);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}