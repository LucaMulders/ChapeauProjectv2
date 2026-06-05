namespace ChapeauProject.Models
{
    public class Payment
    {
        public int PaymentID { get; set; }
        public Bill Bill { get; set; } = new Bill();
        public decimal PaymentAmount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public decimal TipAmount { get; set; }
        public DateTime PaymentTimeStamp { get; set; }
        public string? Feedback { get; set; }
    }
}
