namespace ChapeauProject.Models
{
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

            Course = System.Enum.TryParse(course, true, out CourseName parsedCourse) ? parsedCourse : CourseName.Starter;
            Card = System.Enum.TryParse(card, true, out MenuCard parsedCard) ? parsedCard : MenuCard.Lunch;
        }
    }
}