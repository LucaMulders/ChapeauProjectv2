namespace ChapeauProject.Models
{
    //NOTE MenuItem has no behavior methods or computed properties — rubric requires classes contain behavior related to their data
    public class MenuItem
    {
        public int MenuItemID { get; set; }
        public string ItemName { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public CourseName Course { get; set; }
        public MenuCard Card { get; set; }

        public MenuItem(int id, string name, decimal price, int stock, string course, string card)
        {
            MenuItemID = id;
            ItemName = name;
            Price = price;
            StockQuantity = stock;

            if (card == "Dinner")
                Card = MenuCard.Dinner;
            else if (card == "Drinks")
                Card = MenuCard.Drinks;
            else
                Card = MenuCard.Lunch;
        }
    }
}