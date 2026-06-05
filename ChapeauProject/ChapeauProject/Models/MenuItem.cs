namespace ChapeauProject.Models
{
    public class MenuItem
    {
    
        public int MenuItemID { get; set; }
        public string ItemName { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public Menu AssociatedMenu { get; set; }

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