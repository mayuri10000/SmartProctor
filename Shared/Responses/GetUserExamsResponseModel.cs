using System;
using System.Collections.Generic;

namespace SmartProctor.Shared.Responses
{
    public class ExamDetails
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
    }
    
    public class GetUserExamsResponseModel : BaseResponseModel
    {
        public IList<ExamDetails> ExamDetailsList { get; set; }
    }
}