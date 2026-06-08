using ChapeauProject.Models;
using Microsoft.Data.SqlClient;

namespace ChapeauProject.Repositories
{
    public class TableRepository : RepositoryBase, ITableRepository
    {
        // Use of Constants for Repeated Strings
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
            string query = $@"
                SELECT g.GuestID, g.FirstName, g.LastName,
                       oi.OrderItemID, mi.MenuItemID, mi.ItemName, mi.Price, mi.VatRate, oi.Quantity, oi.PreparationStatus
                FROM Guests g
                JOIN Orders o ON g.GuestID = o.GuestID
                JOIN OrderItems oi ON o.OrderID = oi.OrderID
                JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID
                WHERE g.TableNumber = @TableNumber
                  AND o.OrderStatus = '{StatusPending}'";

            var guestOrders = new Dictionary<int, GuestOrder>();

            using (SqlConnection connection = GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@TableNumber", tableNumber);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int guestId = reader.GetInt32(reader.GetOrdinal("GuestID"));

                        if (!guestOrders.ContainsKey(guestId))
                        {
                            guestOrders[guestId] = new GuestOrder
                            {
                                Guest = new Guest(
                                    guestId,
                                    reader.GetString(reader.GetOrdinal("FirstName")),
                                    reader.GetString(reader.GetOrdinal("LastName"))
                                ),
                                Items = new List<GuestOrderItem>()
                            };
                        }

                        guestOrders[guestId].Items.Add(new GuestOrderItem
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

            return guestOrders.Values.ToList();
        }

        public int GetOrderCount(int tableNumber)
        {
            using (SqlConnection connection = GetConnection())
            {
                string query = $@"
                    SELECT COUNT(*)
                    FROM Orders o
                    JOIN Guests g  ON o.GuestID  = g.GuestID
                    JOIN OrderItems oi ON o.OrderID = oi.OrderID
                    WHERE g.TableNumber  = @TableNumber
                      AND o.OrderStatus = '{StatusPending}'";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TableNumber", tableNumber);
                    return (int)command.ExecuteScalar();
                }
            }
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

        public void MarkTableAsFree(int tableNumber)
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