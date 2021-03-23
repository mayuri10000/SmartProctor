using System.ComponentModel.DataAnnotations;

namespace SmartProctor.Shared.Questions
{
    public class BaseQuestion
    {
        [Required]
        public string Question { get; set; }
        
        [Required]
        public QuestionType QuestionType { get; set; }
    }
}