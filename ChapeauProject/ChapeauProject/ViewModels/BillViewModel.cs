using ChapeauProject.Models;

namespace ChapeauProject.ViewModels
{
    public enum SplitMode { Single, Equal, Custom, ByGuest }

    public class SplitPayerViewModel
    {
        public string Name { get; set; } = string.Empty;
        public int? GuestID { get; set; }                    // set for ByGuest mode
        public decimal AmountDue { get; set; }               // pre-calculated share
        public decimal TipAmount { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = Models.PaymentMethod.Cash;
        public string? Feedback { get; set; }
    }

    public class BillViewModel : TableOrderViewModel
    {
        public SplitMode SplitMode { get; set; } = SplitMode.Single;
        public int SplitCount { get; set; } = 1;

        public List<SplitPayerViewModel> Payers { get; set; } = new();

        public decimal TipAmount { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = Models.PaymentMethod.Cash;
        public string? Feedback { get; set; }

    }
}
