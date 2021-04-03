using System.ComponentModel.DataAnnotations;

namespace SmartProctor.Shared.Questions
{
    public class BaseQuestion
    {
        [Required]
        public string Question { get; set; }
        
        [Required]
        public string QuestionType { get; set; }
    }
}