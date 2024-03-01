//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace QuestionPaperApp.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Answer
    {
        public int AnswerID { get; set; }
        [Required]
        public Nullable<int> QuestionID { get; set; }
        [Required]
        public Nullable<int> QuestionPaperID { get; set; }
        [Required]
        public Nullable<int> UserID { get; set; }
        [Required]
        public string SelectedOption { get; set; }
        [Required]
        public Nullable<bool> IsCorrect { get; set; }
    
        public virtual Question Question { get; set; }
        public virtual QuestionPaper QuestionPaper { get; set; }
        public virtual User User { get; set; }
    }
}