namespace ChapeauProject.ViewModels
{
    public class GuestOrderViewModel
    {
        public int GuestID { get; set; }
        public string GuestName { get; set; }
        public List<OrderItemViewModel> Items { get; set; }
    }
}
