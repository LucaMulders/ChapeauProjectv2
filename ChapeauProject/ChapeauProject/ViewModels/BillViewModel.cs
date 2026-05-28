namespace ChapeauProject.ViewModels
{
    public class BillViewModel
    {
        public int TableNumber { get; set; }
        public decimal SubTotalAmount { get; set; }
        public decimal LowVAT { get; set; }
        public decimal HighVAT { get; set; }
        public decimal TotalAmount { get; set; }   // SubTotal + LowVAT + HighVAT
        public decimal TipAmount { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public string Feedback { get; set; } = string.Empty;
    }
}
