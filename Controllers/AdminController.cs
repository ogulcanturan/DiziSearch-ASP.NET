using DiziSearch.Models.ViewModels;
using DiziSearch.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiziSearch.Models
{
    [Authorize(Roles =Constants.MasterAdminUser)]
    public class AdminController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AdminController(UserManager<AppUser> userManager,RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        //Retrieving all users
        public ViewResult Index() => View(_userManager.Users);

        public ViewResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateModel model)
        {
            if (ModelState.IsValid)
            {
                AppUser user = new AppUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                };

                IdentityResult result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    #region IlgiliRoleYaratilmamissaYarat
                    if(!await _roleManager.RoleExistsAsync(Constants.MasterAdminUser))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(Constants.MasterAdminUser));
                    }
                    if(!await _roleManager.RoleExistsAsync(Constants.NormalAdminUser))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(Constants.NormalAdminUser));
                    }
                    if(!await _roleManager.RoleExistsAsync(Constants.ModeratorUser))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(Constants.ModeratorUser));
                    }
                    #endregion

                    if(model.Admin == Constants.MasterAdminUser)
                    {
                        user.StatusRole = Constants.MasterAdminUser;
                        await _userManager.AddToRoleAsync(user, Constants.MasterAdminUser);
                    }
                    else if(model.Admin == Constants.NormalAdminUser)
                    {
                        user.StatusRole = Constants.NormalAdminUser;
                        await _userManager.AddToRoleAsync(user, Constants.NormalAdminUser);
                    }
                    else if(model.Admin == Constants.ModeratorUser)
                    {
                        user.StatusRole = Constants.ModeratorUser;
                        await _userManager.AddToRoleAsync(user, Constants.ModeratorUser);
                    }
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    foreach(IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id != null)
            {
                var userFromDb = await _userManager.FindByIdAsync(id);
                if (userFromDb == null)
                {
                    return NotFound();
                }
                return View(userFromDb);
            }
            return NotFound();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, AppUser appUser)
        {
            if (id != appUser.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                //assigning correct Id to users
                var userFromDb = _userManager.Users.Where(u => u.Id == id).FirstOrDefault();
                #region EgerIlgiliRoleYaratilmamissaYarat
                if (!await _roleManager.RoleExistsAsync(Constants.MasterAdminUser))
                {
                    await _roleManager.CreateAsync(new IdentityRole(Constants.MasterAdminUser));
                }
                if (!await _roleManager.RoleExistsAsync(Constants.NormalAdminUser))
                {
                    await _roleManager.CreateAsync(new IdentityRole(Constants.NormalAdminUser));
                }
                if (!await _roleManager.RoleExistsAsync(Constants.ModeratorUser))
                {
                    await _roleManager.CreateAsync(new IdentityRole(Constants.ModeratorUser));
                }
                #endregion
                //Eğer MasterAdmin olarak güncellenmişse
                if (appUser.Admin == Constants.MasterAdminUser)
                {
                    userFromDb.StatusRole = Constants.MasterAdminUser;
                    //ilgili userin ilk başta ilgili Role'lerden arındır..
                    await _userManager.RemoveFromRoleAsync(userFromDb, Constants.NormalAdminUser);
                    await _userManager.RemoveFromRoleAsync(userFromDb, Constants.ModeratorUser);
                    //İlgili usere istenilen Rolü ver
                    await _userManager.AddToRoleAsync(userFromDb, Constants.MasterAdminUser);
                }
                if (appUser.Admin == Constants.NormalAdminUser)
                {
                    userFromDb.StatusRole = Constants.NormalAdminUser;
                    //ilgili userin ilk başta ilgili Role'lerden arındır..
                    await _userManager.RemoveFromRoleAsync(userFromDb, Constants.MasterAdminUser);
                    await _userManager.RemoveFromRoleAsync(userFromDb, Constants.ModeratorUser);
                    //İlgili usere istenilen Rolü ver
                    await _userManager.AddToRoleAsync(userFromDb, Constants.NormalAdminUser);
                }
                if (appUser.Admin == Constants.ModeratorUser)
                {
                    userFromDb.StatusRole = Constants.ModeratorUser;
                    //ilgili userin ilk başta ilgili Role'lerden arındır..
                    await _userManager.RemoveFromRoleAsync(userFromDb, Constants.MasterAdminUser);
                    await _userManager.RemoveFromRoleAsync(userFromDb, Constants.NormalAdminUser);
                    //İlgili usere istenilen Rolü ver
                    await _userManager.AddToRoleAsync(userFromDb, Constants.ModeratorUser);
                }
                userFromDb.UserName = appUser.Email;
                userFromDb.Email = appUser.Email;
                await _userManager.UpdateAsync(userFromDb);
                return RedirectToAction(nameof(Index));
            }
            return View(appUser);
        }
        #region DeleteAdmin
        public async Task<IActionResult> Delete(string id)
        {
            var userFromDb = await _userManager.FindByIdAsync(id);
            if (userFromDb != null)
            {
                return View(userFromDb);
            }
            return NotFound();
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var userFromDb = await _userManager.FindByIdAsync(id);
            if (userFromDb != null)
            {
                IdentityResult result = await _userManager.DeleteAsync(userFromDb);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    AddErrorsFromResult(result);
                }
            }
            else
            {
                ModelState.AddModelError("", "User Not Found");
            }
            return View("Index", _userManager.Users);
        }
        #endregion

        private void AddErrorsFromResult(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }

    }
}
