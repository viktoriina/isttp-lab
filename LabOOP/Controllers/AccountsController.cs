using LabOOP.Models;
using LabOOP.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using LabOOP.IdentityClass;
using DocumentFormat.OpenXml.EMMA;

namespace LabOOP.Controllers
{
    public class AccountsController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly EmailService _emailService;
        public AccountsController(UserManager<User> userManager, SignInManager<User> signInManager, EmailService emailService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailService = emailService;
        }
        public IActionResult Register()
        {
            var response = new RegisterViewModel();
            return View(response);
        }
        public IActionResult Login()
        {
            var response = new LoginViewModel();
            return View(response);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if(!ModelState.IsValid) 
            { 
              return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) 
            {
                ModelState.AddModelError(string.Empty, "Email is not Exist");
                return View(model);
            }
            var passwordCheck = await _userManager.CheckPasswordAsync(user, model.Password);
            if (passwordCheck)
            {
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    ModelState.AddModelError(string.Empty, "You you are not confirmed email");
                    return View(model);
                }            
                var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)         
                    return RedirectToAction("Index", "Orders");
            }
            ModelState.AddModelError(string.Empty, "Password is not correct");
            return View(model);
        }
        public IActionResult ForgotPassword()
        {
            var reset = new ResetPassword();
            return View(reset);
        }

        [HttpPost]
        public async Task <IActionResult> ForgotPassword(ResetPassword model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if(user == null)
            {
                ModelState.AddModelError(string.Empty, "Email is not Exist");
                return View(model);
            }
            var validation = new PasswordValidator<User>();
            var result = await validation.ValidateAsync(_userManager, user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError(string.Empty, "You are not confirm email you are not finished registration");
                return View(model);
            }
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action(
            "ResetPassword",
            "Accounts",
                    new { userId = user.Id, token = code, newPassword = model.Password},
                    protocol: HttpContext.Request.Scheme);
            _emailService.CreateMessage(user.Email, callbackUrl, "Reset Password");
            return View("ResetPasswordV");
        }
        public async Task<IActionResult> ResetPassword(string userId, string token, string newPassword)
        {
            if (userId == null || token == null || newPassword == null)
            {
                return View("Error");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("Error");
            }
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (result.Succeeded)
                return RedirectToAction("Index", "Home");
            return View("Error");
             
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Accounts");
        }
        [Authorize]
        public IActionResult ChangePassword()
        {
            var changePassword = new ChangePasswordViewModel();
            return View(changePassword);
        }

        [Authorize]
        public async Task<IActionResult> Details()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if(user==null)
            {
                return NotFound();
            }
            var login = new UserViewModel()
            {
                Email = user.Email,
                LastName = user.LastName,
                Name = user.UserName,
                Username = user.UserName
            };
            return View(login);
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var  userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            var checkPassword = await _userManager.CheckPasswordAsync(user, model.Password);
            if(checkPassword)
            {

                var result = await _userManager.ChangePasswordAsync(user, model.Password, model.NewPassword);
                if (result.Succeeded)
                {         
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }
            }
            TempData["Error"] = "You enter incorrect password";
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var emailExists = await _userManager.FindByEmailAsync(model.Email);
            if (emailExists != null)
            {
                ModelState.AddModelError(string.Empty, "This email already exists!");
                return View(model);
            }
            var usernameExists = await _userManager.FindByNameAsync(model.Username);
            if (usernameExists != null)
            {
                ModelState.AddModelError(string.Empty, "This username already exists!");
                return View(model);
            }
            string role = UserRoles.GetRole(model.Email);
            var newUser = CreateUser(model);
            var newUserResponse = await _userManager.CreateAsync(newUser, model.Password);
            if (newUserResponse.Succeeded)
            {
                await _userManager.AddToRoleAsync(newUser, role);
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                var callbackUrl = Url.Action(
                "ConfirmEmail",
                "Accounts",
                        new { userId = newUser.Id, code = token },
                        protocol: HttpContext.Request.Scheme);           
                _emailService.CreateMessage(newUser.Email, callbackUrl, "Confirm Registration");
                return View("DisplayEmail");
            }
            foreach (var error in newUserResponse.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("Error");
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
                return RedirectToAction("Index", "Home");
            else
                return View("Error");
        }
        private User CreateUser(RegisterViewModel model)
        {
            return new User()
            {
                FirstName = model.Name,
                LastName = model.LastName,
                Email = model.Email,
                UserName = model.Username,
            };
        }
    }
}
