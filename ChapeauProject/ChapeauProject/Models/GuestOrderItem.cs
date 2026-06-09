using ChapeauProject.ViewModels;

namespace ChapeauProject.Models
{
    public class GuestOrderItem
    {
        public int OrderItemID { get; set; }
        public int Quantity { get; set; }
        public PreparationStatus PreparationStatus { get; set; }
        public MenuItem MenuItem { get; set; } = new MenuItem(0, string.Empty, 0, 0, 0, null);

        // Convenience passthroughs for existing code (same as guestorder)
        public string ItemName
        {
            get { return MenuItem.ItemName; }
        }

        public decimal Price
        {
            get { return MenuItem.Price; }
        }

        public decimal VatRate
        {
            get { return MenuItem.VatRate; }
        }
    }
}
