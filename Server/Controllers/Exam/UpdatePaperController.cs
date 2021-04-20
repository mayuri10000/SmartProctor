using Microsoft.AspNetCore.Mvc;
using SmartProctor.Server.Services;
using SmartProctor.Server.Utils;
using SmartProctor.Shared.Requests;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Server.Controllers.Exam
{
    [ApiController]
    [Route("api/exam/[controller]")]
    public class UpdatePaperController : ControllerBase
    { 
        private IExamServices _examServices;

        public UpdatePaperController(IExamServices examServices)
        {
            _examServices = examServices;
        }
        
        [HttpPost]
        public BaseResponseModel Post(UpdatePaperRequestModel model)
        {
            if (User.Identity?.Name == null)
            {
                return ErrorCodes.CreateSimpleResponse(ErrorCodes.NotLoggedIn);
            }

            return ErrorCodes.CreateSimpleResponse(
                _examServices.EditPaper(User.Identity?.Name, 
                    model.ExamId, model.QuestionJsons));
        }
    }
}