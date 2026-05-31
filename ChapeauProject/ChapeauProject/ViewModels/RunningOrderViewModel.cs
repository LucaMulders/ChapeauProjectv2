using ChapeauProject.Models;

namespace ChapeauProject.ViewModels
{
    public class RunningOrderViewModel
    {
        public int OrderID { get; set; }
        public int TableNumber { get; set; }
        public DateTime OrderTime { get; set; }
        public TimeSpan WaitingTime => DateTime.Now - OrderTime;
        public List<RunningOrderItemViewModel> Items { get; set; }
        public string? Status { get; internal set; }
    }

    public class RunningOrderItemViewModel
    {
        public int OrderItemID { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public PreparationStatus PreparationStatus { get; set; }
        public string MenuCard { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
    }
}