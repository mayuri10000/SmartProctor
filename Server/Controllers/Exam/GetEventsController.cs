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

        [HttpPost]
        public BaseResponseModel Post(GetEventsRequestModel model)
        {
            var res = _examServices.GetEvents(model.ExamId, model.Receipt, model.Sender);

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