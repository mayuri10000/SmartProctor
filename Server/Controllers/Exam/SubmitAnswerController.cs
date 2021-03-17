using Microsoft.AspNetCore.Mvc;
using SmartProctor.Server.Services;
using SmartProctor.Server.Utils;
using SmartProctor.Shared.Requests;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Server.Controllers.Exam
{
    [ApiController]
    [Route("api/exam/[controller]")]
    public class SubmitAnswerController : ControllerBase
    {
        private IExamServices _examServices;

        public SubmitAnswerController(IExamServices examServices)
        {
            _examServices = examServices;
        }
        
        [HttpPost]
        public BaseResponseModel Post(SubmitAnswerRequestModel model)
        {
            if (User.Identity?.Name == null)
            {
                return ErrorCodes.CreateSimpleResponse(ErrorCodes.NotLoggedIn);
            }

            var res = _examServices.SubmitAnswer(User.Identity?.Name, model.ExamId, model.QuestionNum,
                model.AnswerJson);

            return ErrorCodes.CreateSimpleResponse(res);
        }
    }
}