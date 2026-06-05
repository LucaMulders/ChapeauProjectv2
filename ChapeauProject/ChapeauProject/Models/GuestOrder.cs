namespace ChapeauProject.Models
{
    //NOTE GuestOrder and GuestOrderItem need to return domain models instead of raw data
    public class GuestOrder
    {
        public int GuestID { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public List<GuestOrderItem> Items { get; set; } = new();
    }
}
