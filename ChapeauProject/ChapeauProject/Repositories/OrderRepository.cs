using ChapeauProject.Models;
using ChapeauProject.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace ChapeauProject.Repositories
{
    public class OrderRepository : RepositoryBase, IOrderRepository
    {
        private readonly IMenuRepository _menuRepository;

        public OrderRepository(IConfiguration configuration, IMenuRepository menuRepository) : base(configuration)
        {
            _menuRepository = menuRepository;
        }

        public void SaveNewOrder(Order order)
        {
            using (SqlConnection connection = GetConnection())
            {
                int generatedOrderID = InsertOrder(order, connection);
                InsertOrderItems(order.OrderItems, generatedOrderID, connection);
            }
        }


        private int InsertOrder(Order order, SqlConnection connection)
        {
            string sql = @"
                INSERT INTO Orders (GuestID, OrderTimeStamp, OrderStatus)
                VALUES (@GuestID, @OrderTimeStamp, @OrderStatus);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            using (SqlCommand cmd = new SqlCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@GuestID",        order.Guest.GuestID);
                cmd.Parameters.AddWithValue("@OrderTimeStamp", order.OrderTimeStamp);
                cmd.Parameters.AddWithValue("@OrderStatus",    order.Status.ToString());
                return (int)cmd.ExecuteScalar();
            }
        }

        private void InsertOrderItems(List<OrderItem> items, int orderId, SqlConnection connection)
        {
            string sql = @"
                INSERT INTO OrderItems (OrderID, MenuItemID, Quantity, PreparationStatus, Comment)
                VALUES (@OrderID, @MenuItemID, @Quantity, @PrepStatus, @Comment);";

            foreach (var item in items)
            {
                using (SqlCommand cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@OrderID",    orderId);
                    cmd.Parameters.AddWithValue("@MenuItemID", item.MenuItemID);
                    cmd.Parameters.AddWithValue("@Quantity",   item.Quantity);
                    cmd.Parameters.AddWithValue("@PrepStatus", item.PreparationStatus.ToString());
                    cmd.Parameters.AddWithValue("@Comment",    (object?)item.Comment ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }

                _menuRepository.DeductStockQuantity(item.MenuItemID, item.Quantity);
            }
        }

        //NOTE strings 'Pending', 'Served', 'Complete', 'Preparing', 'Ready' are hardcoded, need to be constant instead
        public List<RunningOrderViewModel> GetAllOrdersByStatus()
        {
            var orders = new Dictionary<int, RunningOrderViewModel>();

            using (SqlConnection connection = GetConnection())
            {
                string query = @"
                    SELECT o.OrderID, g.TableNumber, o.OrderTimeStamp,
                           oi.OrderItemID, mi.ItemName, oi.Quantity, oi.PreparationStatus,
                           mi.MenuCard, ISNULL(c.CourseName, 'Other') AS CourseName,
                           oi.Comment
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
                                    OrderID     = orderID,
                                    TableNumber = reader.GetInt32(reader.GetOrdinal("TableNumber")),
                                    OrderTime   = DateTime.SpecifyKind(reader.GetDateTime(reader.GetOrdinal("OrderTimeStamp")), DateTimeKind.Utc),
                                    Items       = new List<RunningOrderItemViewModel>()
                                };
                            }

                            string? comment;
                            if (reader.IsDBNull(reader.GetOrdinal("Comment")))
                            {
                                comment = null;
                            }
                            else
                            {
                                comment = reader.GetString(reader.GetOrdinal("Comment"));
                            }

                            orders[orderID].Items.Add(new RunningOrderItemViewModel
                            {
                                OrderItemID       = reader.GetInt32(reader.GetOrdinal("OrderItemID")),
                                ItemName          = reader.GetString(reader.GetOrdinal("ItemName")),
                                Quantity          = reader.GetInt32(reader.GetOrdinal("Quantity")),
                                PreparationStatus = Enum.Parse<PreparationStatus>(reader.GetString(reader.GetOrdinal("PreparationStatus"))),
                                MenuCard          = reader.GetString(reader.GetOrdinal("MenuCard")),
                                CourseName        = reader.GetString(reader.GetOrdinal("CourseName")),
                                Comment           = comment
                            });
                        }
                    }
                }
            }

            return orders.Values.ToList();
        }

        public List<RunningOrderViewModel> GetFinishedOrdersToday()
        {
            var orders = new Dictionary<int, RunningOrderViewModel>();

            using (SqlConnection connection = GetConnection())
            {
                string query = @"
                    SELECT o.OrderID, g.TableNumber, o.OrderTimeStamp,
                           oi.OrderItemID, mi.ItemName, oi.Quantity, oi.PreparationStatus,
                           mi.MenuCard, ISNULL(c.CourseName, 'Other') AS CourseName,
                           oi.Comment
                    FROM Orders o
                    JOIN Guests g ON o.GuestID = g.GuestID
                    JOIN OrderItems oi ON o.OrderID = oi.OrderID
                    JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID
                    LEFT JOIN Courses c ON mi.CourseID = c.CourseID
                    WHERE o.OrderStatus IN ('Complete', 'Served')
                      AND CAST(o.OrderTimeStamp AS DATE) = CAST(GETDATE() AS DATE)
                    ORDER BY o.OrderTimeStamp DESC, c.CourseID ASC";

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
                                    OrderID     = orderID,
                                    TableNumber = reader.GetInt32(reader.GetOrdinal("TableNumber")),
                                    OrderTime   = DateTime.SpecifyKind(reader.GetDateTime(reader.GetOrdinal("OrderTimeStamp")), DateTimeKind.Utc),
                                    Items       = new List<RunningOrderItemViewModel>()
                                };
                            }

                            string? comment;
                            if (reader.IsDBNull(reader.GetOrdinal("Comment")))
                            {
                                comment = null;
                            }
                            else
                            {
                                comment = reader.GetString(reader.GetOrdinal("Comment"));
                            }

                            orders[orderID].Items.Add(new RunningOrderItemViewModel
                            {
                                OrderItemID       = reader.GetInt32(reader.GetOrdinal("OrderItemID")),
                                ItemName          = reader.GetString(reader.GetOrdinal("ItemName")),
                                Quantity          = reader.GetInt32(reader.GetOrdinal("Quantity")),
                                PreparationStatus = Enum.Parse<PreparationStatus>(reader.GetString(reader.GetOrdinal("PreparationStatus"))),
                                MenuCard          = reader.GetString(reader.GetOrdinal("MenuCard")),
                                CourseName        = reader.GetString(reader.GetOrdinal("CourseName")),
                                Comment           = comment
                            });
                        }
                    }
                }
            }

            return orders.Values.ToList();
        }

        public void ToggleCoursePreparation(int orderId, string courseName)
        {
            using (SqlConnection connection = GetConnection())
            {
                string query = @"
                    UPDATE oi
                    SET oi.PreparationStatus =
                        CASE
                            WHEN (SELECT COUNT(*) FROM OrderItems oi2
                                  JOIN MenuItems mi2 ON oi2.MenuItemID = mi2.MenuItemID
                                  LEFT JOIN Courses c2 ON mi2.CourseID = c2.CourseID
                                  WHERE oi2.OrderID = @OrderID
                                    AND ISNULL(c2.CourseName, 'Other') = @CourseName
                                    AND oi2.PreparationStatus = 'Pending') > 0
                                THEN 'Preparing'
                            WHEN (SELECT COUNT(*) FROM OrderItems oi2
                                  JOIN MenuItems mi2 ON oi2.MenuItemID = mi2.MenuItemID
                                  LEFT JOIN Courses c2 ON mi2.CourseID = c2.CourseID
                                  WHERE oi2.OrderID = @OrderID
                                    AND ISNULL(c2.CourseName, 'Other') = @CourseName
                                    AND oi2.PreparationStatus = 'Preparing') > 0
                                THEN 'Ready'
                            ELSE 'Pending'
                        END
                    FROM OrderItems oi
                    JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID
                    LEFT JOIN Courses c ON mi.CourseID = c.CourseID
                    WHERE oi.OrderID = @OrderID
                      AND ISNULL(c.CourseName, 'Other') = @CourseName";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OrderID",    orderId);
                    command.Parameters.AddWithValue("@CourseName", courseName);
                    command.ExecuteNonQuery();
                }
            }
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

        public bool AllItemsReady(int orderId)
        {
            using (SqlConnection connection = GetConnection())
            {
                string query = @"
                    SELECT COUNT(*) FROM OrderItems
                    WHERE OrderID = @OrderID AND PreparationStatus != 'Ready'";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OrderID", orderId);
                    return (int)command.ExecuteScalar() == 0;
                }
            }
        }
    }
}
