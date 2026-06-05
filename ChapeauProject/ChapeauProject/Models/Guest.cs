namespace ChapeauProject.Models
{
    public class Guest
    {
        public int GuestID { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string FullName
        {
            get
            {
                string name = $"{FirstName} {LastName}".Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    return "Unnamed Guest";
                }
                return name;
            }
        }

        public Guest() { }

        public Guest(int guestID, string firstName, string lastName)
        {
            GuestID = guestID;
            FirstName = firstName;
            LastName = lastName;
        }
    }
}
