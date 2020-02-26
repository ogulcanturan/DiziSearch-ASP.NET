using DiziSearch.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DiziSearch.Components
{
    public class UserNameViewComponent : ViewComponent
    {
        private readonly AppIdentityDbContext _userApp;

        public UserNameViewComponent(AppIdentityDbContext userApp)
        {
            _userApp = userApp;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            #region SuankiKullaniciBulma
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);//This is ID = claim.Value
            ViewBag.currentName = claimsIdentity.Name.Substring(0,claimsIdentity.Name.IndexOf('@')); //This is Name
            #endregion
            return View();

        }
    }
}
