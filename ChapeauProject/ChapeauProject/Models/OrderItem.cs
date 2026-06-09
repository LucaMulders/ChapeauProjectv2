using ChapeauProject.ViewModels;

namespace ChapeauProject.Models
{
    public class OrderItem
    {
        public int OrderItemID { get; set; }
        public int Quantity { get; set; }
        public PreparationStatus PreparationStatus { get; set; }
        public CourseName? CourseName { get; set; }
        public string Comment { get; set; } = string.Empty;
        public Order? Order { get; set; }
        public MenuItem? MenuItem { get; set; }
    }
}