namespace ChapeauProject.ViewModels
{
    public enum SplitMode { Single, Equal, Custom, ByGuest }

    public class SplitPayerViewModel
    {
        public string Name { get; set; } = string.Empty;
        public int? GuestID { get; set; }                    // set for ByGuest mode
        public decimal AmountDue { get; set; }               // pre-calculated share
        public decimal TipAmount { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public string? Feedback { get; set; }
    }

    public class BillViewModel
    {
        public int TableNumber { get; set; }
        public List<GuestOrderViewModel> Guests { get; set; } = new();
        public decimal SubTotalAmount { get; set; }
        public decimal LowVAT { get; set; }
        public decimal HighVAT { get; set; }
        public decimal TotalAmount { get; set; }

        // Split mode selection
        public SplitMode SplitMode { get; set; } = SplitMode.Single;
        public int SplitCount { get; set; } = 1;             // for Equal mode

        // Per-payer rows (populated based on mode)
        public List<SplitPayerViewModel> Payers { get; set; } = new();

        // Single-pay fields (used when SplitMode == Single)
        public decimal TipAmount { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public string? Feedback { get; set; }

        // Running total already paid (for Custom mode's remaining display)
        public decimal AmountAlreadyPaid { get; set; }
        public decimal Remaining => TotalAmount - AmountAlreadyPaid;
    }
}
