using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using SmartProctor.Server.Services;
using SmartProctor.Server.Utils;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Server.Controllers.Exam
{
    /// <summary>
    /// Controller called before entering an exam. Will return success if
    /// the exam taker is eligible for the exam. Otherwise an error code
    /// will be returned.
    /// </summary>
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
            // Check if logged in
            if (User.Identity?.Name == null)
            {
                return ErrorCodes.CreateSimpleResponse(ErrorCodes.NotLoggedIn);
            }
            
            var uid = User.Identity.Name;

            // Call the service tier, if the exam taker was banned, the reason will be returned
            var res = _services.Attempt(eid, uid, out var banReason);

            return new AttemptExamResponseModel()
            {
                Code = res,
                BanReason = banReason
            };
        }
    }
}