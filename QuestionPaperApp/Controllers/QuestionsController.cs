using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
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


    public class QuestionsController : Controller
    {
        private QAManagementEntities db = new QAManagementEntities();

        // GET: Questions
        public ActionResult Index(int? questionPaperID)
        {
            if (questionPaperID!=null)
            {
                ViewBag.QuestionPaperID = Convert.ToInt32(((int)questionPaperID).ToString());
                var questions = db.Questions.Where(q => q.QuestionPaperID == questionPaperID);
                return View(questions.ToList());

            }
            else
            {
                return View();
            }
            
        }

        // GET: Questions/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Question question = db.Questions.Find(id);
            if (question == null)
            {
                return HttpNotFound();
            }
            return View(question);
        }

        // GET: Questions/Create
        public ActionResult Create(int ? QuestionPaperID)
        
        {
          
            int currentQuestionPaperId = Convert.ToInt32(QuestionPaperID);

       
            var currentQuestionPaperTitle = db.QuestionPapers
                                                .Where(q => q.QuestionPaperID == currentQuestionPaperId)
                                                .Select(q => q.Title)
                                                .FirstOrDefault();

          
            if (!string.IsNullOrEmpty(currentQuestionPaperTitle))
            {

                ViewBag.QuestionPaperNameID = currentQuestionPaperTitle;
            }
            else
            {
          
                ViewBag.QuestionPaperNameID = "No current question paper available";
            }

            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Question question)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Questions.Add(question);
                    db.SaveChanges();
                    TempData["success"] = "Question Created Successfully";
                    return RedirectToAction("Index", new { questionPaperID = question.QuestionPaperID });
                }
                catch (DbEntityValidationException ex)
                {
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);
                        }
                    }
              
                }
            }

            ViewBag.QuestionPaperID = new SelectList(db.QuestionPapers, "QuestionPaperID", "Title", question.QuestionPaperID);
            return View(question);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Question question = db.Questions.Find(id);
            if (question == null)
            {
                return HttpNotFound();
            }
            ViewBag.QuestionPaperID = new SelectList(db.QuestionPapers, "QuestionPaperID", "Title", question.QuestionPaperID);
            return View(question);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "QuestionID,QuestionPaperID,QuestionText,OptionA,OptionB,OptionC,OptionD,CorrectAnswer")] Question question)
        {
            if (ModelState.IsValid)
            {
                db.Entry(question).State = EntityState.Modified;
                db.SaveChanges();
                TempData["success"] = "Change Saved Successfully";
                return RedirectToAction("Index", new { questionPaperID = question.QuestionPaperID });
            }
            ViewBag.QuestionPaperID = new SelectList(db.QuestionPapers, "QuestionPaperID", "Title", question.QuestionPaperID);
            return View(question);
        }

        // GET: Questions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Question question = db.Questions.Find(id);
            if (question == null)
            {
                return HttpNotFound();
            }
            return View(question);
        }

        // POST: Questions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        
            public ActionResult DeleteConfirmed(int id)
            {
                Question question = db.Questions.Find(id);
                int questionPaperID = Convert.ToInt32(question.QuestionPaperID); 
                var answers = db.Answers.Where(a => a.QuestionID == id).ToList(); 

                foreach (var answer in answers)
                {
                    db.Answers.Remove(answer);
                }

                db.Questions.Remove(question); 
                db.SaveChanges();

                TempData["success"] = "Question and related answers deleted successfully";
                return RedirectToAction("Index", new { questionPaperID });
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
