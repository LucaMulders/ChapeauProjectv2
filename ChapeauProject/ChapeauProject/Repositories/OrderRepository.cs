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
                int guestId = order.GuestID;

       
                string insertOrderSql = @"
                    INSERT INTO Orders (GuestID, OrderTimeStamp, OrderStatus) 
                    VALUES (@GuestID, @OrderTimeStamp, @OrderStatus);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                int generatedOrderID;
                using (SqlCommand orderCmd = new SqlCommand(insertOrderSql, connection))
                {
                    orderCmd.Parameters.AddWithValue("@GuestID", guestId);
                    orderCmd.Parameters.AddWithValue("@OrderTimeStamp", order.OrderTimeStamp);
                    orderCmd.Parameters.AddWithValue("@OrderStatus", order.Status.ToString());
                    generatedOrderID = (int)orderCmd.ExecuteScalar();
                }

                foreach (var item in order.OrderItems)
                {
                    string insertItemSql = @"
                        INSERT INTO OrderItems (OrderID, MenuItemID, Quantity, PreparationStatus, Comment) 
                        VALUES (@OrderID, @MenuItemID, @Quantity, @PrepStatus, @Comment);";

                    using (SqlCommand itemCmd = new SqlCommand(insertItemSql, connection))
                    {
                        itemCmd.Parameters.AddWithValue("@OrderID", generatedOrderID);
                        itemCmd.Parameters.AddWithValue("@MenuItemID", item.MenuItemID);
                        itemCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                        itemCmd.Parameters.AddWithValue("@PrepStatus", item.PreparationStatus.ToString());
                        itemCmd.Parameters.AddWithValue("@Comment", item.Comment ?? string.Empty);
                        itemCmd.ExecuteNonQuery();
                    }

                    _menuRepository.DeductStockQuantity(item.MenuItemID, item.Quantity);
                }
            }
        }


        public List<RunningOrderViewModel> GetAllOrdersByStatus()
        {
            List<RunningOrderViewModel> orders = new List<RunningOrderViewModel>();

            using (SqlConnection connection = GetConnection())
            {
              
                string query = @"
<<<<<<< HEAD
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
=======
                    SELECT O.OrderID, O.GuestID, O.OrderStatus, O.OrderTimeStamp, G.TableNumber
                    FROM Orders O
                    JOIN Guests G ON O.GuestID = G.GuestID
                    WHERE O.OrderStatus = 'Pending'";

>>>>>>> 30510b82cd4e3de0231429c8b51cee21878212d0

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int orderId = (int)reader["OrderID"];
                            System.DateTime timeStamp = reader["OrderTimeStamp"] != System.DBNull.Value
                                ? (System.DateTime)reader["OrderTimeStamp"]
                                : System.DateTime.Now;

                            orders.Add(new RunningOrderViewModel
                            {
<<<<<<< HEAD
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
=======
                                OrderID = orderId,
                                Status = reader["OrderStatus"].ToString(),
                                
                                TableNumber = reader["TableNumber"] != System.DBNull.Value ? (int)reader["TableNumber"] : 0,
                                OrderTime = timeStamp,
                                // Initialize an empty so cannot be null!
                                Items = new List<RunningOrderItemViewModel>()
>>>>>>> 30510b82cd4e3de0231429c8b51cee21878212d0
                            });
                        }
                    }
                }

                foreach (var order in orders)
                {
                    string itemsQuery = @"
                SELECT OI.OrderItemID, MI.ItemName, OI.Quantity, OI.PreparationStatus 
                FROM OrderItems OI
                JOIN MenuItems MI ON OI.MenuItemID = MI.MenuItemID
                WHERE OI.OrderID = @OrderID;";

                    using (SqlCommand itemCommand = new SqlCommand(itemsQuery, connection))
                    {
                        itemCommand.Parameters.AddWithValue("@OrderID", order.OrderID);

                        using (SqlDataReader itemReader = itemCommand.ExecuteReader())
                        {
                            while (itemReader.Read())
                            {
                                
                                order.Items.Add(new RunningOrderItemViewModel
                                {
                                    OrderItemID = (int)itemReader["OrderItemID"],
                                    ItemName = itemReader["ItemName"].ToString(),
                                    Quantity = (int)itemReader["Quantity"],
                                    
                                    PreparationStatus = itemReader["PreparationStatus"].ToString() == "Done"
                                        ? PreparationStatus.Ready
                                        : System.Enum.Parse<PreparationStatus>(itemReader["PreparationStatus"].ToString(), true)
                                });
                            }
                        }
                    }
                }
            }
            return orders;
        }

        public void ToggleItemPreparation(int orderItemId)
        {
            using (SqlConnection connection = GetConnection())
            {
<<<<<<< HEAD
                string query = @"UPDATE OrderItems SET PreparationStatus =
                    CASE PreparationStatus
                        WHEN 'Pending'   THEN 'Preparing'
                        WHEN 'Preparing' THEN 'Ready'
                        WHEN 'Ready'     THEN 'Pending'
=======
                string query = @"
                    UPDATE OrderItems 
                    SET PreparationStatus = CASE
                        WHEN PreparationStatus = 'Pending' THEN 'Preparing'
                        WHEN PreparationStatus = 'Preparing' THEN 'Ready'
                        ELSE 'Pending'
>>>>>>> 30510b82cd4e3de0231429c8b51cee21878212d0
                    END
                    WHERE OrderItemID = @OrderItemID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OrderItemID", orderItemId);

                    if (connection.State != System.Data.ConnectionState.Open)
                    {
                        connection.Open();
                    }

                    command.ExecuteNonQuery();
                }
            }
        }

        public void CompleteOrder(int orderId)
        {
            using (SqlConnection connection = GetConnection())
            {
                string query = "UPDATE Orders SET OrderStatus = 'Served' WHERE OrderID = @OrderID";
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