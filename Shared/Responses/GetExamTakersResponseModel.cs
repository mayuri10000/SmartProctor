using System.Collections.Generic;

namespace SmartProctor.Shared.Responses
{
    public class GetExamTakersResponseModel : BaseResponseModel
    {
        public IList<string> ExamTakers { get; set; }
    }
}