using System.Collections.Generic;

namespace SmartProctor.Shared.Requests
{
    public class UpdatePaperRequestModel
    {
        public int ExamId { get; set; }
        public IList<string> QuestionJsons { get; set; }
    }
}