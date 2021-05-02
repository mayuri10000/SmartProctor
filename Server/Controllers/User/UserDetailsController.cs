using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartProctor.Server.Services;
using SmartProctor.Server.Utils;
using SmartProctor.Shared.Requests;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Server.Controllers.User
{
    /// <summary>
    /// Controller used for getting/setting the details of the current user. When GET, returns the
    /// user information, while POST will update the user information.
    /// </summary>
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
            if (User.Identity?.Name == null)
            {
                return ErrorCodes.CreateSimpleResponse(ErrorCodes.NotLoggedIn);
            }

            var uid = User.Identity.Name;
            
            var u = _services.GetObject(uid);
            return new UserDetailsResponseModel()
            {
                Code = 0,
                Id = u.Id,
                NickName = u.NickName,
                Email = u.Email,
                Phone = u.Phone,
                Avatar = u.Avatar
            };
        }

        [HttpPost]
        public BaseResponseModel Post(UserDetailsRequestModel model)
        {
            if (User.Identity?.Name == null)
            {
                return ErrorCodes.CreateSimpleResponse(ErrorCodes.NotLoggedIn);
            }

            var uid = User.Identity.Name;

            var res = _services.ChangeUserInfo(uid, model.NickName, model.Email, model.Phone);
            
            return ErrorCodes.CreateSimpleResponse(res); 
        }
    }
}