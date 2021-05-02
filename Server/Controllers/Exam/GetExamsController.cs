using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using SmartProctor.Server.Services;
using SmartProctor.Server.Utils;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Server.Controllers.Exam
{
    /// <summary>
    /// This controller is used for fetching a list of exam for the given role of the current user.
    /// Note that when role = 3, exam created by the current user will be returned, otherwise the role
    /// is defined in <see cref="SmartProctor.Shared.Consts"/>
    /// </summary>
    [ApiController]
    [Route("api/exam/[controller]")]
    public class GetExamsController : ControllerBase
    {
        private IExamServices _services;

        public GetExamsController(IExamServices services)
        {
            _services = services;
        }

        [HttpGet("{role}")]
        public BaseResponseModel Get(int role)
        {
            if (User.Identity?.Name == null)
            {
                return ErrorCodes.CreateSimpleResponse(ErrorCodes.NotLoggedIn);
            }
            
            var uid = User.Identity.Name;


            IList<ExamDetails> res;
            
            // when role = 3, gets exam created by the current user
            if (role == 3)
            {
                res = _services.GetCreatedExams(uid);
            }
            // Otherwise use the role defined in Consts
            else
            {
                res = _services.GetExamsForUser(uid, role);
            }

            if (res != null)
            {
                return new GetUserExamsResponseModel()
                {
                    Code = 0,
                    ExamDetailsList = res
                };
            }

            return ErrorCodes.CreateSimpleResponse(ErrorCodes.UnknownError);
        }
    }
}