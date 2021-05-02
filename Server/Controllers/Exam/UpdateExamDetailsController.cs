using Microsoft.AspNetCore.Mvc;
using SmartProctor.Server.Services;
using SmartProctor.Server.Utils;
using SmartProctor.Shared.Requests;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Server.Controllers.Exam
{
    /// <summary>
    /// Controller used by the exam creator to update the exam information
    /// </summary>
    [ApiController]
    [Route("api/exam/[controller]")]
    public class UpdateExamDetailsController : ControllerBase
    {
        private IExamServices _examServices;

        public UpdateExamDetailsController(IExamServices examServices)
        {
            _examServices = examServices;
        }
        
        [HttpPost]
        public BaseResponseModel Post(UpdateExamDetailsRequestModel model)
        {
            if (User.Identity?.Name == null)
            {
                // Not logged in
                return ErrorCodes.CreateSimpleResponse(ErrorCodes.NotLoggedIn);
            }

            var exam = _examServices.GetObject(model.Id);

            if (exam == null)
            {
                // Exam not exists
                return ErrorCodes.CreateSimpleResponse(ErrorCodes.ExamNotExist);
            }

            if (exam.Creator != User.Identity?.Name)
            {
                return ErrorCodes.CreateSimpleResponse(ErrorCodes.ExamNotPermitToEdit);
            }
            
            var duration = model.Duration.Hour * 3600 + model.Duration.Minute * 60 + model.Duration.Second;

            exam.Name = model.Name;
            exam.Description = model.Description;
            exam.Duration = duration;
            exam.StartTime = model.StartTime;
            exam.OpenBook = model.OpenBook;
            exam.MaximumTakersNum = model.MaximumTakersNum;
            
            _examServices.SaveObject(exam);

            return ErrorCodes.CreateSimpleResponse(ErrorCodes.Success);
        }
    }
}