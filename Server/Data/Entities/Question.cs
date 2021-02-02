using System;
using System.Collections.Generic;

#nullable disable

namespace SmartProctor.Server.Data.Entities
{
    public partial class Question
    {
        public int ExamId { get; set; }
        public int Number { get; set; }
        public string QuestionJson { get; set; }
    }
}
