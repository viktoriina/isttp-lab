using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using LabOOP.Models;
using Microsoft.EntityFrameworkCore;

namespace LabOOP.Data
{
    public class UserAndContext : IdentityDbContext<User>
    {
        public UserAndContext(DbContextOptions<UserAndContext> options) : base (options)
        {
            
        }
       
    }
}
