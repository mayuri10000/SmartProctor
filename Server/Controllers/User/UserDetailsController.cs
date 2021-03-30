using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartProctor.Server.Services;
using SmartProctor.Server.Utils;
using SmartProctor.Shared.Requests;
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
            
            var u = _services.GetObject(uid);

            u.Email = model.Email;
            u.Phone = model.Phone;
            u.NickName = model.NickName;
            
            _services.SaveObject(u);
            
            return ErrorCodes.CreateSimpleResponse(ErrorCodes.Success); 
        }
    }
}