namespace ChapeauProject.Models
{
    public class GuestOrder
    {
        public Guest Guest { get; set; } = new Guest();
        public List<GuestOrderItem> Items { get; set; } = new();

        // Convenience passthrough for code that uses GuestName/GuestID directly
        public int GuestID => Guest.GuestID;
        public string GuestName => Guest.FullName;
    }
}
