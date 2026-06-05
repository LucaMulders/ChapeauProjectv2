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

        public string StatusPillClass => PreparationStatus switch
        {
            PreparationStatus.Pending   => "status-pill status-ordered",
            PreparationStatus.Preparing => "status-pill status-preparing",
            PreparationStatus.Ready     => "status-pill status-ready",
            PreparationStatus.Served    => "status-pill status-served",
            _ => "status-pill"
        };

        public string StatusLabel => PreparationStatus switch
        {
            PreparationStatus.Pending   => "Ordered",
            PreparationStatus.Preparing => "Being Prepared",
            PreparationStatus.Ready     => "Ready to Serve",
            PreparationStatus.Served    => "Served",
            _                           => PreparationStatus.ToString()
        };
    }
}
