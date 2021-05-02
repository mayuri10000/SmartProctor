using System;
using Microsoft.AspNetCore.Mvc;
using SmartProctor.Server.Services;
using SmartProctor.Server.Utils;
using SmartProctor.Shared.Requests;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Server.Controllers.Exam
{
    /// <summary>
    /// Controller used by the exam takers and exam creator to get the answers.
    /// Exam takers can only get his/her own answer during the exam session, while
    /// exam creator can get answer from any exam taker in the exam at any time.
    /// </summary>
    [ApiController]
    [Route("api/exam/[controller]")]
    public class GetAnswerController : ControllerBase
    {
        private IExamServices _services;
        private IUserServices _userServices;

        public GetAnswerController(IExamServices services, IUserServices userServices)
        {
            _services = services;
            _userServices = userServices;
        }

        [HttpPost]
        public BaseResponseModel Post(GetAnswerRequestModel model)
        {
            if (User.Identity?.Name == null)
            {
                return ErrorCodes.CreateSimpleResponse(ErrorCodes.NotLoggedIn);
            }

            var res = _services.GetAnswer(User.Identity.Name, model.UserId, model.ExamId, model.QuestionNum,
                out var json, out var time);

            if (res != ErrorCodes.Success)
            {
                return ErrorCodes.CreateSimpleResponse(res);
            }
            
            return new GetAnswerResponseModel()
            {
                AnswerJson = json,
                AnswerTime = time,
                Code = res
            };
        }
    }
}