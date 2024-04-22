using DocumentFormat.OpenXml.Drawing.Charts;
using LabOOP.Models;
using LabOOP.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LabOOP.Controllers
{
    [Authorize(Roles = "admin, superAdmin")]
    public class UsersController : Controller
    {
        private readonly DBSHOPContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        public UsersController(DBSHOPContext context, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            List<UserViewModel> result = new List<UserViewModel>();
            foreach (var user in users)
            {
                if(await _userManager.IsEmailConfirmedAsync(user) && (!await _userManager.IsInRoleAsync(user, UserRoles.Admin)) && (! await _userManager.IsInRoleAsync(user, UserRoles.SuperAdmin)))
                {
                    var item = new UserViewModel()
                    {
                        Id = user.Id,
                        Name = user.FirstName,
                        Email = user.Email,
                        LastName = user.LastName,
                        Username = user.UserName
                    };
                    result.Add(item);
                }
            }
            return View(result);
        }
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
                return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();
            var userEdit = new UserChangeView()
            {
                Id = user.Id,
                Name = user.FirstName,
                Email = user.Email,
                LastName = user.LastName,
                Username = user.UserName
            };
            return View(userEdit);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(UserChangeView model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }
            var EmailExist = await _userManager.FindByEmailAsync(model.Email);
            if (EmailExist != null && EmailExist.Id != model.Id)
            {
                ModelState.AddModelError(string.Empty, "Email is exist");
                return View(model);
            }
            var UserNameExist = await _userManager.FindByNameAsync(model.Username);
            if (UserNameExist != null && UserNameExist.Id != model.Id)
            {
                ModelState.AddModelError(string.Empty, "UserName is exist");
                return View(model);
            }
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user != null)
            {
                user.Email = model.Email;
                user.FirstName = model.Name;
                user.LastName = model.LastName;
                user.UserName = model.Username;
                var result = await _userManager.UpdateAsync(user);
                if(result.Succeeded)
                {
                   return RedirectToAction("Index");
                }
                else
                {
                    foreach(var item in result.Errors) 
                    {
                        ModelState.AddModelError(string.Empty, item.Description);
                    }
                    return View(model);
                }

            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if(id ==null)
            {
                return NotFound();

            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) 
            {
                return NotFound();
            }
            var orders =_context.Orders.
                Where(e => e.UserId == id).
                Include(a => a.ProductsOrders).
                Include(f=>f.Feedbacks).ToList();

            foreach(var order in orders) 
            {
                foreach (var o in order.ProductsOrders)
                    _context.Remove(o);
                foreach (var o in order.Feedbacks)
                    _context.Remove(o);
                _context.Orders.Remove(order);
            }
            await _context.SaveChangesAsync();

            var roles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, roles);
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("Logout", "Accounts");
            }
            return NotFound();
        }
    }
}
