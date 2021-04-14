using System;

namespace SmartProctor.Shared.Responses
{
    public class GetAnswerResponseModel : BaseResponseModel
    {
        public string AnswerJson { get; set; }
        public DateTime? AnswerTime { get; set; }
    }
}