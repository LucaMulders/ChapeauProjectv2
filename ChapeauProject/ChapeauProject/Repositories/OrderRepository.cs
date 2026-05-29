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
              
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }

          
                string getGuestSql = "SELECT TOP 1 GuestID FROM Guests WHERE TableNumber = @TableNumber ORDER BY GuestID DESC;";
                int guestId = 0;

                using (SqlCommand guestCmd = new SqlCommand(getGuestSql, connection))
                {
                    guestCmd.Parameters.AddWithValue("@TableNumber", order.TableNumber);
                    object result = guestCmd.ExecuteScalar();

                    if (result != null)
                    {
                        guestId = (int)result;
                    }
                    else
                    {
                        
                        string createGuestSql = "INSERT INTO Guests (TableNumber, FirstName, LastName) VALUES (@TableNumber, 'Table', @TableNumber); SELECT CAST(SCOPE_IDENTITY() as int);";
                        using (SqlCommand createGuestCmd = new SqlCommand(createGuestSql, connection))
                        {
                            createGuestCmd.Parameters.AddWithValue("@TableNumber", order.TableNumber);
                            guestId = (int)createGuestCmd.ExecuteScalar();
                        }
                    }
                }

       
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
               
                string query = "SELECT OrderID, GuestID, OrderStatus FROM Orders WHERE OrderStatus = 'Pending';";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    if (connection.State != System.Data.ConnectionState.Open)
                    {
                        connection.Open();
                    }

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orders.Add(new RunningOrderViewModel
                            {
                                OrderID = (int)reader["OrderID"],
                                Status = reader["OrderStatus"].ToString(),
                                
                                TableNumber = reader["GuestID"] != System.DBNull.Value ? (int)reader["GuestID"] : 0
                            });
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
                string query = @"
                    UPDATE OrderItems 
                    SET PreparationStatus = CASE 
                        WHEN PreparationStatus = 'Pending' THEN 'Ready' 
                        ELSE 'Pending' 
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
               
                string query = "UPDATE Orders SET OrderStatus = 'Complete' WHERE OrderID = @OrderID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OrderID", orderId);

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