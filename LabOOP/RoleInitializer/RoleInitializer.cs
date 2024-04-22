using LabOOP.Models;
using Microsoft.AspNetCore.Identity;

namespace LabOOP.RoleInitializer
{
    public class RoleInitializer
    {
        public static async Task InitializeAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            if (await roleManager.FindByNameAsync(UserRoles.Admin) == null)
            {
                await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            }
            if (await roleManager.FindByNameAsync(UserRoles.User) == null)
            {
                await roleManager.CreateAsync(new IdentityRole(UserRoles.User));
            }
            if (await roleManager.FindByNameAsync(UserRoles.SuperAdmin) == null)
            {
                await roleManager.CreateAsync(new IdentityRole(UserRoles.SuperAdmin));
            }
        }
    }
}
