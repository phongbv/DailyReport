using DailyReport.Controllers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Web;

namespace DailyReport.Models
{
    public class UserModel : IPrincipal
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        [JsonIgnore]
        public bool IsAdmin => AccountController.AdminUsers.Contains((Username + "").ToLower());
        private IIdentity _identity;
        [JsonIgnore]
        public IIdentity Identity
        {
            get
            {
                if (_identity == null)
                    _identity = new GenericIdentity(Username);

                return _identity;
            }
            private set
            {
                this._identity = value;
            }
        }

        public bool IsInRole(string role) => false;
    }

    public class HashPassword
    {
        public static string Hash(string loginId, string password)
        {
            MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
            byte[] hashedBytes;
            UTF8Encoding encoder = new UTF8Encoding();
            var encryptedStr = loginId.Substring(0, loginId.Length / 2).ToLower() + password.Substring(0, password.Length / 2) + loginId.Substring(loginId.Length / 2, loginId.Length - loginId.Length / 2).ToLower() + password.Substring(password.Length / 2, password.Length - password.Length / 2);
            hashedBytes = md5Hasher.ComputeHash(encoder.GetBytes(encryptedStr));
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToUpper();
        }
    }
}