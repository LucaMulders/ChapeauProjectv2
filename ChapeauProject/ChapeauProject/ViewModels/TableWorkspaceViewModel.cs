using ChapeauProject.Models;

namespace ChapeauProject.ViewModels
{
    public class TableWorkspaceViewModel
    {
        public TableOrderViewModel TableOrders { get; set; }
        public Order ActiveBasket { get; set; }
        public List<Guest> Guests { get; set; }
        public List<MenuItem> MenuItems { get; set; }
        public MenuCard CardFilter { get; set; }
        public string CourseFilter { get; set; }
    }
}
