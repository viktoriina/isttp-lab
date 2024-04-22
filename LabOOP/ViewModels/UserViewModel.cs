using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace LabOOP.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
    }
}
