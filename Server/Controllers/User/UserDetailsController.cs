using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartProctor.Server.Services;
using SmartProctor.Server.Utils;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Server.Controllers.User
{
    [ApiController]
    [Route("api/user/[controller]")]
    public class UserDetailsController : ControllerBase
    {
        private IUserServices _services;

        public UserDetailsController(IUserServices services)
        {
            _services = services;
        }

        [HttpGet]
        public BaseResponseModel Get()
        {
            if (!HttpContext.Session.IsAvailable)
            {
                return ErrorCodes.CreateSimpleResponse(ErrorCodes.NotLoggedIn);
            }
            
            var uid = HttpContext.Session.GetString("UID");
            if (uid == null)
            {
                return ErrorCodes.CreateSimpleResponse(ErrorCodes.NotLoggedIn);
            }

            var u = _services.GetObject(uid);
            return new UserDetailsResponseModel()
            {
                Code = 0,
                Message = "Success",
                Id = u.Id,
                NickName = u.NickName,
                Email = u.Email,
                Phone = u.Phone
            };
        }
    }
}