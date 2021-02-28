using Microsoft.AspNetCore.Mvc;
using SmartProctor.Server.Services;
using SmartProctor.Server.Utils;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Server.Controllers.Exam
{
    [ApiController]
    [Route("api/exam/[controller]")]
    public class GetExamTakersController : ControllerBase
    {
        private IExamServices _services;

        public GetExamTakersController(IExamServices services)
        {
            _services = services;
        }

        [HttpGet("{eid}")]
        public BaseResponseModel Get(int eid)
        {
            var e = _services.GetExamTakers(eid);
            if (e != null)
            {
                return new GetExamTakersResponseModel()
                {
                    Code = 0,
                    Message = "Success",
                    ExamTakers = e
                };
            }

            return ErrorCodes.CreateSimpleResponse(ErrorCodes.ExamNotExist);
        }
    }
}