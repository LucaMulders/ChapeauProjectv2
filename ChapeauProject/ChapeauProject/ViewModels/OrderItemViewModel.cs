using ChapeauProject.Models;

namespace ChapeauProject.ViewModels
{
    public class OrderItemViewModel
    {
        public int OrderItemID { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal VatRate { get; set; }
        public PreparationStatus PreparationStatus { get; set; }

        public string StatusPillClass => PreparationStatus.ToStatusPillClass();
        public string StatusLabel     => PreparationStatus.ToStatusLabel();
    }
}
