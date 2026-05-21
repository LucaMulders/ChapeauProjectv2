using System.ComponentModel.DataAnnotations;
namespace ChapeauProject.ViewModels
{

    public class LoginViewModel
    {
        [Display(Name = "Staff ID")]
        public int StaffID { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}