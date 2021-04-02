using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using SmartProctor.Server.Services;
using SmartProctor.Server.Utils;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Server.Controllers.Exam
{
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
            
            if (role == 3)
            {
                res = _services.GetCreatedExams(uid);
            }
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