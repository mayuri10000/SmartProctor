using Microsoft.AspNetCore.Mvc;
using SmartProctor.Server.Services;
using SmartProctor.Server.Utils;
using SmartProctor.Shared.Requests;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Server.Controllers.Exam
{
    [ApiController]
    [Route("api/exam/[controller]")]
    public class GetEventsController : ControllerBase
    {
        private IExamServices _examServices;

        public GetEventsController(IExamServices examServices)
        {
            _examServices = examServices;
        }

        [HttpGet("{eid:int}")]
        public BaseResponseModel Get(int eid)
        {
            if (User.Identity?.Name == null)
            {
                return ErrorCodes.CreateSimpleResponse(ErrorCodes.NotLoggedIn);
            }
                        
            var uid = User.Identity.Name;
            
            var res = _examServices.GetEvents(uid, eid);

            if (res == null)
            {
                return ErrorCodes.CreateSimpleResponse(ErrorCodes.UnknownError);
            }
            
            return new GetEventsResponseModel
            {
                Code = ErrorCodes.Success,
                Events = res
            };
        }
    }
}