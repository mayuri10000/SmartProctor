using Microsoft.AspNetCore.Mvc;
using SmartProctor.Server.Services;
using SmartProctor.Server.Utils;
using SmartProctor.Shared.Requests;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Server.Controllers.Exam
{
    /// <summary>
    /// Controller used to add proctor to the exam, should always called by the exam creater
    /// </summary>
    [ApiController]
    [Route("api/exam/[controller]")]
    public class AddProctorController : ControllerBase
    {
        private IExamServices _services;

        public AddProctorController(IExamServices services)
        {
            _services = services;
        }

        [HttpPost]
        public BaseResponseModel Post(AddProctorRequestModel model)
        {
            // Check if logged in
            if (User.Identity?.Name == null)
            {
                return ErrorCodes.CreateSimpleResponse(ErrorCodes.NotLoggedIn);
            }
            
            var uid = User.Identity.Name;

            var res = _services.AddProctor(uid, model.UserId, model.ExamId);

            return ErrorCodes.CreateSimpleResponse(res);
        }
    }
}