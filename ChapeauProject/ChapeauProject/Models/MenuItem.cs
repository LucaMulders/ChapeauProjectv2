namespace ChapeauProject.Models
{
    public class MenuItem
    {
        public int MenuItemID { get; set; }
        public string ItemName { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public Menu AssociatedMenu { get; set; }

        public bool IsAvailable => StockQuantity > 0;
        public bool IsInStock(int quantity) => quantity < StockQuantity;
        public string PriceDisplay => $"€{Price:F2}";

        public MenuItem(int id, string name, decimal price, int stock, Menu menu)
        {
            MenuItemID = id;
            ItemName = name;
            Price = price;
            StockQuantity = stock;
            AssociatedMenu = menu;
        }
    }
}