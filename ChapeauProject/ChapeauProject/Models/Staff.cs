namespace ChapeauProject.Models
{
    public class Staff
    {
        public int StaffID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        //NOTE Role should be an enum
        public string Role { get; set; }
        public string Password { get; set; }

        public Staff(int id, string firstName, string lastName, string role, string password)
        {
            StaffID = id;
            FirstName = firstName;
            LastName = lastName;
            Role = role;
            Password = password;
        }

        public Staff()
        {
            StaffID = 0;
            FirstName = "";
            LastName = "";
            Role = "";
            Password = "";
        }
    }
}
