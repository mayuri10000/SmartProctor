using System.Collections.Generic;

namespace SmartProctor.Shared.Responses
{
    public class UserBasicInfo
    {
        public string Id { get; set; }
        public string Nickname { get; set; }
        public string Avatar { get; set; }
    }
    
    public class GetExamTakersResponseModel : BaseResponseModel
    {
        public IList<UserBasicInfo> ExamTakers { get; set; }
    }
}