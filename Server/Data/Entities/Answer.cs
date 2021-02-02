using System;
using System.Collections.Generic;

#nullable disable

namespace SmartProctor.Server.Data.Entities
{
    public partial class Answer
    {
        public string UserId { get; set; }
        public int ExamId { get; set; }
        public int QuestionNum { get; set; }
        public string AnswerJson { get; set; }
        public DateTime? AnswerTime { get; set; }
    }
}
