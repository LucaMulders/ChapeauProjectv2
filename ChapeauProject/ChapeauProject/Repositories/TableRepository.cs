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

        public List<Table> GetAllTables()         => GetTablesWhere();
        public List<Table> GetAllOccupiedTables() => GetTablesWhere("IsOccupied = 1");

        private List<Table> GetTablesWhere(string? condition = null)
        {
            var tables = new List<Table>();
            using (SqlConnection connection = GetConnection())
            {
                string query = "SELECT TableNumber, Seats, IsOccupied FROM Tables";
                if (condition != null)
                    query += " WHERE " + condition;
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

        private Guest ReadGuest(SqlDataReader reader)
        {
            return new Guest
            {
                GuestID   = reader.GetInt32(reader.GetOrdinal("GuestID")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName  = reader.GetString(reader.GetOrdinal("LastName"))
            };
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
                       oi.OrderItemID, mi.MenuItemID, mi.ItemName, mi.Price, mi.VatRate, mi.MenuCard,
                       ISNULL(s.Quantity, 0) AS Stock, oi.Quantity, oi.PreparationStatus
                FROM Guests g
                JOIN Orders o ON g.GuestID = o.GuestID
                JOIN OrderItems oi ON o.OrderID = oi.OrderID
                JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID
                LEFT JOIN Stock s ON mi.MenuItemID = s.MenuItemID
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
                                reader.GetInt32(reader.GetOrdinal("Stock")),
                                new Menu(Enum.Parse<MenuCard>(reader.GetString(reader.GetOrdinal("MenuCard"))))
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

        // Inserts a new guest at the given table and returns the generated GuestID.
        // Used to auto-create an unnamed guest when an order is placed at a table with no registered guests.
        public int InsertGuest(int tableNumber, string firstName, string lastName)
        {
            using (SqlConnection connection = GetConnection())
            {
                string query = @"
                    INSERT INTO Guests (TableNumber, FirstName, LastName)
                    VALUES (@TableNumber, @FirstName, @LastName);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@TableNumber", tableNumber);
                    cmd.Parameters.AddWithValue("@FirstName",   firstName);
                    cmd.Parameters.AddWithValue("@LastName",    lastName);
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        public OrderCategories GetRunningOrderCategories(int tableNumber)
        {
            var cards    = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var statuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using (SqlConnection connection = GetConnection())
            {
                // Exclude Served items
                string query = $@"
                    SELECT DISTINCT mi.MenuCard, oi.PreparationStatus
                    FROM Orders o
                    JOIN Guests g ON o.GuestID = g.GuestID
                    JOIN OrderItems oi ON o.OrderID = oi.OrderID
                    JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID
                    WHERE g.TableNumber = @TableNumber
                      AND o.OrderStatus = '{StatusPending}'
                      AND oi.PreparationStatus != '{nameof(PreparationStatus.Served)}'";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TableNumber", tableNumber);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cards.Add(reader.GetString(0));
                            statuses.Add(reader.GetString(1));
                        }
                    }
                }
            }

            // Status: Pending (ordered) > Preparing > Ready
            string? overallStatus = null;
            if (statuses.Count > 0)
            {
                if (statuses.Contains(nameof(PreparationStatus.Pending)))
                    overallStatus = nameof(PreparationStatus.Pending);
                else if (statuses.Contains(nameof(PreparationStatus.Preparing)))
                    overallStatus = nameof(PreparationStatus.Preparing);
                else
                    overallStatus = nameof(PreparationStatus.Ready);
            }

            return new OrderCategories
            {
                HasFood       = cards.Contains(nameof(MenuCard.Lunch)) || cards.Contains(nameof(MenuCard.Dinner)),
                HasDrink      = cards.Contains(nameof(MenuCard.Drinks)),
                OverallStatus = overallStatus
            };
        }
    }
}