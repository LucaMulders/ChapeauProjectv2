namespace ChapeauProject.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public int GuestID { get; set; }
        public int StaffID { get; set; }
        public DateTime OrderTimeStamp { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new();
    }
}