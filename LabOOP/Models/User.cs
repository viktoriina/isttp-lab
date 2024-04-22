using Microsoft.AspNetCore.Identity;

namespace LabOOP.Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }    
    }
}
