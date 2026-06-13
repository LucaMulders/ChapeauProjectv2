namespace ChapeauProject.ViewModels
{
    public class OrdersIndexViewModel
    {
        public List<TableOrderGroupViewModel> TableGroups { get; set; }
        public string Filter { get; set; }
        public string PageTitle { get; set; }
        public string EmptyMessage { get; set; }
    }
}
