namespace ChapeauProject.Models
{
    public class OrderItem
    {
        public int OrderItemID { get; set; }
        public int OrderID { get; set; }
        public int MenuItemID { get; set; }
        public int Quantity { get; set; }
        public PreparationStatus PreparationStatus { get; set; }
        public int? CourseID { get; set; }
    }
}