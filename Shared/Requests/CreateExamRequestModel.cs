using System;
using System.ComponentModel.DataAnnotations;

namespace SmartProctor.Shared.Requests
{
    public class CreateExamRequestModel
    {
        [MaxLength(30)]
        [Required]
        public string ExamTitle { get; set; }
        
        public string Description { get; set; }
        
        [Required]
        public DateTime StartTime { get; set; }
        
        [Required]
        public DateTime Duration { get; set; }
        
        [Required]
        public bool OpenBook { get; set; }
        
        [Required]
        public int MaxTakers { get; set; }
    }
}