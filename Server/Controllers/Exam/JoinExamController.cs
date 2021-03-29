using Microsoft.AspNetCore.Mvc;
using SmartProctor.Server.Services;
using SmartProctor.Server.Utils;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Server.Controllers.Exam
{
    [ApiController]
    [Route("api/exam/[controller]")]
    public class JoinExamController : ControllerBase
    {
        private IExamServices _services;

        public JoinExamController(IExamServices services)
        {
            _services = services;
        }

        [HttpGet("{eid}")]
        public BaseResponseModel Get(int eid)
        {
            if (User.Identity?.Name == null)
            {
                return ErrorCodes.CreateSimpleResponse(ErrorCodes.NotLoggedIn);
            }
            
            var uid = User.Identity.Name;

            var res = _services.JoinExam(uid, eid, out var banReason);

            return new AttemptExamResponseModel()
            {
                Code = res,
                BanReason = banReason
            };
        }
    }
}