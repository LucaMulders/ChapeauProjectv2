using ChapeauProject.Models;

namespace ChapeauProject.ViewModels
{
    public class TablesViewModel
    {
        public Table Table { get; set; }
        public int OrderCount { get; set; }
        public int GuestCount { get; set; }
        public bool HasFoodOrder { get; set; }
        public bool HasDrinkOrder { get; set; }
    }
}
