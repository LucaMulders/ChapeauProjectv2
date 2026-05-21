namespace ChapeauProject.Models
{
    public class MenuItem
    {
        public int MenuItemID { get; set; }
        public string ItemName { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; } 
        public string CourseName { get; set; }  //NOTE make enum
        public string MenuCard { get; set; }    //NOTE change to menuID

        public MenuItem(int id, string name, decimal price, int stock, string course, string card)
        {
            MenuItemID = id;
            ItemName = name;
            Price = price;
            StockQuantity = stock;
            CourseName = course;
            MenuCard = card;
        }
    }
}
//NOTE change to menu 
//NOTE enum for category 