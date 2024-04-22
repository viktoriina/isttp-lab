using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace LabOOP.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "You must enter your email!")]
        [EmailAddress]
        [RegularExpression(@"^[^@\s]+@gmail\.com$", ErrorMessage = "Email must end with gmail.com")]
        public string Email { get; set; }

        [Required(ErrorMessage = "You must enter your name!")]
        public string Name { get; set; }

        [Required(ErrorMessage = "You must enter your last name!")]
        [DisplayName("Last name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "You must enter your username!")]
        public string Username { get; set; }

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
