using ChapeauProject.Models;
using ChapeauProject.ViewModels;
using Microsoft.Data.SqlClient;

namespace ChapeauProject.Repositories
{
    public class TableRepository : ITableRepository
    {
        private readonly string? _connectionString;

        public TableRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ChapeauProject");
        }

        public List<Table> GetAll() //NOTE getalltables
        {
            var tables = new List<Table>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT TableNumber, Seats, IsOccupied FROM Tables";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tables.Add(ReadTable(reader));
                        }
                    }
                }
            }
            return tables;
        }

        public Table? GetByTableNumber(int tableNumber)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT TableNumber, Seats, IsOccupied FROM Tables WHERE TableNumber = @TableNumber";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TableNumber", tableNumber);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                            return ReadTable(reader);
                    }
                }
            }
            return null;
        }

        private Table ReadTable(SqlDataReader reader)
        {
            int tableNumber = reader.GetInt32(reader.GetOrdinal("TableNumber"));
            int seats = reader.GetInt32(reader.GetOrdinal("Seats"));
            bool isOccupied = reader.GetBoolean(reader.GetOrdinal("IsOccupied"));
            return new Table(tableNumber, seats, isOccupied);
        }
        public void ToggleOccupied(int tableNumber)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "UPDATE Tables SET IsOccupied = ~IsOccupied WHERE TableNumber = @TableNumber";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TableNumber", tableNumber);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        //NOTE table orders or smth
        public TableOrderViewModel GetTableOrders(int tableNumber)
        {
            var guests = new List<GuestOrderViewModel>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // get all guests at this table
                string guestQuery = "SELECT GuestID, FirstName, LastName FROM Guests WHERE TableNumber = @TableNumber";
                using (SqlCommand cmd = new SqlCommand(guestQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@TableNumber", tableNumber);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            guests.Add(new GuestOrderViewModel
                            {
                                GuestName = reader.GetString(reader.GetOrdinal("FirstName")) + " " + reader.GetString(reader.GetOrdinal("LastName")),
                                GuestID = reader.GetInt32(reader.GetOrdinal("GuestID")),
                                Items = new List<OrderItemViewModel>()
                            });
                        }
                    }
                }

                // for each guest get their order items
                foreach (var guest in guests)
                {
                    string itemQuery = @"
                SELECT mi.ItemName, mi.Price, mi.VatRate, SUM(oi.Quantity) as Quantity
                FROM Orders o
                JOIN OrderItems oi ON o.OrderID = oi.OrderID
                JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID
                WHERE o.GuestID = @GuestID
                GROUP BY mi.ItemName, mi.Price, mi.VatRate";

                    using (SqlCommand cmd = new SqlCommand(itemQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@GuestID", guest.GuestID);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                guest.Items.Add(new OrderItemViewModel
                                {
                                    ItemName = reader.GetString(reader.GetOrdinal("ItemName")),
                                    Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                    VatRate = reader.GetDecimal(reader.GetOrdinal("VatRate"))
                                });
                            }
                        }
                    }
                }
            }

            var allItems = guests.SelectMany(g => g.Items).ToList();
            decimal subtotal = allItems.Sum(i => i.Price * i.Quantity);
            decimal lowVat = allItems.Where(i => i.VatRate == 0.09m).Sum(i => i.Price * i.Quantity * i.VatRate);
            decimal highVat = allItems.Where(i => i.VatRate == 0.21m).Sum(i => i.Price * i.Quantity * i.VatRate);

            return new TableOrderViewModel //NOTE make model NO VIEWMODELS IN REPO PLEASE!!!!!!!!!!!!!!!!!!!!!!!!
            {
                TableNumber = tableNumber,
                Guests = guests,
                TotalAmount = subtotal + lowVat + highVat,
                LowVAT = lowVat,
                HighVAT = highVat
            };
        }

        public int GetOrderCount(int tableNumber) //NOTE use orderitems.count
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = @"SELECT COUNT(*) FROM OrderItems oi
                        JOIN Orders o ON oi.OrderID = o.OrderID
                        JOIN Guests g ON o.GuestID = g.GuestID
                        WHERE g.TableNumber = @TableNumber";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TableNumber", tableNumber);
                    connection.Open();
                    return (int)command.ExecuteScalar();
                }
            }
        }
    }
}