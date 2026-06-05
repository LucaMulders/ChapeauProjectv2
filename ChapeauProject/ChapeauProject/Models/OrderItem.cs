namespace ChapeauProject.Models
{
    //NOTE OrderItem still has raw int MenuItemID and int OrderID, needs to be objects
    public class OrderItem
    {
        public int OrderItemID { get; set; }
        public int OrderID { get; set; }
        public int MenuItemID { get; set; }
        public int Quantity { get; set; }
        public PreparationStatus PreparationStatus { get; set; }
        public int? CourseID { get; set; }
        public string Comment { get; set; } = string.Empty;
        public MenuItem? MenuItem { get; set; }
    }
}