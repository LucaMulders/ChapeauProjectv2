namespace ChapeauProject.ViewModels
{
    public class TableOrderViewModel
    {
        public int TableNumber { get; set; }
        public List<GuestOrderViewModel> Guests { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal LowVAT { get; set; }
        public decimal HighVAT { get; set; }
    }
}