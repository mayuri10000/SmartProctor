using Microsoft.AspNetCore.Authorization;
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

        [HttpGet("{eid:int}")]
        public BaseResponseModel Get(int eid)
        {
            if (User.Identity?.Name == null)
            {
                return ErrorCodes.CreateSimpleResponse(ErrorCodes.NotLoggedIn);
            }
            
            var uid = User.Identity.Name;

            var res = _services.Attempt(eid, uid, out var banReason);

            return new AttemptExamResponseModel()
            {
                Code = res,
                BanReason = banReason
            };
        }
    }
}