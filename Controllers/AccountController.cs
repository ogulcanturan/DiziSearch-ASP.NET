using DiziSearch.Models;
using DiziSearch.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiziSearch.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        
        public AccountController(UserManager<AppUser> userManager,SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        } 

        [AllowAnonymous]
        public ViewResult Login(string returnUrl)
        {
            return View(new LoginModel
            {
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            if (ModelState.IsValid)
            {
                AppUser user = await _userManager.FindByEmailAsync(loginModel.Email);

                if(user != null)
                {
                    await _signInManager.SignOutAsync(); //Eğer kullanıcı girişi varsa çıkış yap.
                    if((await _signInManager.PasswordSignInAsync(user, loginModel.Password, false, false)).Succeeded)
                    {
                    
                    return Redirect(loginModel.ReturnUrl ?? "~/"); //Eğer Returnurl null ise Index'e at.
                    }
                }
            }
            ModelState.AddModelError("", "İsim veya şifre yanlış");
            return View(loginModel);
        }
        public async Task<IActionResult> Logout(string returnUrl ="/")
        {
            await _signInManager.SignOutAsync();//Çıkış yap
            return Redirect(returnUrl);
        }
    }
}
