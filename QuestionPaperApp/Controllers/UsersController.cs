using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using QuestionPaperApp.Models;

namespace QuestionPaperApp.Controllers
{

    public class UsersController : Controller
    {
        private QAManagementEntities db = new QAManagementEntities();

        // GET: Users
        public ActionResult Index()
        {
            return View();
        }

       

        // GET: Users/Create
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create( User user)
        {
           
                var newUser = new User
                {
                    Username = user.Username,
                    Email = user.Email,
                    PasswordHash = EncryptString(user.PasswordHash),
                    Role = "Student" // Set role to "student" by default
                };
                db.Users.Add(newUser);
                db.SaveChanges();
                TempData["success"] = "User Created Successfully";
                return RedirectToAction("Login");
            

            return View(user);
        }

     
        public ActionResult Login(User user)
        {
            if (Session["UserID"] != null)
            {
                // Redirect to the respective dashboard based on user role
                if (Session["UserRole"].ToString() == "Admin")
                {
                    return RedirectToAction("Index", "Admin");
                }
                else if (Session["UserRole"].ToString() == "Teacher")
                {
                    return RedirectToAction("Index", "QuestionPapers");
                }
                else if (Session["UserRole"].ToString() == "Student")
                {
                    return RedirectToAction("Index", "Student");
                }
            }

            // If the user is not already authenticated, return the login view
            return View();


            // If model state is not valid or authentication fails, return the login view with errors
            return View("Login");
        }
        [HttpPost]
        [ActionName("Login")]
       
        public ActionResult IsLogin(User user)
        {
            var Isuser = db.Users.FirstOrDefault(u => u.Username == user.Username);

                if (Isuser != null)
                {
                    // Decrypt the password from the database
                    string decryptedPassword = DecryptString(Isuser.PasswordHash);

  
                    if (user.PasswordHash == decryptedPassword)
                    {
                        TempData["success"] = "Login Successfully";
                    Session["UserID"] = Isuser.UserID;
                    Session["UserRole"] = Isuser.Role;
                    Session["UserName"] = Isuser.Username;
                    if (Isuser.Role=="Admin")
                    {
                        return RedirectToAction("Index", "Admin");
                    }else if (Isuser.Role=="Teacher")
                    {
                        return RedirectToAction("Index", "QuestionPapers");
                    }
                    else {

                        return RedirectToAction("Index", "Student");
                    }
                    
                    }
                }

                // If the username or password is incorrect, add a model error
         
            
            TempData["error"] = "Login Failed";
            // If model state is not valid or authentication fails, return the login view with errors
            return View();
        }

        public ActionResult Logout() {

            TempData["success"] = "Login Successfully";
            Session["UserID"] = "";
            Session["UserRole"] = "";
            Session["UserName"] = "";
            return RedirectToAction("Login");
        }
        private string EncryptString(string plainText)
        {
            const string EncryptionKey = "qWE7&5pZ@2#9Df!1gH*3sKl$8oP5mN^0";

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(EncryptionKey);
                aesAlg.IV = new byte[16];

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (var msEncrypt = new System.IO.MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new System.IO.StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        public static string DecryptString(string cipherText)
        {
            const string EncryptionKey = "qWE7&5pZ@2#9Df!1gH*3sKl$8oP5mN^0";
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(EncryptionKey);
                aesAlg.IV = new byte[16];

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                byte[] cipherBytes = Convert.FromBase64String(cipherText);

                using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }




        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
