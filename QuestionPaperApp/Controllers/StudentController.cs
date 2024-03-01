using Q_AManagement.Filter;
using QuestionPaperApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace QuestionPaperApp.Controllers
{
    [CustomAuthorize]
    [StudentAuthorizeFilter]
    
    public class StudentController : Controller
    {
        private QAManagementEntities db = new QAManagementEntities();

        public ActionResult Index()
        {
       
            int userId = (int)Session["UserID"];

          
            var questionPapers = db.QuestionPapers.Where(q => q.Status == "Approved").ToList();

       
            var attemptedQuestionPapers = new Dictionary<int, bool>();

           
            foreach (var paper in questionPapers)
            {
                bool hasAttempt = db.Answers.Any(a => a.QuestionPaperID == paper.QuestionPaperID && a.UserID == userId);
                attemptedQuestionPapers.Add(paper.QuestionPaperID, hasAttempt);
            }

   
            ViewBag.QuestionPapers = questionPapers;
            ViewBag.AttemptedQuestionPapers = attemptedQuestionPapers; 

            return View(questionPapers);
        }
        public ActionResult Details(int id) 
        {

            var questionpaper = db.QuestionPapers.Find(id);
            return View(questionpaper);
        }

        public ActionResult AttemptPaper(int id) {
            var questionpaper = db.QuestionPapers.Find(id);
            ViewBag.QuestionpaperID=questionpaper.QuestionPaperID;
            var questions = db.Questions.Where(q => q.QuestionPaperID == id).ToList();
            return View(questions);
        
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AnswerQuestionPaper(int questionPaperId, FormCollection answer)
        {

            for (int i = 1; i < answer.Count; i++)
            {
                var questionId = Convert.ToInt16(answer.GetKey(i));
                var tickedAnswer = answer[i];
                var correctAnswer = db.Questions.Where(model => model.QuestionID == questionId).FirstOrDefault().CorrectAnswer.ToString();
                bool iscorrect;
                if (correctAnswer == tickedAnswer) iscorrect = true;
                else iscorrect = false;
                var submission = new Answer
                {
                    UserID = Convert.ToInt32(Session["UserID"]),
                    QuestionPaperID =questionPaperId,
                    SelectedOption = tickedAnswer,
                    QuestionID = questionId,
                    IsCorrect = iscorrect
                };
                db.Answers.Add(submission);
            }

            TempData["success"] = "You have successfully submitted the exam!!";
            db.SaveChanges();

            return RedirectToAction("Index");
        }
      
        public ActionResult ViewAnswer(int Id) {
              
            int userId = (int)Session["UserID"];
            var answers = db.Answers.Where(a => a.QuestionPaperID == Id && a.UserID == userId).ToList();
            return  View(answers);  

        }

    }

}