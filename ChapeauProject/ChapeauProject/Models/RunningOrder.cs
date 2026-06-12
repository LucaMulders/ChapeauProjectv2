//NOTE remove running order
namespace ChapeauProject.Models
{
    public class RunningOrder
    {
        public int OrderID { get; set; }
        public int TableNumber { get; set; }
        public DateTime OrderTime { get; set; }
        public List<RunningOrderItem> Items { get; set; } = new();
    }

    public class RunningOrderItem
    {
        public int OrderItemID { get; set; }
        public MenuItem MenuItem { get; set; } = new MenuItem();
        public int Quantity { get; set; }
        public PreparationStatus PreparationStatus { get; set; }
        public CourseName CourseName { get; set; } = CourseName.Other;
        public string? Comment { get; set; }
    }
}
