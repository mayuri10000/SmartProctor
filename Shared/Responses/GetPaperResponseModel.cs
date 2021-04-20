using System.Collections.Generic;

namespace SmartProctor.Shared.Responses
{
    public class GetPaperResponseModel : BaseResponseModel
    {
        public IList<string> QuestionJsons { get; set; }
    }
}