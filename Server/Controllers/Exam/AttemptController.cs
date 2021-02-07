using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using SmartProctor.Server.Services;
using SmartProctor.Server.Utils;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Server.Controllers.Exam
{
    [ApiController]
    [Route("api/exam/[controller]")]
    public class AttemptController : ControllerBase
    {
        private IExamServices _services;

        public AttemptController(IExamServices services)
        {
            _services = services;
        }

        [HttpGet("{eid}")]
        public BaseResponseModel Get(int eid)
        {
            if (!HttpContext.Session.IsAvailable)
            {
                return ErrorCodes.CreateSimpleResponse(ErrorCodes.NotLoggedIn);
            }
            
            var uid = HttpContext.Session.GetString("UID");
            if (uid == null)
            {
                return ErrorCodes.CreateSimpleResponse(ErrorCodes.NotLoggedIn);
            }

            return ErrorCodes.CreateSimpleResponse(_services.Attempt(eid, uid));
        }
    }
}