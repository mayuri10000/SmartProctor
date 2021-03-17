using Microsoft.AspNetCore.Mvc;
using SmartProctor.Server.Services;
using SmartProctor.Server.Utils;
using SmartProctor.Shared.Requests;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Server.Controllers.Exam
{
    [ApiController]
    [Route("api/exam/[controller]")]
    public class GetQuestionController : ControllerBase
    {
        private IExamServices _examServices;

        public GetQuestionController(IExamServices examServices)
        {
            _examServices = examServices;
        }

        [HttpPost]
        public BaseResponseModel Post(GetQuestionRequestModel model)
        {
            if (User.Identity?.Name == null)
            {
                return ErrorCodes.CreateSimpleResponse(ErrorCodes.NotLoggedIn);
            }

            var res = _examServices.GetQuestion(User.Identity?.Name, model.ExamId, model.QuestionNumber, out var q);

            if (res == ErrorCodes.Success)
            {
                return new GetQuestionResponseModel()
                {
                    Code = res,
                    QuestionJson = q.QuestionJson
                };
            }
            else
            {
                return ErrorCodes.CreateSimpleResponse(res);
            }
        }
    }
}