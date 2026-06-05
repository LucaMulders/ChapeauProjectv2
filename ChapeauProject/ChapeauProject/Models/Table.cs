namespace ChapeauProject.Models
{
    public class Table
    {
        // TableNumber is already an IDENTITY column in the DB, so it auto-generates and gaps from deletions are handled automatically. No separate TableID needed (in my opinion).
        public int TableNumber { get; set; }
        public int Seats { get; set; }
        public bool IsOccupied { get; set; }

        public Table() { }

        public Table(int tableNumber, int seats, bool isOccupied)
        {
            TableNumber = tableNumber;
            Seats = seats;
            IsOccupied = isOccupied;
        }
    }
}