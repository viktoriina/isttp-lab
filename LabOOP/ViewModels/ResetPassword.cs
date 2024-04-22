using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace LabOOP.ViewModels
{
    public class ResetPassword
    {
        [Required(ErrorMessage = "Email is required!")]
        [EmailAddress]
        [RegularExpression(@"^[^@\s]+@gmail\.com$", ErrorMessage = "Email must end with gmail.com")]
        public string Email { get; set; }

        [Required(ErrorMessage = "You must enter password!")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "You must confirm password!")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords don't match!")]
        [DisplayName("Confirm password")]
        public string ConfirmPassword { get; set; }
    }
}
