using DailyReport.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;

namespace DailyReport.Controllers
{
    public class AccountController : Controller
    {
        public static readonly List<string> AdminUsers = (ConfigurationManager.AppSettings["admin-user"] + "").Split(';')
            .Select(e => e.ToLower()).ToList();
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserModel user)
        {
            if (ModelState.IsValid)
            {
                using (var dbContext = new DataContext())
                {
                    var hashPassword = HashPassword.Hash(user.Username.ToLower(), user.Password);
                    var logonUser = dbContext.Users
                                           .FirstOrDefault(u => u.Username.ToLower() == user
                                           .Username.ToLower() && u
                                           .Password == hashPassword);
                    if (logonUser != null)
                    {
                        user.Id = logonUser.Id;
                        user.Password = null;
                        SetAuthCookie(user);
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            ModelState.AddModelError("", "invalid Username or Password");
            return View();
        }
        protected void SetAuthCookie(UserModel model)
        {
            string userData = JsonConvert.SerializeObject(model);
            TimeSpan configTimeout = new TimeSpan(TimeSpan.TicksPerMinute * 60);
            if (System.Configuration.ConfigurationManager.GetSection("system.web/authentication") != null)
            {
                var configurationSection = ((System.Web.Configuration.AuthenticationSection)System.Configuration.ConfigurationManager.GetSection("system.web/authentication"));

                if (configurationSection.Forms != null)
                    configTimeout = configurationSection.Forms.Timeout;
            }
            var expiration = DateTime.Now.AddTicks(configTimeout.Ticks);

            FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(
                     1,
                     model.Username,
                     DateTime.Now,
                     expiration,
                     true,
                     userData);

            string encTicket = FormsAuthentication.Encrypt(authTicket);
            HttpCookie faCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);

            Response.Cookies.Set(faCookie);
            System.Web.HttpContext.Current.User = model;
        }
        [Authorize]
        public string ChangePass(string password)
        {
            var username = (User as UserModel).Username.ToLower();
            using (var dbContext = new DataContext())
            {
                string newPass = HashPassword.Hash(username, password);
                var user = dbContext.Users.FirstOrDefault(e => e.Username.ToLower() == username);
                if (user == null) return "User không hợp lệ";
                user.Password = newPass;
                dbContext.SaveChanges();
            }
            return "Thành công";
        }
        [Authorize]
        public string ResetPass(string username)
        {
            var currentUsername = (User as UserModel).Username.ToLower();
            if (AdminUsers.Contains(currentUsername) == false)
            {
                return "Bạn không có quyền truy cập";
            }
            using (var dbContext = new DataContext())
            {
                string newPass = HashPassword.Hash(username, "123456a@");
                var user = dbContext.Users.FirstOrDefault(e => e.Username.ToLower() == username);
                if (user == null) return "User không hợp lệ";
                user.Password = newPass;
                dbContext.SaveChanges();
            }
            return "Thành công";
        }
        [Authorize]
        public string Hash(string username, string password)
        {
            if (AdminUsers.Contains((User as UserModel).Username.ToLower()))
                return HashPassword.Hash(username, password);
            else
            {
                return "Bạn không có quyền truy cập chức năng này";
            }
        }
    }
}