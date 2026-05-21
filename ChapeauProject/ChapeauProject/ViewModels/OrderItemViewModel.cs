namespace ChapeauProject.ViewModels
{
    public class OrderItemViewModel
    {
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal VatRate { get; set; }
    }
}
