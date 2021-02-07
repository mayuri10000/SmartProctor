using Microsoft.AspNetCore.Mvc;
using SmartProctor.Server.Services;
using SmartProctor.Server.Utils;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Server.Controllers.Exam
{
    [ApiController]
    [Route("api/exam/[controller]")]
    public class ExamDetailsController : ControllerBase
    {
        private IExamServices _services;

        public ExamDetailsController(IExamServices services)
        {
            _services = services;
        }

        [HttpGet("{eid}")]
        public BaseResponseModel Get(int eid)
        {
            var e = _services.GetObject(eid);
            if (eid != null)
            {
                return new ExamDetailsResponseModel()
                {
                    Code = 0,
                    Message = "Success",
                    Description = e.Description,
                    Duration = e.Duration,
                    Name = e.Name,
                    StartTime = e.StartTime
                };
            }

            return ErrorCodes.CreateSimpleResponse(ErrorCodes.ExamNotExist);
        }
    }
}