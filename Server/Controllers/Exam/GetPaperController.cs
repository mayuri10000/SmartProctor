using Microsoft.AspNetCore.Mvc;
using SmartProctor.Server.Services;
using SmartProctor.Server.Utils;
using SmartProctor.Shared.Requests;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Server.Controllers.Exam
{
    [ApiController]
    [Route("api/exam/[controller]")]
    public class GetPaperController : ControllerBase
    {
        private IExamServices _examServices;

        public GetPaperController(IExamServices examServices)
        {
            _examServices = examServices;
        }

        [HttpGet("{examId}")]
        public BaseResponseModel Get(int examId)
        {
            if (User.Identity?.Name == null)
            {
                return ErrorCodes.CreateSimpleResponse(ErrorCodes.NotLoggedIn);
            }

            var res = _examServices.GetPaper(User.Identity?.Name, examId, out var q);

            if (res == ErrorCodes.Success)
            {
                return new GetPaperResponseModel()
                {
                    Code = res,
                    QuestionJsons = q
                };
            }
            else
            {
                return ErrorCodes.CreateSimpleResponse(res);
            }
        }
    }
}