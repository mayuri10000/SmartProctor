﻿using Microsoft.AspNetCore.Mvc;
using SmartProctor.Server.Services;
using SmartProctor.Server.Utils;
using SmartProctor.Shared.Requests;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Server.Controllers.Exam
{
    /// <summary>
    /// Controller used for creating a new exam. The new exam will not include any question at first.
    /// </summary>
    [ApiController]
    [Route("api/exam/[controller]")]
    public class CreateExamController : ControllerBase
    {
        private IExamServices _services;

        public CreateExamController(IExamServices services)
        {
            _services = services;
        }

        [HttpPost]
        public BaseResponseModel Post(CreateExamRequestModel model)
        {
            if (User.Identity?.Name == null)
            {
                return ErrorCodes.CreateSimpleResponse(ErrorCodes.NotLoggedIn);
            }

            try
            {
                // convert the duration
                var duration = model.Duration.Hour * 3600 + model.Duration.Minute * 60 + model.Duration.Second;

                var a = new Data.Entities.Exam()
                {
                    Creator = User.Identity?.Name,
                    Description = model.Description,
                    Duration = duration,
                    Name = model.ExamTitle,
                    StartTime = model.StartTime,
                    MaximumTakersNum = model.MaxTakers,
                    OpenBook = model.OpenBook
                };
                
                _services.SaveObject(a);
                return ErrorCodes.CreateSimpleResponse(ErrorCodes.Success);
            }
            catch
            {
                return ErrorCodes.CreateSimpleResponse(ErrorCodes.UnknownError);
            }

        }
    }
}