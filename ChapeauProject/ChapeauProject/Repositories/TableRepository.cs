using ChapeauProject.Models;
using Microsoft.Data.SqlClient;

namespace ChapeauProject.Repositories
{
    public class TableRepository : RepositoryBase, ITableRepository
    {
        // Rubric Item: Use of Constants for Repeated Strings

        private const string StatusPending = nameof(OrderStatus.Pending);

        public TableRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public List<Table> GetAllTables()
        {
            var tables = new List<Table>();
            using (SqlConnection connection = GetConnection())
            {
                string query = "SELECT TableNumber, Seats, IsOccupied FROM Tables";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
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

        public List<Table> GetAllOccupiedTables()
        {
            var tables = new List<Table>();
            using (SqlConnection connection = GetConnection())
            {
                string query = "SELECT TableNumber, Seats, IsOccupied FROM Tables WHERE IsOccupied = 1";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
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
            using (SqlConnection connection = GetConnection())
            {
                string query = "SELECT TableNumber, Seats, IsOccupied FROM Tables WHERE TableNumber = @TableNumber";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TableNumber", tableNumber);
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
            using (SqlConnection connection = GetConnection())
            {
                string query = "UPDATE Tables SET IsOccupied = ~IsOccupied WHERE TableNumber = @TableNumber";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TableNumber", tableNumber);
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<GuestOrder> GetTableOrders(int tableNumber)
        {
            using (SqlConnection connection = GetConnection())
            {
                var guests = GetGuestsAtTable(tableNumber, connection);

                foreach (var guest in guests)
                {
                    guest.Items = GetItemsForGuest(guest.GuestID, connection);
                }

                return guests;
            }
        }

        private Guest ReadGuest(SqlDataReader reader)
        {
            return new Guest(
                reader.GetInt32(reader.GetOrdinal("GuestID")),
                reader.GetString(reader.GetOrdinal("FirstName")),
                reader.GetString(reader.GetOrdinal("LastName"))
            );
        }

        private List<GuestOrder> GetGuestsAtTable(int tableNumber, SqlConnection connection)
        {
            var guests = new List<GuestOrder>();

            string query = "SELECT GuestID, FirstName, LastName FROM Guests WHERE TableNumber = @TableNumber";
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@TableNumber", tableNumber);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        guests.Add(new GuestOrder
                        {
                            Guest = ReadGuest(reader),
                            Items = new List<GuestOrderItem>()
                        });
                    }
                }
            }

            return guests;
        }

        private List<GuestOrderItem> GetItemsForGuest(int guestId, SqlConnection connection)
        {
            var items = new List<GuestOrderItem>();

            string query = $@"
                SELECT oi.OrderItemID, mi.MenuItemID, mi.ItemName, mi.Price, mi.VatRate, oi.Quantity, oi.PreparationStatus
                FROM Orders o
                JOIN OrderItems oi ON o.OrderID = oi.OrderID
                JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID
                WHERE o.GuestID = @GuestID
                  AND o.OrderStatus = '{StatusPending}'";

            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@GuestID", guestId);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(new GuestOrderItem
                        {
                            OrderItemID       = reader.GetInt32(reader.GetOrdinal("OrderItemID")),
                            Quantity          = reader.GetInt32(reader.GetOrdinal("Quantity")),
                            PreparationStatus = Enum.Parse<PreparationStatus>(reader.GetString(reader.GetOrdinal("PreparationStatus"))),
                            MenuItem          = new MenuItem(
                                reader.GetInt32(reader.GetOrdinal("MenuItemID")),
                                reader.GetString(reader.GetOrdinal("ItemName")),
                                reader.GetDecimal(reader.GetOrdinal("Price")),
                                reader.GetDecimal(reader.GetOrdinal("VatRate")),
                                0,
                                null
                            )
                        });
                    }
                }
            }

            return items;
        }

        public int GetOrderCount(int tableNumber)
        {
            return GetTableOrders(tableNumber).Sum(g => g.Items.Count);
        }

        public int GetGuestCount(int tableNumber)
        {
            using (SqlConnection connection = GetConnection())
            {
                string query = "SELECT COUNT(*) FROM Guests WHERE TableNumber = @TableNumber";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TableNumber", tableNumber);
                    return (int)command.ExecuteScalar();
                }
            }
        }

        public List<Guest> GetGuestsByTable(int tableNumber)
        {
            var guests = new List<Guest>();
            using (SqlConnection connection = GetConnection())
            {
                string query = "SELECT GuestID, FirstName, LastName FROM Guests WHERE TableNumber = @TableNumber";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TableNumber", tableNumber);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                            guests.Add(ReadGuest(reader));
                    }
                }
            }
            return guests;
        }

        public void SetFree(int tableNumber)
        {
            using (SqlConnection connection = GetConnection())
            {
                string query = "UPDATE Tables SET IsOccupied = 0 WHERE TableNumber = @TableNumber";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TableNumber", tableNumber);
                    command.ExecuteNonQuery();
                }
            }
        }

        // Added this so that when the bill has been processed the table actually becomes free. 
        public void RemoveGuests(int tableNumber)
        {
            using (SqlConnection connection = GetConnection())
            {
                string query = "UPDATE Guests SET TableNumber = NULL WHERE TableNumber = @TableNumber";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TableNumber", tableNumber);
                    command.ExecuteNonQuery();
                }
            }
        }

        public (bool HasFood, bool HasDrink) GetRunningOrderCategories(int tableNumber)
        {
            var cards = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using (SqlConnection connection = GetConnection())
            {
                string query = $@"
                    SELECT DISTINCT mi.MenuCard
                    FROM Orders o
                    JOIN Guests g ON o.GuestID = g.GuestID
                    JOIN OrderItems oi ON o.OrderID = oi.OrderID
                    JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID
                    WHERE g.TableNumber = @TableNumber
                      AND o.OrderStatus = '{StatusPending}'";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TableNumber", tableNumber);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cards.Add(reader.GetString(0));
                        }
                    }
                }
            }

            // MenuCard values are "Lunch", "Dinner" (food), and "Drinks"
            bool hasFood  = cards.Contains("Lunch") || cards.Contains("Dinner");
            bool hasDrink = cards.Contains("Drinks");
            return (hasFood, hasDrink);
        }
    }
}