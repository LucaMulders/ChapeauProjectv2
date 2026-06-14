namespace ChapeauProject.Models
{
    public class MenuItem
    {
        public int MenuItemID { get; set; }
        public string ItemName { get; set; }
        public decimal Price { get; set; }
        public decimal VatRate { get; set; }
        public int StockQuantity { get; set; }
        public MenuCard Card { get; set; }

        public bool IsAvailable
        {
            get { return StockQuantity > 0; }
        }

        // Returns true if the requested quantity can still be fulfilled from current stock
        public bool IsInStock(int quantity)
        {
            return quantity < StockQuantity;
        }

        public string PriceDisplay
        {
            get { return $"€{Price:F2}"; }
        }

        public MenuItem() { ItemName = string.Empty; }

        public MenuItem(int id, string name, decimal price, decimal vatRate, int stock, MenuCard card)
        {
            MenuItemID    = id;
            ItemName      = name;
            Price         = price;
            VatRate       = vatRate;
            StockQuantity = stock;
            Card          = card;
        }
    }
}