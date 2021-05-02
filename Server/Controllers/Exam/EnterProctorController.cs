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
    /// Controller called before a proctor enter the proctoring session of the exam, will return success
    /// if the current user is eligible for the current proctoring session.
    /// </summary>
    [ApiController]
    [Route("api/exam/[controller]")]
    public class EnterProctorController : ControllerBase
    {
        private IExamServices _services;

        public EnterProctorController(IExamServices services)
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

            return ErrorCodes.CreateSimpleResponse(_services.EnterProctor(eid, uid));
        }
    }
}