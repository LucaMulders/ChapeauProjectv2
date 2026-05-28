namespace ChapeauProject.Models
{
    public class GuestOrder
    {
        public int GuestID { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public List<GuestOrderItem> Items { get; set; } = new();
    }
}
