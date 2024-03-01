using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Q_AManagement.Filter;
using QuestionPaperApp.Models;

namespace QuestionPaperApp.Controllers
{
    [CustomAuthorize]
    [TeacherAuthorizatonFilter]
   
    public class QuestionPapersController : Controller
    {
        private QAManagementEntities db = new QAManagementEntities();
        
        // GET: QuestionPapers
        public ActionResult Index()
        {

            int userid = Convert.ToInt32(Session["UserId"]);
            var questionPaper = db.QuestionPapers.Where(model => model.CreatorID == userid);


            return View(questionPaper.ToList());
        }

        // GET: QuestionPapers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            QuestionPaper questionPaper = db.QuestionPapers.Find(id);
            if (questionPaper == null)
            {
                return HttpNotFound();
            }
            return View(questionPaper);
        }

        // GET: QuestionPapers/Create
        public ActionResult Create()
        {
            ViewBag.CreatorID = new SelectList(db.Users, "UserID", "Username");
            return View();
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "QuestionPaperID,Title,Description,CreatorID")] QuestionPaper questionPaper)
        {
            var role = Session["UserRole"] as string; // Assuming role is stored in session as "UserRole"

            var NewQuestionPaper = new QuestionPaper
            {
                QuestionPaperID = questionPaper.QuestionPaperID,
                Title = questionPaper.Title,
                Description = questionPaper.Description,
                Status = (role == "Admin") ? "Pending" : "Draft", // Set status based on role
                CreationDate = DateTime.Now,
                CreatorID = Convert.ToInt32(Session["UserId"]),
            };

            if (ModelState.IsValid)
            {
                db.QuestionPapers.Add(NewQuestionPaper);
                db.SaveChanges();
                if (Convert.ToString(Session["UserRole"])=="Admin")
                {
                    return RedirectToAction("QuestionPapers","Admin");
                }
                else { 
                return RedirectToAction("Index");
                }
            }

          
            return View(questionPaper);
        }


        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            QuestionPaper questionPaper = db.QuestionPapers.Find(id);
            if (questionPaper == null)
            {
                return HttpNotFound();
            }
            ViewBag.CreatorID = new SelectList(db.Users, "UserID", "Username", questionPaper.CreatorID);
            return View(questionPaper);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "QuestionPaperID,Title,Description")] QuestionPaper questionPaper)
        {
            var existingPaper = db.QuestionPapers.Find(questionPaper.QuestionPaperID);

            
            existingPaper.Title = questionPaper.Title;
            existingPaper.Description = questionPaper.Description;
            


            db.Entry(existingPaper).State = EntityState.Modified;

            db.SaveChanges();

            TempData["success"] = "Change Saved Successfully";
            if (Convert.ToString(Session["UserRole"]) == "Admin")
            {
                return RedirectToAction("QuestionPapers", "Admin");
            }
            else
            {
                return RedirectToAction("Index");
            }

        }

 
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            QuestionPaper questionPaper = db.QuestionPapers.Find(id);
            if (questionPaper == null)
            {
                return HttpNotFound();
            }
            return View(questionPaper);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (id > 0)
            {
                var questionPaper = db.QuestionPapers.FirstOrDefault(model => model.QuestionPaperID == id);
                if (questionPaper != null)
                {
                    var questions = db.Questions.Where(model => model.QuestionPaperID == id).ToList();
                    if (questions.Any())
                    {
                        foreach (var question in questions)
                        {
                           
                            var answers = db.Answers.Where(a => a.QuestionID == question.QuestionID).ToList();
                            if (answers.Any())
                            {
                                foreach (var answer in answers)
                                {
                                    db.Answers.Remove(answer);
                                }
                            }
                           
                            db.Questions.Remove(question);
                        }
                    }

                    db.Entry(questionPaper).State = EntityState.Deleted;
                    db.SaveChanges();
                    TempData["success"] = "Question Paper Deleted Successfully!!";
                }
            }
            if (Convert.ToString(Session["UserRole"]) == "Admin")
            {
                return RedirectToAction("QuestionPapers", "Admin");
            }
            else
            {
                return RedirectToAction("Index");
            }
        }



        public ActionResult ChangeStatus(int id, string newStatus)
        {
            var questionPaper = db.QuestionPapers.FirstOrDefault(qp => qp.QuestionPaperID == id);
            if (questionPaper != null)
            {
                questionPaper.Status = newStatus;
                db.SaveChanges();
            }

            return RedirectToAction("Index");
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
