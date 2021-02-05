using DailyReport.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace DailyReport.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserModel user)
        {
            if (ModelState.IsValid)
            {
                using (var dbContext = new DataContext())
                {
                    bool IsValidUser = dbContext.Users
                                           .Any(u => u.Username.ToLower() == user
                                           .Username.ToLower() && user
                                           .Password == user.Password);
                    if (IsValidUser)
                    {
                        FormsAuthentication.SetAuthCookie(user.Username, false);
                        return RedirectToAction("Index", "Home");
                    }
                }


            }
            ModelState.AddModelError("", "invalid Username or Password");
            return View();
        }
    }
}