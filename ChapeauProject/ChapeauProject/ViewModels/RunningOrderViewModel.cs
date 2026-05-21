namespace ChapeauProject.ViewModels
{
    public class RunningOrderViewModel
    {
        public int OrderID { get; set; }
        public int TableNumber { get; set; }
        public DateTime OrderTime { get; set; }
        public TimeSpan WaitingTime => DateTime.Now - OrderTime;
        public List<RunningOrderItemViewModel> Items { get; set; }
    }

    public class RunningOrderItemViewModel
    {
        public int OrderItemID { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public bool IsPrepared { get; set; }
    }
}