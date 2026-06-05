namespace ChapeauProject.Models
{
    public class Payment
    {
        public int PaymentID { get; set; }
        public int BillID { get; set; }
        public decimal PaymentAmount { get; set; }  // TotalAmount + Tip
        //NOTE should be enum
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal TipAmount { get; set; }
        public DateTime PaymentTimeStamp { get; set; }
        public string? Feedback { get; set; }
    }
}
