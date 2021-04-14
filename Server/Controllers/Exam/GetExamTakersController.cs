using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using SmartProctor.Server.Services;
using SmartProctor.Server.Utils;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Server.Controllers.Exam
{
    [ApiController]
    [Route("api/exam/[controller]")]
    public class GetExamTakersController : ControllerBase
    {
        private IExamServices _services;
        private IUserServices _userServices;

        public GetExamTakersController(IExamServices services, IUserServices userServices)
        {
            _services = services;
            _userServices = userServices;
        }

        [HttpGet("{eid}")]
        public BaseResponseModel Get(int eid)
        {
            var e = _services.GetExamTakers(eid);
            if (e != null)
            {
                var list = new List<UserBasicInfo>();
                foreach (var uid in e)
                {
                    var u = _userServices.GetObject(uid);

                    if (u != null)
                    {
                        list.Add(new UserBasicInfo()
                        {
                            Id = u.Id,
                            Nickname = u.NickName,
                            Avatar = u.Avatar
                        });
                    }
                }
                
                return new GetExamTakersResponseModel()
                {
                    Code = 0,
                    ExamTakers = list
                };
            }

            return ErrorCodes.CreateSimpleResponse(ErrorCodes.ExamNotExist);
        }
    }
}