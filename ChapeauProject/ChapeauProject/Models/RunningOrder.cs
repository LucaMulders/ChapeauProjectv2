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
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public PreparationStatus PreparationStatus { get; set; }
        public string MenuCard { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string? Comment { get; set; }
    }
}
