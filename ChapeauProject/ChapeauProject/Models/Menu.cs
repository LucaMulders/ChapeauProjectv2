namespace ChapeauProject.Models
{
    public class Menu
    {
      
        public MenuCard CardType { get; set; }

        public Menu(MenuCard cardType)
        {
            CardType = cardType;
        }
    }
}