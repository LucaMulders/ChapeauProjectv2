namespace ChapeauProject.Models
{
    //NOTE Guest has no behavior methods or computed properties — rubric requires classes contain behavior related to their data
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
