using System.ComponentModel.DataAnnotations;

namespace ChapeauProject.Models
{
    public class LoginModel
    {
        [Display(Name = "Staff ID:")]
        public int StaffID { get; set; }
        [Display(Name = "Password:")]
        public string Password { get; set; }

        public LoginModel()
        {

        }

        public LoginModel(int staffID, string password)
        {
            StaffID = staffID;
            Password = password;
        }
    }
}
