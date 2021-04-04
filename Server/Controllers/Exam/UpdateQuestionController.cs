using Microsoft.AspNetCore.Mvc;
using SmartProctor.Server.Services;
using SmartProctor.Server.Utils;
using SmartProctor.Shared.Requests;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Server.Controllers.Exam
{
    [ApiController]
    [Route("api/exam/[controller]")]
    public class UpdateQuestionController : ControllerBase
    { 
        private IExamServices _examServices;

        public UpdateQuestionController(IExamServices examServices)
        {
            _examServices = examServices;
        }
        
        [HttpPost]
        public BaseResponseModel Post(UpdateQuestionRequestModel model)
        {
            if (User.Identity?.Name == null)
            {
                return ErrorCodes.CreateSimpleResponse(ErrorCodes.NotLoggedIn);
            }

            return ErrorCodes.CreateSimpleResponse(
                _examServices.EditQuestion(User.Identity?.Name, 
                    model.ExamId, model.QuestionNumber, model.QuestionJson));
        }
    }
}