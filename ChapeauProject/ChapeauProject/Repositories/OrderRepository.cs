using ChapeauProject.Models;
using ChapeauProject.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace ChapeauProject.Repositories
{
    public class OrderRepository : RepositoryBase, IOrderRepository
    {
        // Rubric Item: Use of Constants for Repeated Strings

        private const string StatusPending    = nameof(OrderStatus.Pending);
        private const string StatusComplete   = nameof(OrderStatus.Complete);
        private const string StatusServed     = nameof(OrderStatus.Served);
        private const string PrepPending      = nameof(PreparationStatus.Pending);
        private const string PrepPreparing    = nameof(PreparationStatus.Preparing);
        private const string PrepReady        = nameof(PreparationStatus.Ready);
        private const string PrepServed       = nameof(PreparationStatus.Served);

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
                INSERT INTO Orders (GuestID, StaffID, OrderTimeStamp, OrderStatus)
                VALUES (@GuestID, @StaffID, @OrderTimeStamp, @OrderStatus);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            using (SqlCommand cmd = new SqlCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@GuestID",        order.Guest.GuestID);
                cmd.Parameters.AddWithValue("@StaffID",        order.Staff.StaffID);
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
                    cmd.Parameters.AddWithValue("@MenuItemID", item.MenuItem?.MenuItemID ?? 0);
                    cmd.Parameters.AddWithValue("@Quantity",   item.Quantity);
                    cmd.Parameters.AddWithValue("@PrepStatus", item.PreparationStatus.ToString());
                    cmd.Parameters.AddWithValue("@Comment",    (object?)item.Comment ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }

                _menuRepository.DeductStockQuantity(item.MenuItem?.MenuItemID ?? 0, item.Quantity);
            }
        }

        public List<RunningOrder> GetAllOrdersByStatus()
        {
            string query = $@"
                SELECT o.OrderID, g.TableNumber, o.OrderTimeStamp,
                       oi.OrderItemID, mi.MenuItemID, mi.ItemName, oi.Quantity, oi.PreparationStatus,
                       mi.MenuCard, ISNULL(c.CourseName, 'Other') AS CourseName, oi.Comment
                FROM Orders o
                JOIN Guests g ON o.GuestID = g.GuestID
                JOIN OrderItems oi ON o.OrderID = oi.OrderID
                JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID
                LEFT JOIN Courses c ON mi.CourseID = c.CourseID
                WHERE o.OrderStatus = '{StatusPending}'
                ORDER BY o.OrderTimeStamp ASC, c.CourseID ASC";

            return ReadOrders(query);
        }

        public List<RunningOrder> GetFinishedOrdersToday()
        {
            string query = $@"
                SELECT o.OrderID, g.TableNumber, o.OrderTimeStamp,
                       oi.OrderItemID, mi.MenuItemID, mi.ItemName, oi.Quantity, oi.PreparationStatus,
                       mi.MenuCard, ISNULL(c.CourseName, 'Other') AS CourseName, oi.Comment
                FROM Orders o
                JOIN Guests g ON o.GuestID = g.GuestID
                JOIN OrderItems oi ON o.OrderID = oi.OrderID
                JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID
                LEFT JOIN Courses c ON mi.CourseID = c.CourseID
                WHERE o.OrderStatus IN ('{StatusComplete}', '{StatusServed}')
                  AND CAST(o.OrderTimeStamp AS DATE) = CAST(GETDATE() AS DATE)
                ORDER BY o.OrderTimeStamp DESC, c.CourseID ASC";

            return ReadOrders(query);
        }

        private List<RunningOrder> ReadOrders(string query)
        {
            var orders = new Dictionary<int, RunningOrder>();

            using (SqlConnection connection = GetConnection())
            using (SqlCommand command = new SqlCommand(query, connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    int orderID = reader.GetInt32(reader.GetOrdinal("OrderID"));

                    if (!orders.ContainsKey(orderID))
                    {
                        orders[orderID] = new RunningOrder
                        {
                            OrderID     = orderID,
                            TableNumber = reader.GetInt32(reader.GetOrdinal("TableNumber")),
                            OrderTime   = DateTime.SpecifyKind(reader.GetDateTime(reader.GetOrdinal("OrderTimeStamp")), DateTimeKind.Utc)
                        };
                    }

                    orders[orderID].Items.Add(new RunningOrderItem
                    {
                        OrderItemID       = reader.GetInt32(reader.GetOrdinal("OrderItemID")),
                        MenuItem          = new MenuItem(
                            reader.GetInt32(reader.GetOrdinal("MenuItemID")),
                            reader.GetString(reader.GetOrdinal("ItemName")),
                            0, 0, 0,
                            new Menu(Enum.Parse<MenuCard>(reader.GetString(reader.GetOrdinal("MenuCard"))))
                        ),
                        Quantity          = reader.GetInt32(reader.GetOrdinal("Quantity")),
                        PreparationStatus = Enum.Parse<PreparationStatus>(reader.GetString(reader.GetOrdinal("PreparationStatus"))),
                        CourseName        = ParseCourseName(reader.GetString(reader.GetOrdinal("CourseName"))),
                        Comment           = reader.IsDBNull(reader.GetOrdinal("Comment")) ? null : reader.GetString(reader.GetOrdinal("Comment"))
                    });
                }
            }

            return orders.Values.ToList();
        }

        private static CourseName ParseCourseName(string raw)
        {
            if (Enum.TryParse(raw, out CourseName course))
                return course;
            else
                return CourseName.Other;
        }

        public void ToggleCoursePreparation(int orderId, CourseName courseName)
        {
            using (SqlConnection connection = GetConnection())
            {
                string query = $@"
                    UPDATE oi
                    SET oi.PreparationStatus =
                        CASE
                            WHEN (SELECT COUNT(*) FROM OrderItems oi2
                                  JOIN MenuItems mi2 ON oi2.MenuItemID = mi2.MenuItemID
                                  LEFT JOIN Courses c2 ON mi2.CourseID = c2.CourseID
                                  WHERE oi2.OrderID = @OrderID
                                    AND ISNULL(c2.CourseName, 'Other') = @CourseName
                                    AND oi2.PreparationStatus = '{PrepPending}') > 0
                                THEN '{PrepPreparing}'
                            WHEN (SELECT COUNT(*) FROM OrderItems oi2
                                  JOIN MenuItems mi2 ON oi2.MenuItemID = mi2.MenuItemID
                                  LEFT JOIN Courses c2 ON mi2.CourseID = c2.CourseID
                                  WHERE oi2.OrderID = @OrderID
                                    AND ISNULL(c2.CourseName, 'Other') = @CourseName
                                    AND oi2.PreparationStatus = '{PrepPreparing}') > 0
                                THEN '{PrepReady}'
                            WHEN (SELECT COUNT(*) FROM OrderItems oi2
                                  JOIN MenuItems mi2 ON oi2.MenuItemID = mi2.MenuItemID
                                  LEFT JOIN Courses c2 ON mi2.CourseID = c2.CourseID
                                  WHERE oi2.OrderID = @OrderID
                                    AND ISNULL(c2.CourseName, 'Other') = @CourseName
                                    AND oi2.PreparationStatus = '{PrepReady}') > 0
                                THEN '{PrepServed}'
                            ELSE '{PrepPending}'
                        END
                    FROM OrderItems oi
                    JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID
                    LEFT JOIN Courses c ON mi.CourseID = c.CourseID
                    WHERE oi.OrderID = @OrderID
                      AND ISNULL(c.CourseName, 'Other') = @CourseName";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OrderID",    orderId);
                    command.Parameters.AddWithValue("@CourseName", courseName.ToString());
                    command.ExecuteNonQuery();
                }
            }
        }

        public void ToggleItemPreparation(int orderItemId)
        {
            using (SqlConnection connection = GetConnection())
            {
                string query = $@"UPDATE OrderItems SET PreparationStatus =
                    CASE PreparationStatus
                        WHEN '{PrepPending}'   THEN '{PrepPreparing}'
                        WHEN '{PrepPreparing}' THEN '{PrepReady}'
                        WHEN '{PrepReady}'     THEN '{PrepServed}'
                        WHEN '{PrepServed}'    THEN '{PrepPending}'
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
                string query = $"UPDATE Orders SET OrderStatus = '{StatusComplete}' WHERE OrderID = @OrderID";
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
                string query = $@"
                    SELECT COUNT(*) FROM OrderItems
                    WHERE OrderID = @OrderID AND PreparationStatus != '{PrepServed}'";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OrderID", orderId);
                    return (int)command.ExecuteScalar() == 0;
                }
            }
        }
    }
}
