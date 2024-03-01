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
using Q_AManagement.Filter;
using QuestionPaperApp.Models;

namespace QuestionPaperApp.Controllers
{
    [CustomAuthorize]
    [AdminAuthorizationFilter]
    public class AdminController : Controller
    {
        private QAManagementEntities db = new QAManagementEntities();

        public ActionResult Index()
        {
            return View(db.Users.ToList());
        }

   
 
        public ActionResult Create()
        {
            return View();
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "UserID,Username,PasswordHash,Email,Role")] User user)
        {
            if (ModelState.IsValid)
            {
                var newUser = new User
                {
                    Username = user.Username,
                    Email = user.Email,
                    PasswordHash = EncryptString(user.PasswordHash),
                    Role = user.Role // Set role to "student" by default
                };
                db.Users.Add(newUser);
                db.SaveChanges();
                TempData["success"] = "User Created Successfully";
                return RedirectToAction("Index");
            }

            return View(user);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
           
            return View(user);
        }

     
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserID,Username,Email,Role")] User user)
        {
          
                var existingUser = db.Users.Find(user.UserID);

                
                existingUser.Username = user.Username;
                existingUser.Email = user.Email;
                existingUser.Role = user.Role;

             
                db.Entry(existingUser).State = EntityState.Modified;

            
                db.SaveChanges();

            TempData["success"] = "User Edited Successfully";
            return RedirectToAction("Index");
            

          
         
        }


        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

 
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (id > 0)
            {
                var user = db.Users.FirstOrDefault(model => model.UserID == id);
                if (user != null)
                {
                    // Find and delete all question papers associated with the user
                    var questionPapers = db.QuestionPapers.Where(model => model.CreatorID == id).ToList();
                    foreach (var questionPaper in questionPapers)
                    {
                        // Find and delete all questions associated with each question paper
                        var questions = db.Questions.Where(model => model.QuestionPaperID == questionPaper.QuestionPaperID).ToList();
                        foreach (var question in questions)
                        {
                            // Find and delete all answers associated with each question
                            var answers = db.Answers.Where(a => a.QuestionID == question.QuestionID).ToList();
                            db.Answers.RemoveRange(answers);
                            db.SaveChanges(); // Save changes after removing answers
                            db.Questions.Remove(question);
                        }
                        db.SaveChanges(); // Save changes after removing questions
                        db.QuestionPapers.Remove(questionPaper);
                    }
                    db.SaveChanges(); // Save changes after removing questionPapers
                    db.Users.Remove(user);
                    db.SaveChanges(); // Save changes after removing user
                    TempData["success"] = "User and associated data deleted successfully!";
                }
            }
            return RedirectToAction("Index");
        }



        public ActionResult QuestionPapers()
        {
            var questionpaper=db.QuestionPapers.Where(q=>q.Status!="Draft");
            return View(questionpaper);
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

        public ActionResult EditPaper(int id)
        {
            var questionpaper=db.QuestionPapers.Find(id);
            return View(questionpaper);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("EditPaper")]
        public ActionResult EditPaperConfirm(QuestionPaper questionpaper) {


            var existingQuestionPaper = db.QuestionPapers.Find(questionpaper.QuestionPaperID);


            existingQuestionPaper.Title = questionpaper.Title;
            existingQuestionPaper.Description = questionpaper.Description;
            existingQuestionPaper.Status = questionpaper.Status;


            db.Entry(existingQuestionPaper).State = EntityState.Modified;


            db.SaveChanges();

            TempData["success"] = "QuestionPaper Changed Successfully";

            return RedirectToAction("QuestionPapers");
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
