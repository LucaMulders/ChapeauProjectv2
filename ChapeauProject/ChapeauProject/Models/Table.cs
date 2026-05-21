namespace ChapeauProject.Models
{
    public class Table
    {
        public int TableNumber { get; set; }
        public int Seats { get; set; }
        public bool IsOccupied { get; set; }

        public Table(int tableNumber, int seats, bool isOccupied)
        {
            TableNumber = tableNumber;
            Seats = seats;
            IsOccupied = isOccupied;
        }
    }
}