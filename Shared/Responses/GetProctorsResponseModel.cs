using System.Collections.Generic;

namespace SmartProctor.Shared.Responses
{
    public class GetProctorsResponseModel : BaseResponseModel
    {
        public IList<UserBasicInfo> Proctors { get; set; }
    }
}