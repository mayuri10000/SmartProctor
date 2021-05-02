using Microsoft.AspNetCore.Mvc;
using SmartProctor.Server.Services;
using SmartProctor.Server.Utils;
using SmartProctor.Shared.Requests;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Server.Controllers.Exam
{
    /// <summary>
    /// Controller used by both the exam takers and the exam creator to view the exam questions.
    /// Exam takers can only get questions during the exam session, while exam creator can get the
    /// question at any time.
    /// </summary>
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