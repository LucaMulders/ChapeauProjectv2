using ChapeauProject.Models;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;

namespace ChapeauProject.Repositories
{
    public class OrderRepository : RepositoryBase, IOrderRepository
    {
        // Use of Constants for Repeated Strings
        private const string StatusPending    = nameof(OrderStatus.Pending);
        private const string StatusComplete   = nameof(OrderStatus.Complete);
        private const string StatusServed     = nameof(OrderStatus.Served);
        private const string PrepPending      = nameof(PreparationStatus.Pending);
        private const string PrepPreparing    = nameof(PreparationStatus.Preparing);
        private const string PrepReady        = nameof(PreparationStatus.Ready);
        private const string PrepServed       = nameof(PreparationStatus.Served);
        private const string CourseOther      = nameof(CourseName.Other);

        public OrderRepository(string connectionString) : base(connectionString)
        {
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

        private void InsertOrderItems(IReadOnlyList<OrderItem> items, int orderId, SqlConnection connection)
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

            }
        }

        // Added BuildOrderQuery for repeated code
        public List<Order> GetAllOrdersByStatus()
        {
            return ReadOrders(BuildOrderQuery(
                $"WHERE o.OrderStatus = '{StatusPending}' ORDER BY o.OrderTimeStamp ASC, c.CourseID ASC"));
        }

        public List<Order> GetFinishedOrdersToday()
        {
            return ReadOrders(BuildOrderQuery(
                $"WHERE o.OrderStatus IN ('{StatusComplete}', '{StatusServed}') AND CAST(o.OrderTimeStamp AS DATE) = CAST(GETDATE() AS DATE) ORDER BY o.OrderTimeStamp DESC, c.CourseID ASC"));
        }

        private string BuildOrderQuery(string whereClause) => $@"
                SELECT o.OrderID, g.TableNumber, o.OrderTimeStamp,
                       g.GuestID, g.FirstName, g.LastName,
                       oi.OrderItemID, mi.MenuItemID, mi.ItemName, mi.Price, mi.VatRate, oi.Quantity, oi.PreparationStatus,
                       mi.MenuCard, ISNULL(c.CourseName, '{CourseOther}') AS CourseName, oi.Comment,
                       ISNULL(s.Quantity, 0) AS Stock
                FROM Orders o
                JOIN Guests g ON o.GuestID = g.GuestID
                JOIN OrderItems oi ON o.OrderID = oi.OrderID
                JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID
                LEFT JOIN Courses c ON mi.CourseID = c.CourseID
                LEFT JOIN Stock s ON mi.MenuItemID = s.MenuItemID
                {whereClause}";

        private List<Order> ReadOrders(string query)
        {
            var orders = new Dictionary<int, Order>();

            using (SqlConnection connection = GetConnection())
            using (SqlCommand command = new SqlCommand(query, connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    int orderID = reader.GetInt32(reader.GetOrdinal("OrderID"));

                    if (!orders.ContainsKey(orderID))
                    {
                        orders[orderID] = new Order
                        {
                            OrderID        = orderID,
                            Table          = new Table { TableNumber = reader.GetInt32(reader.GetOrdinal("TableNumber")) },
                            OrderTimeStamp = reader.GetDateTime(reader.GetOrdinal("OrderTimeStamp")),
                            Guest          = new Guest(
                                reader.GetInt32(reader.GetOrdinal("GuestID")),
                                reader.IsDBNull(reader.GetOrdinal("FirstName")) ? string.Empty : reader.GetString(reader.GetOrdinal("FirstName")),
                                reader.IsDBNull(reader.GetOrdinal("LastName"))  ? string.Empty : reader.GetString(reader.GetOrdinal("LastName"))
                            )
                        };
                    }

                    orders[orderID].OrderItems.Add(new OrderItem
                    {
                        OrderItemID       = reader.GetInt32(reader.GetOrdinal("OrderItemID")),
                        MenuItem          = new MenuItem(
                            reader.GetInt32(reader.GetOrdinal("MenuItemID")),
                            reader.GetString(reader.GetOrdinal("ItemName")),
                            reader.GetDecimal(reader.GetOrdinal("Price")),
                            reader.GetDecimal(reader.GetOrdinal("VatRate")),
                            reader.GetInt32(reader.GetOrdinal("Stock")),
                            Enum.Parse<MenuCard>(reader.GetString(reader.GetOrdinal("MenuCard")))
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


        // Found repeated code so looked up how to shorten it. (might be too advanced for our SQL level) 
        public void ToggleCoursePreparation(int orderId, CourseName courseName)
        {
            using (SqlConnection connection = GetConnection())
            {
                string query = $@"
                    WITH NextStatus AS (
                        SELECT CASE
                                   WHEN COUNT(CASE WHEN oi2.PreparationStatus = '{PrepPending}'   THEN 1 END) > 0 THEN '{PrepPreparing}'
                                   WHEN COUNT(CASE WHEN oi2.PreparationStatus = '{PrepPreparing}' THEN 1 END) > 0 THEN '{PrepReady}'
                                   WHEN COUNT(CASE WHEN oi2.PreparationStatus = '{PrepReady}'     THEN 1 END) > 0 THEN '{PrepServed}'
                                   ELSE '{PrepPending}'
                               END AS NewStatus
                        FROM OrderItems oi2
                        JOIN MenuItems mi2 ON oi2.MenuItemID = mi2.MenuItemID
                        LEFT JOIN Courses c2 ON mi2.CourseID = c2.CourseID
                        WHERE oi2.OrderID = @OrderID
                          AND ISNULL(c2.CourseName, '{CourseOther}') = @CourseName
                    )
                    UPDATE oi
                    SET oi.PreparationStatus = ns.NewStatus
                    FROM OrderItems oi
                    JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID
                    LEFT JOIN Courses c ON mi.CourseID = c.CourseID
                    CROSS JOIN NextStatus ns
                    WHERE oi.OrderID = @OrderID
                      AND ISNULL(c.CourseName, '{CourseOther}') = @CourseName";

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
                        ELSE PreparationStatus
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
                string query = $"UPDATE Orders SET OrderStatus = '{StatusServed}' WHERE OrderID = @OrderID";
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
