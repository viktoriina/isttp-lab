using LabOOP.Models;
using LabOOP.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LabOOP.Controllers
{
    [Authorize(Roles = "superAdmin")]
    public class RolesController : Controller
    {
        RoleManager<IdentityRole> _roleManager;
        UserManager<User> _userManager;
        public RolesController(RoleManager<IdentityRole> roleManager, UserManager<User> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }
        public IActionResult Index() => View(_roleManager.Roles.Where(e => e.Name != UserRoles.SuperAdmin).ToList());

        public IActionResult Create()
        {
           return View(new CreateRoleViewModel());
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateRoleViewModel model)
        {
            if (!string.IsNullOrEmpty(model.Role))
            {
                var result = await _roleManager.CreateAsync(new IdentityRole(model.Role)) ;
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            var users = await _userManager.Users.ToListAsync();
            var filteredUsers = new List<User>(); 
            if (role != null)
            {
                foreach(var user in users)
                   if(await _userManager.IsInRoleAsync(user, role.Name) && await _userManager.IsEmailConfirmedAsync(user))
                        filteredUsers.Add(user);
                if(filteredUsers.Count == 0)
                    await _roleManager.DeleteAsync(role);
                else
                    return View("UserWithRole", filteredUsers);
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> UserList() 
        {
            var users = _userManager.Users.ToList();
            var Filteredusers = new List<User>();
            foreach(var user in users)
            {
                if(await _userManager.IsEmailConfirmedAsync(user) && (!await _userManager.IsInRoleAsync(user, UserRoles.SuperAdmin)))
                  Filteredusers.Add(user);
            }
            return View(Filteredusers);          
        }

        public async Task<IActionResult> Edit(string userId)
        {
            User user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var model = await CreateChangeRolemodel(user);
                return View(model);
            }
            return NotFound();
        }
        [HttpPost]
        public async Task<IActionResult> Edit(string userId, List<string> roles)
        {
            User user = await _userManager.FindByIdAsync(userId);
            if(user == null )
            {
                return NotFound();
            }
            if (roles == null || roles.Count == 0)
            {
                var model = await CreateChangeRolemodel(user);
                ModelState.AddModelError(string.Empty, "Choose the role");
                return View(model);
            }
                var userRoles = await _userManager.GetRolesAsync(user);
                var allRoles = _roleManager.Roles.Where(e => e.Name != UserRoles.SuperAdmin).ToList();
                var addedRoles = roles.Except(userRoles);
                var removedRoles = userRoles.Except(roles);

                await _userManager.AddToRolesAsync(user, addedRoles);

                await _userManager.RemoveFromRolesAsync(user, removedRoles);

                return RedirectToAction("UserList");
        }
        private async Task<ChangeRoleViewModel> CreateChangeRolemodel(User user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = _roleManager.Roles.Where(e => e.Name != UserRoles.SuperAdmin).ToList();
            var model = new ChangeRoleViewModel
            {
                UserId = user.Id,
                UserEmail = user.Email,
                UserRoles = userRoles,
                AllRoles = allRoles
            };
            return model;
        }
    }
}