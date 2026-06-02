namespace ChapeauProject.Models
{
    public class Guest
    {
        public int GuestID { get; set; }
        public string GuestName { get; set; } = string.Empty;

        public Guest() { }

        public Guest(int guestID, string guestName)
        {
            GuestID = guestID;
            GuestName = guestName;
        }
    }
}
