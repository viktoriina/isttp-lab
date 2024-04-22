using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace LabOOP.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "You must enter password!")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "You must enter new password!")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "You must confirm new password!")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords don't match!")]
        [DisplayName("Confirm password")]
        public string ConfirmPassword { get; set; }
    }
}
